using System;

namespace MediaDetectionSystem.Models
{
    /// <summary>
    /// 应用程序设置
    /// </summary>
    public class AppSettings
    {
        // ==================== 启动设置 ====================
        
        /// <summary>
        /// 开机自启动
        /// </summary>
        public bool StartWithWindows { get; set; } = false;
        
        /// <summary>
        /// 启动时最小化到托盘
        /// </summary>
        public bool StartMinimized { get; set; } = false;
        
        // ==================== 界面设置 ====================
        
        /// <summary>
        /// 主题模式 (Light, Dark, Auto)
        /// </summary>
        public string ThemeMode { get; set; } = "Light";
        
        // ==================== 通知设置 ====================
        
        /// <summary>
        /// 启用系统托盘通知
        /// </summary>
        public bool EnableTrayNotifications { get; set; } = true;
        
    /// <summary>
    /// 进程启动时弹窗提醒
    /// </summary>
    public bool ShowStartupDialog { get; set; } = true;
    
    /// <summary>
    /// 显示超时警告弹窗（MessageBox）
    /// </summary>
    public bool ShowTimeoutDialog { get; set; } = false;
        
        // ==================== 监控设置 ====================
        
        /// <summary>
        /// 监控检测间隔（秒）
        /// </summary>
        public int MonitorInterval { get; set; } = 1;
        
        /// <summary>
        /// 启用自动操作（挂起/终止）
        /// </summary>
        public bool EnableAutoAction { get; set; } = true;
        
        /// <summary>
        /// 记录详细日志
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = true;
        
        // ==================== 高级设置 ====================
        
        /// <summary>
        /// 日志保留天数
        /// </summary>
        public int LogRetentionDays { get; set; } = 30;
        
        /// <summary>
        /// 自动清理日志
        /// </summary>
        public bool AutoCleanLogs { get; set; } = true;
        
        /// <summary>
        /// 检查更新
        /// </summary>
        public bool CheckForUpdates { get; set; } = true;
        
        /// <summary>
        /// 数据统计
        /// </summary>
        public bool EnableAnalytics { get; set; } = false;
        
        // ==================== 安全设置 ====================
        
        /// <summary>
        /// 启用操作密码保护
        /// </summary>
        public bool EnablePasswordProtection { get; set; } = false;
        
        /// <summary>
        /// 操作密码（加密存储）
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;
    }
}

