## 数据底座 API 使用示例

> 本文档提供数据底座各模块的**典型调用代码示例**，方便功能模块开发者快速接入。

---

### 1. 房间与设备查询

#### 获取房间信息

```csharp
using NiceHouse.Data;

// 通过 RoomManager 获取房间
if (RoomManager.Instance.TryGetRoom("LivingRoom01", out var room))
{
    Debug.Log($"房间名称: {room.displayName}, 类型: {room.roomType}");
}

// 获取所有房间
var allRooms = RoomManager.Instance.GetAllRooms();
foreach (var room in allRooms.Values)
{
    Debug.Log($"房间ID: {room.roomId}, 名称: {room.displayName}");
}
```

#### 获取设备信息

```csharp
// 通过 DeviceManager 获取设备
if (DeviceManager.Instance.TryGetDevice("AC_LivingRoom_01", out var device))
{
    Debug.Log($"设备类型: {device.type}, 所在房间: {device.roomId}");
}

// 获取某个房间的所有设备
var devices = DeviceManager.Instance.GetDevicesInRoom("LivingRoom01");
foreach (var device in devices)
{
    Debug.Log($"设备: {device.deviceId}, 类型: {device.type}");
}
```

---

### 2. 环境数据（温度/湿度/PM2.5）

#### 获取房间环境数据

```csharp
using NiceHouse.Data;

// 获取房间当前环境数据
if (EnvironmentDataStore.Instance.TryGetRoomData("LivingRoom01", out var env))
{
    float temp = env.temperature;      // 温度（℃）
    float humidity = env.humidity;     // 湿度（%）
    float pm25 = env.pm25;             // PM2.5（μg/m³）
    
    Debug.Log($"温度: {temp}°C, 湿度: {humidity}%, PM2.5: {pm25}");
}

// 或者使用 GetOrCreateRoomData（如果数据不存在会自动创建）
var envData = EnvironmentDataStore.Instance.GetOrCreateRoomData("LivingRoom01");
envData.temperature = 25.0f;  // 手动设置温度
```

#### 判断环境是否超标（用于环境智控）

```csharp
// 示例：PM2.5 超标时触发净化器
if (EnvironmentDataStore.Instance.TryGetRoomData("LivingRoom01", out var env))
{
    if (env.pm25 > 75f)  // PM2.5 超过 75 μg/m³
    {
        // 触发空气净化器开启
        // 例如：AirPurifierController.Instance.TurnOn();
        Debug.Log("PM2.5超标，启动净化器");
    }
}
```

---

### 3. 能耗数据

#### 获取设备能耗

```csharp
using NiceHouse.Data;

// 获取设备累计用电量（kWh）
float consumption = EnergyManager.Instance.GetDeviceDailyConsumption("AC_LivingRoom_01");
Debug.Log($"设备累计用电: {consumption} kWh");

// 设备开启时通知 EnergyManager
EnergyManager.Instance.StartConsume("AC_LivingRoom_01");

// 设备关闭时通知 EnergyManager
EnergyManager.Instance.StopConsume("AC_LivingRoom_01");
```

#### 计算房间总能耗

```csharp
// 计算某个房间所有设备的累计能耗
float totalEnergy = 0f;
var devices = DeviceManager.Instance.GetDevicesInRoom("LivingRoom01");
foreach (var device in devices)
{
    totalEnergy += EnergyManager.Instance.GetDeviceDailyConsumption(device.deviceId);
}
Debug.Log($"房间总能耗: {totalEnergy} kWh");
```

---

### 4. 数字人状态

#### 获取数字人当前状态

```csharp
using NiceHouse.Data;

if (PersonStateController.Instance != null && PersonStateController.Instance.Status != null)
{
    var status = PersonStateController.Instance.Status;
    PersonState state = status.state;              // 当前状态
    string roomId = status.currentRoomId;          // 所在房间
    float duration = status.stateDuration;         // 状态持续时间（秒）
    
    Debug.Log($"数字人状态: {state}, 房间: {roomId}, 持续时间: {duration}秒");
}
```

#### 改变数字人状态（用于智能监护）

```csharp
// 改变数字人状态
PersonStateController.Instance.ChangeState(PersonState.Sitting, "LivingRoom01");

// 监听状态变化事件
PersonStateController.Instance.OnStateChanged += (newState, roomId) =>
{
    Debug.Log($"状态变化: {newState} in {roomId}");
    
    // 例如：久坐超过5分钟触发告警
    if (newState == PersonState.Sitting)
    {
        // 启动计时器，5分钟后检查
    }
};
```

#### 判断异常状态（用于智能监护）

```csharp
// 检查久坐/久浴等异常
var status = PersonStateController.Instance.Status;
if (status.state == PersonState.Sitting && status.stateDuration > 300f)  // 5分钟
{
    AlarmManager.Instance.AddAlarm(AlarmType.LongSitting, status.currentRoomId);
}
```

---

### 5. 健康数据

#### 获取生命体征数据

```csharp
using NiceHouse.Data;

if (HealthDataStore.Instance != null && HealthDataStore.Instance.Current != null)
{
    var health = HealthDataStore.Instance.Current;
    int heartRate = health.heartRate;           // 心率（bpm）
    int respiration = health.respirationRate;   // 呼吸率（次/分钟）
    float movement = health.bodyMovement;       // 体动强度（0-1）
    int sleepStage = health.sleepStage;         // 睡眠阶段（0-清醒，1-浅睡，2-深睡）
    
    Debug.Log($"心率: {heartRate} bpm, 呼吸: {respiration} /min");
}
```

#### 判断健康异常（用于健康监测）

```csharp
var health = HealthDataStore.Instance.Current;
if (health.heartRate < 60 || health.heartRate > 100)
{
    AlarmManager.Instance.AddAlarm(AlarmType.HealthAbnormal, "LivingRoom01");
}
```

---

### 6. 活动追踪（用于热力图）

#### 获取房间活动数据

```csharp
using NiceHouse.Data;

// 获取房间活动数据
var activity = ActivityTracker.Instance.GetRoomActivity("LivingRoom01");
int visitCount = activity.visitCount;        // 访问次数
float stayTime = activity.totalStayTime;     // 累计停留时间（秒）

Debug.Log($"访问次数: {visitCount}, 停留时间: {stayTime}秒");

// 计算热力值（用于热力图颜色映射）
float heatValue = visitCount * 0.5f + stayTime / 60f;  // 自定义公式
```

#### 手动记录活动（如果数字人状态变化时没有自动触发）

```csharp
// 数字人进入房间
ActivityTracker.Instance.OnPersonEnterRoom("LivingRoom01");

// 数字人离开房间（需要传入停留时间）
ActivityTracker.Instance.OnPersonLeaveRoom("LivingRoom01", 120f);  // 停留了120秒
```

---

### 7. 安全数据

#### 获取安全数据（烟雾/燃气）

```csharp
using NiceHouse.Data;

// 获取房间安全数据
if (SafetyDataStore.Instance.TryGetRoomSafety("LivingRoom01", out var safety))
{
    float smoke = safety.smokeLevel;  // 烟雾浓度（0-100）
    float gas = safety.gasLevel;      // 燃气浓度（0-100）
    
    Debug.Log($"烟雾: {smoke}, 燃气: {gas}");
}
```

#### 设置安全数据（用于模拟火灾/燃气泄漏）

```csharp
// 模拟烟雾浓度升高
SafetyDataStore.Instance.SetSmokeLevel("LivingRoom01", 80f);

// 模拟燃气泄漏
SafetyDataStore.Instance.SetGasLevel("Kitchen01", 50f);
```

#### 判断安全异常（用于安全防护）

```csharp
if (SafetyDataStore.Instance.TryGetRoomSafety("LivingRoom01", out var safety))
{
    if (safety.smokeLevel > 50f)  // 烟雾浓度超过50
    {
        // 触发烟感报警
        AlarmManager.Instance.AddAlarm(AlarmType.Smoke, "LivingRoom01");
        
        // 自动开窗
        // WindowController.Instance.OpenWindow("LivingRoom01");
    }
}
```

---

### 8. 告警系统

#### 添加告警记录

```csharp
using NiceHouse.Data;

// 添加各种类型的告警
AlarmManager.Instance.AddAlarm(AlarmType.Smoke, "LivingRoom01");
AlarmManager.Instance.AddAlarm(AlarmType.LongSitting, "BedRoom01");
AlarmManager.Instance.AddAlarm(AlarmType.HealthAbnormal, "LivingRoom01");
AlarmManager.Instance.AddAlarm(AlarmType.EmergencyCall, "BathRoom01");
```

#### 获取告警记录

```csharp
// 获取最近5条告警
var recentAlarms = AlarmManager.Instance.GetRecentAlarms(5);
foreach (var alarm in recentAlarms)
{
    Debug.Log($"{alarm.type} in {alarm.roomId} at {alarm.time:HH:mm:ss}");
}

// 获取所有未处理的告警
var unhandled = AlarmManager.Instance.GetUnhandledAlarms();
foreach (var alarm in unhandled)
{
    Debug.Log($"未处理告警: {alarm.type} in {alarm.roomId}");
}

// 标记告警为已处理
AlarmManager.Instance.MarkHandled(alarm);
```

#### 监听告警事件

```csharp
// 监听新告警
AlarmManager.Instance.OnAlarmAdded += (alarm) =>
{
    Debug.Log($"新告警: {alarm.type} in {alarm.roomId}");
    
    // 例如：显示告警UI、播放声音等
    // AlarmUIController.Instance.ShowAlarm(alarm);
};
```

---

### 9. 综合示例：环境智控模块

```csharp
using UnityEngine;
using NiceHouse.Data;

public class EnvironmentControlSystem : MonoBehaviour
{
    public string roomId = "LivingRoom01";
    
    void Update()
    {
        // 1. 获取环境数据
        if (EnvironmentDataStore.Instance.TryGetRoomData(roomId, out var env))
        {
            // 2. 判断PM2.5是否超标
            if (env.pm25 > 75f)
            {
                // 3. 开启空气净化器
                var devices = DeviceManager.Instance.GetDevicesInRoom(roomId);
                foreach (var device in devices)
                {
                    if (device.type == DeviceType.AirPurifier)
                    {
                        // 开启净化器（假设有 AirPurifierController）
                        // AirPurifierController.Instance.TurnOn(device.deviceId);
                        
                        // 4. 记录能耗
                        EnergyManager.Instance.StartConsume(device.deviceId);
                    }
                }
                
                // 5. 添加告警记录
                AlarmManager.Instance.AddAlarm(AlarmType.Smoke, roomId);
            }
        }
    }
}
```

---

### 10. 综合示例：智能监护模块

```csharp
using UnityEngine;
using NiceHouse.Data;

public class MonitoringSystem : MonoBehaviour
{
    void Start()
    {
        // 监听数字人状态变化
        PersonStateController.Instance.OnStateChanged += OnPersonStateChanged;
    }
    
    void OnPersonStateChanged(PersonState newState, string roomId)
    {
        // 检查久坐
        if (newState == PersonState.Sitting)
        {
            StartCoroutine(CheckLongSitting(roomId));
        }
        
        // 检查跌倒
        if (newState == PersonState.Fallen)
        {
            AlarmManager.Instance.AddAlarm(AlarmType.Fall, roomId);
        }
    }
    
    System.Collections.IEnumerator CheckLongSitting(string roomId)
    {
        yield return new WaitForSeconds(300f);  // 等待5分钟
        
        var status = PersonStateController.Instance.Status;
        if (status.state == PersonState.Sitting && status.currentRoomId == roomId)
        {
            AlarmManager.Instance.AddAlarm(AlarmType.LongSitting, roomId);
        }
    }
}
```

---

### 11. 综合示例：热力图渲染

```csharp
using UnityEngine;
using NiceHouse.Data;

public class HeatmapRenderer : MonoBehaviour
{
    public Material heatmapMaterial;  // 热力图材质
    
    void Update()
    {
        // 获取所有房间的活动数据
        var allActivity = ActivityTracker.Instance.GetAllRoomActivity();
        
        foreach (var kvp in allActivity)
        {
            string roomId = kvp.Key;
            var activity = kvp.Value;
            
            // 计算热力值（访问次数 + 停留时间）
            float heatValue = activity.visitCount * 0.5f + activity.totalStayTime / 60f;
            
            // 获取房间的Mesh（假设有 RoomMeshManager）
            // var roomMesh = RoomMeshManager.Instance.GetRoomMesh(roomId);
            
            // 根据热力值设置颜色（红-黄-蓝渐变）
            Color heatColor = GetHeatColor(heatValue);
            // roomMesh.material.color = heatColor;
        }
    }
    
    Color GetHeatColor(float value)
    {
        // 简单的颜色映射：高值=红色，低值=蓝色
        if (value > 10f) return Color.red;
        if (value > 5f) return Color.yellow;
        return Color.blue;
    }
}
```

---

### 快速参考：常用 API 列表

| 功能 | API | 说明 |
|------|-----|------|
| **房间查询** | `RoomManager.Instance.TryGetRoom(roomId, out room)` | 获取房间 |
| **设备查询** | `DeviceManager.Instance.TryGetDevice(deviceId, out device)` | 获取设备 |
| **环境数据** | `EnvironmentDataStore.Instance.TryGetRoomData(roomId, out env)` | 获取温度/湿度/PM2.5 |
| **能耗数据** | `EnergyManager.Instance.GetDeviceDailyConsumption(deviceId)` | 获取设备用电量 |
| **数字人状态** | `PersonStateController.Instance.Status` | 获取当前状态 |
| **改变状态** | `PersonStateController.Instance.ChangeState(state, roomId)` | 改变数字人状态 |
| **健康数据** | `HealthDataStore.Instance.Current` | 获取心率/呼吸等 |
| **活动数据** | `ActivityTracker.Instance.GetRoomActivity(roomId)` | 获取访问次数/停留时间 |
| **安全数据** | `SafetyDataStore.Instance.TryGetRoomSafety(roomId, out safety)` | 获取烟雾/燃气浓度 |
| **添加告警** | `AlarmManager.Instance.AddAlarm(type, roomId)` | 添加告警记录 |
| **获取告警** | `AlarmManager.Instance.GetRecentAlarms(count)` | 获取最近告警 |

---

> **提示**：所有 Manager/Store 都是单例模式，通过 `Instance` 访问。如果 `Instance` 为 `null`，说明组件还没有挂到场景中。

