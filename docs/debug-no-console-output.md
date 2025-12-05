# Debug: No Console Output

## Problem

Console shows no output when trying to interact with AC via mouse click.

---

## Quick Diagnosis Steps

### Step 1: Enable Debug Logging

**On Player GameObject:**

1. Select Player (FPSPlayer)
2. Find `FP Raycast Interactor (Script)` component
3. **Check "Enable Debug Log"**
4. Run game
5. Look at AC
6. Check Console

**Expected Output:**
- If cursor not locked: `[FPRaycastInteractor] Cursor not locked, skipping raycast`
- If raycast hits: `[FPRaycastInteractor] Hit: AC at distance X.XXm`
- If no hit: `[FPRaycastInteractor] No raycast hit` (every 60 frames)

### Step 2: Use DeviceInteractionDebugger (Easier)

**This script has debug enabled by default:**

1. Select Player GameObject
2. Add Component → `Device Interaction Debugger`
3. Run game
4. **You'll see on-screen status** showing:
   - Cursor Locked: YES/NO
   - FPRaycastInteractor: Found/Missing
   - Camera: Name
5. Check Console for detailed logs

### Step 3: Check Common Issues

#### Issue 1: Cursor Not Locked

**Symptom:** Console shows "Cursor not locked" message

**Fix:**
1. In Play mode, press ESC
2. Click in Game view to lock cursor
3. Mouse cursor should disappear

#### Issue 2: Max Distance Too Short

**Symptom:** Console shows "No raycast hit" even when looking at AC

**Fix:**
1. Select Player
2. Find `FP Raycast Interactor` component
3. Increase `Max Distance` from 4 to 10 or 20
4. Test again

#### Issue 3: Something Blocking Raycast

**Symptom:** Raycast hits something else (wall, floor, etc.) but not AC

**Fix:**
1. Enable debug logging
2. Check Console - see what object is being hit
3. If it's a wall or other object, you may be too far or angle is wrong
4. Move closer to AC and try again

#### Issue 4: FPRaycastInteractor Not Running

**Symptom:** No output at all, even with debug enabled

**Check:**
1. Is FPRaycastInteractor component enabled? (checkbox checked)
2. Is Player GameObject active in scene?
3. Is the script attached to the correct GameObject?

---

## Diagnostic Checklist

- [ ] **Enable Debug Log** checked in FPRaycastInteractor
- [ ] **Mouse cursor is locked** (invisible in Play mode)
- [ ] **Max Distance** is sufficient (try 10 or 20)
- [ ] **FPRaycastInteractor** component is enabled
- [ ] **Player GameObject** is active
- [ ] **No other objects** blocking the raycast

---

## Quick Test: Add DeviceInteractionDebugger

**This is the easiest way to diagnose:**

1. **Select Player GameObject**
2. **Add Component** → Search `Device Interaction Debugger`
3. **Run game**
4. **Look at screen** - You'll see status info
5. **Look at AC** - Check Console for raycast hits

**This script will show:**
- Whether cursor is locked
- Whether FPRaycastInteractor exists
- What the raycast is hitting
- Whether DeviceInteractable is found

---

## Expected Console Output (When Working)

**When cursor is locked and looking at AC:**
```
[FPRaycastInteractor] Hit: AC at distance 2.34m
[FPRaycastInteractor] Found interactable: DeviceInteractable on AC
[DeviceInteractable] Hover Enter: AC
```

**When clicking:**
```
[FPRaycastInteractor] Click detected, calling OnRaycastClick on DeviceInteractable
[DeviceInteractable] Turned ON: AC_bedroom
[AirConditioner] AC_bedroom turned ON, target temp: 24°C
```

**If you see nothing:**
- Cursor not locked
- FPRaycastInteractor not enabled
- Max Distance too short
- Something blocking raycast

---

## Still No Output?

1. **Check if FPRaycastInteractor is enabled:**
   - Select Player
   - Find component
   - Ensure checkbox is checked

2. **Check if script is running:**
   - Add a simple `Debug.Log("FPRaycastInteractor Update")` at start of Update()
   - If you see this, script is running
   - If not, component may be disabled or not attached

3. **Check Console filters:**
   - Make sure Console is not filtering out messages
   - Clear search bar
   - Check all log types are visible

---

## Summary

**Most likely causes:**
1. Debug logging not enabled
2. Cursor not locked
3. Max Distance too short
4. Something blocking raycast

**Quick fix:**
1. Enable "Enable Debug Log" in FPRaycastInteractor
2. Or add DeviceInteractionDebugger component
3. Lock cursor in Play mode
4. Check Console output

