using System;

namespace MediaDetectionSystem.Models
{
    public class ProcessInfo
    {
        public int Id { get; set; }
        public int Pid { get; set; } // 别名，兼容性
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public TimeSpan Runtime { get; set; }
        public string Status { get; set; } = "Running";
        public bool Authorized { get; set; } = false;
        public ProcessConfig? Config { get; set; }

        public ProcessInfo()
        {
            Id = 0;
            Pid = 0;
        }
    }
}
