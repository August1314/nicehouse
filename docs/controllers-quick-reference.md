# 设备控制器快速参考

> 快速查找常用代码片段

---

## 🚀 快速开始

### 1. 开启/关闭设备（最简单）

```csharp
// 通过 EnvironmentController（推荐）
EnvironmentController.Instance.ManualControlDevice(
    "LivingRoom01", 
    DeviceType.AirConditioner, 
    true  // true=开启, false=关闭
);
```

### 2. 查找并控制设备

```csharp
// 通过设备ID
if (DeviceManager.Instance.TryGetDevice("AC_LivingRoom_01", out var device))
{
    var controller = device.GetComponent<AirConditionerController>();
    controller?.TurnOn();
}

// 通过房间查找
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

---

## 📋 常用代码片段

### 控制空调

```csharp
// 查找空调
var devices = DeviceManager.Instance.GetDevicesInRoom("LivingRoom01");
var ac = devices.FirstOrDefault(d => d.type == DeviceType.AirConditioner);
var controller = ac?.GetComponent<AirConditionerController>();

// 开启并设置温度
controller?.TurnOn();
controller?.SetTargetTemperature(24f);

// 关闭
controller?.TurnOff();

// 检查状态
if (controller?.IsOn == true)
{
    Debug.Log($"空调已开启，目标温度: {controller.targetTemperature}°C");
}
```

### 控制风扇

```csharp
var devices = DeviceManager.Instance.GetDevicesInRoom("LivingRoom01");
var fan = devices.FirstOrDefault(d => d.type == DeviceType.Fan);
var controller = fan?.GetComponent<FanController>();

controller?.TurnOn();  // 开启
controller?.TurnOff(); // 关闭
```

### 控制净化器

```csharp
var devices = DeviceManager.Instance.GetDevicesInRoom("LivingRoom01");
var purifier = devices.FirstOrDefault(d => d.type == DeviceType.AirPurifier);
var controller = purifier?.GetComponent<AirPurifierController>();

controller?.TurnOn();
controller?.TurnOff();
```

### 控制新风系统

```csharp
var devices = DeviceManager.Instance.GetDevicesInRoom("LivingRoom01");
var freshAir = devices.FirstOrDefault(d => d.type == DeviceType.FreshAirSystem);
var controller = freshAir?.GetComponent<FreshAirController>();

controller?.TurnOn();
controller?.TurnOff();
```

---

## 🎯 实际应用场景

### 场景1：UI按钮控制

```csharp
public void OnACButtonClick()
{
    EnvironmentController.Instance.ManualControlDevice(
        "LivingRoom01", 
        DeviceType.AirConditioner, 
        true
    );
}
```

### 场景2：根据温度自动控制

```csharp
void Update()
{
    if (EnvironmentDataStore.Instance.TryGetRoomData("LivingRoom01", out var env))
    {
        if (env.temperature > 28f)
        {
            // 温度过高，开启空调
            EnvironmentController.Instance.ManualControlDevice(
                "LivingRoom01", 
                DeviceType.AirConditioner, 
                true
            );
        }
    }
}
```

### 场景3：控制房间内所有设备

```csharp
void TurnOnAllDevicesInRoom(string roomId)
{
    var devices = DeviceManager.Instance.GetDevicesInRoom(roomId);
    foreach (var device in devices)
    {
        BaseDeviceController controller = null;
        
        switch (device.type)
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
        
        controller?.TurnOn();
    }
}
```

### 场景4：获取设备状态并显示

```csharp
void DisplayDeviceStatus(string roomId)
{
    var devices = DeviceManager.Instance.GetDevicesInRoom(roomId);
    foreach (var device in devices)
    {
        var controller = device.GetComponent<BaseDeviceController>();
        if (controller != null)
        {
            string status = controller.IsOn ? "开启" : "关闭";
            Debug.Log($"{device.deviceId}: {status}");
        }
    }
}
```

---

## 🔧 控制器类型对照表

| 设备类型 | 控制器类 | 特殊方法 |
|---------|---------|---------|
| `DeviceType.AirConditioner` | `AirConditionerController` | `SetTargetTemperature(float)` |
| `DeviceType.Fan` | `FanController` | 无 |
| `DeviceType.AirPurifier` | `AirPurifierController` | 无 |
| `DeviceType.FreshAirSystem` | `FreshAirController` | 无 |

---

## ⚡ 一行代码控制

```csharp
// 开启空调
EnvironmentController.Instance.ManualControlDevice("LivingRoom01", DeviceType.AirConditioner, true);

// 关闭风扇
EnvironmentController.Instance.ManualControlDevice("LivingRoom01", DeviceType.Fan, false);

// 开启净化器
EnvironmentController.Instance.ManualControlDevice("LivingRoom01", DeviceType.AirPurifier, true);
```

---

> **更多信息**：查看 [完整使用指南](./controllers-usage-guide.md)

