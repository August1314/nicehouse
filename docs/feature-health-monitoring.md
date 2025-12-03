## 健康监测模块开发文档

> 本文档用于规划和管理健康监测模块的开发工作。

---

### 1. 功能模块基本信息

**模块名称：** 健康监测模块

**模块编号：** FM-003

**负责人：** [待分配]

**关联需求：** 基础要求 - 功能项 - 健康监测

**核心价值：** 实现"以人为中心"的健康监护，通过监测数字人的生命体征（心率、呼吸率、体动、睡眠），及时发现健康异常并触发告警。

---

### 2. 功能模块描述

#### 2.1 功能概述

健康监测模块是"好房子"系统的健康核心功能之一，主要实现：

- **生命体征监测**：实时监测数字人的心率、呼吸率、体动强度、睡眠阶段等生命体征数据
- **异常检测**：当检测到健康指标异常（心率过高/过低、呼吸异常、长时间无体动等）时，自动触发告警
- **健康可视化**：在UI上实时显示生命体征数据，以图表形式展示历史趋势
- **告警响应**：健康异常时触发告警，记录到 AlarmManager，并可通过告警响应系统进行UI提示、音效播放等

#### 2.2 核心功能点

- [ ] **生命体征监测**：从 `HealthDataStore` 获取实时健康数据
- [ ] **异常检测逻辑**：判断心率、呼吸率、体动等是否异常
- [ ] **告警触发**：异常时调用 `AlarmManager.AddAlarm(AlarmType.HealthAbnormal, roomId)` 记录告警
- [ ] **健康可视化**：UI显示当前生命体征，图表展示历史趋势
- [ ] **UI面板**：显示健康数据、异常告警、历史趋势图表

#### 2.3 用户场景

**场景1：心率异常告警**
- **触发条件：** 监测到心率 > 100 bpm 或 < 60 bpm（持续超过阈值时间）
- **用户操作：** 无需操作（自动监测）
- **系统响应：** 
  1. 触发告警（AlarmType.HealthAbnormal）
  2. 弹出UI提示"检测到心率异常"
  3. 播放告警音效
  4. 记录告警到 AlarmManager

**场景2：呼吸异常告警**
- **触发条件：** 监测到呼吸率 > 20 次/分钟 或 < 12 次/分钟（持续超过阈值时间）
- **用户操作：** 无需操作（自动监测）
- **系统响应：** 
  1. 触发告警（AlarmType.HealthAbnormal）
  2. 弹出UI提示"检测到呼吸异常"
  3. 记录告警

**场景3：长时间无体动告警**
- **触发条件：** 监测到体动强度 < 0.1 且持续超过30分钟（可能表示昏迷或异常）
- **用户操作：** 无需操作（自动监测）
- **系统响应：** 
  1. 触发告警（AlarmType.HealthAbnormal）
  2. 弹出UI提示"检测到长时间无体动，请检查"
  3. 记录告警

**场景4：查看健康数据**
- **用户操作：** 打开健康监测面板
- **系统响应：** 
  1. 显示当前心率、呼吸率、体动强度、睡眠阶段
  2. 显示健康数据历史趋势图表
  3. 显示最近的健康异常告警

---

### 3. 设计说明

#### 3.1 架构设计

**模块结构：**
```
HealthMonitoring/
├── Controllers/          # 控制逻辑
│   └── HealthMonitoringController.cs    # 核心健康监测控制器
├── UI/                   # UI相关
│   └── HealthMonitoringPanel.cs         # 健康监测面板
└── Config/               # 配置（阈值等）
    └── HealthThresholds.cs (ScriptableObject)  # 健康阈值配置
```

**关键类设计：**

| 类名 | 职责 | 依赖 |
|------|------|------|
| `HealthMonitoringController` | 核心监测逻辑，异常检测，告警触发 | `HealthDataStore`, `AlarmManager` |
| `HealthMonitoringPanel` | UI显示与控制，健康数据展示，图表 | `HealthMonitoringController`, `HealthDataStore` |
| `HealthThresholds` | 存储健康阈值（心率、呼吸率等） | `ScriptableObject` |

#### 3.2 数据接口依赖

**使用的数据底座接口：**

- `HealthDataStore.Instance.Current` - 获取当前生命体征数据
  - `heartRate` - 心率（bpm）
  - `respirationRate` - 呼吸率（次/分钟）
  - `bodyMovement` - 体动强度（0-1）
  - `sleepStage` - 睡眠阶段（0-清醒，1-浅睡，2-深睡）
- `AlarmManager.Instance.AddAlarm(type, roomId)` - 添加告警记录
- `AlarmManager.Instance.GetRecentAlarms(count)` - 获取最近告警
- `PersonStateController.Instance.Status.currentRoomId` - 获取当前房间ID

#### 3.3 健康阈值设计

**心率阈值：**
- 正常范围：60-100 bpm
- 异常：< 60 bpm（心动过缓）或 > 100 bpm（心动过速）
- 持续异常时间阈值：30秒（可配置）

**呼吸率阈值：**
- 正常范围：12-20 次/分钟
- 异常：< 12 次/分钟（呼吸过缓）或 > 20 次/分钟（呼吸过速）
- 持续异常时间阈值：30秒（可配置）

**体动阈值：**
- 正常体动：> 0.1
- 异常：< 0.1 且持续超过30分钟（可能表示昏迷或异常）
- 持续异常时间阈值：30分钟（可配置）

---

### 4. 开发任务

#### 4.1 核心功能开发

**任务 4.1.1：** 实现 HealthMonitoringController 核心逻辑
- **描述：** 创建健康监测控制器，实现异常检测和告警触发
- **输入：** HealthDataStore 的生命体征数据
- **输出：** 告警记录（通过 AlarmManager）
- **验收标准：** 
  - 心率异常检测正确
  - 呼吸率异常检测正确
  - 体动异常检测正确
  - 告警触发正确
- **预计工时：** 4小时
- **关键代码：**
```csharp
public class HealthMonitoringController : MonoBehaviour
{
    [Header("健康阈值")]
    public int heartRateMin = 60;
    public int heartRateMax = 100;
    public int respirationRateMin = 12;
    public int respirationRateMax = 20;
    public float bodyMovementMin = 0.1f;
    
    [Header("异常持续时间阈值（秒）")]
    public float abnormalDurationThreshold = 30f;
    public float noMovementDurationThreshold = 1800f; // 30分钟
    
    private float _heartRateAbnormalTime = 0f;
    private float _respirationAbnormalTime = 0f;
    private float _noMovementTime = 0f;
    
    private void Update()
    {
        if (HealthDataStore.Instance == null) return;
        
        var health = HealthDataStore.Instance.Current;
        if (health == null) return;
        
        CheckHeartRate(health.heartRate);
        CheckRespirationRate(health.respirationRate);
        CheckBodyMovement(health.bodyMovement);
    }
    
    private void CheckHeartRate(int heartRate)
    {
        bool isAbnormal = heartRate < heartRateMin || heartRate > heartRateMax;
        
        if (isAbnormal)
        {
            _heartRateAbnormalTime += Time.deltaTime;
            if (_heartRateAbnormalTime >= abnormalDurationThreshold)
            {
                TriggerHealthAlarm("Heart rate abnormal");
                _heartRateAbnormalTime = 0f; // 重置计时器
            }
        }
        else
        {
            _heartRateAbnormalTime = 0f;
        }
    }
    
    // ... 其他检测方法
}
```

**任务 4.1.2：** 实现健康阈值配置（ScriptableObject）
- **描述：** 创建 HealthThresholds ScriptableObject，用于配置健康阈值
- **输入：** 健康阈值配置
- **输出：** ScriptableObject 资源文件
- **验收标准：** 
  - 可以创建多个阈值配置
  - 可以在 Inspector 中编辑
- **预计工时：** 1小时

#### 4.2 UI开发

**任务 4.2.1：** 实现 HealthMonitoringPanel UI面板
- **描述：** 创建健康监测UI面板，显示生命体征数据和图表
- **输入：** HealthDataStore 数据
- **输出：** UI显示
- **验收标准：** 
  - 实时显示心率、呼吸率、体动、睡眠阶段
  - 数据更新正常
  - UI布局合理
- **预计工时：** 5小时
- **UI元素：**
  - 当前心率显示（大字体，颜色编码）
  - 当前呼吸率显示
  - 体动强度显示（进度条）
  - 睡眠阶段显示
  - 健康数据历史趋势图表（可选，使用简单折线图）
  - 最近健康异常告警列表

**任务 4.2.2：** 实现健康数据可视化（图表）
- **描述：** 使用 Unity UI 或简单图表库展示健康数据历史趋势
- **输入：** 历史健康数据（需要存储）
- **输出：** 折线图显示
- **验收标准：** 
  - 图表正确显示
  - 数据更新正常
- **预计工时：** 3小时（可选，如果时间紧张可以简化）

#### 4.3 集成与测试

**任务 4.3.1：** 与数据底座集成
- **描述：** 确保与 HealthDataStore、AlarmManager 正确集成
- **验收标准：** 
  - 数据获取正常
  - 告警触发正常
- **预计工时：** 1小时

**任务 4.3.2：** 功能测试
- **测试用例：**
  1. 心率异常告警触发正确
  2. 呼吸率异常告警触发正确
  3. 体动异常告警触发正确
  4. UI数据实时更新
  5. 健康数据可视化正确（如果有图表）
- **验收标准：** 所有测试用例通过
- **预计工时：** 2小时

---

### 5. 开发计划

#### 5.1 时间安排（建议）

| 阶段 | 任务 | 开始时间 | 结束时间 | 负责人 |
|------|------|----------|----------|--------|
| 准备 | 需求确认、设计评审 | 第1天 | 第1天 | [姓名] |
| 开发 | HealthMonitoringController核心逻辑 | 第2天 | 第3天 | [姓名] |
| 开发 | HealthThresholds配置 | 第3天 | 第3天 | [姓名] |
| 开发 | HealthMonitoringPanel UI面板 | 第4天 | 第5天 | [姓名] |
| 开发 | 健康数据可视化（可选） | 第6天 | 第6天 | [姓名] |
| 测试 | 功能测试与优化 | 第7天 | 第7天 | [姓名] |
| 集成 | 与数据底座联调 | 第8天 | 第8天 | [姓名] |

#### 5.2 里程碑

- **M1：** 核心逻辑完成 - 第3天
  - 完成内容：HealthMonitoringController实现，异常检测和告警触发逻辑
  
- **M2：** UI完成 - 第5天
  - 完成内容：HealthMonitoringPanel完成，健康数据实时显示
  
- **M3：** 模块完成 - 第8天
  - 完成内容：测试通过，与数据底座集成完成

#### 5.3 风险与依赖

**技术风险：**
- [ ] 风险1：健康数据更新频率过高导致性能问题 - 应对措施：控制更新频率，使用对象池
- [ ] 风险2：图表可视化实现复杂 - 应对措施：使用简单UI实现，或使用轻量级图表库
- [ ] 风险3：异常检测误报 - 应对措施：增加持续异常时间阈值，避免瞬时波动触发告警

**依赖项：**
- [x] 依赖1：数据底座完成 - 提供方：数据底座团队 - 状态：已完成
- [x] 依赖2：HealthDataStore 可用 - 状态：已完成
- [x] 依赖3：AlarmManager 可用 - 状态：已完成

---

### 6. 代码示例

#### 6.1 HealthMonitoringController 示例

```csharp
using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.HealthMonitoring
{
    public class HealthMonitoringController : MonoBehaviour
    {
        public static HealthMonitoringController Instance { get; private set; }

        [Header("健康阈值")]
        public int heartRateMin = 60;
        public int heartRateMax = 100;
        public int respirationRateMin = 12;
        public int respirationRateMax = 20;
        public float bodyMovementMin = 0.1f;

        [Header("异常持续时间阈值（秒）")]
        public float abnormalDurationThreshold = 30f;
        public float noMovementDurationThreshold = 1800f; // 30分钟

        [Header("监测设置")]
        public float checkInterval = 1f;
        public bool enableMonitoring = true;

        private float _timer;
        private float _heartRateAbnormalTime = 0f;
        private float _respirationAbnormalTime = 0f;
        private float _noMovementTime = 0f;
        private float _lastBodyMovement = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (!enableMonitoring) return;

            _timer += Time.deltaTime;
            if (_timer < checkInterval) return;
            _timer = 0f;

            CheckHealthStatus();
        }

        private void CheckHealthStatus()
        {
            if (HealthDataStore.Instance == null) return;

            var health = HealthDataStore.Instance.Current;
            if (health == null) return;

            CheckHeartRate(health.heartRate);
            CheckRespirationRate(health.respirationRate);
            CheckBodyMovement(health.bodyMovement);
        }

        private void CheckHeartRate(int heartRate)
        {
            bool isAbnormal = heartRate < heartRateMin || heartRate > heartRateMax;

            if (isAbnormal)
            {
                _heartRateAbnormalTime += Time.deltaTime;
                if (_heartRateAbnormalTime >= abnormalDurationThreshold)
                {
                    TriggerHealthAlarm("Heart rate abnormal: " + heartRate + " bpm");
                    _heartRateAbnormalTime = 0f;
                }
            }
            else
            {
                _heartRateAbnormalTime = 0f;
            }
        }

        private void CheckRespirationRate(int respirationRate)
        {
            bool isAbnormal = respirationRate < respirationRateMin || respirationRate > respirationRateMax;

            if (isAbnormal)
            {
                _respirationAbnormalTime += Time.deltaTime;
                if (_respirationAbnormalTime >= abnormalDurationThreshold)
                {
                    TriggerHealthAlarm("Respiration rate abnormal: " + respirationRate + " /min");
                    _respirationAbnormalTime = 0f;
                }
            }
            else
            {
                _respirationAbnormalTime = 0f;
            }
        }

        private void CheckBodyMovement(float bodyMovement)
        {
            if (bodyMovement < bodyMovementMin)
            {
                _noMovementTime += Time.deltaTime;
                if (_noMovementTime >= noMovementDurationThreshold)
                {
                    TriggerHealthAlarm("No body movement detected for extended period");
                    _noMovementTime = 0f;
                }
            }
            else
            {
                _noMovementTime = 0f;
            }
        }

        private void TriggerHealthAlarm(string message)
        {
            string roomId = PersonStateController.Instance?.Status?.currentRoomId ?? "Unknown";

            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.AddAlarm(AlarmType.HealthAbnormal, roomId);
                Debug.Log($"[HealthMonitoring] {message} in {roomId}");
            }
        }
    }
}
```

#### 6.2 HealthMonitoringPanel UI 示例

```csharp
using UnityEngine;
using TMPro;
using NiceHouse.Data;

namespace NiceHouse.HealthMonitoring
{
    public class HealthMonitoringPanel : MonoBehaviour
    {
        [Header("生命体征显示")]
        public TextMeshProUGUI heartRateText;
        public TextMeshProUGUI respirationRateText;
        public TextMeshProUGUI bodyMovementText;
        public TextMeshProUGUI sleepStageText;

        [Header("更新设置")]
        public float updateInterval = 0.5f;

        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;

            UpdateHealthData();
        }

        private void UpdateHealthData()
        {
            if (HealthDataStore.Instance == null) return;

            var health = HealthDataStore.Instance.Current;
            if (health == null) return;

            // 更新心率
            if (heartRateText != null)
            {
                string color = GetHeartRateColor(health.heartRate);
                heartRateText.text = $"<color=#CCCCCC>Heart Rate:</color> <color={color}><b>{health.heartRate}</b></color> <color=#CCCCCC>bpm</color>";
            }

            // 更新呼吸率
            if (respirationRateText != null)
            {
                string color = GetRespirationRateColor(health.respirationRate);
                respirationRateText.text = $"<color=#CCCCCC>Respiration:</color> <color={color}><b>{health.respirationRate}</b></color> <color=#CCCCCC>/min</color>";
            }

            // 更新体动
            if (bodyMovementText != null)
            {
                bodyMovementText.text = $"<color=#CCCCCC>Body Movement:</color> <b>{health.bodyMovement:F2}</b>";
            }

            // 更新睡眠阶段
            if (sleepStageText != null)
            {
                string stageName = GetSleepStageName(health.sleepStage);
                sleepStageText.text = $"<color=#CCCCCC>Sleep Stage:</color> <b>{stageName}</b>";
            }
        }

        private string GetHeartRateColor(int heartRate)
        {
            if (heartRate < 60 || heartRate > 100) return "#FF0000"; // 红色：异常
            return "#00FF00"; // 绿色：正常
        }

        private string GetRespirationRateColor(int respirationRate)
        {
            if (respirationRate < 12 || respirationRate > 20) return "#FF0000"; // 红色：异常
            return "#00FF00"; // 绿色：正常
        }

        private string GetSleepStageName(int stage)
        {
            return stage switch
            {
                0 => "Awake",
                1 => "Light Sleep",
                2 => "Deep Sleep",
                _ => "Unknown"
            };
        }
    }
}
```

---

### 7. 测试计划

#### 7.1 单元测试

- [ ] 心率异常检测逻辑测试
- [ ] 呼吸率异常检测逻辑测试
- [ ] 体动异常检测逻辑测试
- [ ] 告警触发逻辑测试

#### 7.2 集成测试

- [ ] 与 HealthDataStore 集成测试
- [ ] 与 AlarmManager 集成测试
- [ ] UI数据更新测试
- [ ] 告警响应测试

#### 7.3 用户测试

- [ ] 健康数据实时显示测试
- [ ] 异常告警触发测试
- [ ] UI交互测试

---

### 8. 参考资料

- 数据设计文档：`docs/data-design.md`
- 数据API参考：`docs/data-quick-reference.md`
- 智能监护模块文档：`docs/feature-smart-monitoring.md`（可参考架构设计）

---

**最后更新：** 2024-01-XX  
**维护者：** [待分配]

