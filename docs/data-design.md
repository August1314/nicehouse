## 数据设计与接口说明

> 目标：梳理“好房子”系统中需要用到的**核心数据类型**，以及它们的**设计、存储方式与脚本接口**，并标注各功能模块所依赖的数据。

---

### 1. 数据总览

系统中的主要数据可以分为以下几类：

- **场景结构数据**
  - 房间与区域信息（Room / Area）
  - 设备与传感器信息（Device / Sensor）
- **环境与能耗数据**
  - 温度、湿度、PM2.5 / PM10、能耗（电量）
- **数字人与健康相关数据**
  - 行为状态（站立、行走、久坐、久浴、跌倒等）
  - 生命体征（心率、呼吸、体动、睡眠阶段）
- **安全与告警数据**
  - 烟雾浓度、燃气泄漏、告警记录
  - 一键呼叫事件
- **活动与热力图数据**
  - 各房间活动频次 / 停留时间
  - 时间轴上的历史数据（可选）

---

### 2. 场景结构数据

#### 2.1 房间与区域数据（Room / Area）

- **用途**
  - 承载环境数据（温度、PM2.5 等）、活动数据（热力图）、设备分布信息。

- **设计建议**

```csharp
public enum RoomType { LivingRoom, Bedroom, Kitchen, Bathroom, Study, Other }

public class RoomDefinition : MonoBehaviour
{
    public string roomId;          // 唯一 ID，例如 "LivingRoom01"
    public RoomType roomType;
    public string displayName;     // 在 UI 中显示的名称
}
```

- **存储方式**
  - 每个房间在场景中对应一个带有 `RoomDefinition` 的 GameObject。
  - 通过一个 `RoomManager` 在启动时收集所有房间引用，形成 `Dictionary<string, RoomDefinition>`。

- **被哪些模块使用**
  - 环境智控 / 能耗智控：按房间显示温度、能耗等。
  - 分层可视化：房间热力图着色。
  - 活动热力图：按房间统计活动频次。

#### 2.2 设备与传感器数据（Device / Sensor）

- **用途**
  - 表示空调、新风系统、空气净化器、风扇、灯具、窗户、烟雾传感器、PM2.5 传感器、一键呼叫按钮等。

- **设计建议**

```csharp
public enum DeviceType
{
    AirConditioner,
    FreshAirSystem,
    AirPurifier,
    Fan,
    Light,
    Window,
    SmokeSensor,
    Pm25Sensor,
    HelpButton,
    Other
}

public class DeviceDefinition : MonoBehaviour
{
    public string deviceId;      // 唯一 ID
    public DeviceType type;
    public string roomId;        // 所在房间，与 RoomDefinition 对应
}
```

- **存储方式**
  - 每个设备对象上挂 `DeviceDefinition`。
  - `DeviceManager` 负责按 ID 或房间查找设备。

- **被哪些模块使用**
  - 环境智控 / 能耗智控：控制空调、新风、净化器、风扇。
  - 安全防护：控制窗户开合、烟雾传感器状态、报警器。
  - 一键呼叫：Help Button 触发事件。

---

### 3. 环境与能耗数据

#### 3.1 房间环境数据（温度 / 湿度 / PM2.5 等）

- **数据内容**
  - 温度（℃）、湿度（%）、PM2.5、PM10 等。

- **设计建议**

```csharp
public class RoomEnvironmentData
{
    public float temperature;
    public float humidity;
    public float pm25;
    public float pm10;
}
```

```csharp
public class EnvironmentDataStore : MonoBehaviour
{
    // roomId -> data
    private Dictionary<string, RoomEnvironmentData> _roomEnvData;

    public RoomEnvironmentData GetRoomData(string roomId) { /* ... */ }
    public void SetRoomData(string roomId, RoomEnvironmentData data) { /* ... */ }
}
```

- **存储方式**
  - 常驻内存的字典（按房间 ID 索引）。
  - 初始值可固定写死，或从 JSON / ScriptableObject 读取。

- **接口使用者**
  - 环境智控模块：根据房间环境数据做阈值判断、联动设备。
  - 分层可视化模块：从这里读取温度 / PM2.5，转换成颜色。
  - UI 面板：点击房间时展示当前数据。

#### 3.2 能耗数据

- **数据内容**
  - 各设备耗电功率、各房间/全屋的累计用电量。

- **设计建议**

```csharp
public class EnergyData
{
    public float currentPower;     // 当前功率（W）
    public float dailyConsumption; // 当天累计（kWh）
}

public class EnergyManager : MonoBehaviour
{
    // deviceId -> energy data
    private Dictionary<string, EnergyData> _deviceEnergy;

    public void UpdateEnergyUsage(string deviceId, float deltaTime) { /* ... */ }
    public float GetRoomDailyConsumption(string roomId) { /* ... */ }
}
```

- **接口使用者**
  - 能耗智控模块：计算节能模式 vs 普通模式的用电差异。
  - UI 图表：展示用电曲线 / 饼图。

---

### 4. 数字人与健康数据

#### 4.1 数字人行为状态

- **数据内容**
  - 当前行为：站立、行走、久坐、久浴、睡觉、跌倒、坠床等。
  - 当前所在房间。

- **设计建议**

```csharp
public enum PersonState
{
    Idle,
    Walking,
    Sitting,
    Bathing,
    Sleeping,
    Fallen,
    OutOfBed
}

public class PersonStatus
{
    public PersonState state;
    public string currentRoomId;
    public float stateDuration; // 当前状态持续时间
}
```

```csharp
public class PersonStateController : MonoBehaviour
{
    public PersonStatus Status { get; private set; }

    public void ChangeState(PersonState newState, string roomId) { /* ... */ }
}
```

- **接口使用者**
  - 智能监护模块：根据 `state` 与 `stateDuration` 判断久坐、久浴、跌倒等异常。
  - 活动热力图：根据 `currentRoomId` 统计活动频次。

#### 4.2 生命体征数据（心率 / 呼吸 / 睡眠）

- **设计建议**

```csharp
public class VitalSignsData
{
    public int heartRate;       // bpm
    public int respirationRate; // 次/分钟
    public float bodyMovement;  // 体动强度
    public int sleepStage;      // 0-清醒 1-浅睡 2-深睡 等
}

public class HealthDataStore : MonoBehaviour
{
    public VitalSignsData Current { get; private set; }

    public void SimulateNextStep() { /* 更新 Current 的值 */ }
}
```

- **接口使用者**
  - 健康监测模块：根据阈值判断是否异常，触发告警。
  - 健康可视化：折线图 / 仪表盘展示数据变化。

---

### 5. 安全与告警数据

#### 5.1 烟雾与燃气浓度

- **数据内容**
  - 各房间的烟雾浓度 / 燃气浓度。

- **设计建议**

```csharp
public class SafetyData
{
    public float smokeLevel;
    public float gasLevel;
}

public class SafetyDataStore : MonoBehaviour
{
    private Dictionary<string, SafetyData> _roomSafety;

    public SafetyData GetRoomSafety(string roomId) { /* ... */ }
}
```

- **接口使用者**
  - 安全防护模块：根据浓度触发烟感 / 报警器 / 自动开窗。

#### 5.2 告警事件与日志

- **数据内容**
  - 告警类型（烟雾 / 跌倒 / 心率异常 / 一键呼叫等）、时间、房间、处理状态。

- **设计建议**

```csharp
public enum AlarmType
{
    Smoke,
    GasLeak,
    Fall,
    LongSitting,
    LongBathing,
    HealthAbnormal,
    EmergencyCall
}

public class AlarmRecord
{
    public AlarmType type;
    public string roomId;
    public System.DateTime time;
    public bool handled;
}

public class AlarmManager : MonoBehaviour
{
    private List<AlarmRecord> _records = new List<AlarmRecord>();

    public void AddAlarm(AlarmType type, string roomId) { /* ... */ }
    public IEnumerable<AlarmRecord> GetRecentAlarms(int count) { /* ... */ }
}
```

- **接口使用者**
  - 智能监护 / 健康监测 / 安全防护 / 一键呼叫模块：统一往这里写入告警记录。
  - UI 告警面板：读取最近告警，展示在列表中。

---

### 6. 活动与热力图数据

#### 6.1 房间活动频次 / 停留时间

- **数据内容**
  - 每个房间在一定时间段内的访问次数、累计停留时间。

- **设计建议**

```csharp
public class ActivityData
{
    public int visitCount;
    public float totalStayTime;
}

public class ActivityTracker : MonoBehaviour
{
    private Dictionary<string, ActivityData> _roomActivity;

    public void OnPersonEnterRoom(string roomId) { /* ... */ }
    public void OnPersonLeaveRoom(string roomId, float stayTime) { /* ... */ }

    public ActivityData GetRoomActivity(string roomId) { /* ... */ }
}
```

- **接口使用者**
  - 活动热力图：根据 `visitCount` 或 `totalStayTime` 计算颜色强度。
  - 分层查看：展示“高频活动区”与“低频活动区”。

#### 6.2 历史数据与时间轴（可选）

- **内容**
  - 按时间采样的环境 / 活动数据，用于回放一天的变化。
- **存储方式**
  - 简单情况下可以只在内存中保存最近 N 分钟数据。
  - 如需持久，可以预先做一份 JSON 配置文件，读取后驱动时间轴播放。

---

### 7. 接口依赖关系一览（模块 → 数据）

- **环境智控**
  - `EnvironmentDataStore`：获取各房间温度 / 湿度 / PM2.5。
  - `DeviceManager`：控制空调、新风、净化器、风扇等设备。
- **能耗智控**
  - `EnergyManager`：设备用电数据。
  - `DeviceManager` / `RoomManager`：按房间统计能耗。
- **智能监护**
  - `PersonStateController`：获取行为状态和所在房间。
  - `AlarmManager`：记录跌倒/久坐/久浴等告警。
- **健康监测**
  - `HealthDataStore`：心率、呼吸、体动、睡眠阶段。
  - `AlarmManager`：记录健康异常告警。
- **一键呼叫**
  - `AlarmManager`：记录 EmergencyCall 类型告警。
  - `DeviceManager`：控制灯光/报警器响应。
- **安全防护**
  - `SafetyDataStore`：烟雾 / 燃气浓度。
  - `DeviceManager`：控制窗户、报警器。
  - `AlarmManager`：记录安全类告警。
- **系统分层查看 / 热力图**
  - `EnvironmentDataStore`：温度 / PM2.5。
  - `ActivityTracker`：活动频次 / 停留时间。
  - `RoomManager`：房间与 Mesh 对应关系。

---

### 8. 实现顺序建议

1. 先搭好 **Room / Device / RoomEnvironmentData / EnvironmentDataStore / RoomManager / DeviceManager** 这些基础数据与管理类。
2. 然后按模块逐步接入：
   - 环境智控 & 分层可视化 → 优先使用环境数据。
   - 活动热力图 → 接入 `PersonStateController` 和 `ActivityTracker`。
   - 智能监护 / 健康监测 / 安全防护 → 逐步往 `AlarmManager` 写入记录。


