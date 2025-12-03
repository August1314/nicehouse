# 健康监测模块测试指南

## 快速测试步骤

### 1. 准备工作（1分钟）

1. **确保场景配置正确：**
   - 在 Hierarchy 中检查是否有 `HealthDataStore` GameObject（通常在 DataRoot 下）
   - 检查是否有 `HealthMonitoringController` GameObject
   - 检查是否有 `HealthMonitoringPanel` GameObject（在 Canvas 下）

2. **运行 UI 生成脚本（如果还没创建）：**
   - 菜单：`NiceHouse > UI > 生成 HealthMonitoringPanel UI`
   - 脚本会自动创建所有 UI 元素和 GameObject

### 2. 基础功能测试（2分钟）

#### 测试 1：运行游戏，检查 Console
1. 点击 Unity 的 **Play** 按钮
2. 打开 **Console** 窗口
3. **检查是否有错误：**
   - ✅ 没有红色错误 = 正常
   - ❌ 有红色错误 = 需要修复

#### 测试 2：检查 UI 显示
1. 运行游戏后，查看 `HealthMonitoringPanel` UI
2. **应该看到：**
   - ✅ "Status: Healthy" 或 "Status: Abnormal"
   - ✅ "Heart Rate: XX bpm (Normal/Low/High)"
   - ✅ "Respiration: XX /min (Normal/Low/High)"
   - ✅ "Body Movement: X.XX"
   - ✅ "Sleep Stage: Awake/Light Sleep/Deep Sleep"
   - ✅ 体动进度条（绿色/黄色/红色）

### 3. 健康数据实时更新测试（1分钟）

#### 测试 3：观察数据变化
1. **观察心率：**
   - 心率应该在 62-82 bpm 之间波动（默认值）
   - 颜色应该是绿色（正常）

2. **观察呼吸率：**
   - 呼吸率应该在 12-20 次/分钟之间波动
   - 颜色应该是绿色（正常）

3. **观察体动：**
   - 切换数字人状态（Walking, Sitting, Sleeping）
   - 体动强度应该相应变化
   - 进度条应该更新

4. **观察睡眠阶段：**
   - 切换到 "Sleeping" 状态
   - 睡眠阶段应该变为 "Light Sleep" 或 "Deep Sleep"
   - 切换到其他状态，应该变为 "Awake"

### 4. 健康异常告警测试（5分钟）

#### 测试 4：心率异常告警（快速测试）

**方法 1：修改 HealthDataStore 参数（推荐）**

1. **在 Hierarchy 中找到 `HealthDataStore` GameObject**
2. **在 Inspector 中修改参数：**
   - 将 `Base Heart Rate` 改为 `110`（超过正常范围 60-100）
   - 或者将 `Heart Rate Amplitude` 改为 `50`（波动范围更大）
3. **运行游戏，等待 30 秒**
4. **检查：**
   - ✅ Console 应该显示：`[HealthMonitoringController] Heart rate abnormal: XX bpm`
   - ✅ 告警列表应该显示新告警（类型：Health Abnormal）
   - ✅ UI 中心率应该显示红色（异常）

**方法 2：修改 HealthMonitoringController 阈值（快速测试）**

1. **在 Hierarchy 中找到 `HealthMonitoringController` GameObject**
2. **在 Inspector 中修改参数：**
   - 将 `Heart Rate Min` 改为 `70`（提高最小值）
   - 将 `Heart Rate Max` 改为 `75`（降低最大值）
   - 将 `Abnormal Duration Threshold` 改为 `5`（秒，快速测试）
3. **运行游戏，等待 5-10 秒**
4. **检查：**
   - ✅ 当心率超出 70-75 范围时，应该触发告警

#### 测试 5：呼吸率异常告警

1. **修改 HealthDataStore 参数：**
   - 将 `Base Respiration Rate` 改为 `25`（超过正常范围 12-20）
2. **运行游戏，等待 30 秒**
3. **检查：**
   - ✅ Console 应该显示呼吸率异常告警
   - ✅ UI 中呼吸率应该显示红色（异常）

#### 测试 6：体动异常告警（长时间无体动）

1. **切换到 Sleeping 状态：**
   - 在 MonitoringPanel 中点击 "Sleeping" 按钮
   - 体动强度应该降低到 0.05 左右
2. **修改 HealthMonitoringController 参数：**
   - 将 `No Movement Duration Threshold` 改为 `10`（秒，快速测试）
   - 将 `Body Movement Min` 改为 `0.2`（提高最小值）
3. **等待 10 秒**
4. **检查：**
   - ✅ 应该触发体动异常告警
   - ✅ Console 应该显示：`No body movement detected for extended period`

### 5. 告警响应测试（2分钟）

#### 测试 7：告警响应
1. **触发一个健康异常告警**
2. **检查：**
   - ✅ Console 应该显示告警日志
   - ✅ 告警列表应该显示新告警（在 MonitoringPanel 中）
   - ✅ 告警类型应该是 "Health Abnormal"
   - ✅ 告警响应系统应该工作（如果配置了音效、灯光）

### 6. UI 颜色编码测试（1分钟）

#### 测试 8：颜色编码
1. **正常状态：**
   - 心率、呼吸率在正常范围内
   - ✅ 应该显示绿色

2. **异常状态：**
   - 修改参数使心率/呼吸率异常
   - ✅ 应该显示红色
   - ✅ 状态文本应该显示 "Abnormal"

## 完整测试清单

### 基础功能
- [ ] 游戏可以正常运行，没有 Console 错误
- [ ] UI 面板正常显示
- [ ] 健康数据实时更新

### 数据更新
- [ ] 心率实时更新
- [ ] 呼吸率实时更新
- [ ] 体动强度实时更新
- [ ] 睡眠阶段实时更新
- [ ] 体动进度条实时更新

### 异常检测
- [ ] 心率异常检测正确
- [ ] 呼吸率异常检测正确
- [ ] 体动异常检测正确
- [ ] 异常持续时间阈值正确

### 告警触发
- [ ] 心率异常告警触发
- [ ] 呼吸率异常告警触发
- [ ] 体动异常告警触发
- [ ] 告警去重机制正常

### UI 显示
- [ ] 颜色编码正确（正常=绿色，异常=红色）
- [ ] 状态文本正确
- [ ] 进度条显示正确

## 快速测试技巧

### 技巧 1：快速触发心率异常
1. 修改 `HealthDataStore` 的 `Base Heart Rate` 为 `110`
2. 修改 `HealthMonitoringController` 的 `Abnormal Duration Threshold` 为 `5`（秒）
3. 运行游戏，等待 5 秒即可触发告警

### 技巧 2：快速触发呼吸率异常
1. 修改 `HealthDataStore` 的 `Base Respiration Rate` 为 `25`
2. 修改 `HealthMonitoringController` 的 `Abnormal Duration Threshold` 为 `5`（秒）
3. 运行游戏，等待 5 秒即可触发告警

### 技巧 3：快速触发体动异常
1. 切换到 "Sleeping" 状态（体动降低）
2. 修改 `HealthMonitoringController` 的 `No Movement Duration Threshold` 为 `10`（秒）
3. 修改 `Body Movement Min` 为 `0.2`
4. 运行游戏，等待 10 秒即可触发告警

## 常见问题

**Q: 为什么没有触发告警？**
A: 检查：
- HealthMonitoringController 的 `Enable Monitoring` 是否勾选？
- 异常持续时间是否超过阈值（默认30秒）？
- 健康数据是否真的异常（检查 HealthDataStore 的参数）？

**Q: UI 不显示数据？**
A: 检查：
- HealthMonitoringPanel 脚本的所有字段是否已绑定？
- HealthDataStore.Instance 是否为 null？
- 运行 UI 生成脚本重新创建 UI

**Q: 数据不更新？**
A: 检查：
- HealthDataStore 的 `Update Interval` 设置
- HealthMonitoringPanel 的 `Update Interval` 设置

## 测试结果记录

**测试日期：** _______________

**测试人员：** _______________

**通过项：** ___ / ___

**失败项：**
- [ ] 问题1：________________
- [ ] 问题2：________________

**备注：**
_________________________________________________

