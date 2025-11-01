using System;

namespace MediaDetectionSystem.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // 别名，兼容性
        public string ProcessName { get; set; } = string.Empty;
        public int? Pid { get; set; }
        public string Details { get; set; } = string.Empty;
        public string UserAction { get; set; } = string.Empty;

        public LogEntry()
        {
            Timestamp = DateTime.Now;
            // EventType 和 Action 同步
            if (string.IsNullOrEmpty(Action) && !string.IsNullOrEmpty(EventType))
            {
                Action = EventType;
            }
        }
    }
}
