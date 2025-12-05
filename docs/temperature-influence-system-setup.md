# Temperature Influence System Setup Guide

## Overview

The `TemperatureInfluenceSystem` makes air conditioners and other devices actually affect room temperature, solving the "fake linkage" problem where ACs turn on but temperature doesn't change.

---

## How It Works

### Current Problem

- **Before:** AC turns on → Temperature doesn't change → "Fake linkage"
- **After:** AC turns on → Temperature gradually moves toward target temperature → Realistic behavior

### Temperature Influence Model

1. **Device Influence:**
   - Air Conditioner: Moves temperature toward target temperature
   - Fan: Slight cooling effect
   - Other devices can be added

2. **Heat Transfer Between Rooms:**
   - Adjacent rooms exchange heat (simplified model)
   - Configurable heat transfer rate

3. **Natural Changes:**
   - `EnvironmentDataSimulator` still provides base temperature variation
   - Device influence is added on top

---

## Setup Steps

### Step 1: Add TemperatureInfluenceSystem to Scene

1. **Find or create DataRoot GameObject**
   - Should be in Hierarchy under "DataRoot"

2. **Add Component**
   - Select DataRoot
   - Click `Add Component`
   - Search `Temperature Influence System`
   - Add component

### Step 2: Configure Parameters

**Default Settings (usually fine):**
```
Temperature Influence System:
├── AC Influence Rate: 0.5 (degrees/second)
├── Fan Cooling Rate: 0.1 (degrees/second)
├── Heat Transfer Rate: 0.05 (0-1, heat transfer between rooms)
├── Enable Heat Transfer: true
├── Update Interval: 0.1 (seconds)
└── Enable Debug Log: false
```

**Parameter Explanation:**

- **AC Influence Rate**: How fast AC changes temperature toward target
  - Higher = faster temperature change
  - Default 0.5 means temperature changes ~0.5°C per second toward target

- **Fan Cooling Rate**: Fan's cooling effect
  - Small value, provides slight cooling

- **Heat Transfer Rate**: How much heat transfers between rooms
  - 0 = no transfer (rooms isolated)
  - 1 = complete transfer (unrealistic)
  - 0.05 = slight transfer (realistic)

- **Update Interval**: How often temperature is updated
  - Lower = smoother but more CPU
  - Default 0.1s is good balance

### Step 3: Test

1. **Run game**
2. **Open Environment Control Panel**
3. **Check current room temperature**
4. **Turn on AC**
5. **Watch temperature gradually change toward target temperature**

---

## How Temperature Changes

### Example Scenario

**Initial State:**
- Room temperature: 28°C
- AC target temperature: 24°C
- AC Influence Rate: 0.5°C/s

**When AC Turns On:**
- Temperature difference: 28°C - 24°C = 4°C
- Change rate: -4°C × 0.5 = -2°C/s
- After 1 second: 28°C - 2°C = 26°C
- After 2 seconds: 26°C - 1°C = 25°C
- Gradually approaches 24°C

**When Temperature Reaches Target:**
- Temperature difference: 24°C - 24°C = 0°C
- Change rate: 0°C/s
- Temperature stabilizes at target

---

## Multiple ACs in Different Rooms

### Scenario: Two ACs in Different Rooms

**Room A:**
- AC target: 24°C
- Current temp: 28°C
- AC influence: -2°C/s

**Room B:**
- AC target: 22°C
- Current temp: 25°C
- AC influence: -1.5°C/s

**Heat Transfer:**
- Room A and Room B exchange heat
- If Room A is warmer, heat flows to Room B
- Rate controlled by `Heat Transfer Rate`

**Result:**
- Each room's temperature moves toward its AC's target
- Rooms also influence each other slightly
- Creates realistic multi-room temperature behavior

---

## Debugging

### Enable Debug Log

1. Select `TemperatureInfluenceSystem` component
2. Check `Enable Debug Log`
3. Run game
4. Check Console for temperature change logs

**Example Output:**
```
[TemperatureInfluence] AC AC_bedroom in BedRoom01: temp=28.50°C, target=24°C, change=-2.250°C
[TemperatureInfluence] BedRoom01 temperature changed: 28.50°C -> 26.25°C (delta: -2.250°C)
```

### Manual Temperature Setting

For testing, you can manually set temperature:

```csharp
TemperatureInfluenceSystem.Instance.SetRoomTemperature("BedRoom01", 30f);
```

---

## Integration with Existing Systems

### Works With:

1. **EnvironmentDataSimulator**
   - Still provides base temperature variation
   - Device influence is added on top

2. **EnvironmentController**
   - Still monitors temperature thresholds
   - Auto-activates AC when temperature is too high/low
   - Now AC actually changes temperature, creating feedback loop

3. **EnvironmentDataStore**
   - Temperature is updated in real-time
   - All consumers (UI, visualizations) see updated values

---

## Configuration Tips

### Faster Temperature Changes

If temperature changes too slowly:
- Increase `AC Influence Rate` (e.g., 1.0 or 2.0)

### Slower Temperature Changes

If temperature changes too fast:
- Decrease `AC Influence Rate` (e.g., 0.2 or 0.3)

### Isolated Rooms

If you want rooms to be completely isolated:
- Set `Enable Heat Transfer` to false
- Or set `Heat Transfer Rate` to 0

### More Realistic Heat Transfer

If you want more realistic room-to-room heat transfer:
- Increase `Heat Transfer Rate` (e.g., 0.1 or 0.15)
- Note: Higher values may cause temperature instability

---

## Troubleshooting

### Temperature Not Changing

**Check:**
1. Is `TemperatureInfluenceSystem` component enabled?
2. Is it added to a GameObject in the scene?
3. Is `Update Interval` too large? (Try 0.1s)
4. Is AC actually on? (Check `IsOn` property)

### Temperature Changes Too Fast/Slow

**Adjust:**
- `AC Influence Rate` parameter
- Lower = slower, Higher = faster

### Temperature Oscillates

**Cause:**
- `Heat Transfer Rate` too high
- `AC Influence Rate` too high

**Fix:**
- Reduce `Heat Transfer Rate`
- Reduce `AC Influence Rate`
- Increase `Update Interval`

---

## Summary

**Before:**
- AC turns on → Temperature doesn't change → Fake behavior

**After:**
- AC turns on → Temperature gradually moves toward target → Realistic behavior

**Setup:**
1. Add `TemperatureInfluenceSystem` to DataRoot
2. Configure parameters (defaults usually fine)
3. Test by turning on AC and watching temperature change

---

> **Note:** The system works alongside `EnvironmentDataSimulator`. The simulator provides base temperature variation, and device influence modifies it in real-time.

