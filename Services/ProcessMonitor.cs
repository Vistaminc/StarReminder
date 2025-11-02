using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaDetectionSystem.Models;

namespace MediaDetectionSystem.Services
{
    public class ProcessEventArgs : EventArgs
    {
        public ProcessInfo ProcessInfo { get; set; } = new ProcessInfo();
        public string Action { get; set; } = string.Empty;
        public MediaDeviceUsage? MediaUsage { get; set; } // 媒体设备使用情况
    }

    public class ProcessMonitor
    {
        private readonly Dictionary<int, DateTime> _processStartTimes = new();
        private readonly Dictionary<int, ProcessInfo> _monitoredProcesses = new();
        private readonly HashSet<int> _exceededProcesses = new(); // 跟踪已超时的进程
        private readonly MediaDeviceMonitor _mediaDeviceMonitor = new(); // 媒体设备监控器
        
        // 优化：记录已知的PID和对应的配置，避免重复扫描
        private readonly Dictionary<int, ProcessConfig> _trackedProcessIds = new();

        public event EventHandler<ProcessEventArgs>? ProcessDetected;
        public event EventHandler<ProcessEventArgs>? ProcessExceededLimit;

        public Dictionary<int, ProcessInfo> MonitoredProcesses => _monitoredProcesses;

        public void CheckProcesses(Dictionary<string, ProcessConfig> processConfigs)
        {
            var currentProcessIds = new ConcurrentBag<int>();
            var enabledConfigs = processConfigs.Values.Where(c => c.IsEnabled).ToList();
            
            // 优化：只有存在媒体相关进程配置时，才检查媒体设备
            var hasMediaRelatedConfigs = enabledConfigs.Any(c => IsMediaRelatedProcess(c.ProcessName));
            Dictionary<int, MediaDeviceUsage> activeDeviceUsersDict = new();
            
            if (hasMediaRelatedConfigs)
            {
                // 从设备角度检测，获取所有正在使用摄像头/麦克风的进程
                var activeDeviceUsers = _mediaDeviceMonitor.GetActiveDeviceUsers();
                activeDeviceUsersDict = activeDeviceUsers.ToDictionary(u => u.ProcessId, u => u);
            }
            
            // ===== 优化：快速路径 - 只检查已知的PID =====
            var trackedPids = _trackedProcessIds.Keys.ToList();
            Parallel.ForEach(trackedPids, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, pid =>
            {
                try
                {
                    // 快速检查：进程是否还在运行
                    var process = Process.GetProcessById(pid);
                    if (process == null || process.HasExited)
                    {
                        // 进程已退出，不添加到currentProcessIds，让后续清理
                        return;
                    }
                    
                    var config = _trackedProcessIds[pid];
                    bool isMediaRelated = IsMediaRelatedProcess(config.ProcessName);
                    bool shouldMonitor = true;
                    MediaDeviceUsage? mediaUsage = null;

                    // 如果是媒体相关进程，检查它是否在使用设备列表中
                    if (isMediaRelated)
                    {
                        if (activeDeviceUsersDict.TryGetValue(pid, out var usage))
                        {
                            mediaUsage = usage;
                            shouldMonitor = true;
                        }
                        else
                        {
                            shouldMonitor = false;
                        }
                    }

                    if (shouldMonitor)
                    {
                        currentProcessIds.Add(pid);
                        
                        // 更新运行时间和检查超时
                        UpdateProcessRuntime(pid, config, mediaUsage);
                    }
                    
                    process.Dispose();
                }
                catch (ArgumentException)
                {
                    // 进程不存在，忽略
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"检查已跟踪进程 PID:{pid} 时出错: {ex.Message}");
                }
            });
            
            // ===== 扫描新进程（实时检测） =====
            // 优化：由于使用了PID跟踪机制，新进程扫描只检查未跟踪的进程，性能开销很小
            // 因此每次CheckProcesses都进行新进程扫描，实现真正的实时检测
            
            // 获取进程列表（用于新进程扫描）
            Process[]? allProcesses = null;
            try
            {
                allProcesses = Process.GetProcesses();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取进程列表失败: {ex.Message}");
                return;
            }
            
            // 并行扫描新进程
            Parallel.ForEach(enabledConfigs, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, config =>
                {
                    try
                    {
                        var targetProcessName = Path.GetFileNameWithoutExtension(config.ProcessName);
                        var processes = allProcesses.Where(p => 
                            p.ProcessName.Equals(targetProcessName, StringComparison.OrdinalIgnoreCase)).ToList();

                        foreach (var process in processes)
                        {
                            try
                            {
                                // 跳过已经跟踪的PID
                                if (_trackedProcessIds.ContainsKey(process.Id))
                                    continue;

                                bool isMediaRelated = IsMediaRelatedProcess(config.ProcessName);
                                bool shouldMonitor = true;
                                MediaDeviceUsage? mediaUsage = null;

                                // 如果是媒体相关进程，检查它是否在使用设备列表中
                                if (isMediaRelated)
                                {
                                    if (activeDeviceUsersDict.TryGetValue(process.Id, out var usage))
                                    {
                                        mediaUsage = usage;
                                        shouldMonitor = true;
                                    }
                                    else
                                    {
                                        shouldMonitor = false;
                                    }
                                }

                                // 如果应该监控此进程
                                if (shouldMonitor)
                                {
                                    currentProcessIds.Add(process.Id);

                                    // 记录新检测到的进程
                                    lock (_processStartTimes)
                                    {
                                        if (!_processStartTimes.ContainsKey(process.Id))
                                        {
                                            // 添加到跟踪列表
                                            _trackedProcessIds[process.Id] = config;
                                            
                                            // 记录开始时间
                                            _processStartTimes[process.Id] = isMediaRelated ? DateTime.Now : process.StartTime;
                                            
                                            var processInfo = new ProcessInfo
                                            {
                                                Id = process.Id,
                                                Name = process.ProcessName,
                                                DisplayName = config.DisplayName,
                                                StartTime = _processStartTimes[process.Id],
                                                Status = "Running",
                                                Config = config
                                            };

                                            _monitoredProcesses[process.Id] = processInfo;
                                            
                                            // 发送进程检测事件
                                            ProcessDetected?.Invoke(this, new ProcessEventArgs 
                                            { 
                                                ProcessInfo = processInfo, 
                                                Action = "Detected",
                                                MediaUsage = mediaUsage
                                            });
                                            
                                            Debug.WriteLine($"[ProcessMonitor] 新进程: {processInfo.DisplayName} (PID:{process.Id})");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"检查进程 {process.ProcessName} 时出错: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"检查配置 {config.ProcessName} 时出错: {ex.Message}");
                    }
            });
            
            // 释放进程对象
            if (allProcesses != null)
            {
                foreach (var p in allProcesses)
                {
                    try { p.Dispose(); } catch { }
                }
            }

            // 清理不再监控的进程（包括已退出的进程和停止使用设备的媒体进程）
            var currentProcessIdSet = new HashSet<int>(currentProcessIds);
            List<int> exitedProcessIds;
            lock (_processStartTimes)
            {
                exitedProcessIds = _processStartTimes.Keys.Where(id => !currentProcessIdSet.Contains(id)).ToList();
            }
            foreach (var id in exitedProcessIds)
            {
                lock (_processStartTimes)
                {
                    _processStartTimes.Remove(id);
                    _exceededProcesses.Remove(id); // 清理超时状态
                    _trackedProcessIds.Remove(id); // 从跟踪列表移除
                    _mediaDeviceMonitor.CleanupExitedProcess(id); // 清理媒体设备监控缓存
                    
                    if (_monitoredProcesses.ContainsKey(id))
                    {
                        var processInfo = _monitoredProcesses[id];
                        processInfo.Status = "Exited";
                        
                        Debug.WriteLine($"[ProcessMonitor] 进程退出: {processInfo.DisplayName} (PID:{id})");
                        ProcessDetected?.Invoke(this, new ProcessEventArgs { ProcessInfo = processInfo, Action = "Exited" });
                        _monitoredProcesses.Remove(id);
                    }
                }
            }
        }
        
        /// <summary>
        /// 更新进程运行时间并检查超时（优化：独立方法，可被快速路径复用）
        /// </summary>
        private void UpdateProcessRuntime(int pid, ProcessConfig config, MediaDeviceUsage? mediaUsage)
        {
            ProcessInfo? processInfoUpdate = null;
            lock (_processStartTimes)
            {
                if (_processStartTimes.TryGetValue(pid, out var startTime) &&
                    _monitoredProcesses.TryGetValue(pid, out processInfoUpdate))
                {
                    var runtime = DateTime.Now - startTime;
                    processInfoUpdate.Runtime = runtime;

                    // 检查是否超时
                    if (config.MaxRuntime.TotalSeconds > 0)
                    {
                        // 检查是否已超时（只触发一次）
                        if (runtime >= config.MaxRuntime && !_exceededProcesses.Contains(pid))
                        {
                            _exceededProcesses.Add(pid);
                            
                            ProcessExceededLimit?.Invoke(this, new ProcessEventArgs 
                            { 
                                ProcessInfo = processInfoUpdate, 
                                Action = config.ActionType 
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 判断是否为媒体相关进程（需要检测摄像头/麦克风使用）
        /// </summary>
        private bool IsMediaRelatedProcess(string processName)
        {
            var mediaProcessNames = new[]
            {
                "media_capture.exe",
                "WeChat.exe",
                "QQ.exe",
                "DingTalk.exe",
                "Zoom.exe",
                "Teams.exe",
                "Skype.exe",
                "chrome.exe",
                "msedge.exe",
                "firefox.exe"
            };

            return mediaProcessNames.Any(name => 
                processName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public ProcessInfo? GetProcessInfo(int processId)
        {
            return _monitoredProcesses.TryGetValue(processId, out var info) ? info : null;
        }

        /// <summary>
        /// 获取当前所有使用媒体设备的进程
        /// </summary>
        public IReadOnlyDictionary<int, MediaDeviceUsage> GetActiveMediaProcesses()
        {
            return _mediaDeviceMonitor.GetActiveMediaProcesses();
        }
    }
}
