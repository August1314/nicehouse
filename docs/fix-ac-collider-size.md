# Fix AC Collider Size Issue

## Problem

AC's Box Collider is too small, causing raycast to miss it.

**Current Collider Size:**
- X: 0.14
- Y: 0.04 (TOO SMALL!)
- Z: 0.04115 (TOO SMALL!)

**Current Collider Center:**
- Y: -9.3132 (Large offset)
- Z: -5.9604 (Large offset)

---

## Solution: Adjust Collider Size

### Method 1: Use Edit Collider (Recommended)

1. **Select AC GameObject**
2. **In Inspector, find Box Collider component**
3. **Click "Edit Collider" button**
4. **In Scene view, drag the collider handles to cover the entire AC model**
5. **Or manually adjust Size values in Inspector**

### Method 2: Manually Set Size

1. **Select AC GameObject**
2. **In Box Collider component:**
   ```
   Size:
   - X: 1.0 (or match AC width)
   - Y: 1.0 (or match AC height)
   - Z: 0.5 (or match AC depth)
   ```

3. **Adjust Center if needed:**
   ```
   Center:
   - X: 0
   - Y: 0 (adjust to center of AC model)
   - Z: 0 (adjust to center of AC model)
   ```

### Method 3: Use Mesh Collider (Alternative)

If Box Collider is hard to fit:

1. **Remove Box Collider**
2. **Add Mesh Collider**
3. **Assign AC mesh to Mesh Collider**
4. **Check "Convex" if needed**
5. **Uncheck "Is Trigger"**

---

## Quick Fix Steps

1. **Select AC GameObject**
2. **Find Box Collider component**
3. **Click "Edit Collider"**
4. **In Scene view, drag handles to cover AC model**
5. **Or set Size to:**
   - X: 1.0
   - Y: 1.0
   - Z: 0.5
6. **Test in Play mode**

---

## Verify Collider Size

**In Scene View:**
1. Select AC GameObject
2. Box Collider should show green wireframe
3. Wireframe should cover entire AC model
4. If wireframe is tiny or in wrong place, adjust Size and Center

---

## Enable Debug Logging

To see if raycast is hitting:

1. **Select Player GameObject**
2. **Find FP Raycast Interactor component**
3. **Check "Enable Debug Log"**
4. **Run game**
5. **Look at AC**
6. **Check Console for:**
   ```
   [FPRaycastInteractor] Hit: AC at distance X.XXm
   [FPRaycastInteractor] Found interactable: DeviceInteractable on AC
   ```

If you see "Hit: AC" but "No interactable found", there's a different issue.

If you see "No raycast hit", the collider is too small or in wrong position.

---

## Recommended Collider Settings

For a typical AC model:

```
Box Collider:
├── Is Trigger: false ✓
├── Size:
│   ├── X: 0.5 - 1.0 (width)
│   ├── Y: 0.5 - 1.0 (height)
│   └── Z: 0.3 - 0.5 (depth)
└── Center:
    ├── X: 0
    ├── Y: 0 (adjust to center)
    └── Z: 0 (adjust to center)
```

---

## Test After Fix

1. **Run game**
2. **Lock mouse cursor**
3. **Look at AC**
4. **Check Console:**
   - Should see: `[FPRaycastInteractor] Hit: AC`
   - Should see: `[FPRaycastInteractor] Found interactable: DeviceInteractable`
5. **Click mouse**
6. **Should see: `[DeviceInteractable] Turned ON/OFF: AC_bedroom`**

