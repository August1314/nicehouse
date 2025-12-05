# How to Verify Air Conditioner Status

This guide explains multiple ways to verify if an air conditioner is turned on.

---

## Method 1: Check in Unity Inspector (Runtime)

### Steps:

1. **Run the game** (Play mode)
2. **Select the AC GameObject** in Hierarchy
3. **Check the Inspector panel**
   - Find `Air Conditioner Controller (Script)` component
   - Check the `Is On` property (if exposed) or check `currentState`

**Note:** The `IsOn` property is not directly visible in Inspector, but you can check the controller's state.

---

## Method 2: Check via Code

### Check Device State Directly

```csharp
using NiceHouse.Data;
using NiceHouse.EnvironmentControl;

// Method 1: Get device by ID
if (DeviceManager.Instance.TryGetDevice("AC_bedroom", out var device))
{
    var controller = device.GetComponent<AirConditionerController>();
    if (controller != null)
    {
        bool isOn = controller.IsOn;
        Debug.Log($"AC is {(isOn ? "ON" : "OFF")}");
        
        if (isOn)
        {
            Debug.Log($"Target Temperature: {controller.targetTemperature}°C");
        }
    }
}

// Method 2: Get device by room
var devices = DeviceManager.Instance.GetDevicesInRoom("BedRoom01");
foreach (var device in devices)
{
    if (device.type == DeviceType.AirConditioner)
    {
        var controller = device.GetComponent<AirConditionerController>();
        if (controller != null)
        {
            Debug.Log($"AC {device.deviceId} is {(controller.IsOn ? "ON" : "OFF")}");
        }
    }
}
```

### Check Energy Consumption

If the AC is on, it should be consuming energy:

```csharp
using NiceHouse.Data;

// Check if device is consuming energy
string deviceId = "AC_bedroom";

// Get current power consumption
var energyData = EnergyManager.Instance.GetDeviceEnergyData(deviceId);
if (energyData != null)
{
    if (energyData.currentPower > 0f)
    {
        Debug.Log($"AC is ON - Current Power: {energyData.currentPower}W");
        Debug.Log($"Daily Consumption: {energyData.dailyConsumption:F3} kWh");
    }
    else
    {
        Debug.Log("AC is OFF - No power consumption");
    }
}
```

---

## Method 3: Check Console Logs

When the AC is turned on/off, it logs messages to the console:

**When turned ON:**
```
[AirConditioner] AC_bedroom turned ON, target temp: 24°C
```

**When turned OFF:**
```
[AirConditioner] AC_bedroom turned OFF
```

**Steps:**
1. Open Unity Console window (`Window` → `General` → `Console`)
2. Turn the AC on/off
3. Check for log messages

---

## Method 4: Check UI Panel Status

### Environment Control Panel

1. **Open the Environment Control Panel**
2. **Select the correct room** (must match AC's roomId: "BedRoom01")
3. **Check the Air Conditioner status text**
   - Should show "ON" (green) if running
   - Should show "OFF" (gray) if stopped
   - Should show "N/A" (red) if device not found

### Control Hub Panel

If using the Control Hub system:
1. Open Control Hub panel
2. Navigate to Environment Control module
3. Check the AC status indicator

---

## Method 5: Visual Indicators

### Animation (if configured)

If the AC has a `ventBlade` Transform configured:
- **When ON**: The vent blade should be sweeping (rotating left and right)
- **When OFF**: The vent blade should be stationary

**Check in Scene view:**
1. Select the AC GameObject
2. In Scene view, watch the vent blade Transform
3. If it's rotating, the AC is on

---

## Method 6: Create a Test Script

Create a simple test script to continuously monitor AC status:

```csharp
using UnityEngine;
using NiceHouse.Data;
using NiceHouse.EnvironmentControl;

public class ACStatusMonitor : MonoBehaviour
{
    [Header("Configuration")]
    public string deviceId = "AC_bedroom";
    public float checkInterval = 1f; // Check every second
    
    private AirConditionerController _controller;
    private float _timer;
    private bool _lastState;
    
    private void Start()
    {
        if (DeviceManager.Instance.TryGetDevice(deviceId, out var device))
        {
            _controller = device.GetComponent<AirConditionerController>();
            if (_controller == null)
            {
                Debug.LogError($"[ACStatusMonitor] AirConditionerController not found on {deviceId}");
                enabled = false;
            }
        }
        else
        {
            Debug.LogError($"[ACStatusMonitor] Device {deviceId} not found");
            enabled = false;
        }
    }
    
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= checkInterval)
        {
            _timer = 0f;
            CheckStatus();
        }
    }
    
    private void CheckStatus()
    {
        if (_controller == null) return;
        
        bool isOn = _controller.IsOn;
        
        // Only log when state changes
        if (isOn != _lastState)
        {
            _lastState = isOn;
            
            if (isOn)
            {
                Debug.Log($"[ACStatusMonitor] AC {deviceId} is now ON (Target: {_controller.targetTemperature}°C)");
            }
            else
            {
                Debug.Log($"[ACStatusMonitor] AC {deviceId} is now OFF");
            }
        }
        
        // Check energy consumption
        var energyData = EnergyManager.Instance.GetDeviceEnergyData(deviceId);
        if (energyData != null && isOn)
        {
            Debug.Log($"[ACStatusMonitor] Power: {energyData.currentPower}W, Daily: {energyData.dailyConsumption:F3} kWh");
        }
    }
}
```

**Usage:**
1. Create an empty GameObject
2. Add this script
3. Set `deviceId` to "AC_bedroom"
4. Run the game
5. Check Console for status updates

---

## Quick Verification Checklist

### ✅ Basic Checks

- [ ] **Console Logs**: Check for "[AirConditioner] AC_bedroom turned ON/OFF"
- [ ] **UI Panel**: Check Environment Control Panel status text
- [ ] **Energy Consumption**: Check if `currentPower > 0` in EnergyManager
- [ ] **Visual**: Check if vent blade is animating (if configured)

### ✅ Code Checks

- [ ] **IsOn Property**: `controller.IsOn` should be `true` when on
- [ ] **DeviceState**: `controller.currentState` should be `DeviceState.Running` when on
- [ ] **EnergyManager**: Device should be in `_activeDevices` set

### ✅ Advanced Checks

- [ ] **Energy Data**: `GetDeviceEnergyData("AC_bedroom").currentPower > 0`
- [ ] **Daily Consumption**: Should increase over time when on
- [ ] **DeviceManager**: Device should be registered and findable

---

## Common Issues

### Issue 1: Status shows "N/A" in UI

**Cause:** Panel's `currentRoomId` doesn't match AC's `roomId`

**Fix:**
- Check AC's `DeviceDefinition.roomId` (should be "BedRoom01")
- Check Panel's `currentRoomId` (should also be "BedRoom01")
- Make sure they match

### Issue 2: Energy consumption is 0

**Cause:** Device not registered in EnergyManager

**Fix:**
1. Check `EnergyManager.deviceConfigs` list
2. Ensure there's an entry with `deviceId = "AC_bedroom"`
3. Ensure `ratedPower > 0`

### Issue 3: Controller.IsOn returns false even when turned on

**Cause:** Controller not properly initialized or state not updated

**Fix:**
1. Check if `DeviceDefinition` component exists
2. Check if `deviceId` is set correctly
3. Check Console for errors
4. Try turning off and on again

---

## Summary

**Easiest Methods:**
1. **Console Logs** - Automatic logging when state changes
2. **UI Panel** - Visual status indicator
3. **Code Check** - `controller.IsOn` property

**Most Reliable:**
1. **Energy Consumption** - If consuming power, definitely on
2. **DeviceState** - Direct state check
3. **EnergyManager Active Devices** - System-level check

---

> **Tip:** For debugging, enable `enableDebugLog` in `DeviceInteractable` component to see detailed interaction logs.

