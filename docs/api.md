# 📘 API 文档

<div align="center">
  <a href="../README.md">返回首页</a> · <a href="architecture.md">技术架构</a> · <a href="development.md">开发指南</a>
</div>

---

## 📦 核心服务 API

### ProcessMonitor

进程监控服务，负责检测目标进程的启动和运行状态。

#### 构造函数

```csharp
public ProcessMonitor()
```

#### 事件

##### ProcessDetected

当检测到新进程或进程退出时触发。

```csharp
public event EventHandler<ProcessEventArgs>? ProcessDetected;
```

**参数**:
```csharp
public class ProcessEventArgs : EventArgs
{
    public ProcessInfo ProcessInfo { get; set; }     // 进程信息
    public string Action { get; set; }               // 操作: "Detected" | "Exited"
    public MediaDeviceUsage? MediaUsage { get; set; }// 媒体设备使用情况
}
```

**示例**:
```csharp
var monitor = new ProcessMonitor();
monitor.ProcessDetected += (sender, e) =>
{
    if (e.Action == "Detected")
    {
        Console.WriteLine($"检测到进程: {e.ProcessInfo.DisplayName}");
        if (e.MediaUsage != null)
        {
            Console.WriteLine($"  摄像头: {e.MediaUsage.IsCameraInUse}");
            Console.WriteLine($"  麦克风: {e.MediaUsage.IsMicrophoneInUse}");
        }
    }
};
```

##### ProcessExceededLimit

当进程运行时间超过限制时触发。

```csharp
public event EventHandler<ProcessEventArgs>? ProcessExceededLimit;
```

**示例**:
```csharp
monitor.ProcessExceededLimit += (sender, e) =>
{
    Console.WriteLine($"进程超时: {e.ProcessInfo.DisplayName}");
    Console.WriteLine($"操作类型: {e.Action}"); // "suspend" | "kill" | "none"
};
```

##### ProcessNearLimit

当进程接近运行时间限制时触发（提前警告）。

```csharp
public event EventHandler<ProcessEventArgs>? ProcessNearLimit;
```

#### 方法

##### CheckProcesses

检查所有配置的进程。

```csharp
public void CheckProcesses(Dictionary<string, ProcessConfig> processConfigs)
```

**参数**:
- `processConfigs`: 进程配置字典，键为进程名称

**示例**:
```csharp
var configs = new Dictionary<string, ProcessConfig>
{
    ["media_capture.exe"] = new ProcessConfig
    {
        ProcessName = "media_capture.exe",
        DisplayName = "媒体捕获",
        IsEnabled = true
    }
};

monitor.CheckProcesses(configs);
```

##### GetProcessInfo

获取指定进程的信息。

```csharp
public ProcessInfo? GetProcessInfo(int processId)
```

**参数**:
- `processId`: 进程 ID

**返回值**: 进程信息，如果进程不存在则返回 `null`

**示例**:
```csharp
var info = monitor.GetProcessInfo(12345);
if (info != null)
{
    Console.WriteLine($"进程名: {info.Name}");
    Console.WriteLine($"运行时间: {info.Runtime}");
}
```

##### GetActiveMediaProcesses

获取当前所有使用媒体设备的进程。

```csharp
public IReadOnlyDictionary<int, MediaDeviceUsage> GetActiveMediaProcesses()
```

**返回值**: 字典，键为进程 ID，值为媒体使用情况

**示例**:
```csharp
var mediaProcesses = monitor.GetActiveMediaProcesses();
foreach (var (pid, usage) in mediaProcesses)
{
    Console.WriteLine($"进程 {usage.ProcessName} (ID:{pid})");
    Console.WriteLine($"  摄像头: {usage.IsCameraInUse}");
    Console.WriteLine($"  麦克风: {usage.IsMicrophoneInUse}");
}
```

---

### MediaDeviceMonitor

媒体设备监控服务，检测摄像头和麦克风的实际使用情况。

#### 构造函数

```csharp
public MediaDeviceMonitor()
```

#### 方法

##### GetActiveDeviceUsers

获取所有正在使用摄像头或麦克风的进程列表。

```csharp
public List<MediaDeviceUsage> GetActiveDeviceUsers()
```

**返回值**: 媒体设备使用情况列表

**示例**:
```csharp
var monitor = new MediaDeviceMonitor();
var users = monitor.GetActiveDeviceUsers();

foreach (var user in users)
{
    Console.WriteLine($"进程: {user.ProcessName} (ID:{user.ProcessId})");
    Console.WriteLine($"  摄像头: {user.IsCameraInUse}");
    Console.WriteLine($"  麦克风: {user.IsMicrophoneInUse}");
    Console.WriteLine($"  检测时间: {user.DetectionTime}");
}
```

##### CleanupExitedProcess

清理已退出进程的缓存。

```csharp
public void CleanupExitedProcess(int processId)
```

**参数**:
- `processId`: 进程 ID

**示例**:
```csharp
monitor.CleanupExitedProcess(12345);
```

##### GetActiveMediaProcesses

获取当前所有使用媒体设备的进程（从缓存）。

```csharp
public IReadOnlyDictionary<int, MediaDeviceUsage> GetActiveMediaProcesses()
```

---

### AlertManager

通知管理服务，负责显示各种通知。

#### 构造函数

```csharp
public AlertManager()
```

#### 方法

##### ShowProcessAlert

显示进程检测通知。

```csharp
public void ShowProcessAlert(ProcessInfo processInfo, ProcessConfig config)
```

**参数**:
- `processInfo`: 进程信息
- `config`: 进程配置（包含通知设置）

**示例**:
```csharp
var manager = new AlertManager();
var info = new ProcessInfo
{
    DisplayName = "媒体捕获",
    StartTime = DateTime.Now
};

var config = new ProcessConfig
{
    Alert = new AlertConfig
    {
        EnableStartupNotification = true,
        NotificationType = "Defender",
        NotificationTitle = "检测到媒体捕获程序",
        NotificationMessage = "正在使用摄像头或麦克风"
    }
};

manager.ShowProcessAlert(info, config);
```

##### ShowTimeoutWarning

显示超时警告通知。

```csharp
public void ShowTimeoutWarning(ProcessInfo processInfo, int secondsRemaining)
```

**参数**:
- `processInfo`: 进程信息
- `secondsRemaining`: 剩余秒数

**示例**:
```csharp
manager.ShowTimeoutWarning(info, 30); // 30秒后将挂起
```

##### DismissAlert

关闭指定进程的通知。

```csharp
public void DismissAlert(int processId)
```

**参数**:
- `processId`: 进程 ID

---

### ProcessController

进程控制服务，负责挂起、恢复和终止进程。

#### 构造函数

```csharp
public ProcessController()
```

#### 方法

##### SuspendProcess

挂起指定进程。

```csharp
public bool SuspendProcess(int processId)
```

**参数**:
- `processId`: 进程 ID

**返回值**: 操作是否成功

**示例**:
```csharp
var controller = new ProcessController();
bool success = controller.SuspendProcess(12345);

if (success)
{
    Console.WriteLine("进程已挂起");
}
else
{
    Console.WriteLine("挂起失败");
}
```

##### ResumeProcess

恢复已挂起的进程。

```csharp
public bool ResumeProcess(int processId)
```

**参数**:
- `processId`: 进程 ID

**返回值**: 操作是否成功

**示例**:
```csharp
bool success = controller.ResumeProcess(12345);
```

##### KillProcess

终止指定进程。

```csharp
public bool KillProcess(int processId)
```

**参数**:
- `processId`: 进程 ID

**返回值**: 操作是否成功

**警告**: 终止进程可能导致数据丢失，请谨慎使用。

**示例**:
```csharp
bool success = controller.KillProcess(12345);
```

---

### ConfigurationManager

配置管理服务，负责加载和保存配置文件。

#### 构造函数

```csharp
public ConfigurationManager()
```

#### 方法

##### LoadProcessConfigs

加载进程配置。

```csharp
public Dictionary<string, ProcessConfig> LoadProcessConfigs()
```

**返回值**: 进程配置字典

**示例**:
```csharp
var manager = new ConfigurationManager();
var configs = manager.LoadProcessConfigs();

foreach (var (name, config) in configs)
{
    Console.WriteLine($"进程: {config.DisplayName}");
    Console.WriteLine($"  启用: {config.IsEnabled}");
    Console.WriteLine($"  最大运行时间: {config.MaxRuntime}");
}
```

##### LoadAppSettings

加载应用程序设置。

```csharp
public AppSettings LoadAppSettings()
```

**返回值**: 应用程序设置

**示例**:
```csharp
var settings = manager.LoadAppSettings();
Console.WriteLine($"开机自启动: {settings.StartWithWindows}");
Console.WriteLine($"主题: {settings.ThemeMode}");
```

##### SaveProcessConfigs

保存进程配置。

```csharp
public void SaveProcessConfigs(Dictionary<string, ProcessConfig> configs)
```

**参数**:
- `configs`: 进程配置字典

**示例**:
```csharp
configs["new_process.exe"] = new ProcessConfig
{
    ProcessName = "new_process.exe",
    DisplayName = "新进程",
    IsEnabled = true
};

manager.SaveProcessConfigs(configs);
```

##### SaveAppSettings

保存应用程序设置。

```csharp
public void SaveAppSettings(AppSettings settings)
```

**参数**:
- `settings`: 应用程序设置

**示例**:
```csharp
settings.ThemeMode = "Dark";
manager.SaveAppSettings(settings);
```

##### ValidateConfig

验证进程配置是否有效。

```csharp
public bool ValidateConfig(ProcessConfig config)
```

**参数**:
- `config`: 进程配置

**返回值**: 配置是否有效

**示例**:
```csharp
var config = new ProcessConfig { ProcessName = "test.exe" };
if (manager.ValidateConfig(config))
{
    Console.WriteLine("配置有效");
}
```

---

### Logger

日志记录服务。

#### 构造函数

```csharp
public Logger()
```

#### 方法

##### LogInfo

记录信息日志。

```csharp
public void LogInfo(string message)
```

**示例**:
```csharp
var logger = new Logger();
logger.LogInfo("系统启动");
```

##### LogWarning

记录警告日志。

```csharp
public void LogWarning(string message)
```

**示例**:
```csharp
logger.LogWarning("配置文件格式可能有误");
```

##### LogError

记录错误日志。

```csharp
public void LogError(string message, Exception? ex = null)
```

**参数**:
- `message`: 错误消息
- `ex`: 异常对象（可选）

**示例**:
```csharp
try
{
    // 某些操作
}
catch (Exception ex)
{
    logger.LogError("操作失败", ex);
}
```

##### GetRecentLogs

获取最近的日志记录。

```csharp
public List<LogEntry> GetRecentLogs(int count)
```

**参数**:
- `count`: 获取的日志条数

**返回值**: 日志列表

**示例**:
```csharp
var recentLogs = logger.GetRecentLogs(50);
foreach (var log in recentLogs)
{
    Console.WriteLine($"[{log.Timestamp}] {log.Level}: {log.Message}");
}
```

##### CleanOldLogs

清理过期日志。

```csharp
public void CleanOldLogs(int retentionDays)
```

**参数**:
- `retentionDays`: 保留天数

**示例**:
```csharp
logger.CleanOldLogs(30); // 清理30天前的日志
```

---

## 📋 数据模型

### ProcessConfig

进程配置模型。

```csharp
public class ProcessConfig
{
    /// <summary>
    /// 进程可执行文件名（如: media_capture.exe）
    /// </summary>
    public string ProcessName { get; set; }
    
    /// <summary>
    /// 显示名称（如: 媒体捕获）
    /// </summary>
    public string DisplayName { get; set; }
    
    /// <summary>
    /// 是否启用监控
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// 最大运行时间（00:00:00 表示不限制）
    /// </summary>
    public TimeSpan MaxRuntime { get; set; }
    
    /// <summary>
    /// 超时后的操作类型: "suspend" | "kill" | "none"
    /// </summary>
    public string ActionType { get; set; }
    
    /// <summary>
    /// 提前警告时间（秒）
    /// </summary>
    public int AlertBeforeAction { get; set; }
    
    /// <summary>
    /// 通知配置
    /// </summary>
    public AlertConfig Alert { get; set; }
}
```

### AlertConfig

通知配置模型。

```csharp
public class AlertConfig
{
    /// <summary>
    /// 启用启动通知
    /// </summary>
    public bool EnableStartupNotification { get; set; }
    
    /// <summary>
    /// 通知类型: "Defender" | "Native"
    /// </summary>
    public string NotificationType { get; set; }
    
    /// <summary>
    /// 通知标题
    /// </summary>
    public string NotificationTitle { get; set; }
    
    /// <summary>
    /// 通知消息
    /// </summary>
    public string NotificationMessage { get; set; }
    
    /// <summary>
    /// 启用持续提醒（水印）
    /// </summary>
    public bool EnableContinuousAlert { get; set; }
    
    /// <summary>
    /// 水印第一行文字
    /// </summary>
    public string WatermarkText1 { get; set; }
    
    /// <summary>
    /// 水印第二行文字
    /// </summary>
    public string WatermarkText2 { get; set; }
    
    /// <summary>
    /// 水印位置: "TopLeft" | "TopRight" | "BottomLeft" | "BottomRight"
    /// </summary>
    public string WatermarkPosition { get; set; }
    
    /// <summary>
    /// 水印透明度 (0.0 - 1.0)
    /// </summary>
    public double WatermarkOpacity { get; set; }
    
    /// <summary>
    /// 水印颜色（十六进制）
    /// </summary>
    public string WatermarkColor { get; set; }
}
```

### ProcessInfo

进程运行时信息。

```csharp
public class ProcessInfo
{
    /// <summary>
    /// 进程 ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 进程名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; set; }
    
    /// <summary>
    /// 启动时间
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// 运行时长
    /// </summary>
    public TimeSpan Runtime { get; set; }
    
    /// <summary>
    /// 状态: "Running" | "Suspended" | "Exited"
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// 关联的配置
    /// </summary>
    public ProcessConfig Config { get; set; }
}
```

### MediaDeviceUsage

媒体设备使用情况。

```csharp
public class MediaDeviceUsage
{
    /// <summary>
    /// 进程 ID
    /// </summary>
    public int ProcessId { get; set; }
    
    /// <summary>
    /// 进程名称
    /// </summary>
    public string ProcessName { get; set; }
    
    /// <summary>
    /// 是否正在使用摄像头
    /// </summary>
    public bool IsCameraInUse { get; set; }
    
    /// <summary>
    /// 是否正在使用麦克风
    /// </summary>
    public bool IsMicrophoneInUse { get; set; }
    
    /// <summary>
    /// 检测时间
    /// </summary>
    public DateTime DetectionTime { get; set; }
}
```

### LogEntry

日志条目。

```csharp
public class LogEntry
{
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// 日志级别: "Info" | "Warning" | "Error"
    /// </summary>
    public string Level { get; set; }
    
    /// <summary>
    /// 日志消息
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// 详细信息（可选）
    /// </summary>
    public object? Details { get; set; }
}
```

### AppSettings

应用程序设置。

```csharp
public class AppSettings
{
    // 启动设置
    public bool StartWithWindows { get; set; } = false;
    public bool StartMinimized { get; set; } = false;
    
    // 界面设置
    public string ThemeMode { get; set; } = "Light";
    
    // 通知设置
    public bool EnableTrayNotifications { get; set; } = true;
    public bool ShowStartupDialog { get; set; } = true;
    
    // 监控设置
    public int MonitorInterval { get; set; } = 1;
    public bool EnableAutoAction { get; set; } = true;
    public bool EnableDetailedLogging { get; set; } = true;
    
    // 高级设置
    public int LogRetentionDays { get; set; } = 30;
    public bool AutoCleanLogs { get; set; } = true;
    public bool CheckForUpdates { get; set; } = true;
    public bool EnableAnalytics { get; set; } = false;
    
    // 安全设置
    public bool EnablePasswordProtection { get; set; } = false;
    public string PasswordHash { get; set; } = string.Empty;
}
```

---

## 🔌 扩展接口

### IAlert（计划中）

自定义通知接口。

```csharp
public interface IAlert
{
    string Name { get; }
    void Show(string title, string message);
    void Dismiss();
}
```

### IPlugin（计划中）

插件接口。

```csharp
public interface IPlugin
{
    string Name { get; }
    string Version { get; }
    void Initialize();
    void OnProcessDetected(ProcessInfo info);
    void OnProcessExited(ProcessInfo info);
}
```

---

## 📝 使用示例

### 完整示例：监控进程

```csharp
using MediaDetectionSystem.Services;
using MediaDetectionSystem.Models;

class Program
{
    static void Main()
    {
        // 1. 创建服务
        var configManager = new ConfigurationManager();
        var processMonitor = new ProcessMonitor();
        var alertManager = new AlertManager();
        var processController = new ProcessController();
        var logger = new Logger();
        
        // 2. 加载配置
        var configs = configManager.LoadProcessConfigs();
        
        // 3. 订阅事件
        processMonitor.ProcessDetected += (sender, e) =>
        {
            if (e.Action == "Detected")
            {
                logger.LogInfo($"检测到进程: {e.ProcessInfo.DisplayName}");
                
                // 显示通知
                var config = e.ProcessInfo.Config;
                alertManager.ShowProcessAlert(e.ProcessInfo, config);
            }
        };
        
        processMonitor.ProcessExceededLimit += (sender, e) =>
        {
            logger.LogWarning($"进程超时: {e.ProcessInfo.DisplayName}");
            
            // 执行操作
            if (e.Action == "suspend")
            {
                processController.SuspendProcess(e.ProcessInfo.Id);
            }
            else if (e.Action == "kill")
            {
                processController.KillProcess(e.ProcessInfo.Id);
            }
        };
        
        // 4. 启动监控
        var timer = new System.Timers.Timer(1000);
        timer.Elapsed += (sender, e) =>
        {
            processMonitor.CheckProcesses(configs);
        };
        timer.Start();
        
        // 5. 保持运行
        Console.WriteLine("监控已启动，按任意键退出...");
        Console.ReadKey();
        
        timer.Stop();
        logger.LogInfo("系统退出");
    }
}
```

---

## 🔗 相关链接

- [技术架构](architecture.md) - 系统架构文档
- [开发指南](development.md) - 开发说明
- [配置指南](configuration.md) - 配置说明

---

<div align="center">
  <sub>API 文档 v1.0 | 最后更新: 2025-11-01</sub>
</div>

