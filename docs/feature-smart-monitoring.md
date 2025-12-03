## 智能监护模块开发文档

> 本文档用于规划和管理智能监护模块的开发工作。

---

### 1. 功能模块基本信息

**模块名称：** 智能监护模块

**模块编号：** FM-002

**负责人：** [梁力航]

**关联需求：** 基础要求 - 功能项 - 智能监护（安全 + 智慧）

**核心价值：** 实现"以人为中心"的安全监护，通过监测数字人行为状态，及时发现异常情况（久坐、久浴、跌倒、坠床等），并触发告警和联动响应。

---

### 2. 功能模块描述

#### 2.1 功能概述

智能监护模块是"好房子"系统的安全核心功能之一，主要实现：

- **行为状态监测**：实时监测数字人的行为状态（站立、行走、久坐、久浴、躺下、跌倒、坠床等）
- **异常检测**：当检测到异常状态（久坐超时、久浴超时、跌倒、坠床）时，自动触发告警
- **告警响应**：告警时弹出UI提示、播放报警音效、闪烁灯光、记录告警信息
- **可视化展示**：在UI上实时显示数字人状态、所在房间、状态持续时间等信息

#### 2.2 核心功能点

- [ ] **状态监测**：从 `PersonStateController` 获取数字人实时状态
- [ ] **异常检测逻辑**：判断久坐、久浴、跌倒、坠床等异常情况
- [ ] **告警触发**：异常时调用 `AlarmManager.AddAlarm` 记录告警
- [ ] **告警响应**：UI提示、音效、灯光闪烁等视觉/听觉反馈
- [ ] **UI面板**：显示数字人状态、告警列表、控制按钮
- [ ] **状态切换控制**：提供按钮或时间线控制数字人状态变化（用于演示）

#### 2.3 用户场景

**场景1：久坐超时告警**
- **触发条件：** 数字人处于"久坐"状态超过30分钟
- **用户操作：** 无需操作（自动监测）
- **系统响应：** 
  1. 触发告警（AlarmType.LongSitting）
  2. 弹出UI提示"检测到久坐，建议起身活动"
  3. 播放提示音效
  4. 记录告警到 AlarmManager
  5. 在告警列表中显示

**场景2：久浴超时告警**
- **触发条件：** 数字人处于"久浴"状态超过20分钟
- **用户操作：** 无需操作（自动监测）
- **系统响应：**
  1. 触发告警（AlarmType.LongBathing）
  2. 弹出UI提示"检测到久浴，注意安全"
  3. 播放告警音效
  4. 闪烁房间灯光（如有）
  5. 记录告警

**场景3：跌倒检测告警**
- **触发条件：** 数字人状态切换为"跌倒"（Fallen）
- **用户操作：** 通过按钮或脚本触发跌倒状态（用于演示）
- **系统响应：**
  1. 立即触发告警（AlarmType.Fall）
  2. 弹出紧急UI提示"检测到跌倒！"
  3. 播放紧急告警音效
  4. 闪烁房间灯光（红色）
  5. 记录告警（标记为未处理）
  6. 在告警列表中高亮显示

**场景4：坠床检测告警**
- **触发条件：** 数字人状态切换为"坠床"（OutOfBed）
- **用户操作：** 通过按钮或脚本触发坠床状态（用于演示）
- **系统响应：**
  1. 立即触发告警（AlarmType.Fall，可扩展为 OutOfBed）
  2. 弹出紧急UI提示"检测到坠床！"
  3. 播放紧急告警音效
  4. 闪烁房间灯光（红色）
  5. 记录告警

**场景5：状态切换演示**
- **触发条件：** 用户点击UI上的状态切换按钮
- **用户操作：** 点击"切换到久坐"按钮
- **系统响应：**
  1. 调用 `PersonStateController.ChangeState(PersonState.Sitting, roomId)`
  2. 更新UI显示当前状态
  3. 开始计时，监测是否超时

**场景6：告警处理**
- **触发条件：** 用户点击告警列表中的"已处理"按钮
- **用户操作：** 点击告警项的"已处理"按钮
- **系统响应：**
  1. 调用 `AlarmManager.MarkHandled(record)`
  2. 更新UI，标记告警为已处理
  3. 停止音效和灯光闪烁（如果该告警是最后一个未处理告警）

---

### 3. 设计说明

#### 3.1 架构设计

**模块结构：**
```
SmartMonitoring/
├── Controllers/          # 控制逻辑
│   ├── MonitoringController.cs      # 核心监测控制器
│   └── PersonStateSimulator.cs      # 数字人状态模拟器（用于演示）
├── UI/                   # UI相关
│   ├── MonitoringPanel.cs            # 监护面板
│   └── AlarmPanel.cs                 # 告警列表面板
└── Utils/                # 工具类
    └── AlarmResponseHelper.cs        # 告警响应辅助类（音效、灯光等）
```

**关键类设计：**

| 类名 | 职责 | 依赖 |
|------|------|------|
| `MonitoringController` | 核心监测逻辑，异常检测，告警触发 | `PersonStateController`, `AlarmManager` |
| `MonitoringPanel` | UI显示与控制，状态展示，告警列表 | `MonitoringController` |
| `PersonStateSimulator` | 数字人状态模拟，用于演示和测试 | `PersonStateController` |
| `AlarmResponseHelper` | 告警响应，音效、灯光控制 | `AlarmManager`, `DeviceManager` |

#### 3.2 数据接口依赖

**使用的数据底座接口：**

- `PersonStateController.Instance.Status` - 获取数字人当前状态
- `PersonStateController.Instance.ChangeState(state, roomId)` - 改变数字人状态
- `PersonStateController.Instance.GetStateDuration()` - 获取状态持续时间
- `PersonStateController.Instance.OnStateChanged` - 状态变化事件
- `AlarmManager.Instance.AddAlarm(type, roomId)` - 添加告警记录
- `AlarmManager.Instance.GetRecentAlarms(count)` - 获取最近告警
- `AlarmManager.Instance.GetUnhandledAlarms()` - 获取未处理告警
- `AlarmManager.Instance.MarkHandled(record)` - 标记告警为已处理
- `AlarmManager.Instance.OnAlarmAdded` - 告警添加事件
- `DeviceManager.Instance.GetDevicesInRoom(roomId)` - 获取房间设备（用于控制灯光）

**参考文档：**
- `docs/data-api-examples.md` - 第3节"数字人状态"、第5节"告警管理"
- `docs/data-quick-reference.md` - PersonStateController、AlarmManager

**错误处理：**
- PersonStateController 未初始化：检查 `Instance != null`，在 `Awake()` 或 `Start()` 中初始化
- 告警重复触发：使用状态标记或时间间隔限制，避免同一异常重复告警
- 设备控制失败：通过 `DeviceManager.Instance.TryGetDevice()` 检查，记录警告日志

#### 3.3 UI设计

**UI组件：**
- [ ] 主面板（Panel）- 半透明背景，分组显示
- [ ] 数字人状态区域
  - 当前状态显示（大号字体，颜色编码）
  - 所在房间显示
  - 状态持续时间显示
- [ ] 状态切换控制区域
  - 状态切换按钮组（Idle, Walking, Sitting, Bathing, Sleeping, Fallen, OutOfBed）
  - 房间选择下拉菜单
- [ ] 告警列表区域
  - 告警列表（滚动视图）
  - 每个告警项显示：类型、房间、时间、处理状态
  - "已处理"按钮
- [ ] 告警设置区域
  - 久坐超时阈值设置（分钟）
  - 久浴超时阈值设置（分钟）
  - 告警音效开关
  - 灯光闪烁开关

**布局说明：**
```
┌─────────────────────────────────┐
│  智能监护面板                    │
├─────────────────────────────────┤
│  数字人状态                      │
│  State: [Sitting]               │
│  Room: LivingRoom01             │
│  Duration: 15:30                │
├─────────────────────────────────┤
│  状态切换                        │
│  [Idle] [Walking] [Sitting] ... │
│  Room: [LivingRoom01 ▼]         │
├─────────────────────────────────┤
│  告警列表                        │
│  • LongSitting (LivingRoom01)   │
│    15:30 [已处理]               │
│  • Fall (BedRoom01)             │
│    14:20 [处理]                 │
├─────────────────────────────────┤
│  告警设置                        │
│  久坐超时: [30] 分钟            │
│  久浴超时: [20] 分钟            │
└─────────────────────────────────┘
```

#### 3.4 交互设计

**交互流程：**

1. **自动监测（默认）**
   - 系统每1秒检查一次数字人状态
   - 如果检测到异常（久坐超时、久浴超时、跌倒、坠床），自动触发告警
   - 用户可以看到告警提示和UI反馈

2. **状态切换演示**
   - 用户点击状态切换按钮
   - 系统调用 `PersonStateController.ChangeState()` 改变状态
   - UI实时更新显示新状态
   - 开始计时，监测是否超时

3. **告警响应**
   - 告警触发时：弹出UI提示、播放音效、闪烁灯光
   - 告警列表自动更新，显示最新告警
   - 用户可以点击"已处理"按钮标记告警

4. **多房间支持**
   - 系统监测数字人在不同房间的状态
   - 告警记录包含房间信息
   - UI可以筛选显示特定房间的告警

---

### 4. 开发任务

#### 4.1 核心功能开发

**任务 4.1.1：** 实现 MonitoringController 核心逻辑
- **描述：** 创建监测控制器，实现状态监测、异常检测、告警触发逻辑
- **输入：** 数字人状态（从 PersonStateController）
- **输出：** 告警记录（通过 AlarmManager）
- **验收标准：** 
  - 能正确读取数字人状态
  - 异常检测逻辑正确（久坐、久浴、跌倒、坠床）
  - 告警触发正确
- **预计工时：** 4小时
- **关键代码：**
```csharp
public class MonitoringController : MonoBehaviour
{
    [Header("异常检测阈值")]
    public float longSittingThreshold = 30f;  // 久坐超时阈值（分钟）
    public float longBathingThreshold = 20f; // 久浴超时阈值（分钟）
    
    private void Update()
    {
        if (PersonStateController.Instance == null) return;
        
        var status = PersonStateController.Instance.Status;
        
        // 检测久坐超时
        if (status.state == PersonState.Sitting)
        {
            float durationMinutes = status.stateDuration / 60f;
            if (durationMinutes > longSittingThreshold)
            {
                TriggerAlarm(AlarmType.LongSitting, status.currentRoomId);
            }
        }
        
        // 检测久浴超时
        if (status.state == PersonState.Bathing)
        {
            float durationMinutes = status.stateDuration / 60f;
            if (durationMinutes > longBathingThreshold)
            {
                TriggerAlarm(AlarmType.LongBathing, status.currentRoomId);
            }
        }
        
        // 检测跌倒
        if (status.state == PersonState.Fallen)
        {
            TriggerAlarm(AlarmType.Fall, status.currentRoomId);
        }
        
        // 检测坠床
        if (status.state == PersonState.OutOfBed)
        {
            TriggerAlarm(AlarmType.Fall, status.currentRoomId); // 或扩展为 OutOfBed
        }
    }
    
    private void TriggerAlarm(AlarmType type, string roomId)
    {
        // 避免重复告警（同一类型、同一房间、短时间内）
        if (IsRecentAlarm(type, roomId)) return;
        
        AlarmManager.Instance?.AddAlarm(type, roomId);
        // 触发告警响应（UI、音效、灯光）
        AlarmResponseHelper.Instance?.RespondToAlarm(type, roomId);
    }
}
```

**任务 4.1.2：** 实现告警响应系统
- **描述：** 创建告警响应辅助类，实现UI提示、音效、灯光闪烁等功能
- **输入：** 告警类型、房间ID
- **输出：** UI提示、音效播放、灯光控制
- **验收标准：** 
  - UI提示正确显示
  - 音效正确播放
  - 灯光闪烁效果正确
- **预计工时：** 3小时
- **关键代码：**
```csharp
public class AlarmResponseHelper : MonoBehaviour
{
    public static AlarmResponseHelper Instance { get; private set; }
    
    [Header("UI提示")]
    public GameObject alarmPopupPrefab;
    public Transform uiCanvas;
    
    [Header("音效")]
    public AudioClip normalAlarmSound;    // 普通告警音效
    public AudioClip emergencyAlarmSound; // 紧急告警音效
    
    [Header("灯光控制")]
    public bool enableLightFlash = true;
    public float flashInterval = 0.5f;
    
    public void RespondToAlarm(AlarmType type, string roomId)
    {
        // 显示UI提示
        ShowAlarmPopup(type, roomId);
        
        // 播放音效
        PlayAlarmSound(type);
        
        // 闪烁灯光
        if (enableLightFlash)
        {
            FlashRoomLights(roomId);
        }
    }
    
    private void ShowAlarmPopup(AlarmType type, string roomId)
    {
        // 实例化UI提示预制体
        // 显示告警信息
    }
    
    private void PlayAlarmSound(AlarmType type)
    {
        AudioClip clip = (type == AlarmType.Fall) ? emergencyAlarmSound : normalAlarmSound;
        // 播放音效
    }
    
    private void FlashRoomLights(string roomId)
    {
        // 获取房间内的灯光设备
        // 控制灯光闪烁
    }
}
```

**任务 4.1.3：** 实现 PersonStateSimulator（可选，用于演示）
- **描述：** 创建状态模拟器，用于演示和测试，可以通过按钮切换数字人状态
- **输入：** 用户按钮点击
- **输出：** 数字人状态变化
- **验收标准：** 
  - 按钮点击能正确切换状态
  - 状态变化能正确反映到 PersonStateController
- **预计工时：** 2小时

#### 4.2 UI开发

**任务 4.2.1：** 实现 MonitoringPanel UI面板
- **描述：** 创建监护面板UI，显示数字人状态、告警列表、控制按钮
- **输入：** 数字人状态、告警记录
- **输出：** UI显示
- **验收标准：** 
  - UI布局合理
  - 数据实时更新
  - 按钮响应正确
- **预计工时：** 4小时
- **UI组件：**
  - TextMeshProUGUI：状态显示、房间显示、持续时间显示
  - Button：状态切换按钮组
  - ScrollView：告警列表
  - Toggle：告警设置开关
  - InputField：阈值设置输入

**任务 4.2.2：** 实现告警列表UI
- **描述：** 创建告警列表组件，显示告警记录，支持标记已处理
- **输入：** 告警记录列表
- **输出：** UI列表显示
- **验收标准：** 
  - 列表正确显示告警信息
  - "已处理"按钮功能正确
  - 未处理告警高亮显示
- **预计工时：** 2小时

#### 4.3 集成与测试

**任务 4.3.1：** 与数据底座集成
- **描述：** 确保 MonitoringController 正确使用 PersonStateController 和 AlarmManager
- **验收标准：** 
  - 能正确读取数字人状态
  - 能正确添加告警记录
  - 能正确响应告警事件
- **预计工时：** 2小时

**任务 4.3.2：** 功能测试
- **测试用例：**
  1. 久坐超时告警触发正确
  2. 久浴超时告警触发正确
  3. 跌倒告警触发正确
  4. 坠床告警触发正确
  5. UI状态显示实时更新
  6. 告警列表正确显示
  7. "已处理"按钮功能正确
  8. 告警响应（UI、音效、灯光）正常
  9. 状态切换按钮功能正确
  10. 多房间告警支持正常
- **验收标准：** 所有测试用例通过
- **预计工时：** 3小时

**任务 4.3.3：** 性能优化
- **优化点：** 状态检查频率、UI更新频率、告警去重逻辑
- **目标：** 保持60 FPS，CPU占用 < 5%
- **预计工时：** 1小时

---

### 5. 开发计划

#### 5.1 时间安排（建议）

| 阶段 | 任务 | 开始时间 | 结束时间 | 负责人 |
|------|------|----------|----------|--------|
| 准备 | 需求确认、设计评审 | 第1天 | 第1天 | [姓名] |
| 开发 | MonitoringController核心逻辑 | 第2天 | 第3天 | [姓名] |
| 开发 | 告警响应系统 | 第4天 | 第4天 | [姓名] |
| 开发 | UI面板开发 | 第5天 | 第6天 | [姓名] |
| 开发 | PersonStateSimulator（可选） | 第7天 | 第7天 | [姓名] |
| 测试 | 功能测试与优化 | 第8天 | 第9天 | [姓名] |
| 集成 | 与数据底座联调 | 第10天 | 第10天 | [姓名] |

#### 5.2 里程碑

- **M1：** 核心逻辑完成 - 第3天
  - 完成内容：MonitoringController实现，异常检测和告警触发逻辑
  
- **M2：** UI完成 - 第6天
  - 完成内容：MonitoringPanel完成，告警列表完成
  
- **M3：** 模块完成 - 第10天
  - 完成内容：测试通过，与数据底座集成完成

#### 5.3 风险与依赖

**技术风险：**
- [ ] 风险1：告警重复触发 - 应对措施：实现告警去重逻辑，使用时间间隔限制
- [ ] 风险2：音效和灯光控制失败 - 应对措施：添加错误处理，记录警告日志
- [ ] 风险3：UI性能问题 - 应对措施：优化UI更新频率，使用对象池

**依赖项：**
- [x] 依赖1：数据底座完成 - 提供方：数据底座团队 - 状态：已完成
- [x] 依赖2：PersonStateController 完成 - 提供方：数据底座团队 - 状态：已完成
- [x] 依赖3：AlarmManager 完成 - 提供方：数据底座团队 - 状态：已完成

---

### 6. 代码示例

#### 6.1 MonitoringController 完整示例

```csharp
using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.SmartMonitoring
{
    /// <summary>
    /// 智能监护控制器，监测数字人状态，检测异常并触发告警
    /// </summary>
    public class MonitoringController : MonoBehaviour
    {
        public static MonitoringController Instance { get; private set; }
        
        [Header("异常检测阈值（分钟）")]
        [Tooltip("久坐超时阈值")]
        public float longSittingThreshold = 30f;
        
        [Tooltip("久浴超时阈值")]
        public float longBathingThreshold = 20f;
        
        [Header("告警去重设置")]
        [Tooltip("同一类型告警的最小间隔（秒）")]
        public float alarmCooldown = 60f;
        
        private System.Collections.Generic.Dictionary<AlarmType, float> _lastAlarmTime = 
            new System.Collections.Generic.Dictionary<AlarmType, float>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            // 订阅状态变化事件
            if (PersonStateController.Instance != null)
            {
                PersonStateController.Instance.OnStateChanged += OnPersonStateChanged;
            }
        }
        
        private void OnDestroy()
        {
            if (PersonStateController.Instance != null)
            {
                PersonStateController.Instance.OnStateChanged -= OnPersonStateChanged;
            }
        }
        
        private void Update()
        {
            if (PersonStateController.Instance == null) return;
            
            var status = PersonStateController.Instance.Status;
            if (status == null) return;
            
            // 检测久坐超时
            if (status.state == PersonState.Sitting)
            {
                float durationMinutes = status.stateDuration / 60f;
                if (durationMinutes > longSittingThreshold)
                {
                    CheckAndTriggerAlarm(AlarmType.LongSitting, status.currentRoomId);
                }
            }
            
            // 检测久浴超时
            if (status.state == PersonState.Bathing)
            {
                float durationMinutes = status.stateDuration / 60f;
                if (durationMinutes > longBathingThreshold)
                {
                    CheckAndTriggerAlarm(AlarmType.LongBathing, status.currentRoomId);
                }
            }
        }
        
        private void OnPersonStateChanged(PersonState newState, string roomId)
        {
            // 检测跌倒
            if (newState == PersonState.Fallen)
            {
                CheckAndTriggerAlarm(AlarmType.Fall, roomId);
            }
            
            // 检测坠床
            if (newState == PersonState.OutOfBed)
            {
                CheckAndTriggerAlarm(AlarmType.Fall, roomId);
            }
        }
        
        private void CheckAndTriggerAlarm(AlarmType type, string roomId)
        {
            // 检查告警冷却时间
            if (_lastAlarmTime.ContainsKey(type))
            {
                float timeSinceLastAlarm = Time.time - _lastAlarmTime[type];
                if (timeSinceLastAlarm < alarmCooldown)
                {
                    return; // 还在冷却期内，不触发告警
                }
            }
            
            // 触发告警
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.AddAlarm(type, roomId);
                _lastAlarmTime[type] = Time.time;
                
                Debug.Log($"[Monitoring] Alarm triggered: {type} in {roomId}");
            }
        }
    }
}
```

#### 6.2 MonitoringPanel UI 示例

```csharp
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NiceHouse.Data;
using System.Linq;

namespace NiceHouse.SmartMonitoring
{
    /// <summary>
    /// 智能监护UI面板
    /// </summary>
    public class MonitoringPanel : MonoBehaviour
    {
        [Header("状态显示")]
        public TextMeshProUGUI stateText;
        public TextMeshProUGUI roomText;
        public TextMeshProUGUI durationText;
        
        [Header("状态切换按钮")]
        public Button idleButton;
        public Button walkingButton;
        public Button sittingButton;
        public Button bathingButton;
        public Button sleepingButton;
        public Button fallenButton;
        public Button outOfBedButton;
        
        [Header("告警列表")]
        public Transform alarmListContent;
        public GameObject alarmItemPrefab;
        
        [Header("更新设置")]
        public float updateInterval = 0.5f;
        
        private float _timer;
        
        private void Start()
        {
            // 绑定状态切换按钮
            if (idleButton != null) idleButton.onClick.AddListener(() => ChangeState(PersonState.Idle));
            if (walkingButton != null) walkingButton.onClick.AddListener(() => ChangeState(PersonState.Walking));
            if (sittingButton != null) sittingButton.onClick.AddListener(() => ChangeState(PersonState.Sitting));
            if (bathingButton != null) bathingButton.onClick.AddListener(() => ChangeState(PersonState.Bathing));
            if (sleepingButton != null) sleepingButton.onClick.AddListener(() => ChangeState(PersonState.Sleeping));
            if (fallenButton != null) fallenButton.onClick.AddListener(() => ChangeState(PersonState.Fallen));
            if (outOfBedButton != null) outOfBedButton.onClick.AddListener(() => ChangeState(PersonState.OutOfBed));
            
            // 订阅告警事件
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.OnAlarmAdded += OnAlarmAdded;
            }
            
            UpdateUI();
        }
        
        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;
            
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            UpdatePersonState();
            UpdateAlarmList();
        }
        
        private void UpdatePersonState()
        {
            if (PersonStateController.Instance == null) return;
            
            var status = PersonStateController.Instance.Status;
            if (status == null) return;
            
            if (stateText != null)
            {
                stateText.text = $"State: <b>{status.state}</b>";
            }
            
            if (roomText != null)
            {
                roomText.text = $"Room: <b>{status.currentRoomId}</b>";
            }
            
            if (durationText != null)
            {
                int minutes = Mathf.FloorToInt(status.stateDuration / 60f);
                int seconds = Mathf.FloorToInt(status.stateDuration % 60f);
                durationText.text = $"Duration: <b>{minutes:D2}:{seconds:D2}</b>";
            }
        }
        
        private void UpdateAlarmList()
        {
            if (AlarmManager.Instance == null || alarmListContent == null) return;
            
            // 清除旧列表项
            foreach (Transform child in alarmListContent)
            {
                Destroy(child.gameObject);
            }
            
            // 获取最近10条告警
            var alarms = AlarmManager.Instance.GetRecentAlarms(10);
            
            foreach (var alarm in alarms)
            {
                // 实例化告警项
                GameObject item = Instantiate(alarmItemPrefab, alarmListContent);
                // 设置告警信息
                // ...
            }
        }
        
        private void ChangeState(PersonState newState)
        {
            if (PersonStateController.Instance == null) return;
            
            // 获取当前房间（可以从UI选择或使用默认）
            string roomId = "LivingRoom01"; // 可以从下拉菜单获取
            
            PersonStateController.Instance.ChangeState(newState, roomId);
        }
        
        private void OnAlarmAdded(AlarmRecord record)
        {
            // 告警添加时更新列表
            UpdateAlarmList();
        }
    }
}
```

---

### 7. 测试用例

#### 7.1 功能测试

1. **久坐超时告警测试**
   - 设置数字人状态为 Sitting
   - 等待超过阈值时间（30分钟，测试时可缩短）
   - 验证告警是否触发
   - 验证UI提示是否显示
   - 验证告警列表是否更新

2. **久浴超时告警测试**
   - 设置数字人状态为 Bathing
   - 等待超过阈值时间（20分钟）
   - 验证告警是否触发

3. **跌倒告警测试**
   - 切换数字人状态为 Fallen
   - 验证立即触发告警
   - 验证紧急告警音效是否播放
   - 验证灯光是否闪烁

4. **告警去重测试**
   - 触发同一类型告警
   - 在冷却期内再次触发
   - 验证是否不会重复告警

5. **UI更新测试**
   - 切换数字人状态
   - 验证UI状态显示是否实时更新
   - 验证持续时间是否正确显示

#### 7.2 集成测试

1. **与数据底座集成测试**
   - 验证 PersonStateController 数据读取正确
   - 验证 AlarmManager 告警记录正确
   - 验证告警事件订阅正确

2. **多房间测试**
   - 在不同房间触发告警
   - 验证告警记录的房间信息正确
   - 验证UI可以正确显示不同房间的告警

---

### 8. 后续扩展

- **告警级别**：区分普通告警和紧急告警，不同级别使用不同的响应方式
- **告警推送**：集成外部通知系统（如短信、邮件）
- **历史记录**：保存告警历史记录，支持查询和统计
- **智能分析**：分析告警模式，提供健康建议
- **多数字人支持**：支持监测多个数字人的状态

---

**文档版本：** v1.0  
**最后更新：** 2024-01-XX  
**维护者：** [待分配]

