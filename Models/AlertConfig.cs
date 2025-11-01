using System;

namespace MediaDetectionSystem.Models
{
    public class AlertConfig
    {
        // === 启动时提醒配置 ===
        /// <summary>
        /// 通知样式类型：Defender（Defender样式）, Native（Windows原生Toast）
        /// </summary>
        public string NotificationType { get; set; } = "Defender";
        
        /// <summary>
        /// 是否启用启动时通知
        /// </summary>
        public bool EnableStartupNotification { get; set; } = true;
        
        /// <summary>
        /// 通知标题（支持变量：{ProcessName}, {DisplayName}）
        /// </summary>
        public string NotificationTitle { get; set; } = "StarReminder - 进程监控";
        
        /// <summary>
        /// 通知消息（支持变量：{ProcessName}, {DisplayName}, {Camera}, {Microphone}）
        /// </summary>
        public string NotificationMessage { get; set; } = "检测到受监控进程\n{DisplayName}";
        
        // === 持续提醒配置 ===
        /// <summary>
        /// 是否启用持续提醒（Windows激活水印样式）
        /// </summary>
        public bool EnableContinuousAlert { get; set; } = true;
        
        /// <summary>
        /// 水印显示的文字（第一行）
        /// </summary>
        public string WatermarkText1 { get; set; } = "激活 Windows";
        
        /// <summary>
        /// 水印显示的文字（第二行）
        /// </summary>
        public string WatermarkText2 { get; set; } = "转到\"设置\"以激活 Windows。";
        
        /// <summary>
        /// 水印位置：BottomRight（右下角）, BottomLeft（左下角）, TopRight（右上角）, TopLeft（左上角）
        /// </summary>
        public string WatermarkPosition { get; set; } = "BottomRight";
        
        /// <summary>
        /// 持续提醒直到："ProcessEnd"（进程结束）, "ProcessSuspend"（进程挂起）
        /// </summary>
        public string ContinuousAlertUntil { get; set; } = "ProcessEnd";
        
        /// <summary>
        /// 日志清理天数
        /// </summary>
        public int LogCleanupDays { get; set; } = 30;
    }
}
