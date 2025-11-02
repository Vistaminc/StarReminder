# 📦 安装指南

<div align="center">
  <a href="../README.md">返回首页</a> · <a href="usage.md">使用教程</a> · <a href="configuration.md">配置指南</a>
</div>

---

## 📋 系统要求

### 操作系统

- Windows 10 版本 1809 (Build 17763) 或更高
- Windows 11 (所有版本)

> [!IMPORTANT]
> 必须以**管理员权限**运行，才能正确检测摄像头和麦克风使用情况。

### 运行时环境

#### 使用预编译版本

- .NET 8.0 Runtime（或更高版本）
- 下载地址：https://dotnet.microsoft.com/download/dotnet/8.0

#### 从源码编译

- .NET 8.0 SDK
- Visual Studio 2022 (可选，推荐)
- PowerShell 5.1+ (用于运行脚本)

### 硬件要求

| 组件 | 最低配置 | 推荐配置 |
|------|---------|---------|
| CPU | 1 GHz 双核 | 2 GHz 四核 |
| 内存 | 2 GB RAM | 4 GB RAM |
| 硬盘 | 100 MB 可用空间 | 200 MB 可用空间 |

---

## 🚀 安装方法

### 方式一：使用预编译版本（推荐）

#### 1. 下载程序

从 GitHub Releases 页面下载最新版本：

```
https://github.com/vistaminc/StarReminder/releases/latest
```

选择以下文件之一：
- `StarReminder-v1.2.0-win-x64.zip` - 需要安装 .NET 8.0 Runtime
- `StarReminder-v1.2.0-win-x64-standalone.zip` - 自包含版本（推荐）

#### 2. 解压文件

将下载的 ZIP 文件解压到任意目录，例如：

```
C:\Program Files\StarReminder\
```

#### 3. 验证文件

确保解压后包含以下文件：

```
StarReminder/
├── StarReminder.exe          # 主程序
├── StarReminder.dll
├── ToastNotifier.exe         # Toast 通知程序
├── config.json               # 配置文件
├── settings.json             # 设置文件
├── logo.png                  # 图标
├── defender.png              # Defender 通知图标
└── logs/                     # 日志目录
```

#### 4. 首次运行

1. 右键点击 `StarReminder.exe`
2. 选择"以管理员身份运行"
3. 如果出现 Windows Defender SmartScreen 警告，点击"更多信息" → "仍要运行"

> [!TIP]
> 可以创建快捷方式并设置为始终以管理员身份运行：
> 右键快捷方式 → 属性 → 高级 → 勾选"用管理员身份运行"

---

### 方式二：从源码编译

#### 1. 克隆仓库

```powershell
git clone https://github.com/vistaminc/StarReminder.git
cd StarReminder
```

#### 2. 进入项目目录

```powershell
cd "C#/MediaDetectionSystem"
```

#### 3. 还原依赖

```powershell
dotnet restore
```

#### 4. 编译项目

##### Debug 版本（开发调试）

```powershell
dotnet build -c Debug
```

##### Release 版本（正式发布）

```powershell
dotnet build -c Release
```

#### 5. 发布自包含版本

生成不需要 .NET Runtime 的独立可执行文件：

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

输出目录：
```
bin/Release/net8.0-windows10.0.17763.0/win-x64/publish/
```

> [!TIP]
> 使用便捷脚本快速编译：
> ```powershell
> .\scripts\发布Release版本.bat
> ```

---

### 方式三：使用 Visual Studio

#### 1. 安装 Visual Studio 2022

下载地址：https://visualstudio.microsoft.com/

需要安装以下工作负载：
- .NET 桌面开发
- Windows 应用 SDK

#### 2. 打开项目

1. 启动 Visual Studio 2022
2. 点击"打开项目或解决方案"
3. 选择 `C#/MediaDetectionSystem/MediaDetectionSystem.csproj`

#### 3. 设置启动项

1. 在解决方案资源管理器中右键项目
2. 选择"设置为启动项目"

#### 4. 配置管理员权限

1. 右键项目 → 属性
2. 在"应用程序"选项卡中点击"应用清单"
3. 确保 `app.manifest` 中包含：

```xml
<requestedExecutionLevel level="requireAdministrator" uiAccess="false" />
```

#### 5. 编译和运行

- **调试运行**: 按 `F5`
- **编译**: 按 `Ctrl+Shift+B`
- **发布**: 右键项目 → 发布

---

## ⚙️ 编译脚本说明

项目提供了多个便捷脚本（位于 `scripts/` 目录）：

### 一键编译.bat

快速编译 Debug 版本：

```powershell
.\scripts\一键编译.bat
```

### 发布Release版本.bat

编译并打包 Release 版本：

```powershell
.\scripts\发布Release版本.bat
```

会自动：
1. 编译 ToastNotifier 子程序
2. 编译主程序
3. 复制必要文件到 `Release/` 目录
4. 生成版本信息文件

### 编译并运行.bat

编译后立即运行（以管理员身份）：

```powershell
.\scripts\编译并运行.bat
```

### 测试通知.bat

测试 Toast 通知功能：

```powershell
.\scripts\测试通知.bat
```

---

## 🔧 配置开机自启动

### 方式一：通过应用设置

1. 启动 StarReminder
2. 点击"设置"按钮
3. 勾选"开机自启动"
4. 程序会自动配置注册表

### 方式二：手动配置

#### 使用任务计划程序（推荐）

1. 打开"任务计划程序"（taskschd.msc）
2. 创建基本任务
   - 名称：StarReminder
   - 触发器：登录时
   - 操作：启动程序
   - 程序：`C:\Program Files\StarReminder\StarReminder.exe`
3. 右键任务 → 属性
4. 勾选"使用最高权限运行"

#### 使用启动文件夹

1. 按 `Win+R` 打开运行对话框
2. 输入 `shell:startup` 并回车
3. 创建快捷方式到该文件夹
4. 右键快捷方式 → 属性 → 高级
5. 勾选"用管理员身份运行"

> [!WARNING]
> 启动文件夹方式可能不会以管理员权限运行，推荐使用任务计划程序。

---

## 🐛 安装故障排除

### 问题 1: 无法启动程序

**错误信息**: "应用程序无法启动，因为应用程序配置不正确"

**解决方案**:
1. 确认已安装 .NET 8.0 Runtime
2. 检查版本：
   ```powershell
   dotnet --version
   ```
3. 如果未安装或版本过低，请下载最新版本

### 问题 2: 媒体设备检测不工作

**现象**: 无法检测到摄像头/麦克风使用

**解决方案**:
1. 确认以管理员权限运行
2. 检查注册表权限：
   ```powershell
   # 以管理员身份运行 PowerShell
   $key = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore"
   Test-Path $key
   ```
3. 如果返回 False，可能需要修改组策略

### 问题 3: Toast 通知不显示

**解决方案**:
1. 检查 Windows 通知设置：
   - 设置 → 系统 → 通知
   - 确保通知已启用
2. 检查 ToastNotifier.exe 是否存在
3. 查看日志文件：`logs/activity_log.json`

### 问题 4: 编译错误

**错误**: "找不到 .NET SDK"

**解决方案**:
```powershell
# 安装 .NET 8.0 SDK
winget install Microsoft.DotNet.SDK.8
```

**错误**: "找不到 Microsoft.Toolkit.Uwp.Notifications"

**解决方案**:
```powershell
# 还原 NuGet 包
dotnet restore
```

### 问题 5: UAC 提示每次都出现

**解决方案**:

创建任务计划程序任务（见上文"配置开机自启动"）可以避免每次都弹出 UAC 提示。

---

## 📝 卸载说明

### 完全卸载

1. 关闭 StarReminder 程序
2. 删除安装目录
3. 删除配置文件（如果需要）：
   ```
   %APPDATA%\StarReminder\
   ```
4. 删除注册表项（如果配置了开机自启动）：
   ```
   HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run\StarReminder
   ```
5. 删除任务计划程序任务（如果有）

### 使用脚本卸载

```powershell
# 停止进程
Stop-Process -Name "StarReminder" -Force -ErrorAction SilentlyContinue

# 删除启动项
Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "StarReminder" -ErrorAction SilentlyContinue

# 删除任务计划
Unregister-ScheduledTask -TaskName "StarReminder" -Confirm:$false -ErrorAction SilentlyContinue

# 删除文件
Remove-Item -Path "C:\Program Files\StarReminder" -Recurse -Force
```

---

## ✅ 安装验证

安装完成后，可以运行以下检查：

### 1. 检查程序版本

启动程序后，在"关于"窗口查看版本信息。

### 2. 测试媒体设备检测

1. 启动任意使用摄像头的程序（如相机应用）
2. 查看 StarReminder 是否检测到
3. 应该会显示通知

### 3. 查看日志

检查 `logs/activity_log.json` 文件，确认日志正常记录：

```json
{
  "timestamp": "2025-11-01T15:30:00",
  "level": "Info",
  "message": "系统启动成功"
}
```

---

## 🔗 相关链接

- [使用教程](usage.md) - 如何使用 StarReminder
- [配置指南](configuration.md) - 详细配置说明
- [开发指南](development.md) - 如何参与开发
- [FAQ](../README.md#-支持) - 常见问题解答

---

<div align="center">
  <sub>如有问题，请提交 <a href="https://github.com/vistaminc/StarReminder/issues">Issue</a></sub>
</div>

