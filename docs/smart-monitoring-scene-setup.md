# 智能监护模块场景配置指南

## 概述

本指南说明如何在 Unity 场景中配置智能监护模块所需的所有 GameObject。

## 必需的 GameObject

### 1. 数据底座 Manager（如果还没有）

确保场景中有以下 Manager GameObject（通常在 `DataRoot` 下）：

- **PersonStateController** - 数字人状态控制器
  - 挂载 `PersonStateController` 脚本
  - 可以创建一个空 GameObject，命名为 "PersonStateController"
  
- **AlarmManager** - 告警管理器
  - 挂载 `AlarmManager` 脚本
  - 可以创建一个空 GameObject，命名为 "AlarmManager"

- **DeviceManager** - 设备管理器（用于灯光控制）
  - 挂载 `DeviceManager` 脚本
  - 如果还没有，需要创建

### 2. 智能监护模块 GameObject

#### MonitoringController
1. 在 Hierarchy 中创建空 GameObject，命名为 `MonitoringController`
2. 添加 `Monitoring Controller (Script)` 组件
3. 配置参数：
   - `Long Sitting Threshold`: 30（分钟）
   - `Long Bathing Threshold`: 20（分钟）
   - `Alarm Cooldown`: 60（秒）
   - `Check Interval`: 1（秒）
   - `Enable Monitoring`: ✓（勾选）

#### AlarmResponseHelper
1. 在 Hierarchy 中创建空 GameObject，命名为 `AlarmResponseHelper`
2. 添加 `Alarm Response Helper (Script)` 组件
3. 配置参数：
   - `Enable Light Flash`: ✓（勾选）
   - `Flash Interval`: 0.5（秒）
   - `Show Console Message`: ✓（勾选）
   - `UI Canvas`: 拖入 Canvas GameObject（可选，用于弹窗）
   - `Normal Alarm Sound`: 拖入普通告警音效（可选）
   - `Emergency Alarm Sound`: 拖入紧急告警音效（可选）

#### PersonStateSimulator（可选，用于演示）
1. 在 Hierarchy 中创建空 GameObject，命名为 `PersonStateSimulator`
2. 添加 `Person State Simulator (Script)` 组件
3. 配置参数：
   - `Current Room Id`: "LivingRoom01"
   - `Enable Auto Switch`: 取消勾选（手动控制）
   - 如果启用自动切换，设置 `Switch Interval` 和 `State Sequence`

### 3. UI 面板

#### MonitoringPanel
- 应该已经通过 Editor 脚本创建
- 确保所有字段都已正确绑定
- 检查告警项预制体是否已创建（`Assets/Prefabs/AlarmItemPrefab.prefab`）

## 快速检查清单

### 数据底座检查
- [x] PersonStateController GameObject 存在且已挂载脚本
- [x] AlarmManager GameObject 存在且已挂载脚本
- [x] DeviceManager GameObject 存在（如果要用灯光闪烁功能）

### 智能监护模块检查
- [x] MonitoringController GameObject 存在且已挂载脚本
- [x] AlarmResponseHelper GameObject 存在且已挂载脚本
- [x] PersonStateSimulator GameObject 存在（可选）

### UI 检查
- [x] MonitoringPanel GameObject 存在
- [x] MonitoringPanel 脚本的所有字段都已绑定
- [x] 告警项预制体已创建（`Assets/Prefabs/AlarmItemPrefab.prefab`）

## 测试步骤

### 1. 基础功能测试

1. **运行游戏**
2. **检查 Console**：确保没有错误信息
3. **检查 UI 显示**：
   - 状态显示是否正确（State, Room, Duration）
   - 告警列表是否显示（可能为空，正常）

### 2. 状态切换测试

1. **点击状态切换按钮**（如 "Sitting"）
2. **观察 UI 更新**：
   - State 文本是否更新
   - Duration 是否开始计时
   - Room 是否正确显示

### 3. 告警触发测试

#### 测试跌倒告警
1. 点击 "Fallen" 按钮
2. 应该立即触发告警
3. 检查：
   - Console 是否有告警日志
   - 告警列表是否显示新告警
   - 灯光是否闪烁（如果有灯光设备）

#### 测试久坐告警
1. 点击 "Sitting" 按钮
2. 等待超过阈值时间（默认30分钟，测试时可以修改阈值）
3. 或者直接修改 `MonitoringController` 的 `Long Sitting Threshold` 为 0.1（分钟），然后点击 "Sitting" 按钮，等待 6 秒
4. 检查告警是否触发

#### 测试久浴告警
1. 点击 "Bathing" 按钮
2. 等待超过阈值时间（默认20分钟，测试时可以修改）
3. 或者修改 `Long Bathing Threshold` 为 0.1（分钟），等待 6 秒
4. 检查告警是否触发

### 4. 告警处理测试

1. 触发一个告警
2. 在告警列表中，点击 "Handle" 按钮
3. 检查：
   - 告警状态是否变为 "Handled"
   - 按钮是否变为不可点击

### 5. 告警设置测试

1. 修改 `Long Sitting Threshold Input` 的值（如改为 1）
2. 点击 "Apply Settings" 按钮
3. 检查 `MonitoringController` 的 `Long Sitting Threshold` 是否更新

## 常见问题

**Q: 告警不触发？**
A: 检查：
- MonitoringController 的 `Enable Monitoring` 是否勾选
- PersonStateController.Instance 是否为 null
- AlarmManager.Instance 是否为 null
- Console 是否有错误信息

**Q: 告警列表不显示？**
A: 检查：
- AlarmListContent 是否正确绑定到 ScrollView 的 Content
- AlarmItemPrefab 是否正确绑定
- AlarmManager.Instance 是否为 null

**Q: 状态切换没有反应？**
A: 检查：
- PersonStateSimulator 是否存在
- PersonStateController.Instance 是否为 null
- 按钮是否正确绑定到脚本字段

**Q: 灯光不闪烁？**
A: 检查：
- AlarmResponseHelper 的 `Enable Light Flash` 是否勾选
- 房间内是否有 Light 设备（DeviceType.Light）
- DeviceManager 是否正确配置

## 下一步

配置完成后，可以：
1. 进行功能测试
2. 提交代码到 Git
3. 继续开发其他功能模块

