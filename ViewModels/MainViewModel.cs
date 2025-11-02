using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MediaDetectionSystem.Models;
using MediaDetectionSystem.Services;
using MediaDetectionSystem.Views;

namespace MediaDetectionSystem.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ConfigurationManager _configManager;
        private readonly ProcessMonitor _processMonitor;
        private readonly ProcessController _processController;
        private readonly Logger _logger;
        private readonly System.Threading.Timer _timer;  // 后台检测Timer
        private readonly DispatcherTimer _uiTimer;  // UI更新Timer（独立，轻量级）
        private readonly Dispatcher _dispatcher;
        private readonly Dictionary<int, DispatcherTimer> _autoResumeTimers = new();
        private readonly Dictionary<int, DateTime> _warnedProcesses = new(); // 跟踪已警告的进程
        private readonly Dictionary<int, AlertManager> _processAlertManagers = new(); // 每个进程的提醒管理器
        
        // 日志图标映射（静态缓存，避免重复创建）
        private static readonly Dictionary<string, string> _logIconMap = new()
        {
            ["started"] = "🟢",
            ["terminated"] = "🔴",
            ["suspended"] = "🟡",
            ["resumed"] = "🔵",
            ["user_action"] = "👤",
            ["config_change"] = "⚙️",
            ["exceeded_limit"] = "⏰"
        };
        
        private bool _isChecking = false;  // 防止重入

        private bool _systemEnabled;
        private string _statusMessage = "系统就绪";

        public bool SystemEnabled
        {
            get => _systemEnabled;
            set
            {
                _systemEnabled = value;
                OnPropertyChanged();
                
                if (value)
                {
                    StatusMessage = "✓ 系统监控已启用";
                }
                else
                {
                    StatusMessage = "⊗ 系统监控已禁用";
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ProcessStatusItem> ProcessStatuses { get; }
        public ObservableCollection<string> LogEntries { get; }
        
        // 公开配置管理器供设置窗口使用
        public ConfigurationManager ConfigManager => _configManager;

        public MainViewModel()
        {
            _dispatcher = System.Windows.Application.Current.Dispatcher;
            _configManager = new ConfigurationManager();
            _logger = new Logger("logs", _configManager.AppSettings.LogRetentionDays, _configManager.AppSettings.EnableDetailedLogging);
            _processController = new ProcessController();
            _processMonitor = new ProcessMonitor();
            
            _processMonitor.ProcessDetected += OnProcessDetected;
            _processMonitor.ProcessExceededLimit += OnProcessExceededLimit;

            ProcessStatuses = new ObservableCollection<ProcessStatusItem>
            {
                new ProcessStatusItem { ProcessName = "media_capture.exe", DisplayName = "摄像头/麦克风" },
                new ProcessStatusItem { ProcessName = "screenCapture.exe", DisplayName = "屏幕捕获" },
                new ProcessStatusItem { ProcessName = "rtcRemoteDesktop.exe", DisplayName = "远程控制" }
            };

            LogEntries = new ObservableCollection<string>();

            _systemEnabled = true;

            // 不加载历史日志，每次启动都从空白开始
            // LoadRecentLogs();

            // 方案：分离检测和UI更新
            // 1. 后台Timer：按用户配置的间隔检测进程（CPU密集型操作）
            var interval = _configManager.AppSettings.MonitorInterval * 1000;
            _timer = new System.Threading.Timer(
                OnTimerTickAsync,
                null,
                1000,  // 1秒后开始
                interval  // 间隔
            );

            // 2. UI Timer：每2秒更新运行时间显示（降低UI刷新频率，提升流畅度）
            _uiTimer = new DispatcherTimer(DispatcherPriority.Background, _dispatcher)
            {
                Interval = TimeSpan.FromSeconds(2)  // 2秒更新一次，减少UI开销
            };
            _uiTimer.Tick += OnUiTimerTick;
            _uiTimer.Start();

            StatusMessage = "✓ 系统监控已启用";
        }

        /// <summary>
        /// 更新监控间隔
        /// </summary>
        public void UpdateMonitorInterval(int intervalSeconds)
        {
            if (_timer != null)
            {
                // 重新创建Timer以更新间隔
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Change(0, intervalSeconds * 1000);
            }
        }

        /// <summary>
        /// 更新日志设置
        /// </summary>
        public void UpdateLoggingSettings(bool enableDetailedLogging)
        {
            _logger?.SetDetailedLogging(enableDetailedLogging);
        }

        private void LoadRecentLogs()
        {
            var logs = _logger.LoadLogs().OrderByDescending(l => l.Timestamp).Take(50);
            foreach (var log in logs.Reverse())
            {
                AddLogEntry(log);
            }
        }

        private void OnProcessDetected(object? sender, ProcessEventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                var processInfo = e.ProcessInfo;
                
                // 处理进程启动
                if (e.Action == "Detected")
                {
                    // 获取对应的进程配置
                    if (_configManager.ProcessConfigs.TryGetValue(processInfo.Name + ".exe", out var config))
                    {
                        // 使用进程独立的AlertConfig，如果没有则使用全局配置
                        var alertConfig = config.AlertConfig ?? _configManager.AlertConfig;
                        
                        // 创建专用的AlertManager实例
                        var processAlertManager = new AlertManager(alertConfig);
                        _processAlertManagers[processInfo.Id] = processAlertManager;
                        
                        // 显示启动提醒（传入媒体设备使用情况）
                        processAlertManager.ShowProcessStartAlert(processInfo.Name, config.DisplayName, e.MediaUsage);
                        
                        // 如果启用了持续提醒，则显示
                        if (_configManager.AlertConfig.EnableContinuousAlert)
                        {
                            processAlertManager.ShowContinuousAlert();
                        }
                    }
                    
                    _logger.Log("started", processInfo.Name, $"进程ID: {processInfo.Id}，已自动允许运行");
                    AddLogEntry(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        EventType = "started",
                        ProcessName = processInfo.Name,
                        Pid = processInfo.Id,
                        Details = $"进程ID: {processInfo.Id}，已自动允许运行",
                        Action = "allowed"
                    });
                    
                    // 立即更新UI显示，不用等待UITimer
                    UpdateProcessStatusUI();
                }
                // 处理进程退出
                else if (e.Action == "Exited")
                {
                    // 清理并隐藏该进程的持续提醒和定时器
                    CleanupProcessResources(processInfo.Id);
                    
                    _logger.Log("exited", processInfo.Name, $"进程ID: {processInfo.Id}，已退出");
                    AddLogEntry(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        EventType = "exited",
                        ProcessName = processInfo.Name,
                        Pid = processInfo.Id,
                        Details = $"进程已退出",
                        Action = "exited"
                    });
                    
                    // 立即更新UI显示，不用等待UITimer
                    UpdateProcessStatusUI();
                }
            });
        }

        private void OnProcessExceededLimit(object? sender, ProcessEventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                var processInfo = e.ProcessInfo;
                var config = processInfo.Config;
                if (config == null) return;
                
                _logger.Log("exceeded_limit", processInfo.Name, $"超出时限 PID:{processInfo.Id}, 最大运行时间:{config.MaxRuntime.TotalMinutes}分钟");
                AddLogEntry(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    EventType = "exceeded_limit",
                    ProcessName = processInfo.Name,
                    Pid = processInfo.Id,
                    Details = $"超出运行时限({config.MaxRuntime.TotalMinutes}分钟)",
                    Action = "detected"
                });

                // 执行配置的操作
                if (_configManager.AppSettings.EnableAutoAction)
                {
                    switch (config.ActionType)
                    {
                        case "Suspend":
                            if (_processController.SuspendProcess(processInfo.Id))
                            {
                                _logger.Log("suspended", processInfo.Name, $"进程已挂起 PID:{processInfo.Id}");
                                AddLogEntry(new LogEntry
                                {
                                    Timestamp = DateTime.Now,
                                    EventType = "suspended",
                                    ProcessName = processInfo.Name,
                                    Pid = processInfo.Id,
                                    Details = $"进程已自动挂起，{(config.AutoResumeAfter.TotalSeconds > 0 ? $"将在{config.AutoResumeAfter.TotalSeconds}秒后自动恢复" : "需手动恢复")}",
                                    Action = "auto_suspend"
                                });
                                
                                // 根据配置决定是否隐藏持续提醒
                                if (_configManager.AlertConfig.ContinuousAlertUntil == "ProcessSuspend")
                                {
                                    if (_processAlertManagers.TryGetValue(processInfo.Id, out var alertManager))
                                    {
                                        alertManager.HideContinuousAlert();
                                    }
                                }
                                
                                // 如果配置了自动恢复，安排恢复任务
                                if (config.AutoResumeAfter.TotalSeconds > 0)
                                {
                                    ScheduleAutoResume(processInfo.Id, config.AutoResumeAfter);
                                }
                            }
                            break;
                            
                        case "Terminate":
                            if (_processController.TerminateProcess(processInfo.Id))
                            {
                                _logger.Log("terminated", processInfo.Name, $"进程已终止 PID:{processInfo.Id}");
                                AddLogEntry(new LogEntry
                                {
                                    Timestamp = DateTime.Now,
                                    EventType = "terminated",
                                    ProcessName = processInfo.Name,
                                    Pid = processInfo.Id,
                                    Details = "进程已自动终止",
                                    Action = "auto_terminate"
                                });
                                
                                // 进程终止后清理资源
                                CleanupProcessResources(processInfo.Id);
                            }
                            break;
                    }
                }
            });
        }

        /// <summary>
        /// 异步定时器回调 - 在后台线程执行，不阻塞UI
        /// </summary>
        private async void OnTimerTickAsync(object? state)
        {
            // 优化：防止重入
            if (_isChecking) return;
            
            try
            {
                _isChecking = true;
                
                // 优化：在后台线程执行检测，不阻塞UI
                await Task.Run(() =>
                {
                    try
                    {
                        if (!_systemEnabled) return;
                        
                        // 检查进程（在后台线程执行）
                        _processMonitor.CheckProcesses(_configManager.ProcessConfigs);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[MainViewModel] 检测错误: {ex.Message}");
                    }
                });
                
                // UI更新已移到独立的UITimer中，这里不再更新UI
                // 后台检测专注于进程检测和事件触发
            }
            finally
            {
                _isChecking = false;
            }
        }

        /// <summary>
        /// UI定时器回调 - 每秒更新运行时间显示（轻量级，仅时间计算）
        /// </summary>
        private void OnUiTimerTick(object? sender, EventArgs e)
        {
            if (!_systemEnabled) return;
            
            try
            {
                UpdateProcessStatusUI();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainViewModel] UI更新错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 更新进程状态UI（轻量级，仅计算运行时间）
        /// </summary>
        private void UpdateProcessStatusUI()
        {
            // 优化：先构建字典，避免重复查找
            var monitoredDict = _processMonitor.MonitoredProcesses.Values
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
            
            // 轻量级操作：只遍历UI列表
            foreach (var status in ProcessStatuses)
            {
                var processName = System.IO.Path.GetFileNameWithoutExtension(status.ProcessName);
                
                // 快速查找对应的进程信息
                if (monitoredDict.TryGetValue(processName, out var runningProcess))
                {
                    // 仅做简单的时间计算，非常轻量
                    status.IsRunning = true;
                    status.RunTime = (int)runningProcess.Runtime.TotalSeconds;
                }
                else
                {
                    status.IsRunning = false;
                    status.RunTime = 0;
                }
            }
        }

        private void AddLogEntry(LogEntry log)
        {
            // 优化：使用静态缓存的图标映射
            var icon = _logIconMap.TryGetValue(log.EventType, out var iconValue) ? iconValue : "ℹ️";

            var logText = $"[{log.Timestamp:HH:mm:ss}] {icon} [{log.EventType}] {log.ProcessName}";
            if (!string.IsNullOrEmpty(log.Details))
            {
                logText += $" - {log.Details}";
            }

            LogEntries.Add(logText);

            // 优化：批量删除，减少UI刷新次数
            if (LogEntries.Count > 100)
            {
                var removeCount = LogEntries.Count - 100;
                for (int i = 0; i < removeCount; i++)
                {
                    LogEntries.RemoveAt(0);
                }
            }
        }

        public void OpenConfigWindow(string processName)
        {
            // 从ConfigManager的ProcessConfigs字典中获取配置
            if (!_configManager.ProcessConfigs.TryGetValue(processName, out var config))
            {
                // 如果不存在，创建默认配置
                config = new ProcessConfig
                {
                    ProcessName = processName,
                    DisplayName = processName,
                    IsEnabled = true,
                    MaxRuntime = TimeSpan.FromMinutes(30),
                    ActionType = "None"
                };
                _configManager.ProcessConfigs[processName] = config;
            }

            var alertConfig = _configManager.AlertConfig;
            
            var configWindow = new ConfigWindow(
                processName, 
                config, 
                OnConfigSaved,
                alertConfig,
                OnAlertConfigSaved
            );
            
            configWindow.Owner = System.Windows.Application.Current.MainWindow;
            configWindow.ShowDialog();
        }

        private void OnConfigSaved(string processName, ProcessConfig config)
        {
            // 更新配置字典
            _configManager.ProcessConfigs[processName] = config;
            _configManager.SaveConfiguration();
            
            _logger.Log("config_change", processName, 
                $"配置已更新: 启用={config.IsEnabled}, 时限={config.MaxRuntime.TotalMinutes}分钟");
            
            AddLogEntry(new LogEntry
            {
                Timestamp = DateTime.Now,
                EventType = "config_change",
                ProcessName = processName,
                Details = $"启用={config.IsEnabled}, 时限={config.MaxRuntime.TotalMinutes}分钟",
                Action = "update"
            });
            
            StatusMessage = $"✓ {processName} 配置已保存";
        }

        private void OnAlertConfigSaved(AlertConfig alertConfig)
        {
            // 直接更新AlertConfig属性（它是引用类型，已经在ConfigWindow中修改了）
            _configManager.SaveConfiguration();
            
            _logger.Log("config_change", "AlertConfig", "提醒配置已更新");
            
            AddLogEntry(new LogEntry
            {
                Timestamp = DateTime.Now,
                EventType = "config_change",
                ProcessName = "AlertConfig",
                Details = "提醒配置已更新",
                Action = "update"
            });
            
            StatusMessage = "✓ 提醒配置已保存";
        }

        /// <summary>
        /// 安排自动恢复任务
        /// </summary>
        private void ScheduleAutoResume(int processId, TimeSpan delay)
        {
            // 如果已有定时器，先停止
            if (_autoResumeTimers.ContainsKey(processId))
            {
                _autoResumeTimers[processId].Stop();
                _autoResumeTimers.Remove(processId);
            }

            var timer = new DispatcherTimer
            {
                Interval = delay
            };

            timer.Tick += (s, e) =>
            {
                timer.Stop();
                _autoResumeTimers.Remove(processId);

                try
                {
                    var processInfo = _processMonitor.GetProcessInfo(processId);
                    if (processInfo != null && _processController.ResumeProcess(processId))
                    {
                        _logger.Log("resumed", processInfo.Name, $"进程已自动恢复 PID:{processId}");
                        AddLogEntry(new LogEntry
                        {
                            Timestamp = DateTime.Now,
                            EventType = "resumed",
                            ProcessName = processInfo.Name,
                            Pid = processId,
                            Details = "进程已自动恢复运行",
                            Action = "auto_resume"
                        });
                        
                        // 恢复后重新显示持续提醒
                        if (_configManager.AlertConfig.EnableContinuousAlert && 
                            _configManager.AlertConfig.ContinuousAlertUntil == "ProcessSuspend")
                        {
                            if (_processAlertManagers.TryGetValue(processId, out var alertManager))
                            {
                                alertManager.ShowContinuousAlert();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"自动恢复进程失败: {ex.Message}");
                }
            };

            _autoResumeTimers[processId] = timer;
            timer.Start();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 清理进程相关的所有资源（AlertManager和Timer）
        /// </summary>
        private void CleanupProcessResources(int processId)
        {
            // 清理AlertManager
            if (_processAlertManagers.TryGetValue(processId, out var alertManager))
            {
                try
                {
                    alertManager.HideContinuousAlert();
                    alertManager.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"清理AlertManager失败: {ex.Message}");
                }
                finally
                {
                    _processAlertManagers.Remove(processId);
                }
            }

            // 清理自动恢复定时器
            if (_autoResumeTimers.TryGetValue(processId, out var timer))
            {
                try
                {
                    timer.Stop();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"停止定时器失败: {ex.Message}");
                }
                finally
                {
                    _autoResumeTimers.Remove(processId);
                }
            }
        }

        /// <summary>
        /// 清理所有资源（应用关闭时调用）
        /// </summary>
        public void Cleanup()
        {
            try
            {
                // 停止定时器
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                _timer?.Dispose();
                _uiTimer?.Stop();

                // 清理所有进程相关的AlertManager
                foreach (var kvp in _processAlertManagers.ToList())
                {
                    CleanupProcessResources(kvp.Key);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清理资源失败: {ex.Message}");
            }
        }
    }

    public class ProcessStatusItem : INotifyPropertyChanged
    {
        private bool _isRunning;
        private int _runTime;

        public string ProcessName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public int RunTime
        {
            get => _runTime;
            set
            {
                _runTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RunTimeText));
            }
        }

        public string StatusText => IsRunning ? "运行中" : "未运行";
        public string StatusColor => IsRunning ? "#4CAF50" : "#999999";

        public string RunTimeText
        {
            get
            {
                if (!IsRunning) return "运行时间: --";

                var hours = RunTime / 3600;
                var minutes = (RunTime % 3600) / 60;
                var seconds = RunTime % 60;

                if (hours > 0)
                    return $"运行时间: {hours}h {minutes}m {seconds}s";
                else if (minutes > 0)
                    return $"运行时间: {minutes}m {seconds}s";
                else
                    return $"运行时间: {seconds}s";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
