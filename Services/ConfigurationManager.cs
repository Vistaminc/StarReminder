using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MediaDetectionSystem.Models;

namespace MediaDetectionSystem.Services
{
    public class ConfigurationManager
    {
        private readonly string _configPath;
        private readonly string _settingsPath;
        private Dictionary<string, ProcessConfig> _processConfigs;
        private AlertConfig _alertConfig;
        private AppSettings _appSettings;

        public Dictionary<string, ProcessConfig> ProcessConfigs => _processConfigs;
        public AlertConfig AlertConfig => _alertConfig;
        public AppSettings AppSettings => _appSettings;

        public ConfigurationManager(string configPath = "config.json", string settingsPath = "settings.json")
        {
            _configPath = configPath;
            _settingsPath = settingsPath;
            _processConfigs = new Dictionary<string, ProcessConfig>();
            _alertConfig = new AlertConfig();
            _appSettings = new AppSettings();
            LoadConfiguration();
        }

        public void LoadConfiguration()
        {
            try
            {
                // 加载进程配置和提醒配置 (config.json)
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    };
                    var config = JsonSerializer.Deserialize<ConfigData>(json, options);
                    
                    if (config != null)
                    {
                        _processConfigs = config.ProcessConfigs ?? new Dictionary<string, ProcessConfig>();
                        _alertConfig = config.AlertConfig ?? new AlertConfig();
                    }
                }
                else
                {
                    // 创建默认配置
                    InitializeDefaultConfiguration();
                    SaveConfiguration();
                }
                
                // 加载应用设置 (settings.json)
                LoadAppSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载配置失败: {ex.Message}");
                InitializeDefaultConfiguration();
            }
        }
        
        private void LoadAppSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    };
                    var settings = JsonSerializer.Deserialize<AppSettings>(json, options);
                    _appSettings = settings ?? new AppSettings();
                }
                else
                {
                    // 如果settings.json不存在，创建默认设置
                    _appSettings = new AppSettings();
                    SaveAppSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载应用设置失败: {ex.Message}");
                _appSettings = new AppSettings();
            }
        }

        public void SaveConfiguration()
        {
            try
            {
                var config = new ConfigData
                {
                    ProcessConfigs = _processConfigs,
                    AlertConfig = _alertConfig
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(_configPath, json);
                
                // 同时保存应用设置到单独的文件
                SaveAppSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置失败: {ex.Message}");
                throw;
            }
        }
        
        public void SaveAppSettings()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(_appSettings, options);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存应用设置失败: {ex.Message}");
                throw;
            }
        }

        private void InitializeDefaultConfiguration()
        {
            _processConfigs = new Dictionary<string, ProcessConfig>
            {
                ["media_capture.exe"] = new ProcessConfig
                {
                    ProcessName = "media_capture.exe",
                    DisplayName = "摄像头捕获",
                    Description = "用于捕获摄像头和麦克风内容的应用",
                    IsEnabled = true,
                    MaxRuntime = TimeSpan.FromMinutes(30),
                    ActionType = "Suspend",
                    AutoResumeAfter = TimeSpan.FromMinutes(5)
                },
                ["screenCapture.exe"] = new ProcessConfig
                {
                    ProcessName = "screenCapture.exe",
                    DisplayName = "屏幕捕获",
                    Description = "用于捕获屏幕当前显示的内容",
                    IsEnabled = true,
                    MaxRuntime = TimeSpan.FromMinutes(30),
                    ActionType = "Suspend",
                    AutoResumeAfter = TimeSpan.FromMinutes(5)
                },
                ["rtcRemoteDesktop.exe"] = new ProcessConfig
                {
                    ProcessName = "rtcRemoteDesktop.exe",
                    DisplayName = "远程桌面",
                    Description = "用于对该电脑进行远程控制",
                    IsEnabled = true,
                    MaxRuntime = TimeSpan.FromMinutes(30),
                    ActionType = "Terminate",
                    AutoResumeAfter = TimeSpan.Zero
                }
            };

            _alertConfig = new AlertConfig
            {
                NotificationType = "Defender",
                EnableStartupNotification = true,
                NotificationTitle = "StarReminder - 进程监控",
                NotificationMessage = "检测到受监控进程\n{DisplayName}",
                EnableContinuousAlert = true,
                WatermarkText1 = "激活 Windows",
                WatermarkText2 = "转到\"设置\"以激活 Windows。",
                WatermarkPosition = "BottomRight",
                ContinuousAlertUntil = "ProcessEnd",
                LogCleanupDays = 30
            };

            _appSettings = new AppSettings
            {
                StartWithWindows = false,
                StartMinimized = false
            };
        }

        private class ConfigData
        {
            public Dictionary<string, ProcessConfig>? ProcessConfigs { get; set; }
            public AlertConfig? AlertConfig { get; set; }
        }
    }
}
