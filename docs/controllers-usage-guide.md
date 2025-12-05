# 设备控制器使用指南

> 本文档说明如何使用 `EnvironmentControl/Controllers` 文件夹中的设备控制器。

---

## 📋 控制器概览

### 控制器继承结构

```
BaseDeviceController (基类)
├── AirConditionerController (空调)
├── FanController (风扇)
├── AirPurifierController (空气净化器)
└── FreshAirController (新风系统)
```

### 控制器列表

| 控制器 | 功能 | 特殊功能 |
|--------|------|----------|
| `BaseDeviceController` | 基类，提供开关接口 | 自动接入能耗系统 |
| `AirConditionerController` | 空调控制 | 目标温度设置、扫风动画 |
| `FanController` | 风扇控制 | 扇叶旋转动画 |
| `AirPurifierController` | 空气净化器控制 | 扇叶旋转动画 |
| `FreshAirController` | 新风系统控制 | 状态指示器 |

---

## 🎮 在 Unity 中使用（场景设置）

### 步骤 1：在 GameObject 上添加组件

1. **选择设备 GameObject**（例如：空调模型）
2. **添加必需组件**：
   - `DeviceDefinition`（必需）
   - 对应的控制器（例如：`AirConditionerController`）

### 步骤 2：配置 DeviceDefinition

在 `DeviceDefinition` 组件中设置：
- **Device Id**: 唯一标识符，例如 `"AC_LivingRoom_01"`
- **Type**: 设备类型（AirConditioner / Fan / AirPurifier / FreshAirSystem）
- **Room Id**: 所在房间ID，例如 `"LivingRoom01"`

### 步骤 3：配置控制器参数

#### AirConditionerController（空调）

```
Inspector 配置：
├── 动画组件
│   ├── Vent Blade: 导风板 Transform（可选）
│   ├── Sweep Angle: 扫风角度（默认 30°）
│   └── Sweep Speed: 扫风速度（默认 60°/秒）
└── 状态
    └── Target Temperature: 目标温度（默认 24°C）
```

#### FanController（风扇）

```
Inspector 配置：
├── 动画组件
│   ├── Fan Blade: 扇叶 Transform（可选）
│   └── Rotation Speed: 旋转速度（默认 360°/秒）
```

#### AirPurifierController（净化器）

```
Inspector 配置：
├── 动画组件
│   ├── Fan Blade: 扇叶 Transform（可选）
│   └── Rotation Speed: 旋转速度（默认 360°/秒）
```

#### FreshAirController（新风系统）

```
Inspector 配置：
└── 状态指示
    └── Status Indicator: 状态指示器 GameObject（可选）
```

### 步骤 4：配置能耗（重要！）

在场景中找到 `EnergyManager` GameObject，在 Inspector 中：
1. 点击 `Device Configs` 列表的 `+` 按钮
2. 添加设备配置：
   - **Device Id**: 与 `DeviceDefinition` 中的 ID 一致
   - **Rated Power**: 额定功率（瓦特），例如空调 1500W

---

## 💻 在代码中使用

### 方式 1：通过 DeviceManager 查找并控制

```csharp
using NiceHouse.Data;
using NiceHouse.EnvironmentControl;

// 1. 通过设备ID查找
if (DeviceManager.Instance.TryGetDevice("AC_LivingRoom_01", out var device))
{
    var controller = device.GetComponent<AirConditionerController>();
    if (controller != null)
    {
        controller.TurnOn();  // 开启空调
        controller.SetTargetTemperature(26f);  // 设置目标温度
    }
}

// 2. 通过房间查找所有设备
var devices = DeviceManager.Instance.GetDevicesInRoom("LivingRoom01");
foreach (var device in devices)
{
    if (device.type == DeviceType.AirConditioner)
    {
        var controller = device.GetComponent<AirConditionerController>();
        controller?.TurnOn();
    }
}
```

### 方式 2：通过 EnvironmentController 统一控制

```csharp
using NiceHouse.EnvironmentControl;
using NiceHouse.Data;

// 手动控制设备（推荐用于UI按钮）
EnvironmentController.Instance.ManualControlDevice(
    roomId: "LivingRoom01",
    deviceType: DeviceType.AirConditioner,
    turnOn: true
);

// 控制其他设备
EnvironmentController.Instance.ManualControlDevice("LivingRoom01", DeviceType.Fan, true);
EnvironmentController.Instance.ManualControlDevice("LivingRoom01", DeviceType.AirPurifier, false);
```

### 方式 3：直接引用控制器组件

```csharp
// 如果已经知道 GameObject 引用
public class MyScript : MonoBehaviour
{
    public AirConditionerController acController;
    
    void Start()
    {
        // 直接控制
        acController.TurnOn();
        acController.SetTargetTemperature(25f);
        
        // 检查状态
        if (acController.IsOn)
        {
            Debug.Log("空调已开启");
        }
    }
}
```

---

## 🔧 控制器 API 参考

### BaseDeviceController（基类）

所有控制器都继承自此类，提供以下接口：

```csharp
// 开启设备
public virtual void TurnOn()

// 关闭设备
public virtual void TurnOff()

// 检查设备是否开启
public bool IsOn { get; }

// 获取当前状态
public DeviceState State { get; }
```

### AirConditionerController（空调）

```csharp
// 基础接口（继承自 BaseDeviceController）
controller.TurnOn();
controller.TurnOff();
controller.IsOn;

// 空调特有接口
controller.SetTargetTemperature(float temp);  // 设置目标温度
controller.targetTemperature;  // 获取/设置目标温度（属性）
```

### FanController / AirPurifierController（风扇/净化器）

```csharp
// 只有基础接口
controller.TurnOn();
controller.TurnOff();
controller.IsOn;
```

### FreshAirController（新风系统）

```csharp
// 只有基础接口
controller.TurnOn();
controller.TurnOff();
controller.IsOn;
```

---

## 📝 完整使用示例

### 示例 1：创建一个简单的设备控制脚本

```csharp
using UnityEngine;
using NiceHouse.Data;
using NiceHouse.EnvironmentControl;

public class SimpleDeviceControl : MonoBehaviour
{
    [Header("设备配置")]
    public string roomId = "LivingRoom01";
    public DeviceType deviceType = DeviceType.AirConditioner;
    
    void Update()
    {
        // 按空格键开启/关闭设备
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleDevice();
        }
    }
    
    void ToggleDevice()
    {
        var devices = DeviceManager.Instance.GetDevicesInRoom(roomId);
        foreach (var device in devices)
        {
            if (device.type == deviceType)
            {
                BaseDeviceController controller = null;
                
                // 根据设备类型获取对应的控制器
                switch (deviceType)
                {
                    case DeviceType.AirConditioner:
                        controller = device.GetComponent<AirConditionerController>();
                        break;
                    case DeviceType.Fan:
                        controller = device.GetComponent<FanController>();
                        break;
                    case DeviceType.AirPurifier:
                        controller = device.GetComponent<AirPurifierController>();
                        break;
                    case DeviceType.FreshAirSystem:
                        controller = device.GetComponent<FreshAirController>();
                        break;
                }
                
                if (controller != null)
                {
                    if (controller.IsOn)
                    {
                        controller.TurnOff();
                        Debug.Log($"关闭 {deviceType}");
                    }
                    else
                    {
                        controller.TurnOn();
                        Debug.Log($"开启 {deviceType}");
                        
                        // 如果是空调，设置目标温度
                        if (controller is AirConditionerController ac)
                        {
                            ac.SetTargetTemperature(24f);
                        }
                    }
                }
            }
        }
    }
}
```

### 示例 2：UI 按钮控制设备

```csharp
using UnityEngine;
using UnityEngine.UI;
using NiceHouse.Data;
using NiceHouse.EnvironmentControl;

public class DeviceControlButton : MonoBehaviour
{
    [Header("配置")]
    public string roomId = "LivingRoom01";
    public DeviceType deviceType = DeviceType.AirConditioner;
    
    private Button button;
    private BaseDeviceController controller;
    
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
        
        // 查找设备控制器
        FindController();
    }
    
    void FindController()
    {
        var devices = DeviceManager.Instance.GetDevicesInRoom(roomId);
        foreach (var device in devices)
        {
            if (device.type == deviceType)
            {
                controller = device.GetComponent<BaseDeviceController>();
                break;
            }
        }
    }
    
    void OnButtonClick()
    {
        if (controller == null)
        {
            Debug.LogWarning("未找到设备控制器");
            return;
        }
        
        // 切换设备状态
        if (controller.IsOn)
        {
            controller.TurnOff();
            button.GetComponentInChildren<Text>().text = "开启";
        }
        else
        {
            controller.TurnOn();
            button.GetComponentInChildren<Text>().text = "关闭";
        }
    }
    
    void Update()
    {
        // 更新按钮文本
        if (controller != null && button != null)
        {
            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = controller.IsOn ? "关闭" : "开启";
            }
        }
    }
}
```

### 示例 3：根据环境数据自动控制空调

```csharp
using UnityEngine;
using NiceHouse.Data;
using NiceHouse.EnvironmentControl;

public class AutoACControl : MonoBehaviour
{
    [Header("配置")]
    public string roomId = "LivingRoom01";
    public float highTempThreshold = 28f;  // 高温阈值
    public float lowTempThreshold = 18f;    // 低温阈值
    public float targetTemp = 24f;          // 目标温度
    
    private AirConditionerController acController;
    
    void Start()
    {
        // 查找空调控制器
        var devices = DeviceManager.Instance.GetDevicesInRoom(roomId);
        foreach (var device in devices)
        {
            if (device.type == DeviceType.AirConditioner)
            {
                acController = device.GetComponent<AirConditionerController>();
                break;
            }
        }
    }
    
    void Update()
    {
        if (acController == null || EnvironmentDataStore.Instance == null)
            return;
        
        // 获取房间温度
        if (EnvironmentDataStore.Instance.TryGetRoomData(roomId, out var env))
        {
            float temp = env.temperature;
            
            // 温度过高，开启制冷
            if (temp > highTempThreshold && !acController.IsOn)
            {
                acController.TurnOn();
                acController.SetTargetTemperature(targetTemp);
                Debug.Log($"温度过高 ({temp}°C)，开启空调制冷");
            }
            // 温度过低，开启制热
            else if (temp < lowTempThreshold && !acController.IsOn)
            {
                acController.TurnOn();
                acController.SetTargetTemperature(targetTemp);
                Debug.Log($"温度过低 ({temp}°C)，开启空调制热");
            }
            // 温度正常，关闭空调
            else if (temp >= lowTempThreshold && temp <= highTempThreshold && acController.IsOn)
            {
                acController.TurnOff();
                Debug.Log($"温度正常 ({temp}°C)，关闭空调");
            }
        }
    }
}
```

---

## ⚠️ 注意事项

### 1. 必需组件

- **DeviceDefinition 是必需的**：所有控制器都需要 `DeviceDefinition` 组件
- **设备ID必须唯一**：确保每个设备的 `deviceId` 在场景中唯一
- **房间ID必须匹配**：`DeviceDefinition.roomId` 必须与 `RoomDefinition.roomId` 一致

### 2. 能耗配置

- **必须配置能耗**：在 `EnergyManager` 中为每个设备配置额定功率
- **设备ID要一致**：`EnergyManager.deviceConfigs` 中的 `deviceId` 必须与 `DeviceDefinition.deviceId` 完全一致

### 3. 自动能耗管理

- 设备开启时自动调用 `EnergyManager.StartConsume()`
- 设备关闭时自动调用 `EnergyManager.StopConsume()`
- 无需手动管理能耗

### 4. 设备状态

- `DeviceState.Off`: 关闭
- `DeviceState.On`: 开启（未运行）
- `DeviceState.Running`: 运行中
- `DeviceState.Error`: 故障

---

## 🔗 相关文档

- [数据基座设计文档](./data-design.md)
- [数据API使用示例](./data-api-examples.md)
- [环境智控功能文档](./feature-environment-control.md)

---

## ❓ 常见问题

### Q: 如何知道设备是否成功开启？

```csharp
if (controller.IsOn)
{
    Debug.Log("设备已开启");
}
```

### Q: 如何获取设备的状态？

```csharp
DeviceState state = controller.State;
switch (state)
{
    case DeviceState.Off:
        Debug.Log("设备关闭");
        break;
    case DeviceState.Running:
        Debug.Log("设备运行中");
        break;
    // ...
}
```

### Q: 为什么设备开启后能耗没有增加？

检查：
1. `EnergyManager` 是否在场景中
2. `EnergyManager.deviceConfigs` 中是否配置了该设备的功率
3. `deviceId` 是否完全一致（大小写敏感）

### Q: 如何同时控制多个设备？

```csharp
var devices = DeviceManager.Instance.GetDevicesInRoom("LivingRoom01");
foreach (var device in devices)
{
    var controller = device.GetComponent<BaseDeviceController>();
    controller?.TurnOn();
}
```

---

> **提示**：更多示例代码请参考 `EnvironmentController.cs` 中的 `ManualControlDevice` 方法。

