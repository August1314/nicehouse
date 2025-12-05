# 第一人称控制器修复说明

## 问题描述

**症状：**
- 转动视角时会有移动的感觉
- 转动视角时会转进墙壁的模型里

**原因分析：**

1. **输入死区缺失**：`Input.GetAxisRaw` 可能返回微小的噪声值（即使没有按键），导致角色在转动视角时意外移动
2. **旋转时移动**：当 `transform.Rotate` 旋转角色时，如果此时有任何微小的输入值，角色会基于新的朝向移动
3. **CharacterController 碰撞**：旋转整个 GameObject 时，CharacterController 可能与墙壁碰撞，导致位置被推入墙壁

---

## 修复方案

### 1. 添加输入死区

在 `FirstPersonController` 中添加了 `inputDeadZone` 参数（默认 0.1），用于过滤微小的输入值：

```csharp
[Tooltip("输入死区阈值，小于此值的输入将被忽略（防止微小输入导致移动）")]
public float inputDeadZone = 0.1f;
```

### 2. 改进移动逻辑

在 `HandleMovement()` 中应用死区检查：

```csharp
// 应用输入死区，防止微小输入导致移动
if (Mathf.Abs(inputX) < inputDeadZone) inputX = 0f;
if (Mathf.Abs(inputZ) < inputDeadZone) inputZ = 0f;

// 只有在有明确输入时才计算移动方向
Vector3 moveDir = Vector3.zero;
if (Mathf.Abs(inputX) > 0f || Mathf.Abs(inputZ) > 0f)
{
    moveDir = (transform.right * inputX + transform.forward * inputZ).normalized;
}
```

### 3. 改进旋转逻辑

在 `HandleMouseLook()` 中添加鼠标移动阈值检查，并使用 `Space.World` 确保旋转在世界空间中：

```csharp
// 只有当鼠标真正移动时才旋转（避免微小抖动）
if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
{
    // 旋转角色（水平视角）- 使用 Space.World 避免累积误差
    transform.Rotate(Vector3.up * mouseX, Space.World);

    // 旋转相机（垂直视角）
    _cameraPitch = Mathf.Clamp(_cameraPitch - mouseY, -pitchLimit, pitchLimit);
    cameraPivot.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
}
```

---

## 修复效果

✅ **修复后：**
- 转动视角时不会意外移动
- 只有明确按下 WASD 键时才会移动
- 不会转进墙壁模型
- 移动和视角控制更加精确

---

## 如何调整

如果仍然遇到问题，可以在 Unity Inspector 中调整以下参数：

1. **Input Dead Zone**（输入死区）
   - 默认值：0.1
   - 如果角色仍然在无输入时移动，可以增大此值（例如 0.15 或 0.2）
   - 如果按键响应不够灵敏，可以减小此值（例如 0.05）

2. **Mouse Sensitivity**（鼠标灵敏度）
   - 默认值：2.0
   - 根据个人喜好调整

---

## 技术细节

### 为什么会出现这个问题？

1. **Input.GetAxisRaw 的特性**：
   - 即使没有按键，也可能返回非常小的值（例如 0.001）
   - 这些微小值在乘以速度后，仍然会导致角色移动

2. **transform.Rotate 的影响**：
   - 旋转整个 GameObject 会改变 `transform.right` 和 `transform.forward` 的方向
   - 如果此时有输入，移动方向会基于新的朝向

3. **CharacterController 的碰撞**：
   - 当角色旋转时，如果 CharacterController 与墙壁接触，Unity 的物理系统可能会将角色推入墙壁

### 修复原理

1. **输入死区**：过滤掉小于阈值的输入值，确保只有明确的按键输入才会触发移动
2. **条件移动**：只有在有明确输入时才计算移动方向，避免基于微小噪声值移动
3. **世界空间旋转**：使用 `Space.World` 确保旋转在世界坐标系中进行，避免累积误差

---

## 测试建议

修复后，请测试以下场景：

1. ✅ **纯视角转动**：只移动鼠标，不按任何键，角色应该保持静止
2. ✅ **移动测试**：按下 WASD 键，角色应该正常移动
3. ✅ **靠近墙壁**：靠近墙壁时转动视角，角色不应该被推入墙壁
4. ✅ **快速转动**：快速转动视角，角色应该保持稳定

---

> **提示**：如果问题仍然存在，请检查 CharacterController 的碰撞设置，确保墙壁有正确的 Collider。

