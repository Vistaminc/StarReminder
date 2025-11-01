using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private readonly HashSet<int> _warnedProcesses = new(); // 跟踪已警告的进程
        private readonly HashSet<int> _exceededProcesses = new(); // 跟踪已超时的进程
        private readonly MediaDeviceMonitor _mediaDeviceMonitor = new(); // 媒体设备监控器

        public event EventHandler<ProcessEventArgs>? ProcessDetected;
        public event EventHandler<ProcessEventArgs>? ProcessExceededLimit;
        public event EventHandler<ProcessEventArgs>? ProcessNearLimit; // 新增：接近超时警告

        public Dictionary<int, ProcessInfo> MonitoredProcesses => _monitoredProcesses;

        private Process[]? _cachedProcesses = null;
        private DateTime _lastProcessCacheTime = DateTime.MinValue;
        private const int PROCESS_CACHE_SECONDS = 2; // 进程列表缓存2秒

        public void CheckProcesses(Dictionary<string, ProcessConfig> processConfigs)
        {
            // 优化：缓存进程列表
            var timeSinceCache = (DateTime.Now - _lastProcessCacheTime).TotalSeconds;
            if (_cachedProcesses == null || timeSinceCache >= PROCESS_CACHE_SECONDS)
            {
                _cachedProcesses = Process.GetProcesses();
                _lastProcessCacheTime = DateTime.Now;
            }
            var allProcesses = _cachedProcesses;
            var currentProcessIds = new HashSet<int>();

            // 第一步：从设备角度检测，获取所有正在使用摄像头/麦克风的进程
            var activeDeviceUsers = _mediaDeviceMonitor.GetActiveDeviceUsers();
            var activeDeviceUsersDict = activeDeviceUsers.ToDictionary(u => u.ProcessId, u => u);
            
            // 优化：减少调试输出
            if (activeDeviceUsers.Count > 0)
            {
                Debug.WriteLine($"[ProcessMonitor] 检测到 {activeDeviceUsers.Count} 个进程正在使用媒体设备");
            }

            foreach (var config in processConfigs.Values.Where(c => c.IsEnabled))
            {
                var targetProcessName = Path.GetFileNameWithoutExtension(config.ProcessName);
                var processes = allProcesses.Where(p => 
                    p.ProcessName.Equals(targetProcessName, StringComparison.OrdinalIgnoreCase)).ToList();

                foreach (var process in processes)
                {
                    try
                    {
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
                                Debug.WriteLine($"[ProcessMonitor] 目标进程 {process.ProcessName} (ID:{process.Id}) 正在使用设备 - 摄像头:{usage.IsCameraInUse}, 麦克风:{usage.IsMicrophoneInUse}");
                            }
                            else
                            {
                                shouldMonitor = false;
                                // Debug.WriteLine($"[ProcessMonitor] 目标进程 {process.ProcessName} (ID:{process.Id}) 未使用设备");
                            }
                        }
                        // 非媒体进程，直接监控
                        else
                        {
                            shouldMonitor = true;
                        }

                        // 如果应该监控此进程
                        if (shouldMonitor)
                        {
                            currentProcessIds.Add(process.Id);

                            // 如果是新检测到的进程（开始监控）
                            if (!_processStartTimes.ContainsKey(process.Id))
                            {
                                // 对于媒体相关进程，使用当前时间作为开始时间（因为是从使用设备开始计时）
                                // 对于普通进程，使用进程启动时间
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
                                
                                Debug.WriteLine($"[ProcessMonitor] 开始监控进程: {processInfo.DisplayName} (ID:{process.Id})");
                                
                                // 发送进程检测事件
                                ProcessDetected?.Invoke(this, new ProcessEventArgs 
                                { 
                                    ProcessInfo = processInfo, 
                                    Action = "Detected",
                                    MediaUsage = mediaUsage
                                });
                            }

                            // 更新运行时间
                            var runtime = DateTime.Now - _processStartTimes[process.Id];
                            var processInfoUpdate = _monitoredProcesses[process.Id];
                            processInfoUpdate.Runtime = runtime;

                            // 检查是否超时
                            if (config.MaxRuntime.TotalSeconds > 0)
                            {
                                var remainingTime = config.MaxRuntime - runtime;

                                // 检查是否已超时（只触发一次）
                                if (runtime >= config.MaxRuntime && !_exceededProcesses.Contains(process.Id))
                                {
                                    _exceededProcesses.Add(process.Id);
                                    Debug.WriteLine($"[ProcessMonitor] 进程超时: {processInfoUpdate.DisplayName} (ID:{process.Id}), 运行时间: {runtime.TotalSeconds}秒");
                                    
                                    ProcessExceededLimit?.Invoke(this, new ProcessEventArgs 
                                    { 
                                        ProcessInfo = processInfoUpdate, 
                                        Action = config.ActionType 
                                    });
                                }
                                // 检查是否接近超时（提前警告，只触发一次）
                                else if (config.AlertBeforeAction > 0 && 
                                        remainingTime.TotalSeconds <= config.AlertBeforeAction && 
                                        remainingTime.TotalSeconds > 0 &&
                                        !_warnedProcesses.Contains(process.Id))
                                {
                                    _warnedProcesses.Add(process.Id);
                                    Debug.WriteLine($"[ProcessMonitor] 进程接近超时: {processInfoUpdate.DisplayName} (ID:{process.Id}), 剩余时间: {remainingTime.TotalSeconds}秒");
                                    
                                    ProcessNearLimit?.Invoke(this, new ProcessEventArgs
                                    {
                                        ProcessInfo = processInfoUpdate,
                                        Action = "warning"
                                    });
                                }
                            }
                        }
                        else
                        {
                            // 如果是媒体相关进程但不再使用设备，且之前在监控中，则移除监控
                            if (isMediaRelated && _processStartTimes.ContainsKey(process.Id))
                            {
                                Debug.WriteLine($"[ProcessMonitor] 媒体进程 {process.ProcessName} (ID:{process.Id}) 停止使用摄像头/麦克风，停止监控");
                                
                                // 不添加到 currentProcessIds，让后续的清理逻辑处理
                                // 这样会触发 ProcessExited 事件
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"检查进程 {process.ProcessName} 时出错: {ex.Message}");
                    }
                }
            }

            // 清理不再监控的进程（包括已退出的进程和停止使用设备的媒体进程）
            var exitedProcessIds = _processStartTimes.Keys.Where(id => !currentProcessIds.Contains(id)).ToList();
            foreach (var id in exitedProcessIds)
            {
                _processStartTimes.Remove(id);
                _warnedProcesses.Remove(id); // 清理警告状态
                _exceededProcesses.Remove(id); // 清理超时状态
                _mediaDeviceMonitor.CleanupExitedProcess(id); // 清理媒体设备监控缓存
                
                if (_monitoredProcesses.ContainsKey(id))
                {
                    var processInfo = _monitoredProcesses[id];
                    processInfo.Status = "Exited";
                    
                    Debug.WriteLine($"[ProcessMonitor] 停止监控进程: {processInfo.DisplayName} (ID:{id})");
                    
                    ProcessDetected?.Invoke(this, new ProcessEventArgs { ProcessInfo = processInfo, Action = "Exited" });
                    _monitoredProcesses.Remove(id);
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
