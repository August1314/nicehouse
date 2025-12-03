# 如何禁用状态自动切换

## 问题说明

`PersonStateSimulator` 有一个自动切换功能，如果启用，会每隔一段时间自动切换数字人的状态。这可能会干扰你的测试。

## 解决方法

### 方法 1：在 Unity Inspector 中禁用（推荐）

1. **在 Hierarchy 中找到 `PersonStateSimulator` GameObject**
2. **在 Inspector 中查看 `Person State Simulator (Script)` 组件**
3. **取消勾选 `Enable Auto Switch`**
   - 这个选项在 "自动切换设置（可选）" 区域
4. **完成！** 现在状态不会自动切换了

### 方法 2：在代码中禁用

如果你想永久禁用自动切换，可以修改 `PersonStateSimulator.cs`：

```csharp
[Tooltip("是否启用自动状态切换")]
public bool enableAutoSwitch = false;  // 改为 false（默认值）
```

## 自动切换的工作原理

如果 `Enable Auto Switch` 被勾选：
- 每 `Switch Interval` 秒（默认 10 秒）自动切换一次状态
- 按照 `State Sequence` 中定义的状态序列循环切换
- 默认序列：Idle → Walking → Sitting → Sleeping → Idle...

## 什么时候使用自动切换？

自动切换功能主要用于：
- **演示场景**：展示系统如何自动监测不同状态
- **自动化测试**：不需要手动点击按钮就能测试告警功能

## 手动控制状态

禁用自动切换后，你可以：
- 使用 UI 按钮手动切换状态
- 通过代码调用 `PersonStateSimulator.ChangeState()` 方法

