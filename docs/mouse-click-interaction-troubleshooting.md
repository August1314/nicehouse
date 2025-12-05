# Mouse Click Interaction Troubleshooting

## Problem

Control panel can toggle AC on/off, but clicking directly on the AC doesn't work.

---

## Root Cause Analysis

The `FPRaycastInteractor` component needs to be properly configured on the Player GameObject. Common issues:

1. **FPRaycastInteractor not added to Player**
2. **Mouse cursor not locked**
3. **Raycast distance too short**
4. **Collider too small or in wrong position**
5. **Camera not found**

---

## Step-by-Step Debugging

### Step 1: Check if FPRaycastInteractor Exists

1. **Find the Player GameObject**
   - In Hierarchy, search for "FPSPlayer" or "Player"
   - Select it

2. **Check for FPRaycastInteractor component**
   - In Inspector, look for `FP Raycast Interactor (Script)` component
   - If **NOT found**, add it:
     - Click `Add Component`
     - Search `FP Raycast Interactor`
     - Add component

### Step 2: Configure FPRaycastInteractor

**Required Settings:**

```
FP Raycast Interactor:
├── Target Camera: (Auto-found or manually assign FpsCamera)
├── Max Distance: 4 (or increase to 10 for testing)
├── Interactable Layers: Everything (or specific layer)
├── Crosshair UI: (Optional, can be None)
└── Require Cursor Lock: true (IMPORTANT!)
```

**How to configure:**

1. **Target Camera:**
   - If empty, it will auto-find camera in children
   - Or manually drag `FpsCamera` from Hierarchy

2. **Max Distance:**
   - Default: 4 meters
   - If AC is far away, increase to 10 or more
   - **For testing, set to 50** to ensure distance isn't the issue

3. **Require Cursor Lock:**
   - **MUST be true** for interaction to work
   - This means mouse cursor must be locked (hidden)

### Step 3: Check Mouse Cursor Lock State

**The interaction only works when mouse is locked!**

1. **In Play mode:**
   - Press `ESC` to unlock mouse (cursor appears)
   - Press `ESC` again or click in Game view to lock mouse (cursor disappears)
   - **Mouse must be locked for FPRaycastInteractor to work**

2. **Check FirstPersonController:**
   - Find `FirstPersonController` component on Player
   - Check if it's locking the cursor on start
   - If not, you need to manually lock it

### Step 4: Verify Collider Setup

**AC GameObject must have a Collider:**

1. **Select AC GameObject**
2. **Check Collider component:**
   - Must have `Box Collider` or `Mesh Collider`
   - **Is Trigger: MUST be unchecked** (not a trigger)
   - Collider size should cover the AC model

3. **If Collider is too small:**
   - Adjust `Size` values in Box Collider
   - Or adjust `Center` if collider is in wrong position

### Step 5: Enable Debug Logging

**Add debug logging to verify raycast is working:**

1. **Enable Debug Log in DeviceInteractable:**
   - Select AC GameObject
   - Find `Device Interactable (Script)` component
   - Check `Enable Debug Log`

2. **Run game and check Console:**
   - When you hover over AC, you should see: `[DeviceInteractable] Hover Enter: AC`
   - When you click, you should see: `[DeviceInteractable] Turned ON/OFF: AC_bedroom`
   - If you see nothing, raycast is not hitting the AC

### Step 6: Test Raycast Manually

**Create a test script to verify raycast:**

```csharp
using UnityEngine;
using NiceHouse.ControlHub;

public class RaycastDebugger : MonoBehaviour
{
    public Camera testCamera;
    public float maxDistance = 10f;
    
    private void Update()
    {
        if (testCamera == null) return;
        
        var ray = new Ray(testCamera.transform.position, testCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            Debug.Log($"[RaycastDebugger] Hit: {hit.collider.name} at distance {hit.distance:F2}m");
            
            var interactable = hit.collider.GetComponent<NiceHouse.Interaction.DeviceInteractable>();
            if (interactable != null)
            {
                Debug.Log($"[RaycastDebugger] Found DeviceInteractable on {hit.collider.name}");
            }
            else
            {
                Debug.Log($"[RaycastDebugger] No DeviceInteractable found on {hit.collider.name}");
            }
        }
        else
        {
            Debug.Log("[RaycastDebugger] No hit");
        }
    }
}
```

**Usage:**
1. Add this script to Player GameObject
2. Assign camera to `testCamera`
3. Run game and look at AC
4. Check Console for raycast hits

---

## Quick Fix Checklist

### ✅ Must Have

- [ ] **FPRaycastInteractor** component on Player GameObject
- [ ] **Target Camera** assigned (or auto-found)
- [ ] **Require Cursor Lock: true**
- [ ] **Mouse cursor is locked** (in Play mode)
- [ ] **AC has Collider** (BoxCollider or MeshCollider)
- [ ] **Collider Is Trigger: unchecked**
- [ ] **DeviceInteractable** component on AC GameObject

### ✅ Recommended

- [ ] **Max Distance: 10** (or more for testing)
- [ ] **Enable Debug Log: true** in DeviceInteractable
- [ ] **Interactable Layers: Everything** (for testing)

---

## Common Issues and Solutions

### Issue 1: No hover/click response

**Symptoms:**
- No console logs when hovering/clicking
- No visual feedback

**Causes:**
1. FPRaycastInteractor not on Player
2. Mouse not locked
3. Camera not found
4. Raycast distance too short

**Fix:**
1. Add FPRaycastInteractor to Player
2. Lock mouse cursor (press ESC twice in Play mode)
3. Check camera assignment
4. Increase Max Distance to 50 for testing

### Issue 2: Raycast hits but no interaction

**Symptoms:**
- Console shows raycast hits
- But DeviceInteractable methods not called

**Causes:**
1. DeviceInteractable not on same GameObject as Collider
2. IRaycastInteractable interface not implemented correctly
3. Component disabled

**Fix:**
1. Ensure DeviceInteractable is on AC GameObject (same as Collider)
2. Check component is enabled
3. Verify DeviceInteractable implements IRaycastInteractable

### Issue 3: Mouse cursor not locking

**Symptoms:**
- Mouse cursor always visible
- FPRaycastInteractor not working

**Causes:**
1. FirstPersonController not locking cursor
2. UI blocking cursor lock
3. requireCursorLock = true but cursor not locked

**Fix:**
1. Check FirstPersonController cursor lock settings
2. Manually lock cursor: `Cursor.lockState = CursorLockMode.Locked;`
3. Or set `Require Cursor Lock: false` in FPRaycastInteractor (for testing)

### Issue 4: Collider too small

**Symptoms:**
- Raycast misses AC
- Need to be very close to interact

**Fix:**
1. Select AC GameObject
2. Adjust Box Collider `Size` to cover entire AC model
3. Adjust `Center` if needed
4. Test with larger collider first

---

## Step-by-Step Setup (From Scratch)

### 1. Add FPRaycastInteractor to Player

```
1. Select Player GameObject (FPSPlayer)
2. Add Component → FP Raycast Interactor
3. Configure:
   - Target Camera: (auto or drag FpsCamera)
   - Max Distance: 10
   - Require Cursor Lock: true
```

### 2. Verify AC Setup

```
1. Select AC GameObject
2. Check components:
   - Device Definition ✓
   - Air Conditioner Controller ✓
   - Box Collider ✓ (Is Trigger: false)
   - Device Interactable ✓
3. Enable Debug Log in Device Interactable
```

### 3. Test in Play Mode

```
1. Enter Play mode
2. Lock mouse cursor (press ESC if needed, then click in Game view)
3. Look at AC
4. Check Console for hover messages
5. Click mouse button
6. Check Console for click messages
```

---

## Debug Script: Enhanced FPRaycastInteractor

If you want more detailed logging, temporarily modify `FPRaycastInteractor.cs`:

```csharp
private void Update()
{
    if (targetCamera == null)
    {
        Debug.LogWarning("[FPRaycastInteractor] Target camera is null!");
        return;
    }
    
    if (requireCursorLock && Cursor.lockState != CursorLockMode.Locked)
    {
        Debug.Log("[FPRaycastInteractor] Cursor not locked, skipping raycast");
        UpdateHoverTarget(null);
        return;
    }

    var ray = new Ray(targetCamera.transform.position, targetCamera.transform.forward);
    Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red);

    if (Physics.Raycast(ray, out var hit, maxDistance, interactableLayers, QueryTriggerInteraction.Ignore))
    {
        Debug.Log($"[FPRaycastInteractor] Hit: {hit.collider.name} at {hit.distance:F2}m");
        
        var interactable = ResolveInteractable(hit.collider);
        if (interactable != null)
        {
            Debug.Log($"[FPRaycastInteractor] Found interactable: {interactable.GetType().Name}");
        }
        else
        {
            Debug.Log($"[FPRaycastInteractor] No interactable found on {hit.collider.name}");
        }
        
        UpdateHoverTarget(interactable);

        if (interactable != null && Input.GetMouseButtonDown(0))
        {
            Debug.Log($"[FPRaycastInteractor] Click detected, calling OnRaycastClick");
            interactable.OnRaycastClick(this);
        }
    }
    else
    {
        Debug.Log("[FPRaycastInteractor] No raycast hit");
        UpdateHoverTarget(null);
    }
}
```

**Note:** Remove debug logs after fixing the issue.

---

## Summary

**Most Common Issue:** FPRaycastInteractor not added to Player GameObject.

**Quick Fix:**
1. Add `FPRaycastInteractor` to Player
2. Set `Require Cursor Lock: true`
3. Lock mouse cursor in Play mode
4. Enable `Enable Debug Log` in DeviceInteractable
5. Test and check Console logs

---

> **Tip:** If still not working, temporarily set `Require Cursor Lock: false` to test if cursor lock is the issue.

