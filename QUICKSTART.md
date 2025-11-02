# ⚡ 快速开始

<div align="center">
  <img src="https://img.shields.io/badge/时间-5分钟-blue?style=for-the-badge" alt="5分钟"/>
  <img src="https://img.shields.io/badge/难度-简单-green?style=for-the-badge" alt="简单"/>
</div>

<br />

欢迎使用 StarReminder！本指南将在 **5 分钟内**帮你完成安装和基本使用。

<div align="center">
  <a href="README.md">详细文档</a> · <a href="docs/installation.md">完整安装指南</a> · <a href="docs/usage.md">使用教程</a>
</div>

---

## 📥 第 1 步：下载程序（1 分钟）

### 选项 A: 下载预编译版本（推荐）

1. 访问 [Releases 页面](https://github.com/vistaminc/StarReminder/releases/latest)
2. 下载 `StarReminder-v1.2.0-win-x64-standalone.zip`
3. 解压到任意目录（如 `C:\Program Files\StarReminder\`）

### 选项 B: 从源码编译

```powershell
git clone https://github.com/vistaminc/StarReminder.git
cd StarReminder
cd "C#/MediaDetectionSystem"
.\scripts\发布Release版本.bat
```

---

## 🚀 第 2 步：首次运行（1 分钟）

1. 找到 `StarReminder.exe`
2. **右键** → **以管理员身份运行**

> ⚠️ **重要**: 必须以管理员权限运行，才能检测摄像头/麦克风使用情况

### 可能遇到的提示

#### Windows Defender SmartScreen

如果出现此提示：

```
Windows 已保护你的电脑
```

点击 **"更多信息"** → **"仍要运行"**

#### 用户账户控制 (UAC)

点击 **"是"** 允许程序运行

---

## 🎯 第 3 步：启用监控（30 秒）

程序启动后：

1. 主界面显示三个监控项：
   - 📹 媒体捕获（摄像头/麦克风）
   - 🖥️ 屏幕捕获
   - 🎮 远程桌面

2. 点击右上角的 **开关** 启用监控

   ```
   [🔄] 监控: 关  →  [✅] 监控: 开
   ```

3. 完成！现在程序正在监控中

---

## 🧪 第 4 步：测试功能（2 分钟）

### 测试摄像头检测

1. 打开 **相机应用**（Win 键搜索"相机"）
2. 应该会弹出 **通知**：

   ```
   🛡️ Windows 安全中心
   ━━━━━━━━━━━━━━━━━━━━━━━━
   检测到媒体捕获程序
   正在使用摄像头或麦克风
   ```

3. 右下角会显示 **水印**：
   ```
   ┌─────────────────┐
   │ 摄像头/麦克风   │
   │ 正在使用中      │
   └─────────────────┘
   ```

4. 关闭相机应用，水印自动消失

### 测试通知

运行测试脚本：

```powershell
.\scripts\测试通知.bat
```

---

## ⚙️ 第 5 步：基本配置（1 分钟）

### 调整运行时间限制

1. 点击进程卡片上的 **"配置"** 按钮
2. 设置 **"最大运行时间"**（如 10 分钟）
3. 选择 **"超时操作"**：
   - **挂起**: 暂停进程（推荐）
   - **终止**: 强制关闭
   - **无操作**: 仅通知
4. 点击 **"保存"**

### 自定义通知

编辑 `config.json`：

```json
{
  "Alert": {
    "NotificationType": "Defender",
    "NotificationTitle": "自定义标题",
    "NotificationMessage": "自定义消息",
    "WatermarkText1": "第一行文字",
    "WatermarkText2": "第二行文字"
  }
}
```

---

## 📝 常用操作

### 查看日志

- 底部窗口显示实时日志
- 完整日志：`logs/activity_log.json`

### 手动控制进程

右键进程卡片：
- **挂起进程**: 暂停运行
- **恢复进程**: 继续运行
- **终止进程**: 强制关闭

### 暂停监控

点击右上角开关即可暂停/恢复监控

---

## 🎓 下一步

### 进阶功能

| 功能 | 文档 |
|------|------|
| 自定义监控进程 | [配置指南](docs/configuration.md) |
| 设置开机自启动 | [安装指南](docs/installation.md#-配置开机自启动) |
| 主题切换 | [使用教程](docs/usage.md#主题切换) |
| 查看所有功能 | [完整文档](README.md) |

## ❓ 常见问题

### 问：为什么需要管理员权限？

**答**: 需要访问系统注册表来检测摄像头/麦克风使用情况。

### 问：检测不到摄像头使用？

**答**: 
1. 确认以管理员身份运行
2. 检查 Windows 隐私设置（设置 → 隐私 → 摄像头）
3. 查看日志文件排查问题

### 问：通知不显示？

**答**:
1. 检查 Windows 通知设置（设置 → 系统 → 通知）
2. 确认 `ToastNotifier.exe` 文件存在
3. 查看日志文件

### 问：如何卸载？

**答**:
1. 关闭程序
2. 删除安装目录
3. 如设置了开机自启动，删除相关注册表项

详细请查看 [安装指南 - 卸载说明](docs/installation.md#-卸载说明)

---

## 🆘 需要帮助？

- 📖 查看 [完整文档](README.md)
- 💬 访问 [讨论区](https://github.com/vistaminc/StarReminder/discussions)
- 🐛 提交 [Issue](https://github.com/vistaminc/StarReminder/issues)
- 📧 联系我们: your-email@example.com

---

## 🎉 开始使用

恭喜！你已经完成了基本设置。现在 StarReminder 将帮助你：

- ✅ 实时监控摄像头和麦克风使用
- ✅ 检测屏幕录制和远程控制
- ✅ 保护你的隐私安全
- ✅ 记录详细的活动日志

**享受使用 StarReminder 吧！** 🌟

---

<div align="center">
  <sub>5 分钟快速开始 | 更多功能请查看<a href="README.md">完整文档</a></sub>
  <br />
  <sub>© 2025 StarReminder Team</sub>
</div>

