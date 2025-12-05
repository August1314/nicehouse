# Quick Fix: Mouse Click AC Not Working

## Problem

Control panel can toggle AC, but clicking directly on AC doesn't work.

---

## Most Likely Cause

**FPRaycastInteractor component is missing on the Player GameObject.**

---

## Quick Fix (3 Steps)

### Step 1: Add FPRaycastInteractor to Player

1. **Find Player GameObject**
   - In Hierarchy, search for "FPSPlayer" or "Player"
   - Select it

2. **Add Component**
   - Click `Add Component` button
   - Search: `FP Raycast Interactor`
   - Click to add

3. **Configure (Auto-configured, but verify):**
   ```
   Target Camera: (Auto-found, should show FpsCamera)
   Max Distance: 4 (or increase to 10 for testing)
   Require Cursor Lock: true ✓
   ```

### Step 2: Lock Mouse Cursor

**In Play Mode:**

1. **Press ESC** - This toggles cursor lock
2. **Click in Game view** - This locks the cursor
3. **Mouse cursor should disappear** - This means it's locked

**Note:** FPRaycastInteractor only works when cursor is locked!

### Step 3: Test

1. **Run game** (Play mode)
2. **Lock mouse cursor** (press ESC, then click in Game view)
3. **Look at AC**
4. **Click mouse button**
5. **Check Console** - Should see: `[DeviceInteractable] Turned ON/OFF: AC_bedroom`

---

## If Still Not Working

### Check 1: Enable Debug Log

1. Select AC GameObject
2. Find `Device Interactable (Script)` component
3. Check `Enable Debug Log`
4. Run game and check Console for messages

### Check 2: Verify Collider

1. Select AC GameObject
2. Check `Box Collider` component
3. **Is Trigger: MUST be unchecked**
4. Collider size should cover AC model

### Check 3: Add Debug Script

1. Select Player GameObject
2. Add Component → `Device Interaction Debugger`
3. Check `Enable Debug Log`
4. Run game - You'll see on-screen status and Console logs

---

## Diagnostic Checklist

- [ ] **FPRaycastInteractor** on Player GameObject
- [ ] **Mouse cursor is locked** (invisible in Play mode)
- [ ] **AC has Collider** (not a trigger)
- [ ] **DeviceInteractable** on AC GameObject
- [ ] **Enable Debug Log** checked in DeviceInteractable

---

## Expected Console Output

**When hovering over AC:**
```
[DeviceInteractable] Hover Enter: AC
```

**When clicking AC:**
```
[DeviceInteractable] Turned ON: AC_bedroom
[AirConditioner] AC_bedroom turned ON, target temp: 24°C
```

**If you see nothing:**
- FPRaycastInteractor not working
- Mouse not locked
- Raycast not hitting AC

---

## Still Not Working?

See detailed troubleshooting guide: `docs/mouse-click-interaction-troubleshooting.md`

