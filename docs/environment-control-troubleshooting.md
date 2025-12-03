# 环境智控模块故障排查指南

## N/A 状态显示问题

### 问题描述
在 EnvironmentControlPanel 中，设备状态显示为红色的 "N/A"，表示设备未找到或未正确配置。

### 原因分析

根据 `EnvironmentControlPanel.cs` 的 `UpdateDeviceStatusText` 方法，显示 N/A 的原因有：

1. **设备 GameObject 上缺少对应的 Controller 组件**
   - AirConditioner 需要 `AirConditionerController` 组件
   - AirPurifier 需要 `AirPurifierController` 组件
   - Fan 需要 `FanController` 组件
   - FreshAirSystem 需要 `FreshAirController` 组件

2. **设备没有正确注册到 DeviceManager**
   - 设备 GameObject 上必须有 `DeviceDefinition` 组件
   - `DeviceDefinition` 的 `roomId` 必须与 `EnvironmentControlPanel` 的 `currentRoomId` 匹配
   - `DeviceDefinition` 的 `type` 必须正确设置

3. **设备不在当前房间中**
   - `EnvironmentControlPanel.currentRoomId` 必须与设备的 `roomId` 匹配

### 排查步骤

#### 步骤 1：检查设备是否有 DeviceDefinition 组件

1. 在 Hierarchy 中找到设备 GameObject（如空调、净化器等）
2. 在 Inspector 中检查是否有 `Device Definition (Script)` 组件
3. 如果没有，添加组件：
   - 点击 `Add Component`
   - 搜索 `Device Definition`
   - 添加组件

4. 配置 DeviceDefinition：
   - `Device Id`: 设置唯一ID（如 "AirConditioner01"）
   - `Type`: 选择对应的设备类型（AirConditioner、AirPurifier、Fan、FreshAirSystem）
   - `Room Id`: 设置为当前房间ID（如 "LivingRoom01"）

#### 步骤 2：检查设备是否有对应的 Controller 组件

对于每种设备类型，需要添加对应的 Controller：

**空调 (AirConditioner):**
- 添加 `Air Conditioner Controller (Script)` 组件

**净化器 (AirPurifier):**
- 添加 `Air Purifier Controller (Script)` 组件

**风扇 (Fan):**
- 添加 `Fan Controller (Script)` 组件

**新风系统 (FreshAirSystem):**
- 添加 `Fresh Air Controller (Script)` 组件

#### 步骤 3：检查 DeviceManager 是否正确注册

1. 确保场景中有 `DeviceManager` GameObject（通常在 `DataRoot` 下）
2. 运行游戏，查看 Console 是否有错误信息
3. 可以在代码中添加调试日志：

```csharp
// 在 EnvironmentControlPanel.cs 的 UpdateDeviceStatus 方法中添加
Debug.Log($"[DeviceStatus] Room: {currentRoomId}, Devices count: {devices.Count}");
foreach (var device in devices)
{
    Debug.Log($"[DeviceStatus] Device: {device.deviceId}, Type: {device.type}, Room: {device.roomId}");
}
```

#### 步骤 4：检查 EnvironmentController 是否正确初始化

1. 确保场景中有 `EnvironmentController` GameObject
2. 检查 `EnvironmentController` 的 `Instance` 是否不为 null
3. 在 `EnvironmentControlPanel.cs` 的 `Start()` 方法中，确保 `envController` 已初始化：

```csharp
private void Start()
{
    envController = EnvironmentController.Instance;
    if (envController == null)
    {
        Debug.LogError("[EnvironmentControlPanel] EnvironmentController.Instance is null!");
    }
    // ...
}
```

### 快速修复清单

- [ ] 设备 GameObject 有 `DeviceDefinition` 组件
- [ ] `DeviceDefinition` 的 `roomId` 与 `EnvironmentControlPanel.currentRoomId` 匹配
- [ ] `DeviceDefinition` 的 `type` 正确设置
- [ ] 设备 GameObject 有对应的 Controller 组件（AirConditionerController、AirPurifierController 等）
- [ ] 场景中有 `DeviceManager` GameObject
- [ ] 场景中有 `EnvironmentController` GameObject
- [ ] 运行游戏时 Console 没有相关错误

### 常见错误示例

**错误 1：设备类型不匹配**
```
DeviceDefinition.type = DeviceType.AirConditioner
但 GameObject 上只有 FanController 组件
```
**解决：** 确保 Controller 类型与 DeviceDefinition.type 匹配

**错误 2：房间ID不匹配**
```
DeviceDefinition.roomId = "Bedroom01"
EnvironmentControlPanel.currentRoomId = "LivingRoom01"
```
**解决：** 将两者设置为相同的房间ID

**错误 3：缺少 Controller 组件**
```
DeviceDefinition 存在，但没有对应的 Controller 组件
```
**解决：** 添加对应的 Controller 组件（如 AirConditionerController）

### 调试工具

可以在 Unity Console 中运行以下命令来检查设备状态：

```csharp
// 在某个 MonoBehaviour 的 Update 方法中添加
if (Input.GetKeyDown(KeyCode.D))
{
    if (DeviceManager.Instance != null)
    {
        var devices = DeviceManager.Instance.GetDevicesInRoom("LivingRoom01");
        Debug.Log($"Devices in LivingRoom01: {devices.Count}");
        foreach (var device in devices)
        {
            Debug.Log($"  - {device.deviceId} ({device.type})");
            var controller = device.GetComponent<BaseDeviceController>();
            Debug.Log($"    Controller: {(controller != null ? controller.GetType().Name : "NULL")}");
        }
    }
}
```

按 `D` 键可以在 Console 中查看当前房间的所有设备及其 Controller 状态。

