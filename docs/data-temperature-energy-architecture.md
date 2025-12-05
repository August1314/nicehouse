# 数据基座：温度与能耗架构说明

> 本文档回答关于多设备接入、温度影响机制、数据读写流程以及热力学建模的问题。

---

## 1. 多个电器如何接入耗电量接口？

### 当前实现机制

**✅ 已实现：通过基类自动接入**

所有电器设备通过继承 `BaseDeviceController` 基类自动接入能耗系统：

```csharp
// BaseDeviceController.cs
public virtual void TurnOn()
{
    // ...
    if (deviceDef != null && !string.IsNullOrEmpty(deviceDef.deviceId))
    {
        EnergyManager.Instance.StartConsume(deviceDef.deviceId);  // 自动接入
    }
}

public virtual void TurnOff()
{
    // ...
    if (deviceDef != null && !string.IsNullOrEmpty(deviceDef.deviceId))
    {
        EnergyManager.Instance.StopConsume(deviceDef.deviceId);  // 自动断开
    }
}
```

### 接入步骤

1. **设备脚本继承基类**
   ```csharp
   public class AirConditionerController : BaseDeviceController { }
   public class FanController : BaseDeviceController { }
   public class LightController : BaseDeviceController { }
   ```

2. **配置设备额定功率**
   - 在 Unity Inspector 中找到 `EnergyManager` 组件
   - 在 `deviceConfigs` 列表中添加设备配置：
     ```csharp
     deviceId: "AC_LivingRoom_01"
     ratedPower: 1500  // 瓦特（W）
     ```

3. **设备自动计费**
   - 设备开启时：`EnergyManager` 在 `Update()` 中持续累加用电量
   - 计算公式：`dailyConsumption += power * deltaTime / 3600f / 1000f` (W·s → kWh)

### 多设备示例

```csharp
// 场景中有多个设备：
// - AC_LivingRoom_01 (1500W)
// - AC_BedRoom_01 (1200W)
// - Fan_Kitchen_01 (50W)
// - Light_LivingRoom_01 (20W)

// 当所有设备开启时：
EnergyManager.Instance.StartConsume("AC_LivingRoom_01");  // 1500W
EnergyManager.Instance.StartConsume("AC_BedRoom_01");    // 1200W
EnergyManager.Instance.StartConsume("Fan_Kitchen_01");   // 50W
EnergyManager.Instance.StartConsume("Light_LivingRoom_01"); // 20W

// EnergyManager 在每帧 Update 中累加所有活跃设备的用电量
// 总功率 = 1500 + 1200 + 50 + 20 = 2770W
```

---

## 2. 两个空调在不同房间同时开启，如何影响全屋温度？

### ⚠️ 当前问题：未实现温度影响机制

**现状：**
- `AirConditionerController` 只设置了目标温度，但**不会实际修改** `EnvironmentDataStore` 中的温度数据
- `EnvironmentDataSimulator` 用简单的正弦波模拟温度，**不考虑设备影响**
- 结果是：空调开启后，温度数据不会变化，形成"假联动"

### 需要实现的机制

#### 方案 A：简化模型（推荐）

**核心思想：** 每个房间独立计算，设备只影响所在房间，相邻房间有轻微影响

```csharp
// 伪代码示例
public class TemperatureInfluenceSystem : MonoBehaviour
{
    // 每个房间的温度变化率
    private Dictionary<string, float> roomTemperatureChangeRate = new();
    
    void Update()
    {
        foreach (var room in RoomManager.Instance.GetAllRooms())
        {
            float changeRate = 0f;
            
            // 1. 检查房间内的设备影响
            var devices = DeviceManager.Instance.GetDevicesInRoom(room.roomId);
            foreach (var device in devices)
            {
                if (device.type == DeviceType.AirConditioner)
                {
                    var ac = device.GetComponent<AirConditionerController>();
                    if (ac.IsOn)
                    {
                        // 空调制冷/制热影响
                        var env = EnvironmentDataStore.Instance.GetRoomData(room.roomId);
                        float diff = env.temperature - ac.targetTemperature;
                        changeRate -= diff * 0.1f; // 向目标温度靠近
                    }
                }
            }
            
            // 2. 相邻房间的热传导（简化）
            changeRate += GetAdjacentRoomInfluence(room.roomId);
            
            // 3. 环境自然变化（原有模拟器）
            changeRate += GetNaturalChange(room.roomId);
            
            // 4. 更新温度
            var data = EnvironmentDataStore.Instance.GetOrCreateRoomData(room.roomId);
            data.temperature += changeRate * Time.deltaTime;
        }
    }
}
```

#### 方案 B：房间连通性模型

**核心思想：** 定义房间之间的连通关系（门、走廊），计算热传导

```csharp
// 需要定义房间连通图
public class RoomConnectivity
{
    public Dictionary<string, List<string>> adjacentRooms; // roomId -> [相邻房间列表]
    public Dictionary<string, float> heatTransferRate;     // 热传导系数
}
```

#### 方案 C：完整热力学建模（不推荐）

**核心思想：** 考虑房间体积、空气流动、热容、热阻等物理参数

- **优点：** 物理准确
- **缺点：** 实现复杂，计算量大，对游戏体验提升有限

---

## 3. 温度数据如何被读写？

### 当前数据流

```
┌─────────────────────────────────────────────────────────┐
│                   温度数据流（当前）                      │
└─────────────────────────────────────────────────────────┘

写入路径：
  EnvironmentDataSimulator (正弦波模拟)
    └─> EnvironmentDataStore.SetRoomData()
        └─> Dictionary<string, RoomEnvironmentData>

读取路径：
  EnvironmentController (环境智控)
    └─> EnvironmentDataStore.GetRoomData()
        └─> 判断温度阈值
            └─> 触发空调开启/关闭

问题：空调开启后不会影响温度，形成单向循环
```

### 理想数据流（需要实现）

```
┌─────────────────────────────────────────────────────────┐
│                 温度数据流（理想）                        │
└─────────────────────────────────────────────────────────┘

写入路径（多源）：
  1. EnvironmentDataSimulator (环境自然变化)
  2. TemperatureInfluenceSystem (设备影响)
  3. 手动设置（测试/调试）

读取路径（多消费者）：
  1. EnvironmentController (环境智控决策)
  2. AirConditionerController (空调目标温度对比)
  3. UI 显示 (DataDashboard, EnvironmentPanel)
  4. 热力图可视化 (LayeredVisualization)
```

### 数据读写接口

**当前接口：**
```csharp
// 读取
EnvironmentDataStore.Instance.TryGetRoomData(roomId, out var env);
float temp = env.temperature;

// 写入（只有 Simulator 在用）
EnvironmentDataStore.Instance.GetOrCreateRoomData(roomId).temperature = newValue;
```

**建议增强：**
```csharp
// 添加温度更新接口，支持增量更新
public void UpdateTemperature(string roomId, float delta)
{
    var data = GetOrCreateRoomData(roomId);
    data.temperature += delta;
}

// 添加温度影响接口，供设备调用
public void ApplyTemperatureInfluence(string roomId, float influenceRate, float targetTemp)
{
    var data = GetOrCreateRoomData(roomId);
    float diff = data.temperature - targetTemp;
    data.temperature -= diff * influenceRate * Time.deltaTime;
}
```

---

## 4. 是否要对温度热力学建模？

### 建议：实现简化模型，而非完整热力学

#### 为什么不需要完整热力学建模？

1. **游戏体验优先**
   - 用户关心的是"空调开启后温度会下降"的直观反馈
   - 不需要精确的物理模拟

2. **性能考虑**
   - 完整热力学需要计算每个房间的体积、热容、热阻、空气流动
   - 计算量大，可能影响帧率

3. **开发成本**
   - 完整建模需要大量参数配置和调试
   - 简化模型更容易实现和维护

#### 推荐的简化模型

**核心参数：**
- 设备影响系数（空调制冷/制热强度）
- 房间间热传导系数（相邻房间温度会相互影响）
- 环境自然变化（原有模拟器）

**实现示例：**
```csharp
public class SimplifiedTemperatureModel
{
    [Header("设备影响参数")]
    public float acCoolingRate = 0.5f;      // 空调制冷速率（度/秒）
    public float acHeatingRate = 0.3f;      // 空调制热速率（度/秒）
    
    [Header("房间间热传导")]
    public float heatTransferRate = 0.05f;  // 相邻房间热传导系数
    
    [Header("环境自然变化")]
    public float naturalChangeRate = 0.01f; // 环境自然变化速率
}
```

#### 何时需要完整热力学建模？

只有在以下情况下才考虑：
- 需要精确的能耗计算（例如：根据房间体积计算空调功率需求）
- 需要模拟复杂的空气流动（例如：新风系统的气流路径）
- 需要用于科学计算或工程仿真

---

## 5. 实现建议

### 优先级排序

1. **高优先级：实现设备对温度的影响**
   - 创建 `TemperatureInfluenceSystem` 组件
   - 让空调开启后能够实际改变房间温度
   - 确保温度变化是可见的（UI 显示、热力图）

2. **中优先级：房间间热传导**
   - 定义房间连通关系
   - 实现相邻房间温度相互影响

3. **低优先级：完整热力学建模**
   - 仅在需要精确计算时考虑

### 实现步骤

1. **创建温度影响系统**
   ```csharp
   // Assets/Scripts/Data/TemperatureInfluenceSystem.cs
   public class TemperatureInfluenceSystem : MonoBehaviour
   {
       // 实现设备对温度的影响计算
   }
   ```

2. **修改空调控制器**
   ```csharp
   // 在 AirConditionerController 中注册温度影响
   public void OnEnable()
   {
       TemperatureInfluenceSystem.Instance.RegisterDevice(this);
   }
   ```

3. **增强 EnvironmentDataStore**
   ```csharp
   // 添加温度更新接口
   public void ApplyTemperatureChange(string roomId, float delta) { }
   ```

4. **测试验证**
   - 开启两个房间的空调
   - 观察温度数据是否按预期变化
   - 验证 UI 显示是否正确

---

## 6. 总结

### 当前状态

| 功能 | 状态 | 说明 |
|------|------|------|
| 多设备能耗接入 | ✅ 已实现 | 通过 `BaseDeviceController` 自动接入 |
| 设备影响温度 | ❌ 未实现 | 需要创建 `TemperatureInfluenceSystem` |
| 房间间热传导 | ❌ 未实现 | 需要定义房间连通关系 |
| 温度数据读写 | ⚠️ 部分实现 | 只有 Simulator 在写，设备不写 |

### 关键问题

1. **多设备接入：** ✅ 已解决，通过基类自动接入
2. **温度影响：** ❌ 需要实现，建议使用简化模型
3. **数据读写：** ⚠️ 需要增强，支持多源写入
4. **热力学建模：** ❌ 不需要完整建模，简化模型即可

### 下一步行动

1. 实现 `TemperatureInfluenceSystem` 组件
2. 让空调控制器能够影响温度数据
3. 测试多房间空调同时开启的场景
4. 验证温度变化在 UI 和热力图中的显示

---

> **提示：** 如需实现温度影响系统，可以参考本文档中的方案 A（简化模型）开始开发。

