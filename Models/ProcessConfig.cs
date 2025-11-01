using System;

namespace MediaDetectionSystem.Models
{
    public class ProcessConfig
    {
        public string ProcessName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public TimeSpan MaxRuntime { get; set; } = TimeSpan.FromMinutes(30);
        public string ActionType { get; set; } = "Suspend"; // Suspend, Terminate, None
        public TimeSpan AutoResumeAfter { get; set; } = TimeSpan.FromMinutes(5);
        public int AlertBeforeAction { get; set; } = 30;
        
        /// <summary>
        /// 该进程的提醒配置（如果为null则使用全局配置）
        /// </summary>
        public AlertConfig? AlertConfig { get; set; }
    }
}
