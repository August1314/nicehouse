## 数据底座快速参考

> 本文档提供数据底座各模块的**快速参考表**，方便开发时查阅。

---

### 单例访问模式

所有 Manager/Store 都是单例，通过 `Instance` 访问：

```csharp
using NiceHouse.Data;

// 检查单例是否存在
if (RoomManager.Instance != null)
{
    // 使用 API
}
```

---

### 1. RoomManager（房间管理）

| API | 说明 | 返回值 |
|-----|------|--------|
| `TryGetRoom(roomId, out room)` | 获取房间 | `bool` |
| `GetAllRooms()` | 获取所有房间 | `IReadOnlyDictionary<string, RoomDefinition>` |

**示例：**
```csharp
if (RoomManager.Instance.TryGetRoom("LivingRoom01", out var room))
{
    Debug.Log(room.displayName);
}
```

---

### 2. DeviceManager（设备管理）

| API | 说明 | 返回值 |
|-----|------|--------|
| `TryGetDevice(deviceId, out device)` | 获取设备 | `bool` |
| `GetDevicesInRoom(roomId)` | 获取房间内所有设备 | `IReadOnlyList<DeviceDefinition>` |
| `GetAllDevices()` | 获取所有设备 | `IReadOnlyDictionary<string, DeviceDefinition>` |

**示例：**
```csharp
var devices = DeviceManager.Instance.GetDevicesInRoom("LivingRoom01");
```

---

### 3. EnvironmentDataStore（环境数据）

| API | 说明 | 返回值 |
|-----|------|--------|
| `TryGetRoomData(roomId, out data)` | 获取环境数据 | `bool` |
| `GetOrCreateRoomData(roomId)` | 获取或创建环境数据 | `RoomEnvironmentData` |
| `GetAllRoomData()` | 获取所有房间环境数据 | `IReadOnlyDictionary<string, RoomEnvironmentData>` |

**数据字段：**
- `temperature` - 温度（℃）
- `humidity` - 湿度（%）
- `pm25` - PM2.5（μg/m³）
- `pm10` - PM10（μg/m³）

**示例：**
```csharp
if (EnvironmentDataStore.Instance.TryGetRoomData("LivingRoom01", out var env))
{
    float temp = env.temperature;
}
```

---

### 4. EnergyManager（能耗管理）

| API | 说明 | 返回值 |
|-----|------|--------|
| `StartConsume(deviceId)` | 设备开始耗电 | `void` |
| `StopConsume(deviceId)` | 设备停止耗电 | `void` |
| `GetDeviceDailyConsumption(deviceId)` | 获取设备累计用电量 | `float` (kWh) |

**示例：**
```csharp
EnergyManager.Instance.StartConsume("AC_LivingRoom_01");
float kwh = EnergyManager.Instance.GetDeviceDailyConsumption("AC_LivingRoom_01");
```

---

### 5. PersonStateController（数字人状态）

| API | 说明 | 返回值 |
|-----|------|--------|
| `Status` | 当前状态（属性） | `PersonStatus` |
| `ChangeState(state, roomId)` | 改变状态 | `void` |
| `GetStateDuration()` | 获取状态持续时间 | `float` (秒) |
| `OnStateChanged` | 状态变化事件 | `Action<PersonState, string>` |

**状态枚举：**
- `Idle` - 空闲
- `Walking` - 行走
- `Sitting` - 久坐
- `Bathing` - 久浴
- `Sleeping` - 睡觉
- `Fallen` - 跌倒
- `OutOfBed` - 坠床

**示例：**
```csharp
PersonStateController.Instance.ChangeState(PersonState.Sitting, "LivingRoom01");
var status = PersonStateController.Instance.Status;
```

---

### 6. HealthDataStore（健康数据）

| API | 说明 | 返回值 |
|-----|------|--------|
| `Current` | 当前生命体征（属性） | `VitalSignsData` |
| `SimulateNextStep()` | 模拟下一步数据变化 | `void` |

**数据字段：**
- `heartRate` - 心率（bpm）
- `respirationRate` - 呼吸率（次/分钟）
- `bodyMovement` - 体动强度（0-1）
- `sleepStage` - 睡眠阶段（0-清醒，1-浅睡，2-深睡）

**示例：**
```csharp
var health = HealthDataStore.Instance.Current;
int hr = health.heartRate;
```

---

### 7. ActivityTracker（活动追踪）

| API | 说明 | 返回值 |
|-----|------|--------|
| `GetRoomActivity(roomId)` | 获取房间活动数据 | `ActivityData` |
| `OnPersonEnterRoom(roomId)` | 数字人进入房间 | `void` |
| `OnPersonLeaveRoom(roomId, stayTime)` | 数字人离开房间 | `void` |
| `GetAllRoomActivity()` | 获取所有房间活动数据 | `IReadOnlyDictionary<string, ActivityData>` |

**数据字段：**
- `visitCount` - 访问次数
- `totalStayTime` - 累计停留时间（秒）

**示例：**
```csharp
var activity = ActivityTracker.Instance.GetRoomActivity("LivingRoom01");
int visits = activity.visitCount;
```

---

### 8. SafetyDataStore（安全数据）

| API | 说明 | 返回值 |
|-----|------|--------|
| `TryGetRoomSafety(roomId, out safety)` | 获取安全数据 | `bool` |
| `GetOrCreateRoomSafety(roomId)` | 获取或创建安全数据 | `SafetyData` |
| `SetSmokeLevel(roomId, level)` | 设置烟雾浓度 | `void` |
| `SetGasLevel(roomId, level)` | 设置燃气浓度 | `void` |

**数据字段：**
- `smokeLevel` - 烟雾浓度（0-100）
- `gasLevel` - 燃气浓度（0-100）

**示例：**
```csharp
if (SafetyDataStore.Instance.TryGetRoomSafety("LivingRoom01", out var safety))
{
    float smoke = safety.smokeLevel;
}
```

---

### 9. AlarmManager（告警管理）

| API | 说明 | 返回值 |
|-----|------|--------|
| `AddAlarm(type, roomId)` | 添加告警记录 | `void` |
| `GetRecentAlarms(count)` | 获取最近告警 | `IEnumerable<AlarmRecord>` |
| `GetUnhandledAlarms()` | 获取未处理告警 | `IEnumerable<AlarmRecord>` |
| `MarkHandled(record)` | 标记告警为已处理 | `void` |
| `OnAlarmAdded` | 告警添加事件 | `Action<AlarmRecord>` |

**告警类型：**
- `Smoke` - 烟雾
- `GasLeak` - 燃气泄漏
- `Fall` - 跌倒
- `LongSitting` - 久坐
- `LongBathing` - 久浴
- `HealthAbnormal` - 健康异常
- `EmergencyCall` - 一键呼叫

**示例：**
```csharp
AlarmManager.Instance.AddAlarm(AlarmType.Smoke, "LivingRoom01");
var alarms = AlarmManager.Instance.GetRecentAlarms(5);
```

---

### 常用组合操作

#### 获取房间所有设备的能耗总和

```csharp
float totalEnergy = 0f;
var devices = DeviceManager.Instance.GetDevicesInRoom("LivingRoom01");
foreach (var device in devices)
{
    totalEnergy += EnergyManager.Instance.GetDeviceDailyConsumption(device.deviceId);
}
```

#### 判断环境超标并触发告警

```csharp
if (EnvironmentDataStore.Instance.TryGetRoomData("LivingRoom01", out var env))
{
    if (env.pm25 > 75f)
    {
        AlarmManager.Instance.AddAlarm(AlarmType.Smoke, "LivingRoom01");
    }
}
```

#### 监听数字人状态变化

```csharp
PersonStateController.Instance.OnStateChanged += (state, roomId) =>
{
    if (state == PersonState.Sitting)
    {
        // 检查久坐时间
        StartCoroutine(CheckLongSitting(roomId));
    }
};
```

---

### 注意事项

1. **单例检查**：使用前务必检查 `Instance != null`
2. **房间ID一致性**：确保 `roomId` 在所有模块中保持一致
3. **设备ID唯一性**：每个设备的 `deviceId` 必须唯一
4. **事件订阅**：记得在 `OnDestroy` 中取消事件订阅，避免内存泄漏

---

> 更多详细示例请参考 `docs/data-api-examples.md`

