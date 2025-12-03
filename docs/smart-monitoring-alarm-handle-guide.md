# 告警处理功能使用指南

## Handle 按钮在哪里？

Handle 按钮位于**告警列表**中的每个告警项里。

### 位置说明

1. **告警列表区域**：
   - 在 `MonitoringPanel` UI 面板中
   - 有一个滚动视图（ScrollView）显示告警列表
   - 每个告警项（AlarmItemPrefab）都包含一个 Handle 按钮

2. **告警项结构**：
   ```
   AlarmItemPrefab
   ├── TypeText (告警类型)
   ├── RoomText (房间)
   ├── TimeText (时间)
   ├── StatusText (状态：Handled/Unhandled)
   └── HandleButton (处理按钮)
   ```

## 如何使用 Handle 按钮

### 步骤 1：触发告警

1. 点击状态按钮（如 "Fallen" 或 "Sitting"）
2. 等待告警触发（跌倒立即触发，久坐需要等待超时）
3. 告警会出现在告警列表中

### 步骤 2：查看告警列表

1. 在 `MonitoringPanel` UI 中找到告警列表区域
2. 应该能看到新触发的告警，显示：
   - **告警类型**（如 "Fall/OutOfBed", "Long Sitting"）
   - **房间**（如 "BedRoom01"）
   - **时间**（如 "14:30:25"）
   - **状态**（"Unhandled" - 红色文字）
   - **Handle 按钮**（可点击）

### 步骤 3：处理告警

1. **找到未处理的告警**（状态显示为 "Unhandled"，红色）
2. **点击该告警项右侧的 "Handle" 按钮**
3. **结果：**
   - ✅ 告警状态变为 "Handled"（灰色文字）
   - ✅ Handle 按钮变为不可点击（灰色）
   - ✅ 告警列表自动更新

## 工作原理

### 代码流程

1. **告警触发**：
   ```
   MonitoringController 检测异常
   → AlarmManager.AddAlarm()
   → 告警添加到列表
   → MonitoringPanel.UpdateAlarmList()
   → 创建告警项 UI
   ```

2. **处理告警**：
   ```
   用户点击 Handle 按钮
   → MonitoringPanel.MarkAlarmHandled()
   → AlarmManager.MarkHandled()
   → 更新告警状态
   → MonitoringPanel.UpdateAlarmList()
   → UI 更新显示
   ```

### 关键代码

**MonitoringPanel.cs - CreateAlarmItem()**：
```csharp
// 查找 Handle 按钮
Button handleButton = item.GetComponentInChildren<Button>();
if (handleButton != null && !alarm.handled)
{
    // 未处理的告警：绑定点击事件
    handleButton.onClick.AddListener(() => MarkAlarmHandled(alarm));
}
else if (handleButton != null && alarm.handled)
{
    // 已处理的告警：禁用按钮
    handleButton.interactable = false;
}
```

**MonitoringPanel.cs - MarkAlarmHandled()**：
```csharp
private void MarkAlarmHandled(AlarmRecord record)
{
    if (AlarmManager.Instance != null)
    {
        AlarmManager.Instance.MarkHandled(record);
        UpdateAlarmList(); // 更新列表显示
    }
}
```

## 如果看不到 Handle 按钮

### 检查清单

1. **告警项预制体是否正确？**
   - 检查 `Assets/Prefabs/AlarmItemPrefab.prefab`
   - 确保预制体中有 `HandleButton` GameObject
   - 确保 `HandleButton` 有 `Button` 组件

2. **告警列表是否正确绑定？**
   - 检查 `MonitoringPanel` 脚本的 `Alarm List Content` 字段
   - 检查 `Alarm Item Prefab` 字段

3. **告警是否已处理？**
   - 已处理的告警，Handle 按钮会变为不可点击（灰色）
   - 只有未处理的告警（"Unhandled"）才有可点击的 Handle 按钮

4. **告警列表是否显示？**
   - 确保有告警被触发
   - 检查告警列表滚动视图是否可见
   - 检查告警列表 Content 是否正确配置

### 调试方法

1. **检查 Console**：
   - 运行游戏，触发告警
   - 查看 Console 是否有错误信息

2. **检查 Hierarchy**：
   - 运行游戏后，在 Hierarchy 中找到 `MonitoringPanel`
   - 展开 `AlarmListScrollView > Viewport > Content`
   - 应该能看到告警项 GameObject
   - 展开告警项，应该能看到 `HandleButton`

3. **检查 Inspector**：
   - 选中告警项中的 `HandleButton`
   - 在 Inspector 中检查：
     - ✅ 有 `Button` 组件
     - ✅ `Interactable` 为 true（未处理时）
     - ✅ 有 `Text (TMP)` 子对象，显示 "Handle"

## 测试步骤

### 完整测试流程

1. **触发告警**：
   - 点击 "Fallen" 按钮
   - 告警应该出现在列表中

2. **查看告警项**：
   - 在告警列表中找到该告警
   - 应该看到：
     - 告警类型：Fall/OutOfBed
     - 房间：当前房间
     - 时间：当前时间
     - 状态：Unhandled（红色）
     - Handle 按钮：可点击

3. **处理告警**：
   - 点击 Handle 按钮
   - 应该看到：
     - 状态变为 Handled（灰色）
     - Handle 按钮变为不可点击

4. **验证结果**：
   - 告警列表应该更新
   - 该告警的状态应该保持为 Handled

## 常见问题

**Q: 为什么看不到 Handle 按钮？**
A: 检查告警项预制体是否正确创建，确保 `HandleButton` GameObject 存在。

**Q: Handle 按钮点击没有反应？**
A: 检查按钮是否正确绑定到 `MarkAlarmHandled` 方法，检查 `AlarmManager.Instance` 是否为 null。

**Q: 已处理的告警还能点击 Handle 按钮吗？**
A: 不能。已处理的告警，Handle 按钮会被禁用（`interactable = false`）。

**Q: 告警列表不显示？**
A: 检查 `AlarmListContent` 是否正确绑定到 ScrollView 的 Content，确保 `AlarmItemPrefab` 已正确绑定。

