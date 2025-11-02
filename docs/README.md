# 📚 StarReminder 文档中心

<div align="center">
  <a href="../README.md">返回主页</a>
</div>

---

欢迎来到 StarReminder 文档中心！这里包含了所有你需要的文档和指南。

## 📖 用户文档

### 🚀 [安装指南](installation.md)

完整的安装和部署说明，包括：
- 系统要求
- 三种安装方法（预编译版本、源码编译、Visual Studio）
- 开机自启动配置
- 故障排除
- 卸载说明

**适合人群**: 所有用户

---

### ⚙️ [配置指南](configuration.md)

详细的配置参数说明，包括：
- 进程监控配置 (`config.json`)
- 应用程序设置 (`settings.json`)
- 通知类型详解
- 水印位置设置
- 预设配置模板
- 配置验证和热加载

**适合人群**: 需要自定义配置的用户

---

### 📖 [使用教程](usage.md)

如何使用 StarReminder，包括：
- 快速上手指南
- 界面导览
- 通知系统说明
- 实际使用场景（隐私保护、家长控制、企业监控等）
- 常用操作
- 自定义设置
- 故障排除
- 使用技巧和最佳实践

**适合人群**: 所有用户

---

## 🔧 技术文档

### 🏗️ [技术架构](architecture.md)

系统架构和设计理念，包括：
- 架构概览和架构图
- 核心组件详解（Models、Services、ViewModels、Views）
- 核心流程（进程检测流程、通知流程）
- 设计模式（MVVM、观察者、单例、工厂）
- 安全考虑
- 性能优化
- 可测试性和扩展性

**适合人群**: 开发者、架构师

---

### 🛠️ [开发指南](development.md)

如何参与开发和贡献，包括：
- 开发环境搭建
- 项目结构说明
- 编译和调试
- 测试方法（单元测试、集成测试、手动测试）
- 编码规范
- 调试技巧
- 发布流程
- 添加新功能示例
- 贡献流程
- 代码审查清单

**适合人群**: 开发者、贡献者

---

### 📘 [API 文档](api.md)

完整的 API 接口文档，包括：
- 核心服务 API
  - ProcessMonitor
  - MediaDeviceMonitor
  - AlertManager
  - ProcessController
  - ConfigurationManager
  - Logger
- 数据模型
  - ProcessConfig
  - AlertConfig
  - ProcessInfo
  - MediaDeviceUsage
  - LogEntry
  - AppSettings
- 扩展接口
- 使用示例

**适合人群**: 开发者

---

## 🗂️ 文档目录

```
docs/
├── README.md              # 本文档（文档索引）
├── installation.md        # 安装指南
├── configuration.md       # 配置指南
├── usage.md              # 使用教程
├── architecture.md        # 技术架构
├── development.md         # 开发指南
└── api.md                # API 文档
```

---

## 🎯 快速导航

### 我是新用户，如何开始？

1. 阅读 [安装指南](installation.md) 安装程序
2. 阅读 [使用教程](usage.md) 的"快速上手"部分
3. 根据需要查看 [配置指南](configuration.md) 进行自定义

### 我想自定义配置

直接查看 [配置指南](configuration.md)，那里有：
- 完整的配置参数说明
- 预设配置模板
- 配置示例

### 我遇到了问题

1. 查看 [使用教程 - 故障排除](usage.md#-故障排除)
2. 查看 [安装指南 - 安装故障排除](installation.md#-安装故障排除)
3. 查看 GitHub [Issues](https://github.com/vistaminc/StarReminder/issues)
4. 提交新的 Issue

### 我想参与开发

1. 阅读 [技术架构](architecture.md) 了解系统设计
2. 阅读 [开发指南](development.md) 搭建开发环境
3. 查看 [API 文档](api.md) 了解接口
4. 遵循 [贡献流程](development.md#-贡献流程)

### 我想了解实现细节

1. 先阅读 [技术架构](architecture.md) 了解整体设计
2. 查看 [API 文档](api.md) 了解具体接口
3. 阅读源代码中的注释

---

## 📊 文档统计

| 文档 | 字数 | 页数 | 难度 |
|------|------|------|------|
| 安装指南 | ~3000 | ~15 | ⭐ 简单 |
| 配置指南 | ~4000 | ~20 | ⭐⭐ 中等 |
| 使用教程 | ~5000 | ~25 | ⭐ 简单 |
| 技术架构 | ~6000 | ~30 | ⭐⭐⭐ 困难 |
| 开发指南 | ~5000 | ~25 | ⭐⭐ 中等 |
| API 文档 | ~7000 | ~35 | ⭐⭐⭐ 困难 |

**总计**: ~30,000 字，~150 页

---

## 🔍 搜索文档

### 按主题搜索

| 主题 | 相关文档 |
|------|---------|
| **安装部署** | [安装指南](installation.md) |
| **配置设置** | [配置指南](configuration.md)、[使用教程](usage.md) |
| **进程监控** | [使用教程](usage.md)、[API 文档](api.md) |
| **媒体设备检测** | [技术架构](architecture.md)、[API 文档](api.md) |
| **通知系统** | [配置指南](configuration.md)、[使用教程](usage.md) |
| **故障排除** | [安装指南](installation.md)、[使用教程](usage.md) |
| **开发贡献** | [开发指南](development.md)、[API 文档](api.md) |
| **架构设计** | [技术架构](architecture.md) |

---

## 💡 文档改进建议

我们欢迎对文档的改进建议！如果你发现：

- ❌ 错别字或语法错误
- ❌ 不准确或过时的信息
- ❌ 缺失的重要内容
- ❌ 令人困惑的描述

请：
1. 提交 [Issue](https://github.com/vistaminc/StarReminder/issues)
2. 或直接提交 [Pull Request](https://github.com/vistaminc/StarReminder/pulls)

---

## 📝 文档版本

| 版本 | 日期 | 变更说明 |
|------|------|---------|
| v1.0 | 2025-11-01 | 初始版本，包含所有基础文档 |

---

## 🌟 致谢

感谢所有为文档做出贡献的人！

---

## 📧 联系我们

- 📮 GitHub Issues: [提交问题](https://github.com/vistaminc/StarReminder/issues)
- 📘 Wiki: [查看 Wiki](https://github.com/vistaminc/StarReminder/wiki)
- 💬 讨论区: [参与讨论](https://github.com/vistaminc/StarReminder/discussions)

---

<div align="center">
  <sub>文档最后更新: 2025-11-01</sub>
  <br />
  <sub>© 2025 StarReminder Team. All rights reserved.</sub>
</div>

