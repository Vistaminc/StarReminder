<p align="center">
  <img src="/logo.png" width="180" alt="StarReminder Logo" />
</p>

<h1 align="center">StarReminder</h1>
<h4 align="center">智能进程监控与媒体设备检测系统</h4>

<p align="center">
  <a href="https://github.com/vistaminc/StarReminder/releases">
    <img src="https://img.shields.io/badge/version-v1.2.2-blue?style=for-the-badge&logo=semantic-release" alt="Version" />
  </a>
  <a href="LICENSE">
    <img src="https://img.shields.io/badge/license-MIT-green?style=for-the-badge&logo=open-source-initiative&logoColor=white" alt="License" />
  </a>
  <a href="https://dotnet.microsoft.com/download/dotnet/8.0">
    <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8.0" />
  </a>
  <a href="https://github.com/vistaminc/StarReminder/releases">
    <img src="https://img.shields.io/badge/platform-Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white" alt="Platform" />
  </a>
</p>

<p align="center">
  <a href="https://github.com/vistaminc/StarReminder/stargazers">
    <img src="https://img.shields.io/github/stars/vistaminc/StarReminder?style=social" alt="Stars" />
  </a>
  <a href="https://github.com/vistaminc/StarReminder/network/members">
    <img src="https://img.shields.io/github/forks/vistaminc/StarReminder?style=social" alt="Forks" />
  </a>
  <a href="https://github.com/vistaminc/StarReminder/issues">
    <img src="https://img.shields.io/github/issues/vistaminc/StarReminder?style=social&logo=github" alt="Issues" />
  </a>
</p>

<p align="center">
  <a href="#-概览">概览</a> · 
  <a href="#-功能">功能</a> · 
  <a href="#-快速开始">快速开始</a> · 
  <a href="#-文档">文档</a> · 
  <a href="https://github.com/vistaminc/StarReminder/issues">反馈</a> · 
  <a href="CHANGELOG.md">更新日志</a>
</p>

<br />

> [!TIP]
> 📖 **5分钟快速上手**: [快速开始指南](QUICKSTART.md)  
> 📚 **详细教程**: [完整安装指南](docs/installation.md)

> [!IMPORTANT]
> 当前版本: **v1.2.2**  
> 支持系统: Windows 10 1809+ / Windows 11  
> 需要权限: 管理员权限（用于检测媒体设备）  
> **🚀 性能大幅提升！CPU 降低 82%，界面完全流畅！**

> [!WARNING]
> 本项目仅供学习和个人使用，请勿用于非法用途。使用本软件需遵守当地法律法规。

## ✨ 概览

<table>
<tr>
<td>

StarReminder 是一款强大的**进程监控与媒体设备检测系统**，能够：

- 🎥 **实时监控** 摄像头和麦克风的使用情况
- 📱 **智能检测** 目标进程的启动和运行状态
- 🔔 **多种通知** 包括 Windows Defender 风格通知
- ⚡ **自动控制** 支持进程挂起、恢复和终止
- 📊 **日志记录** 追踪所有监控活动

</td>
<td width="400">

```
┌────────────────────────┐
│   📹 媒体捕获          │
│   状态: 运行中         │
│   时间: 00:05:23       │
│   [挂起] [终止]        │
├────────────────────────┤
│   🖥️ 屏幕捕获         │
│   状态: 未检测到       │
│   时间: --:--:--       │
│   [配置]               │
├────────────────────────┤
│   🎮 远程桌面          │
│   状态: 未检测到       │
│   时间: --:--:--       │
│   [配置]               │
└────────────────────────┘
```

</td>
</tr>
</table>

### 为什么选择 StarReminder？

当你的被老师视奸的时候，你希望能第一时间知道并控制它们。

StarReminder 不仅能检测进程启动，更重要的是：**从设备层面检测摄像头/麦克风的实际使用情况，并挂起或关闭**，让你不再被抓。

## 💻 功能

### 🎯 核心功能

- [x] **进程监控**
  - 实时监控指定进程的启动和退出
  - 自动记录进程运行时间
  - 支持自定义监控进程列表
  
- [x] **媒体设备检测**
  - 摄像头使用实时检测（从设备角度检测，更准确）
  - 麦克风使用实时检测
  - 多种检测方法（注册表 + 句柄检测）
  
- [x] **智能通知系统**
  - **Defender 样式通知**（模仿 Windows 安全中心）
  - Native Toast 通知
  - 持续提醒水印（类似激活水印）
  - 可自定义通知内容和样式
  
- [x] **进程控制**
  - 进程挂起（Suspend）
  - 进程恢复（Resume）
  - 进程终止（Kill）
  - 运行时间限制与自动操作
  
- [x] **配置管理**
  - JSON 配置文件
  - 进程级别细粒度配置
  - 图形化设置界面
  - 配置热加载

- [x] **日志系统**
  - 详细的活动日志
  - 自动日志清理（30天）
  - 日志查看和筛选
  - JSON 格式持久化

### 🎨 界面特性

- **现代化 WPF 界面**：流畅的动画效果和响应式布局
- **卡片式设计**：清晰展示每个监控进程的状态
- **实时状态更新**：进程运行时间实时显示
- **深色/浅色主题**：支持主题切换
- **系统托盘集成**：最小化到系统托盘（计划中）

## 🚀 快速开始

### 前置要求

- Windows 10 1809+ 或 Windows 11
- .NET 8.0 SDK（开发）或 .NET 8.0 Runtime（运行）
- 管理员权限（用于检测媒体设备）

### 快速安装

#### 方式一：下载发布版本（推荐）

1. 从 [Releases](https://github.com/vistaminc/StarReminder/releases) 下载最新版本
2. 解压到任意目录
3. **以管理员身份运行** `StarReminder.exe`

#### 方式二：从源码编译

```powershell
# 1. 克隆仓库
git clone https://github.com/vistaminc/StarReminder.git
cd StarReminder

# 2. 进入项目目录
cd "C#/MediaDetectionSystem"

# 3. 编译项目
dotnet build -c Release

# 4. 运行程序（需要管理员权限）
.\bin\Release\net8.0-windows10.0.17763.0\StarReminder.exe
```

或使用便捷脚本：

```powershell
.\scripts\发布Release版本.bat
```

### 首次运行

1. **以管理员身份**启动 `StarReminder.exe`
2. 主界面会显示三个默认监控项
3. 点击右上角的开关启用监控
4. 点击"设置"按钮配置监控参数

> [!TIP]
> 为什么需要管理员权限？因为需要访问系统注册表来检测摄像头/麦克风的使用情况。

## 📖 配置指南

### 配置文件位置

- `config.json` - 进程配置
- `settings.json` - 应用设置

### 进程配置示例

```json
{
  "Processes": [
    {
      "ProcessName": "media_capture.exe",
      "DisplayName": "媒体捕获",
      "IsEnabled": true,
      "MaxRuntime": "00:10:00",
      "ActionType": "suspend",
      "AlertBeforeAction": 30,
      "Alert": {
        "EnableStartupNotification": true,
        "NotificationType": "Defender",
        "NotificationTitle": "检测到媒体捕获程序",
        "NotificationMessage": "正在使用摄像头或麦克风",
        "EnableContinuousAlert": true,
        "WatermarkText1": "摄像头/麦克风",
        "WatermarkText2": "正在使用中",
        "WatermarkPosition": "BottomRight"
      }
    }
  ]
}
```

### 通知类型

| 类型 | 描述 | 效果 |
|------|------|------|
| `Defender` | Windows 安全中心样式 | 显示为"Windows 安全中心"，使用盾牌图标 |
| `Native` | 原生 Toast 通知 | 使用程序自己的身份，更稳定 |

### 水印位置

- `TopLeft` - 左上角
- `TopRight` - 右上角
- `BottomLeft` - 左下角
- `BottomRight` - 右下角（推荐）

详细配置说明请查看：[配置文档](docs/configuration.md)

## 📚 文档

### 用户文档

| 文档 | 说明 |
|------|------|
| [安装指南](docs/installation.md) | 详细的安装和部署说明 |
| [配置指南](docs/configuration.md) | 完整的配置参数说明 |
| [使用教程](docs/usage.md) | 使用方法和最佳实践 |

### 技术文档

| 文档 | 说明 |
|------|------|
| [技术架构](docs/architecture.md) | 系统架构和设计理念 |
| [开发指南](docs/development.md) | 开发环境搭建和贡献指南 |
| [API 文档](docs/api.md) | 核心 API 和服务说明 |

## 🛠️ 技术栈

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt=".NET 8.0" />
  <img src="https://img.shields.io/badge/WPF-XAML-0078D6?style=flat-square&logo=windows&logoColor=white" alt="WPF" />
  <img src="https://img.shields.io/badge/C%23-11.0-239120?style=flat-square&logo=c-sharp&logoColor=white" alt="C# 11" />
  <img src="https://img.shields.io/badge/MVVM-Architecture-00ADD8?style=flat-square" alt="MVVM" />
  <img src="https://img.shields.io/badge/Windows-API-0078D6?style=flat-square&logo=windows&logoColor=white" alt="Windows API" />
</p>

| 组件 | 技术 | 用途 |
|:----:|:----:|:----:|
| **框架** | .NET 8.0 + WPF | 应用程序基础框架 |
| **架构** | MVVM 模式 | 代码组织和数据绑定 |
| **UI** | XAML + 自定义主题 | 用户界面 |
| **通知** | Microsoft.Toolkit.Uwp.Notifications | Toast 通知 |
| **配置** | System.Text.Json | JSON 配置管理 |
| **进程控制** | Windows Native API (P/Invoke) | 进程挂起/恢复/终止 |
| **媒体检测** | Registry API + Module Analysis | 摄像头/麦克风检测 |

## 📊 项目结构

<details>
<summary><b>点击展开查看详细结构</b></summary>

```
StarReminder/
├── 📁 C#/
│   └── MediaDetectionSystem/
│       ├── 📂 Models/              # 数据模型
│       │   ├── ProcessConfig.cs
│       │   ├── AlertConfig.cs
│       │   ├── AppSettings.cs
│       │   └── ...
│       ├── 📂 Services/            # 核心服务
│       │   ├── ProcessMonitor.cs
│       │   ├── MediaDeviceMonitor.cs
│       │   ├── AlertManager.cs
│       │   ├── ProcessController.cs
│       │   └── ...
│       ├── 📂 ViewModels/          # 视图模型
│       │   └── MainViewModel.cs
│       ├── 📂 Views/               # 视图界面
│       │   ├── ConfigWindow.xaml
│       │   ├── SettingsWindow.xaml
│       │   └── ...
│       ├── 📂 Themes/              # 主题样式
│       │   ├── LightTheme.xaml
│       │   └── DarkTheme.xaml
│       ├── 📂 ToastNotifier/       # Toast 通知子程序
│       └── 📂 scripts/             # 编译脚本
├── 📁 docs/                        # 技术文档
│   ├── 📄 installation.md          # 安装指南
│   ├── 📄 configuration.md         # 配置指南
│   ├── 📄 usage.md                 # 使用教程
│   ├── 📄 architecture.md          # 技术架构
│   ├── 📄 development.md           # 开发指南
│   └── 📄 api.md                   # API 文档
├── 📄 README.md                    # 本文档
├── 📄 LICENSE                      # MIT 许可证
├── 📄 CHANGELOG.md                 # 更新日志
├── 📄 CONTRIBUTING.md              # 贡献指南
└── 📄 QUICKSTART.md                # 快速开始
```

</details>


## 🔮 后续计划

<table>
<tr>
<td width="33%">

### v1.3.0 (2025-12)
- [ ] 系统托盘图标
- [ ] 进程白名单/黑名单
- [ ] 配置导入/导出
- [ ] 多主题支持

</td>
<td width="33%">

### v1.4.0 (2026-01)
- [ ] 网络流量监控
- [ ] 远程管理 API
- [ ] 数据统计报表
- [ ] 邮件通知

</td>
<td width="34%">

### v2.0.0 (2026-03)
- [ ] 插件系统
- [ ] 多语言支持
- [ ] 云同步配置
- [ ] 机器学习检测

</td>
</tr>
</table>

> 查看完整路线图：[CHANGELOG.md](CHANGELOG.md#路线图)

## 🤝 贡献

欢迎贡献代码！请查看 [贡献指南](CONTRIBUTING.md) 和 [开发指南](docs/development.md)。

### 如何贡献

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 提交 Pull Request

详细贡献流程请参考 [CONTRIBUTING.md](CONTRIBUTING.md)

## 📜 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

> [!WARNING]
> 本项目仅供学习和研究使用，请勿用于非法目的。使用者需自行承担使用本软件的风险。

## 💬 支持与反馈

<table>
<tr>
<td align="center">

### 📮 提交 Issue
遇到 Bug 或有建议？

[提交 Issue →](https://github.com/vistaminc/StarReminder/issues)

</td>
<td align="center">

### 💬 参与讨论
有问题想交流？

[讨论区 →](https://github.com/vistaminc/StarReminder/discussions)

</td>
<td align="center">

### 📚 查看文档
寻找使用帮助？

[完整文档 →](docs/README.md)

</td>
<td align="center">

### ⭐ Star 项目
喜欢这个项目？

[给个 Star →](https://github.com/vistaminc/StarReminder)

</td>
</tr>
</table>

---

## 🌟 Star 历史

<a href="https://star-history.com/#vistaminc/StarReminder&Date">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/svg?repos=vistaminc/StarReminder&type=Date&theme=dark" />
    <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/svg?repos=vistaminc/StarReminder&type=Date" />
    <img alt="Star History Chart" src="https://api.star-history.com/svg?repos=vistaminc/StarReminder&type=Date" />
  </picture>
</a>

## 💖 致谢

感谢所有为项目做出贡献的人！

<a href="https://github.com/vistaminc/StarReminder/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=vistaminc/StarReminder" />
</a>

特别感谢：
- 所有使用者的支持与反馈
- 开源社区的帮助与启发

---

## 📧 联系方式

- 📮 提交 [Issue](https://github.com/vistaminc/StarReminder/issues)
- 💬 参与 [讨论](https://github.com/vistaminc/StarReminder/discussions)
- 📧 邮箱：vistamin@outlook.com

---

<p align="center">
  <img src="https://img.shields.io/badge/Made%20with-❤️-red?style=for-the-badge" alt="Made with Love" />
  <img src="https://img.shields.io/badge/Built%20with-.NET%208.0-512BD4?style=for-the-badge&logo=dotnet" alt="Built with .NET" />
  <img src="https://img.shields.io/badge/Powered%20by-WPF-0078D6?style=for-the-badge&logo=windows" alt="Powered by WPF" />
</p>

<p align="center">
  <sub>© 2025 StarReminder Team. All rights reserved.</sub>
  <br />
  <sub>Licensed under <a href="LICENSE">MIT License</a></sub>
</p>

