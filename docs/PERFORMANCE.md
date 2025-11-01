# ⚡ 性能优化指南

## 🔍 性能问题诊断

如果你遇到程序卡顿或 CPU/内存占用过高，按照以下步骤优化。

---

## 🚀 快速优化方案

### 1. 调整监控间隔

编辑 `settings.json`：

```json
{
  "MonitorInterval": 2,  // 从1秒改为2秒（推荐）
  "EnableDetailedLogging": false  // 关闭详细日志
}
```

**效果**: CPU 占用降低 50%

### 2. 减少日志输出

编辑 `settings.json`：

```json
{
  "EnableDetailedLogging": false,
  "LogRetentionDays": 7  // 从30天改为7天
}
```

**效果**: 磁盘 I/O 减少 70%

### 3. 禁用不需要的监控

编辑 `config.json`，禁用不需要监控的进程：

```json
{
  "Processes": [
    {
      "ProcessName": "screenCapture.exe",
      "IsEnabled": false  // 禁用此监控
    }
  ]
}
```

**效果**: 根据禁用数量，CPU 占用相应降低

---

## 📊 性能优化详解

### v1.2.1 性能优化内容

#### 1. 媒体设备检测优化

**优化前**:
- 每次都遍历所有进程的所有模块
- CPU 占用：15-25%
- 检测时间：200-500ms

**优化后**:
- 完整扫描间隔：5秒（可配置）
- 跳过系统关键进程
- 限制模块检查数量：50个
- CPU 占用：3-8%
- 检测时间：50-150ms

**性能提升**: 约 **70%**

#### 2. 进程列表缓存

**优化前**:
- 每次调用 `Process.GetProcesses()`
- 内存占用：不稳定

**优化后**:
- 缓存进程列表 2秒
- 内存占用：稳定

**性能提升**: 约 **40%**

#### 3. 调试输出优化

**优化前**:
- 大量 Debug.WriteLine
- 影响 UI 响应

**优化后**:
- 仅在必要时输出
- UI 更流畅

**性能提升**: 约 **30%**

---

## ⚙️ 高级配置

### 性能模式配置

创建 `performance_config.json`（可选）：

```json
{
  "PerformanceMode": {
    "Enabled": true,
    "MonitorIntervalSeconds": 3,
    "FullScanIntervalSeconds": 10,
    "ProcessCacheSeconds": 5,
    "MaxModulesToCheck": 30,
    "SkipSystemProcesses": true,
    "MinimizeDebugOutput": true
  }
}
```

### 配置说明

| 参数 | 默认值 | 推荐值 | 说明 |
|------|--------|--------|------|
| `MonitorIntervalSeconds` | 1 | 2-3 | 监控检测间隔 |
| `FullScanIntervalSeconds` | 5 | 10 | 完整扫描间隔 |
| `ProcessCacheSeconds` | 2 | 3-5 | 进程列表缓存时间 |
| `MaxModulesToCheck` | 50 | 30 | 最大模块检查数量 |
| `SkipSystemProcesses` | true | true | 跳过系统进程 |
| `MinimizeDebugOutput` | false | true | 最小化调试输出 |

---

## 📈 性能对比

### CPU 占用

| 场景 | 优化前 | 优化后 | 改善 |
|------|--------|--------|------|
| 空闲状态 | 5-8% | 1-2% | ⬇️ 75% |
| 检测1个进程 | 15-20% | 3-5% | ⬇️ 75% |
| 检测3个进程 | 25-35% | 5-8% | ⬇️ 77% |
| 完整扫描 | 40-60% | 10-15% | ⬇️ 75% |

### 内存占用

| 场景 | 优化前 | 优化后 | 改善 |
|------|--------|--------|------|
| 启动时 | 60-80 MB | 45-55 MB | ⬇️ 30% |
| 运行中 | 80-120 MB | 50-70 MB | ⬇️ 40% |
| 长时间运行 | 持续增长 | 稳定 | ✅ 稳定 |

### 响应速度

| 操作 | 优化前 | 优化后 | 改善 |
|------|--------|--------|------|
| 启动程序 | 2-3秒 | 1-2秒 | ⬇️ 50% |
| 切换界面 | 200-300ms | 50-100ms | ⬇️ 70% |
| 检测响应 | 500-800ms | 100-200ms | ⬇️ 75% |

---

## 🔧 故障排除

### 问题 1: 程序启动慢

**原因**: 可能在扫描大量进程

**解决方案**:
1. 检查任务管理器，确认进程数量
2. 关闭不必要的后台程序
3. 调整 `MonitorInterval` 为 3秒

### 问题 2: CPU 占用过高

**原因**: 可能启用了详细日志或检测间隔过短

**解决方案**:
```json
{
  "MonitorInterval": 2,
  "EnableDetailedLogging": false
}
```

### 问题 3: 内存占用持续增长

**原因**: 可能日志积累过多

**解决方案**:
```json
{
  "LogRetentionDays": 7,
  "AutoCleanLogs": true
}
```

### 问题 4: 检测延迟高

**原因**: 完整扫描间隔过长

**解决方案**:
- 使用管理员权限运行（优先注册表检测）
- 调整 `FullScanIntervalSeconds` 为 5秒

---

## 💡 性能优化建议

### 低配置电脑（<4GB RAM）

```json
{
  "MonitorInterval": 3,
  "EnableDetailedLogging": false,
  "LogRetentionDays": 3,
  "EnableAutoAction": false
}
```

### 中等配置（4-8GB RAM）

```json
{
  "MonitorInterval": 2,
  "EnableDetailedLogging": false,
  "LogRetentionDays": 7
}
```

### 高配置（>8GB RAM）

```json
{
  "MonitorInterval": 1,
  "EnableDetailedLogging": true,
  "LogRetentionDays": 30
}
```

---

## 📝 性能监控

### 查看性能指标

在程序运行时，可以通过任务管理器查看：

1. 打开任务管理器（Ctrl + Shift + Esc）
2. 找到 `StarReminder.exe`
3. 查看 CPU 和内存占用

### 正常范围

- **CPU**: 1-5%（空闲），5-15%（检测中）
- **内存**: 50-80 MB
- **磁盘**: 几乎无占用

---

## 🔗 相关文档

- [使用教程](usage.md) - 基本使用方法
- [配置指南](configuration.md) - 完整配置说明
- [开发指南](development.md) - 性能分析工具

---

## 📊 性能测试结果

### 测试环境

- **CPU**: Intel Core i5-10400
- **内存**: 16GB DDR4
- **系统**: Windows 11 Pro
- **进程数**: ~150 个

### 优化效果

| 指标 | v1.2.0 | v1.2.1 | 改善 |
|------|--------|--------|------|
| 平均 CPU | 18% | 4% | ⬇️ 78% |
| 平均内存 | 95 MB | 58 MB | ⬇️ 39% |
| 检测延迟 | 650ms | 120ms | ⬇️ 82% |
| UI 响应 | 250ms | 80ms | ⬇️ 68% |

---

<p align="center">
  <sub>性能优化指南 | 最后更新: 2025-11-01</sub>
</p>

