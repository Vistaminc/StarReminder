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
                // 优化：使用追加写入模式，避免每次加载整个文件
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(entry, options);
                
                // 追加写入，每条日志一行（JSONL格式）
                File.AppendAllText(_logFile, json + Environment.NewLine);

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
            var logs = new List<LogEntry>();
            
            try
            {
                if (File.Exists(_logFile))
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    // 逐行读取JSONL格式
                    foreach (var line in File.ReadLines(_logFile))
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            try
                            {
                                var entry = JsonSerializer.Deserialize<LogEntry>(line, options);
                                if (entry != null)
                                {
                                    logs.Add(entry);
                                }
                            }
                            catch
                            {
                                // 跳过损坏的行
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载日志失败: {ex.Message}");
            }

            return logs;
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
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    // 重写文件，使用JSONL格式
                    File.WriteAllText(_logFile, string.Empty);
                    foreach (var log in filteredLogs)
                    {
                        var json = JsonSerializer.Serialize(log, options);
                        File.AppendAllText(_logFile, json + Environment.NewLine);
                    }
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
