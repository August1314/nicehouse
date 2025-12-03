# 房间和设备配置指南

## 概述

为了让环境智控模块正常工作，需要为场景中的房间和设备 GameObject 添加相应的组件。

## 配置步骤

### 步骤 1：配置房间（RoomDefinition）

1. **在 Hierarchy 中找到房间 GameObject**
   - 根据你的模型结构，找到各个房间（如 LivingRoom、BedRoom、Kitchen、BathRoom）
   - 如果房间是空的 GameObject，需要创建一个

2. **为每个房间添加 RoomDefinition 组件**
   - 选中房间 GameObject
   - 点击 `Add Component`
   - 搜索 `Room Definition`
   - 添加组件

3. **配置 RoomDefinition 属性**
   - `Room Id`: 设置唯一ID（如 "LivingRoom01", "BedRoom01", "Kitchen01", "BathRoom01"）
   - `Room Type`: 选择房间类型（LivingRoom, Bedroom, Kitchen, Bathroom, Study, Other）
   - `Display Name`: 设置显示名称（如 "客厅", "卧室", "厨房", "卫生间"）

**示例配置：**
```
LivingRoom GameObject:
  - Room Id: "LivingRoom01"
  - Room Type: LivingRoom
  - Display Name: "客厅"

BedRoom GameObject:
  - Room Id: "BedRoom01"
  - Room Type: Bedroom
  - Display Name: "卧室"
```

### 步骤 2：配置设备（DeviceDefinition）

1. **在 Hierarchy 中找到设备 GameObject**
   - 根据你的模型，找到各种设备：
     - 空调（Air Conditioner）
     - 新风系统（Fresh Air System）
     - 空气净化器（Air Purifier）
     - 风扇（Fan）
     - 窗户（Windows）
     - 灯光（Lights）
     - 传感器（Smoke Sensor, PM2.5 Sensor）
     - 一键呼叫按钮（Help Button）

2. **为每个设备添加 DeviceDefinition 组件**
   - 选中设备 GameObject
   - 点击 `Add Component`
   - 搜索 `Device Definition`
   - 添加组件

3. **配置 DeviceDefinition 属性**
   - `Device Id`: 设置唯一ID（如 "AirConditioner01", "Windows01"）
   - `Type`: 选择设备类型（AirConditioner, FreshAirSystem, AirPurifier, Fan, Light, Window, SmokeSensor, Pm25Sensor, HelpButton）
   - `Room Id`: 设置为设备所在房间的ID（如 "LivingRoom01"）

**示例配置：**
```
客厅的空调 GameObject:
  - Device Id: "AirConditioner_LivingRoom01"
  - Type: AirConditioner
  - Room Id: "LivingRoom01"

客厅的窗户 GameObject:
  - Device Id: "Windows_LivingRoom01"
  - Type: Window
  - Room Id: "LivingRoom01"
```

### 步骤 3：为设备添加 Controller 组件

为了让环境智控模块能控制设备，需要添加对应的 Controller：

**空调（AirConditioner）：**
- 添加 `Air Conditioner Controller (Script)` 组件

**净化器（AirPurifier）：**
- 添加 `Air Purifier Controller (Script)` 组件

**风扇（Fan）：**
- 添加 `Fan Controller (Script)` 组件

**新风系统（FreshAirSystem）：**
- 添加 `Fresh Air Controller (Script)` 组件

### 步骤 4：检查配置

1. **确保场景中有 Manager GameObject**
   - `DataRoot` GameObject（包含所有 Manager）
   - `RoomManager` 组件
   - `DeviceManager` 组件
   - `EnvironmentController` 组件

2. **运行游戏测试**
   - 运行游戏
   - 查看 Console 是否有错误
   - 检查 EnvironmentControlPanel 中的设备状态是否显示正常（不再是 N/A）

## 根据你的模型结构配置

根据你提供的 Hierarchy 结构，建议配置如下：

### 房间配置

```
LivingRoom GameObject:
  - RoomDefinition: Room Id="LivingRoom01", Type=LivingRoom, Display Name="客厅"

BedRoom GameObject:
  - RoomDefinition: Room Id="BedRoom01", Type=Bedroom, Display Name="卧室"

Kitchen GameObject:
  - RoomDefinition: Room Id="Kitchen01", Type=Kitchen, Display Name="厨房"

BathRoom GameObject:
  - RoomDefinition: Room Id="BathRoom01", Type=Bathroom, Display Name="卫生间"
```

### 设备配置示例

**客厅（LivingRoom）：**
- Windows → DeviceDefinition: Device Id="Windows_LivingRoom01", Type=Window, Room Id="LivingRoom01"
- 如果有空调 → DeviceDefinition + AirConditionerController
- 如果有净化器 → DeviceDefinition + AirPurifierController

**卧室（BedRoom）：**
- 类似配置

## 快速检查清单

- [ ] 所有房间都有 RoomDefinition 组件
- [ ] 所有设备都有 DeviceDefinition 组件
- [ ] DeviceDefinition 的 Room Id 与房间的 Room Id 匹配
- [ ] 需要控制的设备（空调、净化器等）都有对应的 Controller 组件
- [ ] 场景中有 RoomManager 和 DeviceManager
- [ ] 运行游戏时 Console 没有错误
- [ ] EnvironmentControlPanel 中设备状态显示正常（不是 N/A）

## 常见问题

**Q: 设备显示 N/A 怎么办？**
A: 查看 `docs/environment-control-troubleshooting.md` 排查指南

**Q: 如何知道设备属于哪个房间？**
A: 根据模型结构判断，或者创建一个空 GameObject 作为房间容器，将设备放在房间下

**Q: 房间 GameObject 应该放在哪里？**
A: 可以放在场景根目录，或者放在一个 "Rooms" 父对象下，方便管理

