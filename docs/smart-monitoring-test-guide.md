# 智能监护模块测试指南

## 快速测试步骤

### 1. 准备工作（1分钟）

1. **确保场景配置正确：**
   - 在 Hierarchy 中检查是否有以下 GameObject：
     - `DataRoot`（包含所有 Manager）
     - `MonitoringController`
     - `AlarmResponseHelper`
     - `PersonStateSimulator`
     - `MonitoringPanel`（UI 面板）

2. **检查脚本绑定：**
   - 选中 `MonitoringPanel` GameObject
   - 在 Inspector 中查看 `Monitoring Panel (Script)` 组件
   - 确保所有字段都已绑定（不显示 "None"）

### 2. 基础功能测试（2分钟）

#### 测试 1：运行游戏，检查 Console
1. 点击 Unity 的 **Play** 按钮
2. 打开 **Console** 窗口（`Window > General > Console`）
3. **检查是否有错误：**
   - ✅ 没有红色错误 = 正常
   - ❌ 有红色错误 = 需要修复

#### 测试 2：检查 UI 显示
1. 运行游戏后，查看 `MonitoringPanel` UI
2. **应该看到：**
   - ✅ "State: Idle"（或当前状态）
   - ✅ "Room: LivingRoom01"（或当前房间）
   - ✅ "Duration: 00:00"（或当前持续时间）
   - ✅ 7 个状态按钮（Idle, Walking, Sitting 等）
   - ✅ 房间下拉菜单
   - ✅ 告警列表区域（可能为空）
   - ✅ 告警设置区域（输入框、开关、按钮）

### 3. 状态切换测试（3分钟）

#### 测试 3：切换不同状态
1. **点击 "Walking" 按钮**
   - ✅ State 应该更新为 "Walking"
   - ✅ Duration 应该开始计时（00:01, 00:02...）

2. **点击 "Sitting" 按钮**
   - ✅ State 应该更新为 "Sitting"
   - ✅ Duration 应该重置并重新计时

3. **点击 "Bathing" 按钮**
   - ✅ State 应该更新为 "Bathing"

4. **点击 "Sleeping" 按钮**
   - ✅ State 应该更新为 "Sleeping"

5. **测试房间切换：**
   - 使用房间下拉菜单选择不同房间
   - ✅ Room 文本应该更新

### 4. 告警触发测试（5分钟）

#### 测试 4：跌倒告警（立即触发）
1. **点击 "Fallen" 按钮**
2. **检查：**
   - ✅ Console 应该显示告警日志：`[MonitoringController] Alarm triggered: Fall in [房间ID]`
   - ✅ 告警列表应该显示新告警
   - ✅ 告警类型应该是 "Fall/OutOfBed"
   - ✅ 告警状态应该是 "Unhandled"（红色）

#### 测试 5：久坐告警（超时触发）
1. **修改阈值（快速测试）：**
   - 在 Inspector 中选中 `MonitoringController` GameObject
   - 将 `Long Sitting Threshold` 改为 `0.1`（分钟，即 6 秒）
   - 确保 `Enable Monitoring` 已勾选

2. **触发告警：**
   - 点击 "Sitting" 按钮
   - 等待 6-7 秒
   - ✅ 应该自动触发告警
   - ✅ Console 应该显示：`[MonitoringController] Alarm triggered: LongSitting in [房间ID]`
   - ✅ 告警列表应该显示新告警

#### 测试 6：久浴告警（超时触发）
1. **修改阈值：**
   - 将 `Long Bathing Threshold` 改为 `0.1`（分钟）

2. **触发告警：**
   - 点击 "Bathing" 按钮
   - 等待 6-7 秒
   - ✅ 应该自动触发告警

#### 测试 7：告警去重（冷却时间）
1. **触发第一个告警：**
   - 点击 "Fallen" 按钮
   - ✅ 告警应该触发

2. **立即再次触发：**
   - 再次点击 "Fallen" 按钮（在 60 秒内）
   - ✅ 不应该触发新告警（因为还在冷却期内）

3. **等待冷却期后：**
   - 等待 60 秒后再次点击
   - ✅ 应该可以触发新告警

### 5. 告警处理测试（2分钟）

#### 测试 8：标记告警为已处理
1. **触发一个告警：**
   - 点击 "Fallen" 按钮

2. **处理告警：**
   - 在告警列表中找到该告警
   - 点击 "Handle" 按钮
   - ✅ 告警状态应该变为 "Handled"（灰色）
   - ✅ "Handle" 按钮应该变为不可点击

### 6. 告警设置测试（2分钟）

#### 测试 9：修改告警阈值
1. **修改久坐阈值：**
   - 在 `Long Sitting Threshold Input` 输入框中输入 `1`
   - 点击 "Apply Settings" 按钮
   - ✅ `MonitoringController` 的 `Long Sitting Threshold` 应该更新为 1

2. **修改久浴阈值：**
   - 在 `Long Bathing Threshold Input` 输入框中输入 `0.5`
   - 点击 "Apply Settings" 按钮
   - ✅ `MonitoringController` 的 `Long Bathing Threshold` 应该更新为 0.5

#### 测试 10：启用/禁用监测
1. **禁用监测：**
   - 取消勾选 "Enable Monitoring" Toggle
   - 点击 "Apply Settings" 按钮
   - ✅ `MonitoringController` 的 `Enable Monitoring` 应该变为 false

2. **测试告警不再触发：**
   - 点击 "Sitting" 按钮
   - 等待超过阈值时间
   - ✅ 不应该触发告警（因为监测已禁用）

3. **重新启用：**
   - 勾选 "Enable Monitoring" Toggle
   - 点击 "Apply Settings" 按钮
   - ✅ 告警应该恢复触发

## 完整测试清单

### 基础功能
- [x] 游戏可以正常运行，没有 Console 错误
- [x] UI 面板正常显示
- [x] 状态显示正确（State, Room, Duration）

### 状态切换
- [x] Idle 状态切换正常
- [x] Walking 状态切换正常
- [x] Sitting 状态切换正常
- [x] Bathing 状态切换正常
- [x] Sleeping 状态切换正常
- [x] Fallen 状态切换正常
- [x] OutOfBed 状态切换正常
- [x] 房间切换正常

### 告警触发
- [x] 跌倒告警立即触发
- [x] 久坐告警超时触发
- [x] 久浴告警超时触发
- [x] 告警去重（冷却时间）正常工作

### 告警处理
- [x] 告警列表正确显示
- [x] 告警信息正确（类型、房间、时间）
- [x] 标记告警为已处理功能正常

### 告警设置
- [x] 修改久坐阈值功能正常
- [x] 修改久浴阈值功能正常
- [x] 启用/禁用监测功能正常

## 常见问题排查

### 问题 1：告警不触发

**检查清单：**
1. ✅ `MonitoringController` 的 `Enable Monitoring` 是否勾选？
2. ✅ `PersonStateController.Instance` 是否为 null？（检查 Console）
3. ✅ `AlarmManager.Instance` 是否为 null？（检查 Console）
4. ✅ 阈值设置是否正确？
5. ✅ 是否在冷却期内？

**解决方法：**
- 检查 Console 是否有错误信息
- 确保所有 Manager GameObject 都存在且已挂载脚本
- 检查 `MonitoringController` 的参数设置

### 问题 2：UI 不更新

**检查清单：**
1. ✅ `MonitoringPanel` 脚本的所有字段是否已绑定？
2. ✅ `PersonStateController.Instance` 是否为 null？
3. ✅ `Update Interval` 是否设置合理（默认 0.5 秒）？

**解决方法：**
- 检查 Inspector 中的字段绑定
- 确保 `PersonStateController` GameObject 存在

### 问题 3：告警列表不显示

**检查清单：**
1. ✅ `AlarmListContent` 是否正确绑定到 ScrollView 的 Content？
2. ✅ `AlarmItemPrefab` 是否正确绑定？
3. ✅ `AlarmManager.Instance` 是否为 null？

**解决方法：**
- 检查 `MonitoringPanel` 脚本中的 `Alarm List Content` 和 `Alarm Item Prefab` 字段
- 确保 `AlarmManager` GameObject 存在

### 问题 4：按钮点击无反应

**检查清单：**
1. ✅ 按钮是否正确绑定到脚本字段？
2. ✅ `EventSystem` 是否存在？（应该在 Canvas 下）
3. ✅ `PersonStateSimulator` 是否存在？

**解决方法：**
- 检查按钮绑定
- 确保场景中有 `EventSystem` GameObject

## 性能测试（可选）

### 测试 11：长时间运行
1. 运行游戏 5-10 分钟
2. 检查：
   - ✅ 帧率是否稳定（60 FPS）
   - ✅ 内存使用是否正常
   - ✅ 没有内存泄漏

### 测试 12：大量告警
1. 快速触发多个告警（10-20 个）
2. 检查：
   - ✅ 告警列表正确显示
   - ✅ 只保留最近 100 条告警（如果设置了限制）
   - ✅ UI 性能正常

## 测试结果记录

**测试日期：** _______________

**测试人员：** _______________

**通过项：** ___ / ___

**失败项：**
- [ ] 问题1：________________
- [ ] 问题2：________________

**备注：**
_________________________________________________

## 下一步

测试通过后，可以：
1. ✅ 提交代码到 Git
2. ✅ 继续开发其他功能模块
3. ✅ 优化性能和用户体验

