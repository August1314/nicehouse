# Energy Consumption Panel Setup Guide

## Overview

This guide explains how to add the Energy Consumption panel to your Control Hub system. The panel displays real-time energy consumption data for all devices, including current power consumption and daily cumulative consumption.

---

## Features

- **Total Statistics:**
  - Total daily consumption (kWh)
  - Total current power (W)

- **Device List:**
  - Device name
  - Current power (W)
  - Daily consumption (kWh)
  - Device status (on/off indicator)

---

## Step 1: Create UI Structure in Unity

### 1.1 Create Panel GameObject

1. In Hierarchy, find `EnvironmentControlPanel` GameObject
2. Right-click → Create Empty → Name it `EnergyConsumptionPanel`
3. Add `RectTransform` component (should be automatic)

### 1.2 Create Header Section

1. Under `EnergyConsumptionPanel`, create Empty GameObject → Name it `Header`
2. Add `Horizontal Layout Group` component:
   - Spacing: 20
   - Padding: Left/Right/Top/Bottom: 10
3. Under `Header`, create two TextMeshProUGUI objects:
   - `TotalConsumptionText` - Display total consumption
   - `TotalPowerText` - Display total power

### 1.3 Create Device List Section

1. Under `EnergyConsumptionPanel`, create Empty GameObject → Name it `DeviceListContainer`
2. Add `RectTransform` component
3. Add `Vertical Layout Group` component:
   - Spacing: 5
   - Padding: Left/Right/Top/Bottom: 10
   - Child Control Width: true
   - Child Control Height: false
   - Child Force Expand Width: true
   - Child Force Expand Height: false
4. Add `Content Size Fitter` component:
   - Vertical Fit: Preferred Size
5. Add `Scroll Rect` component (optional, for scrolling if many devices):
   - Content: Drag `DeviceListContainer`
   - Horizontal: false
   - Vertical: true

### 1.4 (Optional) Create Device Item Prefab

1. Create Empty GameObject → Name it `EnergyDeviceItem`
2. Add `RectTransform` component
3. Add `Horizontal Layout Group` component:
   - Spacing: 10
   - Padding: 10
4. Add three TextMeshProUGUI children:
   - `DeviceNameText` - Device name
   - `PowerText` - Current power
   - `ConsumptionText` - Daily consumption
5. (Optional) Add `Image` component for status indicator
6. Add `EnergyDeviceItem` component script
7. Drag references to the TextMeshProUGUI components
8. Save as Prefab in `Assets/Prefabs/`

---

## Step 2: Add Scripts

### 2.1 Add EnergyConsumptionPanel Script

1. Select `EnergyConsumptionPanel` GameObject
2. Add Component → `Energy Consumption Panel (Script)`
3. Configure references:
   - **Total Consumption Text**: Drag `TotalConsumptionText`
   - **Total Power Text**: Drag `TotalPowerText`
   - **Device List Container**: Drag `DeviceListContainer`
   - **Device Item Prefab**: (Optional) Drag `EnergyDeviceItem` prefab
   - **Update Interval**: 1 (seconds)

### 2.2 Add EnergyConsumptionPanelAdapter Script

1. Select `EnergyConsumptionPanel` GameObject (or create a separate adapter GameObject)
2. Add Component → `Energy Consumption Panel Adapter (Script)`
3. Configure:
   - **Module Id**: "Energy"
   - **Display Name**: "Energy Consumption"
   - **Accent Color**: Yellow/Gold (255, 214, 0)
   - **Target Panel**: Drag `EnergyConsumptionPanel` GameObject
   - **Panel Root**: Drag `EnergyConsumptionPanel` GameObject

---

## Step 3: Register with Control Hub

### 3.1 Add to ControlHubStatusAggregator

1. Find `ControlHubStatusAggregator` component in scene
2. In Inspector, find `Module Sources` list
3. Click "+" to add new entry
4. Drag `EnergyConsumptionPanelAdapter` GameObject to the new entry

### 3.2 Verify Registration

1. Run the game
2. Open Control Hub panel
3. You should see "Energy Consumption" entry in the list
4. Click it to open the Energy Consumption panel

---

## Step 4: Configure Layout (Recommended)

### 4.1 Panel Layout

Recommended structure:
```
EnergyConsumptionPanel (RectTransform)
├── Header (Horizontal Layout Group)
│   ├── TotalConsumptionText (TextMeshProUGUI)
│   └── TotalPowerText (TextMeshProUGUI)
├── DeviceListContainer (Vertical Layout Group + ScrollRect)
│   └── (Device items will be created dynamically)
└── (Optional) Footer
```

### 4.2 Text Styling

- **Total Consumption Text:**
  - Font Size: 18-20
  - Color: Gold/Yellow (#FFD700)
  - Alignment: Left

- **Total Power Text:**
  - Font Size: 18-20
  - Color: Red (#FF6B6B)
  - Alignment: Right

- **Device Item Text:**
  - Font Size: 14
  - Color: White (active) / Gray (inactive)

---

## Step 5: Testing

### 5.1 Verify Data Display

1. Run the game
2. Open Energy Consumption panel
3. Check that:
   - Total consumption shows correct value
   - Total power shows correct value
   - Device list shows all devices
   - Each device shows power and consumption

### 5.2 Test Device States

1. Turn on an air conditioner
2. Check that:
   - Device appears in list with power > 0
   - Consumption increases over time
   - Total power increases
   - Total consumption increases

3. Turn off the device
4. Check that:
   - Device power becomes 0
   - Consumption stops increasing
   - Total power decreases

---

## Troubleshooting

### Panel Not Showing

- Check that `EnergyConsumptionPanel` GameObject is active
- Check that `panelRoot` is correctly assigned in adapter
- Check that adapter is registered in `ControlHubStatusAggregator`

### No Data Displayed

- Check that `EnergyManager` exists in scene
- Check that devices are registered in `DeviceManager`
- Check that devices have power configured in `EnergyManager.deviceConfigs`
- Enable debug logs in `EnergyConsumptionPanel`

### Device List Empty

- Check that `DeviceManager` exists in scene
- Check that devices are properly registered
- Check that `deviceListContainer` is assigned correctly

### Text Not Updating

- Check `updateInterval` setting (should be > 0)
- Check that `EnergyManager.Instance` is not null
- Check Unity Console for errors

---

## API Reference

### EnergyManager APIs

```csharp
// Get device current power (W)
float power = EnergyManager.Instance.GetDeviceCurrentPower(deviceId);

// Get device daily consumption (kWh)
float consumption = EnergyManager.Instance.GetDeviceDailyConsumption(deviceId);

// Get device energy data
EnergyData data = EnergyManager.Instance.GetDeviceEnergyData(deviceId);

// Get total daily consumption (kWh)
float total = EnergyManager.Instance.GetTotalDailyConsumption();

// Get total current power (W)
float totalPower = EnergyManager.Instance.GetTotalCurrentPower();

// Get all devices energy data
var allData = EnergyManager.Instance.GetAllDevicesEnergyData();
```

---

## Notes

- The panel automatically creates device items if no prefab is assigned
- Device items are dynamically created/removed based on registered devices
- Update interval can be adjusted for performance (default: 1 second)
- Panel supports both prefab-based and dynamically created device items

