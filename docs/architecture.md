# 🏗️ 技术架构文档

<div align="center">
  <a href="../README.md">返回首页</a> · <a href="api.md">API 文档</a> · <a href="development.md">开发指南</a>
</div>

---

## 📐 架构概览

StarReminder 采用经典的 **MVVM (Model-View-ViewModel)** 架构模式，基于 .NET 8.0 和 WPF 框架构建。

### 架构图

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐        │
│  │ MainWindow │  │ConfigWindow│  │ Settings   │        │
│  │   (XAML)   │  │   (XAML)   │  │Window(XAML)│        │
│  └──────┬─────┘  └──────┬─────┘  └──────┬─────┘        │
│         │                │                │              │
│         └────────────────┼────────────────┘              │
│                          │                               │
├──────────────────────────┼───────────────────────────────┤
│                 ViewModel Layer                          │
│         ┌────────────────┴────────────────┐              │
│         │        MainViewModel            │              │
│         │  (Data Binding & Commands)      │              │
│         └────────────────┬────────────────┘              │
│                          │                               │
├──────────────────────────┼───────────────────────────────┤
│                   Service Layer                          │
│  ┌───────────┐  ┌───────────┐  ┌──────────────┐        │
│  │  Process  │  │   Media   │  │    Alert     │        │
│  │  Monitor  │  │  Device   │  │   Manager    │        │
│  │           │  │  Monitor  │  │              │        │
│  └─────┬─────┘  └─────┬─────┘  └──────┬───────┘        │
│        │              │                │                 │
│  ┌─────┴──────┐ ┌─────┴──────┐  ┌─────┴───────┐        │
│  │  Process   │ │   Config   │  │   Logger    │        │
│  │ Controller │ │  Manager   │  │             │        │
│  └────────────┘ └────────────┘  └─────────────┘        │
│                          │                               │
├──────────────────────────┼───────────────────────────────┤
│                    Model Layer                           │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐        │
│  │  Process   │  │   Alert    │  │    App     │        │
│  │   Config   │  │   Config   │  │  Settings  │        │
│  └────────────┘  └────────────┘  └────────────┘        │
│                                                          │
├──────────────────────────────────────────────────────────┤
│                 Infrastructure Layer                     │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐        │
│  │  Windows   │  │  Registry  │  │   File     │        │
│  │    API     │  │    API     │  │   System   │        │
│  └────────────┘  └────────────┘  └────────────┘        │
└──────────────────────────────────────────────────────────┘
```

---

## 🧩 核心组件

### 1. Models（数据模型层）

#### ProcessConfig.cs

进程监控配置模型：

```csharp
public class ProcessConfig
{
    public string ProcessName { get; set; }        // 进程名称
    public string DisplayName { get; set; }        // 显示名称
    public bool IsEnabled { get; set; }            // 是否启用
    public TimeSpan MaxRuntime { get; set; }       // 最大运行时间
    public string ActionType { get; set; }         // 操作类型
    public int AlertBeforeAction { get; set; }     // 提前警告时间
    public AlertConfig Alert { get; set; }         // 提醒配置
}
```

#### AlertConfig.cs

通知配置模型：

```csharp
public class AlertConfig
{
    public bool EnableStartupNotification { get; set; }
    public string NotificationType { get; set; }
    public string NotificationTitle { get; set; }
    public string NotificationMessage { get; set; }
    public bool EnableContinuousAlert { get; set; }
    public string WatermarkText1 { get; set; }
    public string WatermarkText2 { get; set; }
    public string WatermarkPosition { get; set; }
    public double WatermarkOpacity { get; set; }
    public string WatermarkColor { get; set; }
}
```

#### ProcessInfo.cs

进程运行时信息：

```csharp
public class ProcessInfo
{
    public int Id { get; set; }                    // 进程 ID
    public string Name { get; set; }               // 进程名称
    public string DisplayName { get; set; }        // 显示名称
    public DateTime StartTime { get; set; }        // 启动时间
    public TimeSpan Runtime { get; set; }          // 运行时长
    public string Status { get; set; }             // 状态
    public ProcessConfig Config { get; set; }      // 配置
}
```

#### AppSettings.cs

应用程序设置：

```csharp
public class AppSettings
{
    // 启动设置
    public bool StartWithWindows { get; set; }
    public bool StartMinimized { get; set; }
    
    // 界面设置
    public string ThemeMode { get; set; }
    
    // 通知设置
    public bool EnableTrayNotifications { get; set; }
    public bool ShowStartupDialog { get; set; }
    
    // 监控设置
    public int MonitorInterval { get; set; }
    public bool EnableAutoAction { get; set; }
    public bool EnableDetailedLogging { get; set; }
    
    // 高级设置
    public int LogRetentionDays { get; set; }
    public bool AutoCleanLogs { get; set; }
    public bool CheckForUpdates { get; set; }
}
```

---

### 2. Services（服务层）

#### ProcessMonitor.cs

**职责**: 监控目标进程的启动和运行状态

**核心功能**:
```csharp
public class ProcessMonitor
{
    // 事件
    public event EventHandler<ProcessEventArgs>? ProcessDetected;
    public event EventHandler<ProcessEventArgs>? ProcessExceededLimit;
    public event EventHandler<ProcessEventArgs>? ProcessNearLimit;
    
    // 方法
    public void CheckProcesses(Dictionary<string, ProcessConfig> configs);
    public ProcessInfo? GetProcessInfo(int processId);
    public IReadOnlyDictionary<int, MediaDeviceUsage> GetActiveMediaProcesses();
}
```

**检测流程**:

1. 获取所有运行中的进程
2. 从设备层面检测媒体设备使用情况
3. 匹配配置的监控进程
4. 记录新检测到的进程
5. 更新运行时间
6. 检查超时并触发事件

**特点**:
- 从设备角度检测（更准确）
- 区分媒体相关进程和普通进程
- 支持运行时间限制
- 支持提前警告

#### MediaDeviceMonitor.cs

**职责**: 检测摄像头和麦克风的实际使用情况

**核心功能**:
```csharp
public class MediaDeviceMonitor
{
    public List<MediaDeviceUsage> GetActiveDeviceUsers();
    public IReadOnlyDictionary<int, MediaDeviceUsage> GetActiveMediaProcesses();
    public void CleanupExitedProcess(int processId);
}
```

**检测方法**:

1. **注册表检测**（主要方法）
   - 路径: `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore`
   - 检查 `webcam` 和 `microphone` 子键
   - `LastUsedTimeStop = 0` 表示正在使用

2. **句柄检测**（备用方法）
   - 检查进程加载的 DLL
   - 摄像头相关: `mfreadwrite.dll`, `mf.dll`, `mfplat.dll`, `ksproxy.ax`
   - 麦克风相关: `audioses.dll`, `audioeng.dll`, `winmm.dll`

**优势**:
- 双重检测机制，提高准确性
- 无需轮询进程，性能开销低
- 可检测到任何使用设备的进程

#### AlertManager.cs

**职责**: 管理通知和持续提醒

**核心功能**:
```csharp
public class AlertManager
{
    public void ShowProcessAlert(ProcessInfo processInfo, ProcessConfig config);
    public void ShowTimeoutWarning(ProcessInfo processInfo, int secondsRemaining);
    public void DismissAlert(int processId);
}
```

**通知类型**:

1. **Defender 样式**
   - 调用 `ToastNotifier.exe` 子程序
   - 传递参数: 标题、消息、图标路径
   - 显示为 "Windows 安全中心"

2. **Native 样式**
   - 使用 `Microsoft.Toolkit.Uwp.Notifications`
   - 程序自己的身份
   - 更稳定可靠

3. **持续水印**
   - 创建置顶透明窗口
   - 显示在指定位置
   - 进程结束后自动关闭

#### ProcessController.cs

**职责**: 控制进程的挂起、恢复和终止

**核心功能**:
```csharp
public class ProcessController
{
    public bool SuspendProcess(int processId);
    public bool ResumeProcess(int processId);
    public bool KillProcess(int processId);
}
```

**实现细节**:

使用 Windows Native API (P/Invoke):

```csharp
[DllImport("ntdll.dll")]
private static extern int NtSuspendProcess(IntPtr processHandle);

[DllImport("ntdll.dll")]
private static extern int NtResumeProcess(IntPtr processHandle);
```

**挂起 vs 终止**:

| 操作 | 可恢复 | 数据丢失风险 | 使用场景 |
|------|--------|-------------|----------|
| 挂起 (Suspend) | ✅ | 低 | 时间限制，临时阻止 |
| 终止 (Kill) | ❌ | 高 | 危险进程，必须停止 |

#### ConfigurationManager.cs

**职责**: 配置文件的加载、保存和验证

**核心功能**:
```csharp
public class ConfigurationManager
{
    public Dictionary<string, ProcessConfig> LoadProcessConfigs();
    public AppSettings LoadAppSettings();
    public void SaveProcessConfigs(Dictionary<string, ProcessConfig> configs);
    public void SaveAppSettings(AppSettings settings);
    public bool ValidateConfig(ProcessConfig config);
}
```

**配置加载流程**:

1. 读取 JSON 文件
2. 反序列化为对象
3. 验证配置有效性
4. 填充默认值
5. 返回配置对象

**热加载机制**:

```csharp
private FileSystemWatcher _configWatcher;

_configWatcher = new FileSystemWatcher(".")
{
    Filter = "config.json",
    NotifyFilter = NotifyFilters.LastWrite
};

_configWatcher.Changed += OnConfigChanged;
```

#### Logger.cs

**职责**: 记录系统活动日志

**核心功能**:
```csharp
public class Logger
{
    public void LogInfo(string message);
    public void LogWarning(string message);
    public void LogError(string message, Exception? ex = null);
    public List<LogEntry> GetRecentLogs(int count);
    public void CleanOldLogs(int retentionDays);
}
```

**日志格式**:

```json
{
  "timestamp": "2025-11-01T15:30:00.123Z",
  "level": "Info",
  "message": "检测到进程启动: media_capture.exe",
  "details": {
    "processId": 12345,
    "processName": "media_capture.exe"
  }
}
```

**日志级别**:
- `Info`: 正常信息
- `Warning`: 警告信息
- `Error`: 错误信息

---

### 3. ViewModels（视图模型层）

#### MainViewModel.cs

**职责**: 主窗口的数据绑定和命令处理

**核心属性**:
```csharp
public class MainViewModel : INotifyPropertyChanged
{
    // 可观察集合
    public ObservableCollection<ProcessInfo> MonitoredProcesses { get; set; }
    public ObservableCollection<LogEntry> RecentLogs { get; set; }
    
    // 状态属性
    public bool IsMonitoringEnabled { get; set; }
    public string SystemStatus { get; set; }
    
    // 命令
    public ICommand ToggleMonitoringCommand { get; set; }
    public ICommand OpenSettingsCommand { get; set; }
    public ICommand RefreshCommand { get; set; }
}
```

**MVVM 数据绑定**:

```xml
<!-- View (XAML) -->
<ToggleButton IsChecked="{Binding IsMonitoringEnabled}"
              Command="{Binding ToggleMonitoringCommand}"/>

<ItemsControl ItemsSource="{Binding MonitoredProcesses}"/>
```

```csharp
// ViewModel
public bool IsMonitoringEnabled
{
    get => _isMonitoringEnabled;
    set
    {
        _isMonitoringEnabled = value;
        OnPropertyChanged(nameof(IsMonitoringEnabled));
        UpdateMonitoringState();
    }
}
```

---

### 4. Views（视图层）

#### MainWindow.xaml

主界面：
- 进程状态卡片
- 系统开关
- 活动日志
- 工具栏

#### ConfigWindow.xaml

配置窗口：
- 进程选择
- 参数配置
- 通知设置
- 水印配置

#### SettingsWindow.xaml

设置窗口：
- 启动设置
- 界面设置
- 通知设置
- 高级设置

---

## 🔄 核心流程

### 进程检测流程

```
┌─────────────────────────────────────────────────┐
│              1. 定时器触发 (1秒)                │
└───────────────────┬─────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────┐
│     2. MediaDeviceMonitor 检测设备使用          │
│        - 检查摄像头注册表                       │
│        - 检查麦克风注册表                       │
│        - 返回使用设备的进程列表                 │
└───────────────────┬─────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────┐
│      3. ProcessMonitor 匹配监控进程             │
│        - 遍历配置的监控进程                     │
│        - 检查进程是否在运行                     │
│        - 检查媒体进程是否使用设备               │
└───────────────────┬─────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────┐
│           4. 发现新进程？                       │
└───────┬─────────────────────┬───────────────────┘
        │ Yes                 │ No
        ▼                     ▼
┌───────────────────┐   ┌────────────────────────┐
│ 5. 触发事件       │   │ 6. 更新运行时间        │
│  ProcessDetected  │   │    检查超时            │
└───────┬───────────┘   └────────┬───────────────┘
        │                        │
        ▼                        ▼
┌───────────────────┐   ┌────────────────────────┐
│ 7. AlertManager   │   │ 8. 超时？              │
│    显示通知       │   └────┬───────────────────┘
└───────────────────┘        │ Yes
                             ▼
                    ┌────────────────────────┐
                    │ 9. 执行操作            │
                    │    (suspend/kill)      │
                    └────────────────────────┘
```

### 通知流程

```
┌─────────────────────────────────────────────────┐
│         1. AlertManager.ShowAlert               │
└───────────────────┬─────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────┐
│         2. 选择通知类型                         │
└───────┬─────────────────────┬───────────────────┘
        │ Defender            │ Native
        ▼                     ▼
┌───────────────────┐   ┌────────────────────────┐
│ 3a. 调用          │   │ 3b. 使用               │
│ ToastNotifier.exe │   │ WinRT Toast API        │
│                   │   │                        │
│ 参数:             │   │ ToastNotification      │
│ - 标题            │   │ Builder                │
│ - 消息            │   │                        │
│ - 图标            │   │                        │
└───────┬───────────┘   └────────┬───────────────┘
        │                        │
        └────────────┬───────────┘
                     ▼
┌─────────────────────────────────────────────────┐
│      4. 启用持续提醒？                          │
└───────┬─────────────────────┬───────────────────┘
        │ Yes                 │ No
        ▼                     ▼
┌───────────────────┐   ┌────────────────────────┐
│ 5. 创建水印窗口   │   │ 完成                   │
│  - 置顶透明       │   └────────────────────────┘
│  - 指定位置       │
│  - 显示文本       │
└───────────────────┘
```

---

## 🎯 设计模式

### 1. MVVM 模式

**分离关注点**:
- View: 仅负责 UI 展示
- ViewModel: 数据绑定和命令
- Model: 业务逻辑和数据

**优势**:
- 可测试性强
- 代码复用
- 维护性好

### 2. 观察者模式

**事件驱动**:

```csharp
// 发布者
public event EventHandler<ProcessEventArgs>? ProcessDetected;

// 触发事件
ProcessDetected?.Invoke(this, eventArgs);

// 订阅者
_processMonitor.ProcessDetected += OnProcessDetected;
```

### 3. 单例模式

**全局服务**:

```csharp
public class ConfigurationManager
{
    private static ConfigurationManager? _instance;
    public static ConfigurationManager Instance
    {
        get
        {
            _instance ??= new ConfigurationManager();
            return _instance;
        }
    }
}
```

### 4. 工厂模式

**通知创建**:

```csharp
public class AlertFactory
{
    public static IAlert CreateAlert(string type)
    {
        return type switch
        {
            "Defender" => new DefenderAlert(),
            "Native" => new NativeAlert(),
            _ => throw new ArgumentException()
        };
    }
}
```

---

## 🔐 安全考虑

### 1. 权限管理

- **管理员权限**: 必需，用于访问注册表和控制进程
- **最小权限原则**: 仅请求必要的权限
- **UAC 提示**: 使用应用清单配置

### 2. 进程控制安全

- **防止自我终止**: 检查目标进程 ID
- **系统进程保护**: 不允许挂起/终止关键系统进程
- **错误处理**: 捕获所有异常，防止崩溃

### 3. 数据安全

- **配置文件加密**: 敏感信息加密存储（计划功能）
- **密码保护**: 使用 SHA256 哈希
- **日志脱敏**: 不记录敏感信息

---

## 📊 性能优化

### 1. 检测性能

- **批量检测**: 一次性检测所有设备
- **缓存机制**: 缓存设备使用情况
- **增量更新**: 仅更新变化的进程

### 2. UI 性能

- **虚拟化**: 使用 `VirtualizingStackPanel`
- **异步操作**: 使用 `async/await`
- **UI 线程隔离**: 长时间操作在后台线程

### 3. 内存优化

- **及时清理**: 移除已退出的进程
- **弱引用**: 事件订阅使用弱引用
- **日志轮转**: 自动清理旧日志

---

## 🧪 可测试性

### 单元测试

```csharp
[TestClass]
public class ProcessMonitorTests
{
    [TestMethod]
    public void CheckProcesses_ShouldDetectNewProcess()
    {
        // Arrange
        var monitor = new ProcessMonitor();
        var config = new ProcessConfig { ... };
        
        // Act
        monitor.CheckProcesses(new[] { config });
        
        // Assert
        Assert.IsTrue(monitor.MonitoredProcesses.Count > 0);
    }
}
```

### 集成测试

测试完整流程：
1. 启动测试进程
2. 验证检测
3. 验证通知
4. 验证日志

---

## 🔗 扩展性

### 添加新的监控类型

1. 创建新的 Monitor 类
2. 实现检测逻辑
3. 触发标准事件
4. 集成到 MainViewModel

### 添加新的通知方式

1. 实现 `IAlert` 接口
2. 在 `AlertFactory` 中注册
3. 更新配置模型
4. 更新 UI

### 添加插件系统（计划）

```csharp
public interface IPlugin
{
    string Name { get; }
    void Initialize();
    void OnProcessDetected(ProcessInfo info);
}
```

---

## 📚 相关资源

- [.NET 8.0 文档](https://docs.microsoft.com/dotnet/)
- [WPF 文档](https://docs.microsoft.com/dotnet/desktop/wpf/)
- [MVVM 模式](https://docs.microsoft.com/xamarin/xamarin-forms/enterprise-application-patterns/mvvm)
- [Windows API](https://docs.microsoft.com/windows/win32/api/)

---

<div align="center">
  <sub>架构设计 v1.0 | 最后更新: 2025-11-01</sub>
</div>

