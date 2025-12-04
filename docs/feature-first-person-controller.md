## 第一人称漫游模块开发文档

> 目标：为“好房子”项目提供一个最基础的第一人称可控角色，支持 WASD 移动与鼠标视角旋转，便于课堂演示时自由走动查看各个房间。

---

### 1. 功能模块基本信息

**模块名称：** 第一人称漫游（First-Person Walkthrough）

**模块编号：** FP-001

**负责人：** [待定]

**关联需求：** 提供玩家/讲解者在房屋内自由漫游的能力，辅助分层可视化与各功能模块演示。

**核心价值：** 通过最基础的 WASD + 鼠标操作，让讲解者/参观者可以沉浸式体验好房子场景，无需复杂输入系统即可快速上手。

---

### 2. 功能模块描述

#### 2.1 功能概述

- **第一人称移动**：使用 `CharacterController` 和旧版 Input Manager（`Input.GetAxis`）实现 WASD 行走、Space 跳跃（可选）、重力控制。
- **鼠标视角**：移动鼠标控制水平/垂直视角，支持垂直角度限制（例如 -80° ~ 80°）。
- **碰撞/地面**：`CharacterController` 负责与地面/墙体碰撞，确保玩家不会穿模。
- **UI 暂停/鼠标锁定**：进入漫游时隐藏系统鼠标并锁定；按 `Esc` 可解锁鼠标（可选扩展）。

#### 2.2 核心功能点

- [ ] 基于 `CharacterController` 的 WASD 位移与重力
- [ ] 鼠标水平/垂直视角控制（FPS 相机）
- [ ] Player 预制体（含 Camera、Audio Listener）
- [ ] 场景启动/停止时的鼠标锁定与可视化提示

#### 2.3 用户场景

- **场景 1：课堂漫游**  
  - 触发：讲解者按 Play 进入场景  
  - 操作：使用 WASD 走到客厅、卧室等位置  
  - 响应：角色移动与视角变化平滑，碰到墙体会停止，视角高度约 1.6m

- **场景 2：查看分层图层细节**  
  - 触发：开启分层热力图后，切换到第一人称视角  
  - 操作：走到特定房间观察地面颜色、告警提示  
  - 响应：人物移动时不影响 LayerPanel UI，可随时停下来调整图层

---

### 3. 设计说明

#### 3.1 架构设计

```
FirstPerson/
├── Prefabs/
│   └── FPSPlayer.prefab          # Player + Camera + CharacterController
└── Scripts/
    └── FirstPersonController.cs  # 核心移动/视角逻辑
```

**关键组件：**

| 组件 | 职责 | 依赖 |
|------|------|------|
| `FirstPersonController` | 读取输入、计算移动、应用重力、控制相机旋转 | `CharacterController` |
| `CharacterController` | Unity 内置碰撞盒，处理与地面/墙体的碰撞与坡度限制 | 场景中需具备 Collider 的地板/墙体 |
| `Camera`（Player 子节点） | 第一人称视角，挂载 `AudioListener` | `FirstPersonController` |

#### 3.2 数据与输入依赖

- **输入系统**：使用旧 Input Manager（轴名称 `Horizontal`、`Vertical`、`Mouse X`、`Mouse Y`、`Jump`）。
- **场景碰撞体**：地面至少需要 `MeshCollider` / `BoxCollider`；墙体同理。
- **分层模块联动**：无直接依赖，但 Player 的存在不应影响 LayerPanel UI，建议将 UI Canvas 置于 Screen Space - Overlay。

#### 3.3 交互设计

- **移动键位**：W/S 前后、A/D 左右；速度可配（默认 3.5 m/s）。
- **鼠标**：移动控制视角；灵敏度可配（默认 2.0）；垂直视角限制。
- **跳跃（可选）**：Space 键触发向上速度。
- **鼠标锁定**：进入 Play 时 `Cursor.lockState = Locked`，按 `Esc` 解锁并显示鼠标；再次点击屏幕恢复锁定（可加 UI 提示）。

---

### 4. 开发任务

#### 4.1 核心功能

1. **任务 FP-4.1.1：创建 FirstPersonController 脚本**  
   - 使用 `CharacterController.Move()` 处理位移与重力  
   - 鼠标输入驱动 `transform.Rotate` 和 Camera 的本地 X 旋转  
   - Inspector 暴露参数：移动速度、鼠标灵敏度、重力、跳跃力度、垂直角度范围  
   - 验收：在空场景中可流畅移动、碰撞不穿模

2. **任务 FP-4.1.2：创建 FPSPlayer 预制体**  
   - 结构：`Player (CharacterController + FirstPersonController)`，子节点 `Camera`  
   - 相机高度 ~1.6m，`AudioListener` 挂在 Camera 上  
   - 预制体放入 `Assets/Prefabs/FirstPerson/FPSPlayer.prefab`

3. **任务 FP-4.1.3：场景集成**  
   - 将 FPSPlayer 放入 `SampleScene` 的入口位置（靠近前门）  
   - 关闭/禁用多余的 `MainCamera`（避免两个 AudioListener）  
   - 确认地面/墙体都有 Collider，角色不会下沉

4. **任务 FP-4.1.4：鼠标锁定/解锁**  
   - Play 时自动锁定并隐藏光标  
   - 按 `Esc` 解锁并暂停输入；按鼠标左键恢复  
   - 可加 UI 提示（可选）

#### 4.2 测试

- 行走在房屋内不穿墙、不下沉  
- 上坡/台阶高度（<= CharacterController step offset）可走过  
- 鼠标垂直限制有效，不会翻到背后  
- UI/分层面板可正常操作，与第一人称控制互不干扰  
- 支持不同帧率（使用 `Time.deltaTime`）

---

### 5. 开发计划（示例）

| 阶段 | 任务 | 时间 | 负责人 |
|------|------|------|--------|
| Day1 | 脚本编写 + 预制体搭建 | 上午 | [姓名] |
| Day1 | 场景集成 + 碰撞调试 | 下午 | [姓名] |
| Day2 | 体验调优 + 文档补充 | 上午 | [姓名] |

---

### 6. 风险与注意事项

- **Collider 缺失**：若地面或墙壁没有 Collider，角色会掉落/穿模，需要与模型/场景同学配合补齐。  
- **性能**：`Update Interval` 很小可导致 CPU 占用增加，建议保持 60fps 以上测试。  
- **光标锁定**：在编辑器中调试时可能需要频繁解锁，可在 Inspector 暂时关闭锁定逻辑。  
- **移动速度**：室内空间较小，建议 3~4 m/s，避免眩晕。

---

### 7. 扩展建议（后续）

- 集成 Cinemachine FreeLook 或新输入系统  
- 添加奔跑/蹲伏/交互能力（开门、触发图层）  
- 结合分层 UI，允许玩家在漫游时直接打开/关闭图层、查看房间信息  
- VR/手柄支持

---

### 8. 更新记录

| 日期 | 版本 | 更新内容 | 更新人 |
|------|------|----------|--------|
| 2025-12-03 | 1.0 | 初版规划与任务分解 | 梁力航 |


