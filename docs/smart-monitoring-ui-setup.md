# 智能监护模块 UI 配置指南

## 概述

本指南将帮助你创建和配置 MonitoringPanel 所需的所有 UI 元素。

## UI 结构

```
MonitoringPanel (Panel)
├── StateDisplaySection (Panel, 可选分组)
│   ├── StateText (TextMeshProUGUI)
│   ├── RoomText (TextMeshProUGUI)
│   └── DurationText (TextMeshProUGUI)
│
├── StateControlSection (Panel, 可选分组)
│   ├── IdleButton (Button)
│   ├── WalkingButton (Button)
│   ├── SittingButton (Button)
│   ├── BathingButton (Button)
│   ├── SleepingButton (Button)
│   ├── FallenButton (Button)
│   └── OutOfBedButton (Button)
│
├── RoomSelectionSection (Panel, 可选分组)
│   └── RoomDropdown (TMP_Dropdown)
│
├── AlarmListSection (Panel, 可选分组)
│   ├── AlarmListScrollView (ScrollView)
│   │   └── Viewport
│   │       └── Content (AlarmListContent)
│   └── AlarmItemPrefab (Prefab, 需要创建)
│
└── AlarmSettingsSection (Panel, 可选分组)
    ├── LongSittingThresholdInput (TMP_InputField)
    ├── LongBathingThresholdInput (TMP_InputField)
    ├── EnableMonitoringToggle (Toggle)
    └── ApplySettingsButton (Button)
```

## 详细创建步骤

### 步骤 1：创建状态显示区域

1. **选中 MonitoringPanel GameObject**
2. **创建状态文本：**
   - 右键 `MonitoringPanel` → `UI > Text - TextMeshPro`
   - 命名为 `StateText`
   - 设置文本为 "State: Idle"
   - 调整字体大小（建议 18-20）

3. **创建房间文本：**
   - 右键 `MonitoringPanel` → `UI > Text - TextMeshPro`
   - 命名为 `RoomText`
   - 设置文本为 "Room: LivingRoom01"

4. **创建持续时间文本：**
   - 右键 `MonitoringPanel` → `UI > Text - TextMeshPro`
   - 命名为 `DurationText`
   - 设置文本为 "Duration: 00:00"

### 步骤 2：创建状态切换按钮

1. **创建按钮组：**
   - 右键 `MonitoringPanel` → `UI > Button - TextMeshPro`
   - 命名为 `IdleButton`
   - 在按钮的 `Text (TMP)` 子对象中，设置文本为 "Idle"

2. **重复创建其他按钮：**
   - `WalkingButton` - 文本 "Walking"
   - `SittingButton` - 文本 "Sitting"
   - `BathingButton` - 文本 "Bathing"
   - `SleepingButton` - 文本 "Sleeping"
   - `FallenButton` - 文本 "Fallen"
   - `OutOfBedButton` - 文本 "OutOfBed"

3. **（可选）使用 Horizontal Layout Group 排列按钮：**
   - 创建一个空 GameObject，命名为 `StateButtonsContainer`
   - 添加 `Horizontal Layout Group` 组件
   - 将所有按钮拖到这个容器下
   - 设置 `Spacing` 为 5-10

### 步骤 3：创建房间选择下拉菜单

1. **创建下拉菜单：**
   - 右键 `MonitoringPanel` → `UI > Dropdown - TextMeshPro`
   - 命名为 `RoomDropdown`
   - 在 Inspector 中，找到 `TMP_Dropdown` 组件
   - 点击 `Options` 列表，添加选项：
     - "LivingRoom01"
     - "BedRoom01"
     - "Kitchen01"
     - "BathRoom01"

### 步骤 4：创建告警列表

1. **创建滚动视图：**
   - 右键 `MonitoringPanel` → `UI > Scroll View`
   - 命名为 `AlarmListScrollView`
   - Unity 会自动创建 `Viewport` 和 `Content`

2. **找到 Content：**
   - 展开 `AlarmListScrollView` → `Viewport` → `Content`
   - 这个 `Content` 就是 `AlarmListContent`（Transform）

3. **创建告警项预制体：**
   - 在 Project 中，右键 `Assets` → `Create > Folder`，命名为 `Prefabs`
   - 在 Hierarchy 中，右键 `MonitoringPanel` → `UI > Panel`
   - 命名为 `AlarmItemPrefab`
   - 在这个 Panel 下创建以下子对象：
     - `TypeText` (TextMeshPro) - 显示告警类型
     - `RoomText` (TextMeshPro) - 显示房间
     - `TimeText` (TextMeshPro) - 显示时间
     - `StatusText` (TextMeshPro) - 显示处理状态
     - `HandleButton` (Button) - "已处理"按钮
   - 调整布局（可以使用 Horizontal Layout Group）
   - 将这个 GameObject 拖到 `Assets/Prefabs` 文件夹，创建为 Prefab
   - 删除 Hierarchy 中的原始 GameObject

### 步骤 5：创建告警设置区域

1. **创建久坐阈值输入框：**
   - 右键 `MonitoringPanel` → `UI > Input Field - TextMeshPro`
   - 命名为 `LongSittingThresholdInput`
   - 在 `Text Input` 子对象中，设置占位符文本为 "30"
   - 在 Inspector 的 `TMP_Input Field` 组件中：
     - `Content Type`: Integer Number
     - `Character Limit`: 3

2. **创建久浴阈值输入框：**
   - 右键 `MonitoringPanel` → `UI > Input Field - TextMeshPro`
   - 命名为 `LongBathingThresholdInput`
   - 设置占位符文本为 "20"
   - `Content Type`: Integer Number

3. **创建监测开关：**
   - 右键 `MonitoringPanel` → `UI > Toggle`
   - 命名为 `EnableMonitoringToggle`
   - 在 `Label` 子对象中，设置文本为 "启用监测"

4. **创建应用设置按钮：**
   - 右键 `MonitoringPanel` → `UI > Button - TextMeshPro`
   - 命名为 `ApplySettingsButton`
   - 在按钮的 `Text (TMP)` 子对象中，设置文本为 "应用设置"

### 步骤 6：配置脚本引用

1. **选中 MonitoringPanel GameObject**
2. **在 Inspector 中找到 `Monitoring Panel (Script)` 组件**
3. **将创建的 UI 元素拖到对应字段：**

   **状态显示：**
   - `State Text` → 拖入 `StateText` GameObject
   - `Room Text` → 拖入 `RoomText` GameObject
   - `Duration Text` → 拖入 `DurationText` GameObject

   **状态切换按钮：**
   - `Idle Button` → 拖入 `IdleButton` GameObject
   - `Walking Button` → 拖入 `WalkingButton` GameObject
   - `Sitting Button` → 拖入 `SittingButton` GameObject
   - `Bathing Button` → 拖入 `BathingButton` GameObject
   - `Sleeping Button` → 拖入 `SleepingButton` GameObject
   - `Fallen Button` → 拖入 `FallenButton` GameObject
   - `Out Of Bed Button` → 拖入 `OutOfBedButton` GameObject

   **房间选择：**
   - `Room Dropdown` → 拖入 `RoomDropdown` GameObject

   **告警列表：**
   - `Alarm List Content` → 拖入 `AlarmListScrollView/Viewport/Content` GameObject
   - `Alarm Item Prefab` → 拖入 `Assets/Prefabs/AlarmItemPrefab` Prefab

   **告警设置：**
   - `Long Sitting Thres...` → 拖入 `LongSittingThresholdInput` GameObject
   - `Long Bathing Thre...` → 拖入 `LongBathingThresholdInput` GameObject
   - `Enable Monitoring ...` → 拖入 `EnableMonitoringToggle` GameObject
   - `Apply Settings But...` → 拖入 `ApplySettingsButton` GameObject

## 快速创建模板（可选）

如果你想要更快的创建方式，可以使用以下步骤：

### 使用 Vertical Layout Group 自动排列

1. **选中 MonitoringPanel**
2. **添加 `Vertical Layout Group` 组件**
3. **设置参数：**
   - `Spacing`: 10
   - `Padding`: 10 (上下左右)
   - `Child Force Expand`: 只勾选 Width

4. **创建所有 UI 元素后，它们会自动垂直排列**

### 告警项预制体详细结构

```
AlarmItemPrefab (Panel)
├── HorizontalLayoutGroup (组件)
├── TypeText (TextMeshPro) - 宽度 150
├── RoomText (TextMeshPro) - 宽度 120
├── TimeText (TextMeshPro) - 宽度 100
├── StatusText (TextMeshPro) - 宽度 80
└── HandleButton (Button) - 宽度 80
    └── Text (TMP) - "已处理"
```

## 检查清单

配置完成后，检查以下内容：

- [ ] 所有状态显示文本已创建并绑定
- [ ] 所有状态切换按钮已创建并绑定
- [ ] 房间下拉菜单已创建并绑定
- [ ] 告警列表 Content 已绑定
- [ ] 告警项预制体已创建并绑定
- [ ] 所有告警设置输入框已创建并绑定
- [ ] 监测开关已创建并绑定
- [ ] 应用设置按钮已创建并绑定
- [ ] 运行游戏时没有 Console 错误

## 常见问题

**Q: 告警项预制体应该放在哪里？**
A: 建议放在 `Assets/Prefabs` 文件夹，方便管理和复用。

**Q: 如何让 UI 元素自动排列？**
A: 使用 `Vertical Layout Group` 或 `Horizontal Layout Group` 组件。

**Q: 告警列表不显示怎么办？**
A: 检查 `AlarmListContent` 是否正确绑定到 ScrollView 的 Content，确保 AlarmManager 已初始化。

**Q: 按钮点击没有反应？**
A: 检查 EventSystem 是否存在，确保按钮正确绑定到脚本字段。

## 测试建议

配置完成后：
1. 运行游戏
2. 点击状态切换按钮，观察状态显示是否更新
3. 等待或手动触发告警，观察告警列表是否显示
4. 修改告警设置，点击"应用设置"，观察是否生效

