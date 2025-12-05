# AC 设备完整配置指南

## ✅ 当前配置状态

根据你的截图，以下配置已正确：

1. ✅ **Device Definition**
   - Device Id: "AC_bedroom"
   - Type: Air Conditioner
   - Room Id: "BedRoom01"（已修正）

2. ✅ **AirConditionerController**
   - 已添加组件
   - Target Temperature: 24°C

3. ✅ **EnergyManager**
   - Device Id: "AC_bedroom"
   - Rated Power: 500W

4. ✅ **Mesh Renderer**
   - Materials: AC_mat

---

## 🎯 让面板能控制空调

### 问题：面板的 currentRoomId 必须与设备的 roomId 匹配

**当前设备配置：**
```
Room Id: "BedRoom01"
```

**需要检查的面板配置：**

#### 方法 1：EnvironmentControlPanel（如果使用独立面板）

1. **找到 EnvironmentControlPanel GameObject**
   - 在 Hierarchy 中搜索 "EnvironmentControlPanel"

2. **检查 currentRoomId**
   - 在 Inspector 中找到 `Environment Control Panel (Script)` 组件
   - 查看 `Current Room Id` 字段
   - **必须设置为 `"BedRoom01"`**（与设备的 Room Id 一致）

3. **如果面板显示的是其他房间**
   - 将 `Current Room Id` 改为 `"BedRoom01"`
   - 或者使用房间切换功能切换到卧室

#### 方法 2：EnvironmentControlPanelAdapter（如果使用 Control Hub）

1. **找到 EnvironmentControlPanelAdapter GameObject**
   - 在 Hierarchy 中搜索 "EnvironmentControlPanelAdapter"

2. **检查 currentRoomId**
   - 在 Inspector 中找到 `Environment Control Panel Adapter (Script)` 组件
   - 查看 `Current Room Id` 字段
   - **必须设置为 `"BedRoom01"`**

---

## 🖱️ 让鼠标点击能控制空调

### 步骤 1：添加 DeviceInteractable 组件

1. **选择 AC GameObject**
   - 在 Hierarchy 中找到你的 AC 对象

2. **添加组件**
   - 点击 `Add Component`
   - 搜索 `Device Interactable`
   - 添加组件

3. **配置组件（可选）**
   ```
   Device Interactable:
   ├── Hover Hint: "点击开关空调"（或留空使用默认）
   ├── Click Sound: （可选，拖入音频文件）
   └── Enable Debug Log: false（调试时开启）
   ```

### 步骤 2：确保有 Collider

**检查：**
1. AC GameObject 必须有 `Collider` 组件
   - 可以是 `BoxCollider`、`MeshCollider` 等
   - **不能是 Trigger**（`Is Trigger` 必须取消勾选）

2. **如果没有 Collider：**
   - 点击 `Add Component`
   - 搜索 `Box Collider` 或 `Mesh Collider`
   - 添加并调整大小以覆盖空调模型

### 步骤 3：确保有 FPRaycastInteractor

**检查场景中是否有 FPRaycastInteractor：**

1. **在 Hierarchy 中搜索 "FPRaycastInteractor"**
   - 应该在 Player 或 Camera 对象上

2. **如果没有：**
   - 找到 Player GameObject（第一人称控制器）
   - 点击 `Add Component`
   - 搜索 `FP Raycast Interactor`
   - 添加组件

3. **配置 FPRaycastInteractor：**
   ```
   FP Raycast Interactor:
   ├── Target Camera: （自动找到或手动指定）
   ├── Max Distance: 4（交互距离）
   ├── Interactable Layers: Everything（或指定图层）
   ├── Crosshair UI: （可选，准星UI）
   └── Require Cursor Lock: true（需要锁定鼠标）
   ```

---

## 📋 完整配置检查清单

### AC GameObject 组件清单

- [x] Transform
- [x] Mesh Filter
- [x] Mesh Renderer
- [x] Device Definition
  - [x] Device Id: "AC_bedroom"
  - [x] Type: Air Conditioner
  - [x] Room Id: "BedRoom01"
- [x] Air Conditioner Controller
- [ ] **Collider**（BoxCollider 或 MeshCollider）
  - [ ] **Is Trigger: 取消勾选**
- [ ] **Device Interactable**（新增）

### EnergyManager 配置

- [x] Device Configs
  - [x] Device Id: "AC_bedroom"
  - [x] Rated Power: 500

### 面板配置

- [ ] **EnvironmentControlPanel.currentRoomId = "BedRoom01"**
- [ ] 或 **EnvironmentControlPanelAdapter.currentRoomId = "BedRoom01"**

### 第一人称交互系统

- [ ] **FPRaycastInteractor** 在 Player 上
- [ ] **CrosshairUI**（可选，用于显示交互提示）

---

## 🧪 测试步骤

### 测试 1：面板控制

1. **运行游戏**
2. **打开环境控制面板**
3. **检查房间选择**
   - 确认显示的是 "BedRoom01"（卧室）
   - 如果显示其他房间，切换到卧室
4. **查看空调状态**
   - 应该显示 "ON" 或 "OFF"（不再是 "N/A"）
5. **点击空调按钮**
   - 应该能正常开关空调
   - 状态文本应该更新

### 测试 2：鼠标点击控制

1. **运行游戏**
2. **进入第一人称视角**
3. **将鼠标对准空调**
   - 准星应该改变（如果有 CrosshairUI）
   - 应该显示提示文本（例如："点击开关空调"）
4. **点击鼠标左键**
   - 空调应该切换开关状态
   - 如果开启，应该看到扫风动画（如果有导风板）
5. **检查能耗**
   - 开启后，EnergyManager 应该记录能耗

---

## ⚠️ 常见问题

### Q1: 面板显示 "N/A"

**原因：**
- 面板的 `currentRoomId` 与设备的 `roomId` 不匹配
- 设备没有 `AirConditionerController` 组件

**解决：**
1. 检查面板的 `currentRoomId` 是否为 `"BedRoom01"`
2. 确认 AC 有 `AirConditionerController` 组件

### Q2: 鼠标点击没有反应

**原因：**
- 没有 `DeviceInteractable` 组件
- 没有 `Collider` 或 Collider 是 Trigger
- 没有 `FPRaycastInteractor` 组件
- 鼠标未锁定（`Cursor.lockState != CursorLockMode.Locked`）

**解决：**
1. 添加 `DeviceInteractable` 组件
2. 添加 `Collider`，确保 `Is Trigger` 未勾选
3. 在 Player 上添加 `FPRaycastInteractor`
4. 确保鼠标已锁定（按 ESC 解锁，再按一次锁定）

### Q3: 射线检测不到空调

**原因：**
- Collider 太小或位置不对
- Collider 是 Trigger
- 空调在错误的图层上

**解决：**
1. 调整 Collider 大小，确保覆盖整个空调模型
2. 取消勾选 `Is Trigger`
3. 检查 `FPRaycastInteractor` 的 `Interactable Layers` 设置

### Q4: 点击后没有声音

**原因：**
- 没有设置 `Click Sound`
- 没有 `AudioSource` 组件

**解决：**
1. 在 `DeviceInteractable` 中设置 `Click Sound`
2. 组件会自动添加 `AudioSource`（如果需要）

---

## 🎯 快速配置步骤总结

### 面板控制（必须）

```
1. 找到 EnvironmentControlPanel 或 EnvironmentControlPanelAdapter
2. 设置 currentRoomId = "BedRoom01"
3. 完成！
```

### 鼠标点击控制（必须）

```
1. 选择 AC GameObject
2. 添加 Collider（如果不是 Trigger）
3. 添加 Device Interactable 组件
4. 在 Player 上添加 FPRaycastInteractor（如果没有）
5. 完成！
```

---

## 📝 配置示例

### AC GameObject 完整配置

```
AC (GameObject)
├── Transform
│   └── Position/Rotation/Scale
├── Mesh Filter
│   └── Mesh: AC
├── Mesh Renderer
│   └── Materials: AC_mat
├── Box Collider（或 Mesh Collider）
│   └── Is Trigger: ❌ 未勾选
├── Device Definition
│   ├── Device Id: "AC_bedroom"
│   ├── Type: Air Conditioner
│   └── Room Id: "BedRoom01"
├── Air Conditioner Controller
│   ├── Vent Blade: （可选）
│   ├── Target Temperature: 24
│   └── 其他参数（默认）
└── Device Interactable（新增）
    ├── Hover Hint: "点击开关空调"
    ├── Click Sound: （可选）
    └── Enable Debug Log: false
```

---

> **提示**：配置完成后，记得测试两个功能：
> 1. 面板控制：打开面板，点击空调按钮
> 2. 鼠标点击：第一人称视角，对准空调点击

