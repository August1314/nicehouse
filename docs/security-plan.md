## 安全模块开发计划

> 目标：补齐烟雾/燃气告警链路、完善告警生命周期与演示体验，确保安全相关功能在课堂演示和多场景切换中稳定可用。

### 1. 范围与依赖
- 数据基座：`SafetyDataStore`（烟雾/燃气浓度）、`PersonStateController`（行为）、`HealthDataStore`（生命体征）、`AlarmManager`（统一告警）、`DeviceManager`（灯光/设备控制）。
- 告警类型覆盖：Smoke、GasLeak、Fall、LongSitting、LongBathing、HealthAbnormal、EmergencyCall（预留）。
- UI/演示：`DataDashboard`、智能监护面板、健康监测面板、告警弹窗/音效/灯光（`AlarmResponseHelper`）。

### 2. 现状摘要
- 已有：`AlarmManager` 统一记录/查询/处理，带事件；`SafetyDataStore` 房间级烟雾/燃气随机模拟与读写接口；智能监护与健康监测控制器可触发告警；`AlarmResponseHelper` 支持弹窗/音效/灯光；`DataDashboard` 可展示安全数据与最近告警。
- 缺口：烟雾/燃气未做阈值判断与告警触发；告警/数据未做场景保活；紧急呼叫无入口；UI 阶段性资产（监护/健康面板、阈值配置 SO）待核对；测试覆盖不足。

### 3. 里程碑与时间
- M1 基线接通（1-2 天）：烟雾/燃气阈值检测 + 告警触发 + 响应链路打通；Dashboard/演示按钮验证。
- M2 场景稳定（1 天）：告警/数据单例保活策略、异常日志与降级、防重入检查。
- M3 体验完善（1-2 天）：紧急呼叫入口、UI 配置与示例场景；基础回归脚本/手册。

### 4. 任务拆分
#### 4.1 数据与阈值
- 实现 `SafetyDataThresholds`（ScriptableObject）配置：烟雾、燃气报警/预警阈值、抖动平滑系数。
- 在 `SafetyDataStore` 周期检测阈值：超过报警阈值触发 `AlarmType.Smoke/GasLeak`，加入冷却去重；低于恢复阈值可选清除闪烁/提示。
- 为烟雾/燃气提供演示接口：一键注入高浓度/恢复函数（便于课堂展示）。

#### 4.2 告警管理与保活
- `AlarmManager`、`SafetyDataStore` 采用单例保活策略（场景切换不丢数据，必要时专用根物体 + DontDestroyOnLoad）。
- 为告警记录可选持久化/导出（轻量 JSON 缓存，非必选，但预留接口）。
- 增强去重与日志：同房间同类型冷却时间、缺失依赖（设备/Canvas）时的警告日志。

#### 4.3 告警响应与 UI
- 将烟雾/燃气告警纳入 `AlarmResponseHelper`：默认黄色/红色闪烁与普通/紧急音效映射。
- 完成或复用监护/健康面板：告警列表、处理按钮、阈值调节、状态切换演示。
- `DataDashboard` 增加阈值状态高亮（正常/预警/报警），便于教师讲解。

#### 4.4 场景与演示脚本
- Demo 按钮：一键触发烟雾/燃气超标、一键恢复；跌倒/久坐/久浴快捷触发；健康异常手动触发。
- 演示流程脚本：按顺序播放多个告警，验证列表顺序、灯光/音效、处理状态。

#### 4.5 测试与验收
- 手动检查用例：烟雾超标、燃气超标、跌倒、久坐、久浴、健康异常、紧急呼叫；处理按钮后闪烁/音效停止。
- 自动/半自动：为 `SafetyDataStore` 阈值检测、告警冷却、告警记录上限编写简单 PlayMode 测试或脚本化验证。
- 验收标准：各告警类型可触发、可查看、可处理；灯光/音效响应正确；数据在场景切换后仍在。

### 5. 资源与接口清单
- 代码：`Assets/Scripts/Data/SafetyDataStore.cs`、`AlarmManager.cs`、`SmartMonitoring/Controllers/MonitoringController.cs`、`HealthMonitoring/Controllers/HealthMonitoringController.cs`、`SmartMonitoring/Utils/AlarmResponseHelper.cs`、`UI/DataDashboard.cs`。
- 预置/配置：新增 `SafetyDataThresholds.asset`（待建），告警弹窗 prefab（如有）、音效资源、灯光设备标签。

### 6. 风险与缓解
- 场景切换丢数据：采用 DontDestroyOnLoad 或集中加载；必要时做启动自检日志。
- 误报抖动：阈值加入滞后区/平滑系数 + 冷却时间。
- 依赖缺失（设备/Canvas/音源）：启动时自检并降级为日志提醒。
- 演示复杂度：提供一键触发与恢复按钮，减轻课堂操作成本。

