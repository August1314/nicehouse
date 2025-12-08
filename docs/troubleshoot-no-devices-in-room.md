# 排查：房间下没有设备显示

## 问题描述
在 `EnvironmentControlPanel` 中选择某个房间（如 Bedroom）后，设备列表为空。

## 排查步骤

### 1. 检查 Console 日志

运行游戏后，打开 Console 窗口，查看以下日志：

#### 1.1 检查 `showAllRooms` 设置
- 如果看到 `showAllRooms=true`，说明显示**所有房间**的设备，dropdown 只用于切换环境数据
- 如果看到 `showAllRooms=false`，说明只显示**当前选中房间**的设备

#### 1.2 检查设备注册情况
查找类似这样的日志：
```
[EnvironmentControlPanel] showAllRooms=true, Found X total devices
[EnvironmentControlPanel] Device: deviceId, Room: roomId, Type: DeviceType
```

#### 1.3 检查房间选择
查找类似这样的日志：
```
[EnvironmentControlPanel] OnRoomSelected: index=X, total rooms=Y
[EnvironmentControlPanel] Selected room: roomId, displayName=xxx, showAllRooms=true/false
```

#### 1.4 检查房间设备匹配
如果 `showAllRooms=false` 且没有设备，会看到警告：
```
[EnvironmentControlPanel] No devices found in room 'BedRoom01'. Available rooms and devices:
[EnvironmentControlPanel] Room 'LivingRoom01' has X devices: device1, device2, ...
```

### 2. 检查房间 ID 匹配

#### 2.1 检查 `currentRoomId`
- 在 Inspector 中选中 `EnvironmentControlPanel` GameObject
- 查看 `Current Room Id` 字段的值
- 注意大小写：`BedRoom01` vs `Bedroom01` vs `Bedroom`

#### 2.2 检查设备的 `roomId`
- 在 Hierarchy 中找到设备 GameObject（如 AC、Lamp 等）
- 查看 `DeviceDefinition` 组件的 `Room Id` 字段
- **确保与 `currentRoomId` 完全一致**（包括大小写）

#### 2.3 常见不匹配情况
- `BedRoom01` vs `Bedroom01`（大小写不同）
- `BedRoom01` vs `Bedroom`（缺少数字）
- `BedRoom01` vs `BedRoom`（缺少数字）
- 拼写错误：`BedRoom01` vs `Bedrom01`

### 3. 检查 `showAllRooms` 设置

#### 3.1 在 Inspector 中检查
- 选中 `EnvironmentControlPanel` GameObject
- 找到 `设备列表（新版，支持所有设备）` 部分
- 查看 `Show All Rooms` 复选框
  - ✅ **勾选**：显示所有房间的设备，dropdown 只切换环境数据
  - ❌ **未勾选**：只显示当前房间的设备

#### 3.2 根据需求调整
- **如果想看到所有设备**：勾选 `Show All Rooms`
- **如果只想看到当前房间的设备**：取消勾选 `Show All Rooms`，并确保设备的 `roomId` 与 `currentRoomId` 匹配

### 4. 检查设备注册

#### 4.1 检查 `DeviceManager`
- 在 Hierarchy 中找到 `DeviceManager` GameObject
- 确保它存在于场景中
- 确保它在 `Awake()` 时正确初始化

#### 4.2 检查设备是否有 `DeviceDefinition`
- 在 Hierarchy 中找到设备 GameObject
- 确保它有 `DeviceDefinition` 组件
- 确保 `Device Id` 不为空
- 确保 `Room Id` 不为空且与房间 ID 匹配

#### 4.3 检查设备是否在场景中
- 确保设备 GameObject 在场景中是**激活**的（不是 inactive）
- 确保设备 GameObject 在 Hierarchy 中可见

### 5. 快速诊断脚本

在 Unity Console 中运行以下代码（通过临时脚本或 Inspector 调试）：

```csharp
// 检查所有房间
if (RoomManager.Instance != null)
{
    var rooms = RoomManager.Instance.GetAllRooms();
    Debug.Log($"Total rooms: {rooms.Count}");
    foreach (var room in rooms.Values)
    {
        Debug.Log($"Room: {room.roomId}, DisplayName: {room.displayName}");
    }
}

// 检查所有设备
if (DeviceManager.Instance != null)
{
    var devices = DeviceManager.Instance.GetAllDevices();
    Debug.Log($"Total devices: {devices.Count}");
    foreach (var device in devices.Values)
    {
        Debug.Log($"Device: {device.deviceId}, Room: {device.roomId}, Type: {device.type}");
    }
}

// 检查特定房间的设备
string roomId = "BedRoom01"; // 替换为你要检查的房间 ID
if (DeviceManager.Instance != null)
{
    var devicesInRoom = DeviceManager.Instance.GetDevicesInRoom(roomId);
    Debug.Log($"Devices in '{roomId}': {devicesInRoom.Count}");
    foreach (var device in devicesInRoom)
    {
        Debug.Log($"  - {device.deviceId} ({device.type})");
    }
}
```

### 6. 常见问题和解决方案

#### 问题 1：`showAllRooms=true` 但看不到设备
- **原因**：设备列表显示所有设备，但可能被过滤或隐藏
- **解决**：检查 `UpdateDeviceList()` 中的过滤逻辑，确保设备有 `BaseDeviceController`

#### 问题 2：`showAllRooms=false` 但看不到设备
- **原因**：设备的 `roomId` 与 `currentRoomId` 不匹配
- **解决**：
  1. 检查 Console 日志，查看所有房间的设备分布
  2. 统一房间 ID 的命名（大小写、拼写）
  3. 确保设备的 `roomId` 与房间的 `roomId` 完全一致

#### 问题 3：设备列表为空
- **原因**：`DeviceManager` 未正确初始化或设备未注册
- **解决**：
  1. 检查 `DeviceManager` 是否存在
  2. 检查设备是否有 `DeviceDefinition` 组件
  3. 检查设备的 `deviceId` 和 `roomId` 是否设置

#### 问题 4：Dropdown 显示的房间名与设备不匹配
- **原因**：Dropdown 使用英文映射，但设备的 `roomId` 可能不同
- **解决**：检查 `GetEnglishNameFromRoomId()` 方法中的映射表，确保房间 ID 映射正确

## 调试技巧

1. **启用详细日志**：代码中已添加详细的 `Debug.Log`，查看 Console 输出
2. **使用断点**：在 `UpdateDeviceList()` 方法中设置断点，检查变量值
3. **检查 Inspector**：在运行时检查 `EnvironmentControlPanel` 的 Inspector，查看 `currentRoomId` 和 `showAllRooms` 的值
4. **对比其他房间**：检查其他房间（如 LivingRoom）是否有设备，对比配置差异

## 下一步

如果以上步骤都无法解决问题，请提供：
1. Console 中的完整日志输出
2. `EnvironmentControlPanel` Inspector 截图（显示 `currentRoomId` 和 `showAllRooms`）
3. 一个设备的 `DeviceDefinition` Inspector 截图（显示 `deviceId` 和 `roomId`）
4. `DeviceManager` 和 `RoomManager` 的初始化日志

