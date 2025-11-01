# ⚙️ 配置指南

<div align="center">
  <a href="../README.md">返回首页</a> · <a href="installation.md">安装指南</a> · <a href="usage.md">使用教程</a>
</div>

---

## 📁 配置文件概览

StarReminder 使用两个主要的配置文件：

| 文件 | 用途 | 位置 |
|------|------|------|
| `config.json` | 进程监控配置 | 程序根目录 |
| `settings.json` | 应用程序设置 | 程序根目录 |

> [!TIP]
> 配置文件采用 JSON 格式，支持热加载。修改配置后保存，程序会自动重新加载。

---

## 🎯 进程监控配置 (config.json)

### 完整示例

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
        "WatermarkPosition": "BottomRight",
        "WatermarkOpacity": 0.8,
        "WatermarkColor": "#FF0000"
      }
    },
    {
      "ProcessName": "screenCapture.exe",
      "DisplayName": "屏幕捕获",
      "IsEnabled": true,
      "MaxRuntime": "00:05:00",
      "ActionType": "kill",
      "AlertBeforeAction": 60,
      "Alert": {
        "EnableStartupNotification": true,
        "NotificationType": "Native",
        "NotificationTitle": "检测到屏幕捕获程序",
        "NotificationMessage": "正在录制屏幕内容",
        "EnableContinuousAlert": true,
        "WatermarkText1": "屏幕录制中",
        "WatermarkText2": "",
        "WatermarkPosition": "TopRight",
        "WatermarkOpacity": 1.0,
        "WatermarkColor": "#FFA500"
      }
    },
    {
      "ProcessName": "rtcRemoteDesktop.exe",
      "DisplayName": "远程桌面",
      "IsEnabled": true,
      "MaxRuntime": "00:15:00",
      "ActionType": "suspend",
      "AlertBeforeAction": 120,
      "Alert": {
        "EnableStartupNotification": true,
        "NotificationType": "Defender",
        "NotificationTitle": "检测到远程控制程序",
        "NotificationMessage": "有人正在远程控制您的电脑",
        "EnableContinuousAlert": true,
        "WatermarkText1": "远程控制中",
        "WatermarkText2": "请注意安全",
        "WatermarkPosition": "BottomRight",
        "WatermarkOpacity": 0.9,
        "WatermarkColor": "#DC143C"
      }
    }
  ]
}
```

### 进程配置参数详解

#### 基本配置

| 参数 | 类型 | 必填 | 说明 | 示例 |
|------|------|------|------|------|
| `ProcessName` | string | ✅ | 进程可执行文件名 | `"media_capture.exe"` |
| `DisplayName` | string | ✅ | 显示名称（中文） | `"媒体捕获"` |
| `IsEnabled` | boolean | ✅ | 是否启用监控 | `true` |

#### 运行时控制

| 参数 | 类型 | 必填 | 说明 | 示例 |
|------|------|------|------|------|
| `MaxRuntime` | string | ❌ | 最大运行时间（HH:mm:ss） | `"00:10:00"` |
| `ActionType` | string | ❌ | 超时后的操作 | `"suspend"` / `"kill"` / `"none"` |
| `AlertBeforeAction` | int | ❌ | 操作前提前警告（秒） | `30` |

**ActionType 说明**:
- `suspend`: 挂起进程（推荐，可恢复）
- `kill`: 终止进程
- `none`: 仅通知，不操作

#### Alert 通知配置

| 参数 | 类型 | 必填 | 说明 | 默认值 |
|------|------|------|------|--------|
| `EnableStartupNotification` | boolean | ✅ | 启动时弹出通知 | `true` |
| `NotificationType` | string | ✅ | 通知类型 | `"Defender"` |
| `NotificationTitle` | string | ✅ | 通知标题 | - |
| `NotificationMessage` | string | ✅ | 通知消息 | - |
| `EnableContinuousAlert` | boolean | ❌ | 启用持续水印提醒 | `false` |
| `WatermarkText1` | string | ❌ | 水印第一行文字 | - |
| `WatermarkText2` | string | ❌ | 水印第二行文字 | - |
| `WatermarkPosition` | string | ❌ | 水印位置 | `"BottomRight"` |
| `WatermarkOpacity` | double | ❌ | 水印透明度 (0.0-1.0) | `0.8` |
| `WatermarkColor` | string | ❌ | 水印颜色（十六进制） | `"#FF0000"` |

### 通知类型详解

#### Defender 样式（推荐）

模仿 Windows 安全中心的通知样式：

```json
{
  "NotificationType": "Defender",
  "NotificationTitle": "检测到媒体捕获程序",
  "NotificationMessage": "正在使用摄像头或麦克风"
}
```

**特点**:
- 显示为"Windows 安全中心"
- 使用 Defender 盾牌图标
- 权威感强，用户更重视
- 需要 ToastNotifier.exe 子程序

#### Native 样式

使用程序自己的身份发送通知：

```json
{
  "NotificationType": "Native",
  "NotificationTitle": "StarReminder 提醒",
  "NotificationMessage": "检测到目标进程启动"
}
```

**特点**:
- 显示为"StarReminder"
- 使用程序自己的图标
- 更稳定可靠
- 无需子程序

### 水印位置

水印会在进程运行期间持续显示在屏幕上：

| 值 | 位置 | 示例场景 |
|------|------|----------|
| `TopLeft` | 左上角 | 不遮挡主要内容 |
| `TopRight` | 右上角 | 提醒明显 |
| `BottomLeft` | 左下角 | 不影响任务栏 |
| `BottomRight` | 右下角 | 推荐，类似激活水印 |

### 预设配置模板

#### 严格模式

快速发现并终止所有可疑进程：

```json
{
  "ProcessName": "suspicious.exe",
  "DisplayName": "可疑进程",
  "IsEnabled": true,
  "MaxRuntime": "00:00:30",
  "ActionType": "kill",
  "AlertBeforeAction": 5,
  "Alert": {
    "EnableStartupNotification": true,
    "NotificationType": "Defender",
    "NotificationTitle": "⚠️ 安全警告",
    "NotificationMessage": "检测到可疑进程，即将终止",
    "EnableContinuousAlert": true,
    "WatermarkText1": "⚠️ 警告",
    "WatermarkText2": "检测到可疑活动",
    "WatermarkPosition": "TopRight",
    "WatermarkOpacity": 1.0,
    "WatermarkColor": "#FF0000"
  }
}
```

#### 监控模式

仅通知，不干预进程：

```json
{
  "ProcessName": "monitored.exe",
  "DisplayName": "监控进程",
  "IsEnabled": true,
  "MaxRuntime": "00:00:00",
  "ActionType": "none",
  "AlertBeforeAction": 0,
  "Alert": {
    "EnableStartupNotification": true,
    "NotificationType": "Native",
    "NotificationTitle": "进程启动通知",
    "NotificationMessage": "检测到进程启动",
    "EnableContinuousAlert": false
  }
}
```

#### 家长控制模式

限制使用时间，温和提醒：

```json
{
  "ProcessName": "game.exe",
  "DisplayName": "游戏程序",
  "IsEnabled": true,
  "MaxRuntime": "01:00:00",
  "ActionType": "suspend",
  "AlertBeforeAction": 300,
  "Alert": {
    "EnableStartupNotification": true,
    "NotificationType": "Native",
    "NotificationTitle": "时间提醒",
    "NotificationMessage": "游戏已运行一段时间，注意休息",
    "EnableContinuousAlert": true,
    "WatermarkText1": "已游戏 {runtime}",
    "WatermarkText2": "注意休息哦~",
    "WatermarkPosition": "TopRight",
    "WatermarkOpacity": 0.6,
    "WatermarkColor": "#4169E1"
  }
}
```

---

## 🛠️ 应用程序设置 (settings.json)

### 完整示例

```json
{
  "StartWithWindows": true,
  "StartMinimized": false,
  "ThemeMode": "Light",
  "EnableTrayNotifications": true,
  "ShowStartupDialog": true,
  "MonitorInterval": 1,
  "EnableAutoAction": true,
  "EnableDetailedLogging": true,
  "LogRetentionDays": 30,
  "AutoCleanLogs": true,
  "CheckForUpdates": true,
  "EnableAnalytics": false,
  "EnablePasswordProtection": false,
  "PasswordHash": ""
}
```

### 参数详解

#### 启动设置

| 参数 | 类型 | 说明 | 推荐值 |
|------|------|------|--------|
| `StartWithWindows` | boolean | 开机自启动 | `true` |
| `StartMinimized` | boolean | 启动时最小化到托盘 | `false` |

#### 界面设置

| 参数 | 类型 | 说明 | 可选值 |
|------|------|------|--------|
| `ThemeMode` | string | 主题模式 | `"Light"` / `"Dark"` / `"Auto"` |

#### 通知设置

| 参数 | 类型 | 说明 | 推荐值 |
|------|------|------|--------|
| `EnableTrayNotifications` | boolean | 启用系统托盘通知 | `true` |
| `ShowStartupDialog` | boolean | 进程启动时弹窗提醒 | `true` |

#### 监控设置

| 参数 | 类型 | 说明 | 推荐值 |
|------|------|------|--------|
| `MonitorInterval` | int | 监控检测间隔（秒） | `1` |
| `EnableAutoAction` | boolean | 启用自动操作（挂起/终止） | `true` |
| `EnableDetailedLogging` | boolean | 记录详细日志 | `true` |

> [!WARNING]
> `MonitorInterval` 设置过小可能增加 CPU 占用，建议保持 1-2 秒。

#### 高级设置

| 参数 | 类型 | 说明 | 推荐值 |
|------|------|------|--------|
| `LogRetentionDays` | int | 日志保留天数 | `30` |
| `AutoCleanLogs` | boolean | 自动清理过期日志 | `true` |
| `CheckForUpdates` | boolean | 检查更新 | `true` |
| `EnableAnalytics` | boolean | 数据统计（计划功能） | `false` |

#### 安全设置

| 参数 | 类型 | 说明 |
|------|------|------|
| `EnablePasswordProtection` | boolean | 启用操作密码保护 |
| `PasswordHash` | string | 密码哈希值（自动生成） |

---

## 📝 配置最佳实践

### 1. 性能优化

```json
{
  "MonitorInterval": 2,
  "EnableDetailedLogging": false,
  "AutoCleanLogs": true,
  "LogRetentionDays": 7
}
```

### 2. 安全优先

```json
{
  "EnablePasswordProtection": true,
  "EnableAutoAction": true,
  "ShowStartupDialog": true,
  "EnableTrayNotifications": true
}
```

### 3. 隐蔽监控

```json
{
  "StartMinimized": true,
  "EnableTrayNotifications": false,
  "ShowStartupDialog": false,
  "EnableDetailedLogging": true
}
```

---

## 🔄 配置热加载

StarReminder 支持热加载配置文件，无需重启程序：

1. 修改 `config.json` 或 `settings.json`
2. 保存文件
3. 程序会在 1-2 秒内自动重新加载
4. 查看日志确认：`已重新加载配置`

> [!TIP]
> 如果修改后没有生效，可以尝试重启程序。

---

## 🧪 配置验证

### 使用 JSON 验证器

在修改配置前，可以使用在线工具验证 JSON 格式：
- https://jsonlint.com/
- https://jsonformatter.org/

### 常见配置错误

#### 错误 1: 时间格式错误

```json
❌ "MaxRuntime": "10:00"
✅ "MaxRuntime": "00:10:00"
```

#### 错误 2: 布尔值使用字符串

```json
❌ "IsEnabled": "true"
✅ "IsEnabled": true
```

#### 错误 3: 缺少逗号

```json
❌ {
  "ProcessName": "test.exe"
  "DisplayName": "测试"
}

✅ {
  "ProcessName": "test.exe",
  "DisplayName": "测试"
}
```

#### 错误 4: 颜色格式错误

```json
❌ "WatermarkColor": "red"
✅ "WatermarkColor": "#FF0000"
```

---

## 📚 配置示例库

### 监控常见通讯软件

```json
{
  "Processes": [
    {
      "ProcessName": "WeChat.exe",
      "DisplayName": "微信",
      "IsEnabled": true,
      "MaxRuntime": "00:00:00",
      "ActionType": "none",
      "Alert": {
        "EnableStartupNotification": true,
        "NotificationType": "Native",
        "NotificationTitle": "微信正在使用摄像头/麦克风",
        "NotificationMessage": "请确认是否为您的操作"
      }
    },
    {
      "ProcessName": "QQ.exe",
      "DisplayName": "QQ",
      "IsEnabled": true,
      "MaxRuntime": "00:00:00",
      "ActionType": "none",
      "Alert": {
        "EnableStartupNotification": true,
        "NotificationType": "Native",
        "NotificationTitle": "QQ 正在使用摄像头/麦克风",
        "NotificationMessage": "请确认是否为您的操作"
      }
    }
  ]
}
```

### 监控浏览器

```json
{
  "ProcessName": "chrome.exe",
  "DisplayName": "Chrome 浏览器",
  "IsEnabled": true,
  "MaxRuntime": "00:00:00",
  "ActionType": "none",
  "Alert": {
    "EnableStartupNotification": true,
    "NotificationType": "Defender",
    "NotificationTitle": "浏览器正在使用摄像头/麦克风",
    "NotificationMessage": "请检查是否有网页正在录音或录像"
  }
}
```

---

## 🔗 相关链接

- [使用教程](usage.md) - 如何使用配置
- [API 文档](api.md) - 配置文件结构定义
- [开发指南](development.md) - 如何扩展配置

---

<div align="center">
  <sub>如有配置问题，请提交 <a href="https://github.com/yourusername/StarReminder/issues">Issue</a></sub>
</div>

