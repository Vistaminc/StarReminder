# 🤝 贡献指南

感谢你考虑为 StarReminder 做出贡献！

<div align="center">
  <a href="README.md">返回首页</a> · <a href="docs/development.md">开发指南</a>
</div>

---

## 📋 目录

- [行为准则](#-行为准则)
- [如何贡献](#-如何贡献)
- [报告 Bug](#-报告-bug)
- [提出新功能](#-提出新功能)
- [提交代码](#-提交代码)
- [代码规范](#-代码规范)
- [提交消息规范](#-提交消息规范)
- [Pull Request 流程](#-pull-request-流程)
- [文档贡献](#-文档贡献)

---

## 📜 行为准则

### 我们的承诺

为了营造一个开放且友善的环境，我们作为贡献者和维护者承诺：无论年龄、体型、残疾、种族、性别认同和表达、经验水平、教育程度、社会经济地位、国籍、个人外貌、种族、宗教或性取向如何，参与我们项目和社区的每个人都不会受到骚扰。

### 我们的标准

**积极行为的例子**：
- ✅ 使用友善和包容的语言
- ✅ 尊重不同的观点和经验
- ✅ 优雅地接受建设性批评
- ✅ 关注对社区最有利的事情
- ✅ 对其他社区成员表示同理心

**不可接受的行为**：
- ❌ 使用性化的语言或图像
- ❌ 挑衅、侮辱或贬损的评论
- ❌ 公开或私下的骚扰
- ❌ 未经许可发布他人的私人信息
- ❌ 其他在专业环境中可能被认为不适当的行为

---

## 💡 如何贡献

有很多方式可以为 StarReminder 做出贡献：

### 🐛 报告 Bug

发现 Bug？请提交 Issue！

### ✨ 提出新功能

有好想法？欢迎提出！

### 💻 贡献代码

- 修复 Bug
- 实现新功能
- 优化性能
- 改进文档

### 📚 改进文档

- 修正错别字
- 补充缺失内容
- 翻译文档
- 添加示例

### 🧪 测试

- 测试新功能
- 报告问题
- 提供反馈

### 💬 帮助他人

- 回答 Issues 中的问题
- 在讨论区分享经验
- 编写教程

---

## 🐛 报告 Bug

### 提交前检查

在提交 Bug 报告之前，请：

1. **搜索现有 Issues**: 确保该问题尚未被报告
2. **使用最新版本**: 确认 Bug 在最新版本中仍然存在
3. **简化问题**: 尝试找到最小可复现的情况

### Bug 报告模板

请使用以下模板提交 Bug：

```markdown
## Bug 描述
简要描述 Bug 的现象。

## 复现步骤
1. 启动程序
2. 点击 XXX
3. 执行 YYY
4. 出现错误

## 预期行为
描述你期望发生什么。

## 实际行为
描述实际发生了什么。

## 截图
如果可能，请提供截图。

## 环境信息
- OS: [例如 Windows 11]
- StarReminder 版本: [例如 v1.2.0]
- .NET 版本: [例如 .NET 8.0]

## 日志
粘贴相关的日志内容（位于 logs/activity_log.json）

## 附加信息
其他相关信息
```

### 标签说明

- `bug`: 确认的 Bug
- `needs-investigation`: 需要进一步调查
- `duplicate`: 重复的 Issue
- `invalid`: 无效的 Issue

---

## ✨ 提出新功能

### 功能请求模板

```markdown
## 功能描述
简要描述建议的功能。

## 动机
为什么需要这个功能？解决什么问题？

## 详细说明
详细描述功能的工作方式。

## 替代方案
是否考虑过其他解决方案？

## 附加信息
其他相关信息或截图
```

### 功能评估标准

我们会根据以下标准评估功能请求：

1. **用户价值**: 对用户的价值有多大？
2. **实现复杂度**: 实现难度如何？
3. **维护成本**: 长期维护成本？
4. **兼容性**: 是否破坏现有功能？

---

## 💻 提交代码

### 开发流程

1. **Fork 仓库**
   
   点击 GitHub 页面右上角的 "Fork" 按钮

2. **克隆你的 Fork**
   
   ```bash
   git clone https://github.com/your-username/StarReminder.git
   cd StarReminder
   ```

3. **创建功能分支**
   
   ```bash
   git checkout -b feature/my-awesome-feature
   ```

4. **设置开发环境**
   
   参考 [开发指南](docs/development.md)

5. **进行更改**
   
   - 遵循代码规范
   - 添加必要的测试
   - 更新文档

6. **提交更改**
   
   ```bash
   git add .
   git commit -m "feat: add awesome feature"
   ```

7. **推送到你的 Fork**
   
   ```bash
   git push origin feature/my-awesome-feature
   ```

8. **创建 Pull Request**
   
   在 GitHub 上点击 "New Pull Request"

---

## 📝 代码规范

### C# 编码规范

请参考 [开发指南 - 编码规范](docs/development.md#-编码规范)

**关键要点**：

- ✅ 使用 PascalCase 命名类和方法
- ✅ 使用 camelCase 命名局部变量
- ✅ 私有字段使用 `_camelCase`
- ✅ 添加 XML 文档注释
- ✅ 单一职责原则
- ✅ 适当的错误处理

### XAML 编码规范

- 属性按字母顺序排列
- 使用数据绑定而非代码后置
- 保持 XAML 简洁清晰

### 代码审查标准

提交的代码应该：

- [ ] 遵循项目编码规范
- [ ] 包含适当的注释
- [ ] 包含单元测试
- [ ] 所有测试通过
- [ ] 无编译警告
- [ ] 更新相关文档

---

## 📋 提交消息规范

### Conventional Commits

我们使用 [Conventional Commits](https://www.conventionalcommits.org/) 规范。

### 格式

```
<类型>[可选的作用域]: <描述>

[可选的正文]

[可选的脚注]
```

### 类型

| 类型 | 说明 | 示例 |
|------|------|------|
| `feat` | 新功能 | `feat: 添加网络监控功能` |
| `fix` | Bug 修复 | `fix: 修复进程检测延迟问题` |
| `docs` | 文档更新 | `docs: 更新配置指南` |
| `style` | 代码格式 | `style: 格式化代码` |
| `refactor` | 重构 | `refactor: 重构进程监控服务` |
| `perf` | 性能优化 | `perf: 优化媒体设备检测性能` |
| `test` | 测试 | `test: 添加进程监控测试` |
| `build` | 构建系统 | `build: 更新依赖版本` |
| `ci` | CI 配置 | `ci: 添加 GitHub Actions` |
| `chore` | 其他更改 | `chore: 更新 .gitignore` |

### 示例

```bash
# 新功能
git commit -m "feat: 添加系统托盘图标"

# Bug 修复
git commit -m "fix: 修复配置加载失败的问题"

# 文档
git commit -m "docs: 更新安装指南"

# 带作用域
git commit -m "feat(ui): 添加深色主题支持"

# 破坏性变更
git commit -m "feat!: 重构配置文件格式

BREAKING CHANGE: 配置文件格式从 v1 升级到 v2，需要迁移旧配置"
```

---

## 🔄 Pull Request 流程

### 创建 PR

1. **确保你的分支是最新的**
   
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **推送到你的 Fork**
   
   ```bash
   git push origin feature/my-awesome-feature
   ```

3. **在 GitHub 上创建 PR**

### PR 模板

```markdown
## 描述
简要描述这个 PR 的内容。

## 相关 Issue
关联的 Issue: #123

## 更改类型
- [ ] Bug 修复
- [ ] 新功能
- [ ] 破坏性变更
- [ ] 文档更新

## 测试
描述如何测试这些更改。

## 截图
如果有 UI 变更，请提供截图。

## 检查清单
- [ ] 代码遵循项目规范
- [ ] 添加了必要的测试
- [ ] 所有测试通过
- [ ] 更新了相关文档
- [ ] 提交消息遵循规范
```

### PR 审查流程

1. **自动检查**: CI 会自动运行测试
2. **代码审查**: 维护者会审查代码
3. **反馈**: 根据反馈进行修改
4. **合并**: 审查通过后合并

### 合并要求

- ✅ 所有测试通过
- ✅ 至少一个维护者批准
- ✅ 无合并冲突
- ✅ 遵循所有规范

---

## 📚 文档贡献

文档和代码一样重要！

### 文档类型

- **用户文档**: 面向最终用户
  - 安装指南
  - 使用教程
  - 配置指南

- **技术文档**: 面向开发者
  - 架构文档
  - API 文档
  - 开发指南

### 文档规范

- 使用清晰简洁的语言
- 提供代码示例
- 添加必要的截图
- 保持格式一致
- 定期更新

### 文档位置

```
docs/
├── README.md           # 文档索引
├── installation.md     # 安装指南
├── configuration.md    # 配置指南
├── usage.md           # 使用教程
├── architecture.md     # 技术架构
├── development.md      # 开发指南
└── api.md             # API 文档
```

---

## 🎁 贡献者奖励

### 贡献者徽章

我们会在 README 中感谢所有贡献者！

### 特殊贡献

对于重要贡献，我们会：
- 在更新日志中特别提及
- 在项目文档中署名
- 邀请成为项目维护者

---

## 📞 获取帮助

### 遇到问题？

- 💬 在 [Discussions](https://github.com/yourusername/StarReminder/discussions) 提问
- 📮 查看 [Issues](https://github.com/yourusername/StarReminder/issues)
- 📧 联系维护者

### 沟通渠道

- GitHub Issues: Bug 报告和功能请求
- GitHub Discussions: 一般讨论和问题
- Pull Requests: 代码贡献

---

## 📜 许可证

贡献到 StarReminder 的所有代码都将采用 [MIT 许可证](LICENSE)。

提交 PR 即表示你同意：
- 你的贡献采用 MIT 许可证
- 你有权提交这些代码
- 你的贡献是你的原创作品

---

## 🌟 感谢你的贡献！

每一个贡献都让 StarReminder 变得更好。无论大小，我们都非常感谢！

<div align="center">
  <sub>© 2025 StarReminder Team</sub>
</div>

