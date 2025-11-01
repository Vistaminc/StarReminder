using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MediaDetectionSystem.Services
{
    /// <summary>
    /// 媒体设备使用情况
    /// </summary>
    public class MediaDeviceUsage
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public bool IsCameraInUse { get; set; }
        public bool IsMicrophoneInUse { get; set; }
        public DateTime DetectionTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 媒体设备监控器 - 检测摄像头和麦克风的实际使用情况
    /// </summary>
    public class MediaDeviceMonitor
    {
        private readonly Dictionary<int, MediaDeviceUsage> _deviceUsageCache = new();
        private DateTime _lastFullScan = DateTime.MinValue;
        private const int FULL_SCAN_INTERVAL_SECONDS = 5; // 完整扫描间隔：5秒

        /// <summary>
        /// 获取所有正在使用摄像头或麦克风的进程列表（从设备角度检测）
        /// </summary>
        public List<MediaDeviceUsage> GetActiveDeviceUsers()
        {
            var activeUsers = new List<MediaDeviceUsage>();

            try
            {   
                // 方法1：优先使用注册表检测（性能更好）
                var cameraUsers = GetDeviceUsers("webcam", true);
                var microphoneUsers = GetDeviceUsers("microphone", false);

                // 优化：只在必要时且满足间隔时使用备用方法
                var timeSinceLastFullScan = (DateTime.Now - _lastFullScan).TotalSeconds;
                if (cameraUsers.Count == 0 && microphoneUsers.Count == 0 && 
                    timeSinceLastFullScan >= FULL_SCAN_INTERVAL_SECONDS)
                {
                    Debug.WriteLine("[MediaDeviceMonitor] 注册表未检测到，使用备用方法（上次完整扫描：{0}秒前）", 
                        (int)timeSinceLastFullScan);
                    var handleBasedUsers = GetDeviceUsersByHandles();
                    activeUsers.AddRange(handleBasedUsers);
                    _lastFullScan = DateTime.Now;
                }
                else if (cameraUsers.Count == 0 && microphoneUsers.Count == 0 && timeSinceLastFullScan < FULL_SCAN_INTERVAL_SECONDS)
                {
                    // 优化：使用缓存结果，避免频繁的完整扫描
                    return _deviceUsageCache.Values.ToList();
                }
                else
                {
                    // 合并结果（同一进程可能同时使用摄像头和麦克风）
                    var processDict = new Dictionary<int, MediaDeviceUsage>();

                    foreach (var usage in cameraUsers)
                    {
                        if (!processDict.ContainsKey(usage.ProcessId))
                        {
                            processDict[usage.ProcessId] = usage;
                        }
                        else
                        {
                            processDict[usage.ProcessId].IsCameraInUse = true;
                        }
                    }

                    foreach (var usage in microphoneUsers)
                    {
                        if (!processDict.ContainsKey(usage.ProcessId))
                        {
                            processDict[usage.ProcessId] = usage;
                        }
                        else
                        {
                            processDict[usage.ProcessId].IsMicrophoneInUse = true;
                        }
                    }

                    activeUsers.AddRange(processDict.Values);
                }
                
                // 更新缓存
                _deviceUsageCache.Clear();
                foreach (var usage in activeUsers)
                {
                    _deviceUsageCache[usage.ProcessId] = usage;
                }
                
                Debug.WriteLine($"[MediaDeviceMonitor] ========== TOTAL FOUND: {activeUsers.Count} process(es) ==========");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MediaDeviceMonitor] ERROR in GetActiveDeviceUsers: {ex.Message}");
            }

            return activeUsers;
        }
        
        /// <summary>
        /// 通过检查进程句柄来检测摄像头/麦克风使用（备用方法）
        /// 优化：减少扫描范围，提高性能
        /// </summary>
        private List<MediaDeviceUsage> GetDeviceUsersByHandles()
        {
            var users = new List<MediaDeviceUsage>();
            
            try
            {
                // 优化：减少调试输出
                // Debug.WriteLine("[MediaDeviceMonitor] Checking processes for media device handles...");
                
                var allProcesses = System.Diagnostics.Process.GetProcesses();
                
                // 优化：使用 Parallel 提高性能（小心线程安全）
                var processesToCheck = allProcesses.Where(p => 
                {
                    try
                    {
                        // 优化：跳过系统关键进程，减少扫描
                        var name = p.ProcessName.ToLower();
                        return !name.StartsWith("system") && 
                               !name.StartsWith("svchost") && 
                               !name.StartsWith("registry") &&
                               p.Id != 0 && p.Id != 4;
                    }
                    catch
                    {
                        return false;
                    }
                }).ToList();
                
                foreach (var process in processesToCheck)
                {
                    try
                    {
                        // 优化：快速检查，找到一个就停止
                        bool hasCamera = false;
                        bool hasMicrophone = false;
                        
                        // 优化：限制模块检查数量
                        int moduleCount = 0;
                        const int MAX_MODULES_TO_CHECK = 50; // 只检查前50个模块
                        
                        foreach (ProcessModule module in process.Modules)
                        {
                            if (++moduleCount > MAX_MODULES_TO_CHECK && (hasCamera || hasMicrophone))
                                break; // 优化：已经找到了就提前退出
                            
                            string moduleName = module.ModuleName.ToLower();
                            
                            // 优化：使用更精确的匹配，减少字符串比较
                            if (!hasCamera && (moduleName.Contains("mfreadwrite") || 
                                moduleName.Contains("mfplat") ||
                                moduleName.Contains("ksproxy")))
                            {
                                hasCamera = true;
                            }
                            
                            if (!hasMicrophone && (moduleName.Contains("audioses") ||
                                moduleName.Contains("audioeng")))
                            {
                                hasMicrophone = true;
                            }
                            
                            // 优化：找到两个就可以退出了
                            if (hasCamera && hasMicrophone)
                                break;
                        }
                        
                        if (hasCamera || hasMicrophone)
                        {
                            users.Add(new MediaDeviceUsage
                            {
                                ProcessId = process.Id,
                                ProcessName = process.ProcessName,
                                IsCameraInUse = hasCamera,
                                IsMicrophoneInUse = hasMicrophone
                            });
                            
                            Debug.WriteLine($"[MediaDeviceMonitor] 发现: {process.ProcessName} (ID:{process.Id}) 摄像头:{hasCamera} 麦克风:{hasMicrophone}");
                        }
                    }
                    catch
                    {
                        // 无法访问某些进程，忽略
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MediaDeviceMonitor] 句柄检测错误: {ex.Message}");
            }
            
            return users;
        }

        private DateTime _lastRegistryCheck = DateTime.MinValue;
        private const int REGISTRY_CHECK_INTERVAL_MS = 500; // 注册表检查间隔：500ms
        
        /// <summary>
        /// 从注册表获取正在使用指定设备的进程列表
        /// 优化：减少注册表访问频率
        /// </summary>
        private List<MediaDeviceUsage> GetDeviceUsers(string deviceType, bool isCamera)
        {
            var users = new List<MediaDeviceUsage>();

            try
            {
                // 优化：限制注册表访问频率
                var timeSinceLastCheck = (DateTime.Now - _lastRegistryCheck).TotalMilliseconds;
                if (timeSinceLastCheck < REGISTRY_CHECK_INTERVAL_MS && _deviceUsageCache.Count > 0)
                {
                    // 使用缓存结果
                    return _deviceUsageCache.Values.Where(u => 
                        (isCamera && u.IsCameraInUse) || (!isCamera && u.IsMicrophoneInUse)
                    ).ToList();
                }
                _lastRegistryCheck = DateTime.Now;
                
                // 优化：减少调试输出
                // Debug.WriteLine($"[MediaDeviceMonitor] Start checking {deviceType} device...");
                
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    $@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\{deviceType}"))
                {
                    if (key == null)
                    {
                        // 优化：只在首次失败时输出错误
                        if (_deviceUsageCache.Count == 0)
                        {
                            Debug.WriteLine($"[MediaDeviceMonitor] 需要管理员权限访问注册表: {deviceType}");
                        }
                        return users;
                    }
                    
                    var subKeyNames = key.GetSubKeyNames();
                    // 优化：减少调试输出
                    // Debug.WriteLine($"[MediaDeviceMonitor] Found {subKeyNames.Length} subkeys");
                    
                    foreach (var subKeyName in subKeyNames)
                    {
                        // 优化：只检查传统桌面应用（NonPackaged）
                        if (subKeyName.StartsWith("NonPackaged"))
                        {
                            using (var subKey = key.OpenSubKey(subKeyName))
                            {
                                if (subKey != null)
                                {
                                    var lastUsedTimeStop = subKey.GetValue("LastUsedTimeStop");
                                    
                                    // 优化：减少调试输出
                                    // Debug.WriteLine($"[MediaDeviceMonitor] Check subkey: {subKeyName}");
                                    // Debug.WriteLine($"[MediaDeviceMonitor]   LastUsedTimeStop = {lastUsedTimeStop}");
                                    
                                    // LastUsedTimeStop = 0 means in use
                                    if (lastUsedTimeStop != null && lastUsedTimeStop.ToString() == "0")
                                    {
                                        // 优化：提取进程信息
                                        // Format: NonPackaged#C:\Path\To\Process.exe
                                        string processPath = subKeyName.Replace("NonPackaged#", "").Replace("#", "\\");
                                        
                                        try
                                        {
                                            string processName = System.IO.Path.GetFileNameWithoutExtension(processPath);
                                            
                                            // 优化：直接获取进程，减少查询次数
                                            var matchingProcesses = System.Diagnostics.Process.GetProcessesByName(processName);
                                            
                                            if (matchingProcesses.Length > 0)
                                            {
                                                foreach (var proc in matchingProcesses)
                                                {
                                                    try
                                                    {
                                                        users.Add(new MediaDeviceUsage
                                                        {
                                                            ProcessId = proc.Id,
                                                            ProcessName = proc.ProcessName,
                                                            IsCameraInUse = isCamera,
                                                            IsMicrophoneInUse = !isCamera
                                                        });
                                                        
                                                        // 优化：只在发现时输出
                                                        Debug.WriteLine($"[MediaDeviceMonitor] {proc.ProcessName} (ID:{proc.Id}) 使用 {(isCamera ? "摄像头" : "麦克风")}");
                                                    }
                                                    catch
                                                    {
                                                        // 忽略无法访问的进程
                                                    }
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            // 忽略解析错误
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                // 优化：只在有结果时输出
                if (users.Count > 0)
                {
                    Debug.WriteLine($"[MediaDeviceMonitor] 检测完成: {users.Count} 个进程使用 {(isCamera ? "摄像头" : "麦克风")}");
                }
            }
            catch (Exception ex)
            {
                // 优化：减少错误输出频率
                if (_deviceUsageCache.Count == 0)
                {
                    Debug.WriteLine($"[MediaDeviceMonitor] 检测错误: {ex.Message}");
                }
            }

            return users;
        }

        /// <summary>
        /// 检测指定进程是否正在使用摄像头或麦克风（已废弃，改用 GetActiveDeviceUsers）
        /// </summary>
        [Obsolete("请使用 GetActiveDeviceUsers() 方法")]
        public MediaDeviceUsage? CheckProcessMediaUsage(int processId, string processName)
        {
            try
            {
                bool isCameraInUse = IsCameraInUseByProcess(processId);
                bool isMicrophoneInUse = IsMicrophoneInUseByProcess(processId);

                if (isCameraInUse || isMicrophoneInUse)
                {
                    var usage = new MediaDeviceUsage
                    {
                        ProcessId = processId,
                        ProcessName = processName,
                        IsCameraInUse = isCameraInUse,
                        IsMicrophoneInUse = isMicrophoneInUse
                    };

                    _deviceUsageCache[processId] = usage;
                    return usage;
                }

                // 如果没有使用，从缓存中移除
                _deviceUsageCache.Remove(processId);
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"检测进程 {processName} ({processId}) 媒体设备使用情况时出错: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 检测摄像头是否被任意进程使用（通过检测摄像头设备句柄）
        /// </summary>
        private bool IsCameraInUseByProcess(int processId)
        {
            try
            {
                // 方法1: 检查进程是否打开了摄像头相关的句柄
                var process = Process.GetProcessById(processId);
                var processHandles = GetProcessHandleCount(process);
                
                // 方法2: 通过WMI查询摄像头设备状态
                return IsCameraDeviceActive() && HasCameraRelatedModules(process);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检测麦克风是否被指定进程使用
        /// </summary>
        private bool IsMicrophoneInUseByProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                
                // 检查进程是否加载了音频相关的模块
                return IsMicrophoneDeviceActive() && HasAudioRelatedModules(process);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检测摄像头设备是否处于活动状态
        /// </summary>
        private bool IsCameraDeviceActive()
        {
            try
            {
                // 检查摄像头设备是否被占用
                // 通过检测 HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam"))
                {
                    if (key != null)
                    {
                        // 检查最近访问时间
                        var subKeyNames = key.GetSubKeyNames();
                        foreach (var subKeyName in subKeyNames)
                        {
                            if (subKeyName.StartsWith("NonPackaged"))
                            {
                                using (var subKey = key.OpenSubKey(subKeyName))
                                {
                                    if (subKey != null)
                                    {
                                        var lastUsedTime = subKey.GetValue("LastUsedTimeStop");
                                        if (lastUsedTime != null && lastUsedTime.ToString() == "0")
                                        {
                                            // LastUsedTimeStop为0表示正在使用
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"检测摄像头活动状态时出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检测麦克风设备是否处于活动状态
        /// </summary>
        private bool IsMicrophoneDeviceActive()
        {
            try
            {
                // 检查麦克风设备是否被占用
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone"))
                {
                    if (key != null)
                    {
                        var subKeyNames = key.GetSubKeyNames();
                        foreach (var subKeyName in subKeyNames)
                        {
                            if (subKeyName.StartsWith("NonPackaged"))
                            {
                                using (var subKey = key.OpenSubKey(subKeyName))
                                {
                                    if (subKey != null)
                                    {
                                        var lastUsedTime = subKey.GetValue("LastUsedTimeStop");
                                        if (lastUsedTime != null && lastUsedTime.ToString() == "0")
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"检测麦克风活动状态时出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查进程是否加载了摄像头相关的模块
        /// </summary>
        private bool HasCameraRelatedModules(Process process)
        {
            try
            {
                var cameraRelatedDlls = new[]
                {
                    "mfreadwrite.dll",    // Media Foundation
                    "mf.dll",             // Media Foundation
                    "mfplat.dll",         // Media Foundation Platform
                    "ksproxy.ax",         // Kernel Streaming Proxy
                    "vidcap.ax",          // Video Capture
                    "opencv_videoio",     // OpenCV
                    "avicap32.dll"        // AVI Capture
                };

                foreach (ProcessModule module in process.Modules)
                {
                    foreach (var dll in cameraRelatedDlls)
                    {
                        if (module.ModuleName.IndexOf(dll, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查进程是否加载了音频相关的模块
        /// </summary>
        private bool HasAudioRelatedModules(Process process)
        {
            try
            {
                var audioRelatedDlls = new[]
                {
                    "audioses.dll",       // Audio Session
                    "audioeng.dll",       // Audio Engine
                    "mfreadwrite.dll",    // Media Foundation
                    "mf.dll",             // Media Foundation
                    "winmm.dll",          // Windows Multimedia
                    "dsound.dll"          // DirectSound
                };

                foreach (ProcessModule module in process.Modules)
                {
                    foreach (var dll in audioRelatedDlls)
                    {
                        if (module.ModuleName.IndexOf(dll, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取进程的句柄数量（用于辅助判断）
        /// </summary>
        private int GetProcessHandleCount(Process process)
        {
            try
            {
                return process.HandleCount;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 清理已退出进程的缓存
        /// </summary>
        public void CleanupExitedProcess(int processId)
        {
            _deviceUsageCache.Remove(processId);
        }

        /// <summary>
        /// 获取当前所有使用媒体设备的进程
        /// </summary>
        public IReadOnlyDictionary<int, MediaDeviceUsage> GetActiveMediaProcesses()
        {
            return _deviceUsageCache;
        }
    }
}

