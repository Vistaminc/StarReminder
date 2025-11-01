using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MediaDetectionSystem.Models;

namespace MediaDetectionSystem.Services
{
    public class Logger
    {
        private readonly string _logDirectory;
        private readonly string _logFile;
        private readonly int _cleanupDays;
        private bool _enableDetailedLogging;

        public Logger(string logDirectory = "logs", int cleanupDays = 30, bool enableDetailedLogging = true)
        {
            _logDirectory = logDirectory;
            _cleanupDays = cleanupDays;
            _enableDetailedLogging = enableDetailedLogging;
            
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            _logFile = Path.Combine(_logDirectory, "activity_log.json");
            CleanupOldLogs();
        }

        /// <summary>
        /// 更新详细日志设置
        /// </summary>
        public void SetDetailedLogging(bool enabled)
        {
            _enableDetailedLogging = enabled;
        }

        public void Log(string action, string processName, string details = "")
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Action = action,
                ProcessName = processName,
                Details = details
            };

            try
            {
                var logs = LoadLogs();
                logs.Add(entry);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(logs, options);
                File.WriteAllText(_logFile, json);

                // 如果启用了详细日志，同时输出到调试窗口
                if (_enableDetailedLogging)
                {
                    System.Diagnostics.Debug.WriteLine($"[Logger] [{action}] {processName}: {details}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"记录日志失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 记录详细日志（只有在启用详细日志时才记录）
        /// </summary>
        public void LogDetailed(string action, string processName, string details = "")
        {
            if (_enableDetailedLogging)
            {
                Log(action, processName, details);
            }
        }

        public List<LogEntry> LoadLogs()
        {
            try
            {
                if (File.Exists(_logFile))
                {
                    var json = File.ReadAllText(_logFile);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    return JsonSerializer.Deserialize<List<LogEntry>>(json, options) ?? new List<LogEntry>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载日志失败: {ex.Message}");
            }

            return new List<LogEntry>();
        }

        private void CleanupOldLogs()
        {
            try
            {
                var logs = LoadLogs();
                var cutoffDate = DateTime.Now.AddDays(-_cleanupDays);
                var filteredLogs = logs.Where(l => l.Timestamp > cutoffDate).ToList();

                if (filteredLogs.Count != logs.Count)
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var json = JsonSerializer.Serialize(filteredLogs, options);
                    File.WriteAllText(_logFile, json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理旧日志失败: {ex.Message}");
            }
        }

        public void ClearLogs()
        {
            try
            {
                if (File.Exists(_logFile))
                {
                    File.Delete(_logFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清除日志失败: {ex.Message}");
            }
        }
    }
}
