# 🛠️ 开发指南

<div align="center">
  <a href="../README.md">返回首页</a> · <a href="architecture.md">技术架构</a> · <a href="api.md">API 文档</a>
</div>

---

## 🎯 开发环境搭建

### 必需工具

| 工具 | 版本 | 用途 |
|------|------|------|
| Visual Studio 2022 | 17.8+ | 主要 IDE |
| .NET 8.0 SDK | 8.0+ | 编译和运行 |
| Git | 最新 | 版本控制 |
| PowerShell | 5.1+ | 脚本运行 |

### 可选工具

| 工具 | 用途 |
|------|------|
| Visual Studio Code | 轻量级编辑 |
| ReSharper | 代码分析和重构 |
| dotTrace | 性能分析 |
| dotMemory | 内存分析 |

---

## 📦 环境安装

### 1. 安装 Visual Studio 2022

下载地址：https://visualstudio.microsoft.com/

**必需工作负载**:
- ✅ .NET 桌面开发
- ✅ Windows 应用 SDK (UWP)

**可选组件**:
- NuGet 包管理器
- Git for Windows
- C++ 工具（用于性能分析）

### 2. 安装 .NET 8.0 SDK

```powershell
# 使用 winget 安装
winget install Microsoft.DotNet.SDK.8

# 验证安装
dotnet --version
# 输出: 8.0.x
```

### 3. 克隆仓库

```powershell
# 克隆主仓库
git clone https://github.com/vistaminc/StarReminder.git
cd StarReminder

# 进入项目目录
cd "C#/MediaDetectionSystem"
```

### 4. 还原依赖

```powershell
# 还原 NuGet 包
dotnet restore

# 或使用 Visual Studio
# 打开解决方案后会自动还原
```

---

## 🏗️ 项目结构

```
C#/MediaDetectionSystem/
├── Models/                      # 数据模型
│   ├── ProcessConfig.cs
│   ├── AlertConfig.cs
│   ├── ProcessInfo.cs
│   ├── LogEntry.cs
│   └── AppSettings.cs
│
├── Services/                    # 核心服务
│   ├── ProcessMonitor.cs       # 进程监控
│   ├── MediaDeviceMonitor.cs   # 媒体设备检测
│   ├── AlertManager.cs         # 通知管理
│   ├── ProcessController.cs    # 进程控制
│   ├── ConfigurationManager.cs # 配置管理
│   ├── Logger.cs               # 日志服务
│   └── UpdateChecker.cs        # 更新检查
│
├── ViewModels/                  # 视图模型
│   └── MainViewModel.cs
│
├── Views/                       # 视图界面
│   ├── ConfigWindow.xaml
│   ├── ConfigWindow.xaml.cs
│   ├── SettingsWindow.xaml
│   ├── SettingsWindow.xaml.cs
│   ├── RestartConfirmDialog.xaml
│   └── RestartConfirmDialog.xaml.cs
│
├── Themes/                      # 主题样式
│   ├── LightTheme.xaml
│   └── DarkTheme.xaml
│
├── ToastNotifier/              # Toast 通知子程序
│   ├── Program.cs
│   └── ToastNotifier.csproj
│
├── scripts/                     # 编译脚本
│   ├── 一键编译.bat
│   ├── 发布Release版本.bat
│   └── 测试通知.bat
│
├── MainWindow.xaml             # 主窗口
├── MainWindow.xaml.cs
├── App.xaml                    # 应用程序
├── App.xaml.cs
├── app.manifest                # 应用清单
└── MediaDetectionSystem.csproj # 项目文件
```

---

## 🔨 编译和调试

### Visual Studio 调试

#### 1. 配置启动项目

1. 右键项目 → 设置为启动项目
2. 右键项目 → 属性 → 调试
3. 勾选"启用本机代码调试"

#### 2. 设置断点

在代码行号左侧点击，或按 `F9`

#### 3. 开始调试

- **F5**: 启动调试
- **Ctrl+F5**: 运行但不调试
- **F10**: 单步跳过
- **F11**: 单步进入

#### 4. 调试窗口

- **即时窗口**: 调试 → 窗口 → 即时窗口
- **监视窗口**: 调试 → 窗口 → 监视
- **输出窗口**: 查看 Debug.WriteLine 输出

### 命令行编译

```powershell
# Debug 版本
dotnet build -c Debug

# Release 版本
dotnet build -c Release

# 清理编译
dotnet clean

# 运行
dotnet run
```

### 编译脚本

```powershell
# 一键编译 Debug 版本
.\scripts\一键编译.bat

# 编译并运行
.\scripts\编译并运行.bat

# 发布 Release 版本
.\scripts\发布Release版本.bat
```

---

## 🧪 测试

### 单元测试

创建测试项目：

```powershell
# 创建测试项目
dotnet new xunit -n StarReminder.Tests

# 添加项目引用
dotnet add StarReminder.Tests reference MediaDetectionSystem
```

示例测试：

```csharp
using Xunit;
using MediaDetectionSystem.Services;

public class ProcessMonitorTests
{
    [Fact]
    public void CheckProcesses_ShouldDetectProcess()
    {
        // Arrange
        var monitor = new ProcessMonitor();
        
        // Act
        var result = monitor.CheckProcesses(configs);
        
        // Assert
        Assert.NotNull(result);
    }
}
```

运行测试：

```powershell
dotnet test
```

### 集成测试

测试完整功能流程：

```csharp
[Fact]
public void EndToEnd_ProcessDetectionAndNotification()
{
    // 1. 启动监控
    var app = new App();
    app.InitializeComponent();
    
    // 2. 启动测试进程
    var testProcess = Process.Start("notepad.exe");
    
    // 3. 等待检测
    Thread.Sleep(2000);
    
    // 4. 验证检测到
    Assert.True(app.IsProcessDetected(testProcess.Id));
    
    // 5. 清理
    testProcess.Kill();
}
```

### 手动测试

使用测试脚本：

```powershell
# 测试通知功能
.\scripts\测试通知.bat

# 启动测试进程
notepad.exe

# 打开相机应用测试媒体检测
start ms-camera:
```

---

## 📝 编码规范

### C# 命名规范

| 类型 | 规范 | 示例 |
|------|------|------|
| 类 | PascalCase | `ProcessMonitor` |
| 接口 | I + PascalCase | `IAlertManager` |
| 方法 | PascalCase | `CheckProcesses()` |
| 属性 | PascalCase | `ProcessName` |
| 私有字段 | _camelCase | `_processMonitor` |
| 局部变量 | camelCase | `processId` |
| 常量 | UPPER_CASE | `MAX_RETRY_COUNT` |

### 代码风格

#### 1. 使用 var 关键字

```csharp
// ✅ 推荐
var monitor = new ProcessMonitor();

// ❌ 不推荐
ProcessMonitor monitor = new ProcessMonitor();
```

#### 2. 空行分隔

```csharp
public void Method()
{
    // 初始化
    var config = LoadConfig();
    
    // 处理
    ProcessConfig(config);
    
    // 清理
    CleanupResources();
}
```

#### 3. 单一职责

```csharp
// ✅ 推荐：职责单一
public class ProcessMonitor
{
    public void CheckProcesses() { }
}

public class AlertManager
{
    public void ShowAlert() { }
}

// ❌ 不推荐：职责混杂
public class ProcessManager
{
    public void CheckProcesses() { }
    public void ShowAlert() { }
    public void SaveConfig() { }
}
```

#### 4. 异常处理

```csharp
// ✅ 推荐
try
{
    ProcessOperation();
}
catch (SpecificException ex)
{
    Logger.LogError("操作失败", ex);
    throw; // 重新抛出
}

// ❌ 不推荐：吞掉异常
try
{
    ProcessOperation();
}
catch (Exception)
{
    // 什么都不做
}
```

#### 5. 使用 using 语句

```csharp
// ✅ 推荐
using (var process = Process.GetProcessById(id))
{
    process.Kill();
}

// 或使用 using 声明 (C# 8.0+)
using var process = Process.GetProcessById(id);
process.Kill();
```

#### 6. 异步编程

```csharp
// ✅ 推荐
public async Task<ProcessInfo> GetProcessInfoAsync(int id)
{
    await Task.Delay(100);
    return new ProcessInfo();
}

// 调用
var info = await GetProcessInfoAsync(123);
```

### 注释规范

#### XML 文档注释

```csharp
/// <summary>
/// 检测目标进程的启动和运行状态
/// </summary>
/// <param name="configs">进程配置字典</param>
/// <returns>检测到的进程数量</returns>
/// <exception cref="ArgumentNullException">configs 为 null</exception>
public int CheckProcesses(Dictionary<string, ProcessConfig> configs)
{
    // ...
}
```

#### 行内注释

```csharp
// 检查进程是否在使用摄像头
if (IsCameraInUse(process))
{
    // 发送通知
    ShowAlert(process);
}
```

---

## 🔧 调试技巧

### 1. 条件断点

右键断点 → 条件 → 输入条件：

```csharp
processId == 12345
```

### 2. 数据断点

监视特定变量的值变化：

调试 → 新建断点 → 数据断点

### 3. Tracepoint

输出调试信息而不中断执行：

右键断点 → 操作 → 记录消息

```
进程 ID: {processId}, 名称: {processName}
```

### 4. 即时窗口

在调试时执行代码：

```csharp
? processId       // 查看变量值
processId = 999   // 修改变量值
MyMethod()        // 调用方法
```

### 5. 诊断工具

查看性能和内存：

调试 → 窗口 → 显示诊断工具

---

## 🚀 发布

### 发布配置

#### 1. 单文件发布

```powershell
dotnet publish -c Release -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true
```

#### 2. 框架依赖发布

```powershell
dotnet publish -c Release -r win-x64 `
  --self-contained false
```

#### 3. 修剪发布

减小文件大小：

```powershell
dotnet publish -c Release -r win-x64 `
  --self-contained true `
  -p:PublishTrimmed=true
```

### 发布脚本

使用项目提供的脚本：

```powershell
.\scripts\发布Release版本.bat
```

自动完成：
1. 编译 ToastNotifier
2. 编译主程序
3. 复制资源文件
4. 生成版本信息
5. 打包到 Release 目录

---

## 🎨 添加新功能

### 示例：添加网络监控

#### 1. 创建模型

```csharp
// Models/NetworkUsage.cs
public class NetworkUsage
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; }
    public long BytesSent { get; set; }
    public long BytesReceived { get; set; }
}
```

#### 2. 创建服务

```csharp
// Services/NetworkMonitor.cs
public class NetworkMonitor
{
    public event EventHandler<NetworkUsage>? NetworkActivity;
    
    public void StartMonitoring()
    {
        // 实现监控逻辑
    }
    
    public NetworkUsage GetUsage(int processId)
    {
        // 获取网络使用情况
        return new NetworkUsage();
    }
}
```

#### 3. 集成到 ViewModel

```csharp
// ViewModels/MainViewModel.cs
private NetworkMonitor _networkMonitor = new();

public void Initialize()
{
    _networkMonitor.NetworkActivity += OnNetworkActivity;
    _networkMonitor.StartMonitoring();
}

private void OnNetworkActivity(object? sender, NetworkUsage usage)
{
    // 处理网络活动
}
```

#### 4. 更新 UI

```xml
<!-- MainWindow.xaml -->
<TextBlock Text="{Binding NetworkUsageText}"/>
```

---

## 🐛 常见问题

### 编译错误

#### 错误: CS0246 找不到类型

**原因**: 缺少引用或命名空间

**解决**:
```csharp
using MediaDetectionSystem.Models;
```

#### 错误: MSB3644 找不到 .NET Framework

**原因**: 未安装目标框架

**解决**:
```powershell
winget install Microsoft.DotNet.SDK.8
```

### 运行时错误

#### 错误: 拒绝访问注册表

**原因**: 未以管理员权限运行

**解决**: 以管理员身份运行

#### 错误: ToastNotifier.exe 未找到

**原因**: 子程序未编译

**解决**:
```powershell
cd ToastNotifier
dotnet build -c Release
```

---

## 📚 学习资源

### 官方文档

- [.NET 文档](https://docs.microsoft.com/dotnet/)
- [WPF 教程](https://docs.microsoft.com/dotnet/desktop/wpf/)
- [C# 编程指南](https://docs.microsoft.com/dotnet/csharp/)

### 推荐书籍

- 《C# 11.0 in a Nutshell》
- 《Pro WPF in C# 2012》
- 《CLR via C#》

### 视频教程

- [Microsoft Learn - WPF](https://learn.microsoft.com/training/paths/wpf/)
- [Pluralsight - WPF MVVM](https://www.pluralsight.com/courses/wpf-mvvm-in-depth)

---

## 🤝 贡献流程

### 1. Fork 仓库

点击 GitHub 页面右上角的 "Fork" 按钮

### 2. 克隆 Fork

```powershell
git clone https://github.com/your-username/StarReminder.git
cd StarReminder
```

### 3. 创建分支

```powershell
git checkout -b feature/my-awesome-feature
```

### 4. 提交更改

```powershell
git add .
git commit -m "feat: add awesome feature"
```

提交信息规范：
- `feat`: 新功能
- `fix`: 修复 Bug
- `docs`: 文档更新
- `style`: 代码格式
- `refactor`: 重构
- `test`: 测试
- `chore`: 构建/工具

### 5. 推送到 Fork

```powershell
git push origin feature/my-awesome-feature
```

### 6. 创建 Pull Request

在 GitHub 页面点击 "New Pull Request"

---

## 📋 代码审查清单

提交 PR 前检查：

- [ ] 代码遵循编码规范
- [ ] 添加了必要的注释
- [ ] 更新了相关文档
- [ ] 添加了单元测试
- [ ] 所有测试通过
- [ ] 没有编译警告
- [ ] 没有性能问题
- [ ] 没有内存泄漏
- [ ] UI 响应流畅
- [ ] 日志记录完整

---

## 🔗 相关链接

- [技术架构](architecture.md) - 系统架构设计
- [API 文档](api.md) - API 接口文档
- [配置指南](configuration.md) - 配置说明

---

<div align="center">
  <sub>欢迎贡献！一起让 StarReminder 变得更好 🌟</sub>
</div>

