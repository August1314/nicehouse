# 灯光交互功能开发文档

> 本文档记录灯光交互功能的开发实现，包括灯光控制器和可交互灯光组件。

---

## 📋 功能模块基本信息

**模块名称：** 灯光交互模块

**模块编号：** FM-007

**负责人：** [雷颜玮]

**关联需求：** 基础要求 - 功能项 - 室内灯光控制

**核心价值：** 实现室内灯光的第一人称射线交互，玩家可以通过准星瞄准灯具并点击来开关灯，增强场景沉浸感。

---

## 🎯 功能概述

灯光交互模块实现了：

- **射线交互**：通过第一人称准星瞄准灯具，点击鼠标左键开关灯
- **视觉反馈**：准星悬停时显示"开灯"/"关灯"提示，灯具高亮
- **灯光效果**：控制 Unity Light 组件实现真实的光照效果
- **能耗关联**：开关灯时自动记录能耗数据到 EnergyManager

---

## 📁 新增文件

### 1. LightController.cs

**路径：** `Assets/Scripts/EnvironmentControl/Controllers/LightController.cs`

**功能：** 灯光控制器，继承自 BaseDeviceController，控制灯光的开关和亮度。

**主要接口：**

| 方法 | 功能 |
|-----|------|
| `TurnOn()` | 开灯，设置亮度为 onIntensity |
| `TurnOff()` | 关灯，设置亮度为 offIntensity |
| `Toggle()` | 切换灯光状态 |
| `SetIntensity(float)` | 设置灯光强度 |

**Inspector 配置：**

```
Light Controller (Script)
├── 灯光组件
│   ├── Target Light: Light 组件引用
│   ├── On Intensity: 开灯亮度（默认 1）
│   └── Off Intensity: 关灯亮度（默认 0）
├── 发光材质（可选）
│   ├── Emissive Renderer: 灯罩 Renderer
│   ├── Emission Color Property: "_EmissionColor"
│   ├── Emission On Color: 开灯发光颜色
│   └── Emission Off Color: 关灯发光颜色
└── 状态
    └── Is Light On: 当前灯光状态
```

---

### 2. InteractableLight.cs

**路径：** `Assets/Scripts/Interaction/InteractableLight.cs`

**功能：** 可交互灯光组件，实现 IRaycastInteractable 接口，响应玩家的射线点击。

**主要接口：**

| 方法 | 功能 |
|-----|------|
| `OnHoverEnter()` | 准星悬停进入时，显示高亮 |
| `OnHoverExit()` | 准星离开时，恢复原色 |
| `OnRaycastClick()` | 点击时，切换灯光状态 |
| `HoverHint` | 返回悬停提示文字 |

**Inspector 配置：**

```
Interactable Light (Script)
├── 灯光控制器
│   └── Light Controller: LightController 引用
├── 交互设置
│   ├── Interaction Distance: 交互距离限制
│   ├── Turn Off Hint: "关灯"
│   └── Turn On Hint: "开灯"
└── 视觉反馈
    ├── Highlight Color: 悬停高亮颜色
    └── Enable Highlight: 是否启用高亮
```

---

## 🏗️ 架构设计

### 组件关系图

```
灯具 GameObject（父物体）
├── Mesh Filter + Mesh Renderer （灯具外观）
├── Device Definition          （设备数据）
├── Light Controller           （灯光控制逻辑）
├── Box Collider               （射线检测区域）
├── Interactable Light         （交互响应）
│
└── Light_source（子物体）
    └── Light (Point)          （实际光源）
           ▲
           │ Target Light 引用
           │
    Light Controller ──────────┘
```

### 数据流向

```
玩家点击
    │
    ▼
FPRaycastInteractor（射线检测）
    │
    ▼
InteractableLight.OnRaycastClick()
    │
    ▼
LightController.Toggle()
    │
    ├──→ Light.intensity = onIntensity / offIntensity
    │
    └──→ EnergyManager.StartConsume() / StopConsume()
              │
              ▼
         能耗数据更新 → UI 面板显示
```

---

## 🎮 Unity 场景配置指南

### 步骤 1：配置灯具父物体

1. 选中灯具模型（如 `BedroomLampleft`）
2. 添加以下组件：
   - **Device Definition**
     - Device Id: `Light_Bedroom_Left`（唯一标识）
     - Type: `Light`
     - Room Id: `Bedroom01`
   - **Light Controller**
     - On Intensity: `10`
     - Off Intensity: `0`
   - **Box Collider**
     - 调整大小包裹灯具模型
     - Is Trigger: ✅
   - **Interactable Light**
     - 保持默认设置

### 步骤 2：创建光源子物体

1. 右键灯具 → **Create Empty** → 命名 `Light_source`
2. 移动子物体到**灯泡位置**（灯罩内部）
3. 添加 **Light** 组件：
   - Type: `Point`
   - Range: `5` ~ `10`
   - Color: 暖黄色（可选）

### 步骤 3：关联引用

1. 选中父物体
2. 在 **Light Controller** 中：
   - 将 **Target Light** 设为子物体 `Light_source` 上的 Light 组件

### 步骤 4：保存测试

1. **Ctrl+S** 保存场景
2. 运行游戏，走到灯具旁
3. 准星对准灯具，点击鼠标左键测试

---

## 📋 已配置灯具清单

| 灯具名称 | Device Id | 房间 | 状态 |
|---------|-----------|------|------|
| BedroomLampleft | Light_Bedroom_Left | Bedroom01 | ✅ 已完成 |
| BedroomLampright | Light_Bedroom_Right | Bedroom01 | ⬜ 待配置 |
| DiningLamp1 | Light_Dining_01 | DiningRoom01 | ⬜ 待配置 |
| DiningLamp2 | Light_Dining_02 | DiningRoom01 | ⬜ 待配置 |
| LivingroomLamp | Light_Living_01 | LivingRoom01 | ⬜ 待配置 |
| OfficeLamp | Light_Office_01 | Office01 | ⬜ 待配置 |

---

## 🔗 与其他模块的关联

### 能耗系统（EnergyManager）

- 开灯时调用 `EnergyManager.Instance.StartConsume(deviceId)`
- 关灯时调用 `EnergyManager.Instance.StopConsume(deviceId)`
- 能耗数据可在 DataDashboard 中查看

### 射线交互系统（FPRaycastInteractor）

- InteractableLight 实现 `IRaycastInteractable` 接口
- 需确保灯具 Layer 在 FPRaycastInteractor 的 Interactable Layers 中

### 设备管理系统（DeviceManager）

- 通过 DeviceDefinition 注册到 DeviceManager
- 可通过 `DeviceManager.Instance.GetDevicesInRoom(roomId)` 获取房间内所有灯具

---

## ⚠️ 常见问题

### Q1: 点击无反应
- 检查是否添加了 Collider
- 检查 Layer 是否在 FPRaycastInteractor 的检测范围内

### Q2: 灯亮了但光照在错误位置
- 检查 Light_source 子物体的位置是否在灯泡处
- Light 组件应该在子物体上，不是父物体上

### Q3: 开灯后亮度不变
- 检查 Light Controller 的 Target Light 是否正确关联
- 检查 On Intensity 是否设置了正确的值

### Q4: 没有悬停提示
- 检查场景中是否有 CrosshairUI 组件
- 检查 FPRaycastInteractor 是否配置了 Crosshair UI 引用

---

## 📝 更新日志

| 日期 | 更新内容 |
|-----|---------|
| 2025-12-05 | 创建 LightController.cs 和 InteractableLight.cs |
| 2025-12-05 | 完成 BedroomLampleft 灯具配置 |
| 2025-12-05 | 编写灯光交互功能文档 |
