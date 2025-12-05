# 第一人称相机穿模问题修复

## 问题描述

**症状：**
- 第一人称视角靠近墙壁或其他物体时，能看到模型后面的东西
- 相机"穿"进了模型内部
- 看到模型内部的几何体或背面

**原因：**
1. **相机近裁剪面（Near Clipping Plane）太大**
2. **相机位置进入了模型内部**（碰撞检测不够精确）
3. **模型没有背面剔除**（双面渲染）

---

## 解决方案

### 方案 1：调整相机近裁剪面（最简单有效）

**当前设置：**
```
Camera:
└── Near Clip Plane: 0.3  ← 太大了！
```

**修复方法：**

1. **在 Unity 中选择第一人称相机**
   - 找到 `FPSPlayer` → `FpsCamera`

2. **调整 Near Clip Plane**
   ```
   Camera 组件：
   └── Near: 0.01 或 0.05  ← 改小这个值
   ```

**推荐值：**
- **0.01**：最接近，但可能在某些情况下有精度问题
- **0.05**：推荐值，平衡了精度和稳定性
- **0.1**：如果 0.05 还有问题，可以尝试

**为什么？**
- Near Clip Plane 是相机能看到的最近距离
- 如果设置为 0.3，相机在距离墙壁 0.3 米时就会"进入"墙壁
- 改小后，相机需要更靠近才能看到模型内部

### 方案 2：添加相机碰撞检测（更彻底）

在 `FirstPersonController` 中添加相机碰撞检测，防止相机进入模型：

```csharp
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("相机设置")]
    [Tooltip("相机碰撞检测半径")]
    public float cameraCollisionRadius = 0.2f;
    
    [Tooltip("相机碰撞检测距离")]
    public float cameraCollisionDistance = 0.5f;
    
    private Camera _camera;
    private float _defaultNearPlane;
    
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _camera = GetComponentInChildren<Camera>();
        
        if (_camera != null)
        {
            _defaultNearPlane = _camera.nearClipPlane;
            // 设置较小的近裁剪面
            _camera.nearClipPlane = 0.05f;
        }
        
        if (cameraPivot == null && _camera != null)
        {
            cameraPivot = _camera.transform;
        }
    }
    
    private void LateUpdate()
    {
        if (_camera == null || cameraPivot == null) return;
        
        // 检测相机前方是否有障碍物
        Vector3 cameraPos = cameraPivot.position;
        Vector3 cameraForward = cameraPivot.forward;
        
        // 使用 SphereCast 检测碰撞
        RaycastHit hit;
        if (Physics.SphereCast(
            cameraPos, 
            cameraCollisionRadius, 
            cameraForward, 
            out hit, 
            cameraCollisionDistance,
            ~0,  // 所有层
            QueryTriggerInteraction.Ignore))
        {
            // 如果检测到碰撞，将相机拉回
            float distance = hit.distance - cameraCollisionRadius;
            if (distance < cameraCollisionDistance)
            {
                cameraPivot.localPosition = new Vector3(
                    cameraPivot.localPosition.x,
                    cameraPivot.localPosition.y,
                    -distance + 0.1f  // 保持一点距离
                );
            }
        }
        else
        {
            // 没有碰撞，恢复默认位置
            cameraPivot.localPosition = Vector3.zero;
        }
    }
}
```

### 方案 3：使用 CharacterController 的碰撞（最简单）

确保 CharacterController 的碰撞正常工作：

1. **检查 CharacterController 设置**
   ```
   CharacterController:
   ├── Radius: 0.4（默认）
   ├── Height: 1.8（默认）
   └── Center: (0, 0.9, 0)（默认）
   ```

2. **确保墙壁有正确的 Collider**
   - 参考 `docs/meshcollider-for-walls.md`
   - MeshCollider: Convex=false, Is Trigger=false

3. **相机位置应该在 CharacterController 内部**
   - 相机应该在角色头部位置（约 Y = 1.6）
   - 不应该超出 CharacterController 的边界

---

## 实际修复步骤

### 步骤 1：调整相机近裁剪面（必须）

1. **选择 FPSPlayer → FpsCamera**
2. **在 Inspector 中找到 Camera 组件**
3. **将 Near 从 0.3 改为 0.05**
   ```
   Camera:
   └── Near: 0.05  ← 改这里
   ```

### 步骤 2：检查相机位置

1. **确认相机在角色头部位置**
   - FpsCamera 的 Local Position 应该是 `(0, 1.6, 0)` 左右
   - 不应该超出 CharacterController 的边界

2. **检查相机层级关系**
   ```
   FPSPlayer
   └── FpsCamera (Camera)
       └── 位置应该在角色头部
   ```

### 步骤 3：测试

1. **运行游戏**
2. **走向墙壁**
3. **相机应该停止，不会看到墙壁内部**

---

## 代码实现：自动调整近裁剪面

如果需要用代码自动设置：

```csharp
// 在 FirstPersonController 的 Awake 中添加
private void Awake()
{
    _controller = GetComponent<CharacterController>();
    
    var cam = GetComponentInChildren<Camera>();
    if (cam != null)
    {
        // 设置较小的近裁剪面，防止穿模
        cam.nearClipPlane = 0.05f;
        cameraPivot = cam.transform;
    }
}
```

---

## 不同场景的推荐值

| 场景类型 | Near Clip Plane | 说明 |
|---------|----------------|------|
| **室内场景** | 0.05 | 推荐值，平衡精度和稳定性 |
| **近距离交互** | 0.01 | 需要非常接近物体时 |
| **大型场景** | 0.1 | 如果 0.05 还有问题 |
| **VR 场景** | 0.1 | VR 通常需要更大的值 |

---

## 其他注意事项

### 1. 模型背面剔除

确保模型使用正确的渲染设置：
- 材质应该启用背面剔除（默认）
- 如果模型是双面的，可能需要特殊处理

### 2. 相机碰撞层

如果使用相机碰撞检测，注意：
- 设置正确的 Layer Mask
- 避免检测到 UI 或触发器

### 3. 性能考虑

- 相机碰撞检测会增加性能开销
- 如果只是调整 Near Clip Plane，性能影响可忽略

---

## 最新修复（已实现）

### ✅ 自动相机碰撞检测

`FirstPersonController` 现在已包含自动相机碰撞检测功能：

**新增功能：**
- ✅ 自动检测相机前方的碰撞
- ✅ 自动将相机拉回，防止进入模型内部
- ✅ 可配置的碰撞检测参数

**Inspector 参数：**
```
FirstPersonController:
├── Camera Near Clip Plane: 0.05  ← 近裁剪面
├── Enable Camera Collision: ✅ true  ← 启用碰撞检测
├── Camera Collision Radius: 0.2  ← 检测半径
└── Default Camera Distance: 0  ← 默认距离（自动检测）
```

**工作原理：**
1. 在 `LateUpdate` 中检测相机前方是否有障碍物
2. 使用 `SphereCast` 从角色头部向相机位置检测
3. 如果检测到碰撞，将相机拉回到安全距离
4. 如果没有碰撞，恢复默认位置

---

## 总结

### ✅ 已实现的修复

```
✅ 自动设置相机近裁剪面（0.05）
✅ 自动相机碰撞检测
✅ 可配置参数
```

**无需手动操作，代码已自动处理！**

### 🎯 推荐配置

```
Camera:
├── Near: 0.05  ← 防止穿模
├── Far: 1000   ← 根据场景调整
└── Field of View: 60  ← 根据喜好调整
```

### ⚠️ 如果问题仍然存在

1. 检查 CharacterController 的碰撞是否正常工作
2. 检查墙壁的 Collider 配置
3. 考虑添加相机碰撞检测（方案 2）

---

> **提示**：Near Clip Plane 不能设置为 0，Unity 的最小值是 0.01。如果设置为 0.01 仍然有问题，可能需要添加相机碰撞检测。

