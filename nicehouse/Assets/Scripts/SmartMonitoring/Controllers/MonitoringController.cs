using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.SmartMonitoring
{
    /// <summary>
    /// 智能监护控制器
    /// 监测数字人状态，检测异常（久坐、久浴、跌倒、坠床）并触发告警
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

        [Tooltip("测试模式：禁用告警冷却时间（用于测试）")]
        public bool testMode = false;

        [Header("监测设置")]
        [Tooltip("状态检查间隔（秒）")]
        public float checkInterval = 1f;

        [Tooltip("是否启用自动监测")]
        public bool enableMonitoring = true;

        private float _timer;
        private System.Collections.Generic.Dictionary<AlarmType, float> _lastAlarmTime = 
            new System.Collections.Generic.Dictionary<AlarmType, float>();
        private System.Collections.Generic.Dictionary<AlarmType, string> _lastAlarmRoom = 
            new System.Collections.Generic.Dictionary<AlarmType, string>();

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
            // 延迟检查，确保所有 Manager 都已初始化
            StartCoroutine(DelayedSubscribeEvents());
        }

        private System.Collections.IEnumerator DelayedSubscribeEvents()
        {
            // 等待一帧，确保所有 Awake 和 Start 都已执行
            yield return null;

            // 订阅多 Agent 事件
            if (PersonStateManager.Instance != null)
            {
                PersonStateManager.Instance.OnAnyStateChanged += OnPersonStateChanged;
                Debug.Log("[MonitoringController] PersonStateManager is ready");
            }
            else if (PersonStateController.Instance != null)
            {
                // 兼容旧单实例
                PersonStateController.Instance.OnStateChanged += OnLegacyPersonStateChanged;
                Debug.Log("[MonitoringController] Legacy PersonStateController is used (single agent)");
            }
            else
            {
                Debug.LogError("[MonitoringController] PersonStateManager/PersonStateController not found!");
            }

            // 订阅告警管理器事件
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.OnAlarmAdded += OnAlarmAdded;
                Debug.Log("[MonitoringController] AlarmManager.Instance is ready");
            }
            else
            {
                Debug.LogError("[MonitoringController] AlarmManager.Instance is null after initialization!");
            }
        }

        private void OnDestroy()
        {
            if (PersonStateManager.Instance != null)
            {
                PersonStateManager.Instance.OnAnyStateChanged -= OnPersonStateChanged;
            }
            if (PersonStateController.Instance != null)
            {
                PersonStateController.Instance.OnStateChanged -= OnLegacyPersonStateChanged;
            }

            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.OnAlarmAdded -= OnAlarmAdded;
            }
        }

        private void Update()
        {
            if (!enableMonitoring) return;

            _timer += Time.deltaTime;
            if (_timer < checkInterval) return;
            _timer = 0f;

            CheckPersonState();
        }

        /// <summary>
        /// 检查数字人状态，检测异常
        /// </summary>
        private void CheckPersonState()
        {
            var agents = PersonStateManager.Instance != null
                ? PersonStateManager.Instance.GetAllAgents()
                : null;

            if (agents != null && agents.Count > 0)
            {
                foreach (var agent in agents)
                {
                    CheckPersonState(agent);
                }
            }
            else if (PersonStateController.Instance != null)
            {
                CheckPersonState(PersonStateController.Instance);
            }
        }

        /// <summary>
        /// 检查单个 Agent 状态，检测异常。
        /// </summary>
        private void CheckPersonState(PersonStateController agent)
        {
            if (agent == null || agent.Status == null) return;

            var status = agent.Status;

            // 久坐
            if (status.state == PersonState.Sitting)
            {
                float durationMinutes = status.stateDuration / 60f;
                if (durationMinutes > longSittingThreshold)
                {
                    CheckAndTriggerAlarm(agent, AlarmType.LongSitting, status.currentRoomId);
                }
            }

            // 久浴
            if (status.state == PersonState.Bathing)
            {
                float durationMinutes = status.stateDuration / 60f;
                if (durationMinutes > longBathingThreshold)
                {
                    CheckAndTriggerAlarm(agent, AlarmType.LongBathing, status.currentRoomId);
                }
            }
        }

        /// <summary>
        /// 多 Agent 事件处理
        /// </summary>
        private void OnPersonStateChanged(PersonStateController agent, PersonState newState, string roomId)
        {
            if (agent == null) return;
            HandleStateChange(agent, newState, roomId);
        }

        /// <summary>
        /// 兼容旧单实例事件
        /// </summary>
        private void OnLegacyPersonStateChanged(PersonState newState, string roomId)
        {
            HandleStateChange(PersonStateController.Instance, newState, roomId);
        }

        private void HandleStateChange(PersonStateController agent, PersonState newState, string roomId)
        {
            // 跌倒
            if (newState == PersonState.Fallen)
            {
                CheckAndTriggerAlarm(agent, AlarmType.Fall, roomId);
            }

            // 坠床
            if (newState == PersonState.OutOfBed)
            {
                CheckAndTriggerAlarm(agent, AlarmType.Fall, roomId);
            }
        }

        /// <summary>
        /// 检查并触发告警（带去重逻辑）
        /// </summary>
        private void CheckAndTriggerAlarm(PersonStateController agent, AlarmType type, string roomId)
        {
            // 测试模式下跳过冷却时间检查
            if (!testMode)
            {
                // 检查告警冷却时间
                if (_lastAlarmTime.ContainsKey(type))
                {
                    float timeSinceLastAlarm = Time.time - _lastAlarmTime[type];
                    if (timeSinceLastAlarm < alarmCooldown)
                    {
                        // 检查是否是同一房间
                        if (_lastAlarmRoom.ContainsKey(type) && _lastAlarmRoom[type] == roomId)
                        {
                            return; // 还在冷却期内且是同一房间，不触发告警
                        }
                    }
                }
            }

            // 触发告警
            if (AlarmManager.Instance != null)
            {
                var personId = agent != null ? agent.PersonId : null;
                AlarmManager.Instance.AddAlarm(type, roomId, personId);
                _lastAlarmTime[type] = Time.time;
                _lastAlarmRoom[type] = roomId;

                Debug.Log($"[MonitoringController] Alarm triggered: {type} in {roomId} (person={personId})");
            }
            else
            {
                Debug.LogWarning("[MonitoringController] AlarmManager.Instance is null!");
            }
        }

        /// <summary>
        /// 告警添加事件处理
        /// </summary>
        private void OnAlarmAdded(AlarmRecord record)
        {
            // 可以在这里添加额外的响应逻辑
            // 例如：触发告警响应系统
            if (AlarmResponseHelper.Instance != null)
            {
                AlarmResponseHelper.Instance.RespondToAlarm(record.type, record.roomId);
            }
        }

        /// <summary>
        /// 手动触发告警（用于测试）
        /// </summary>
        public void TriggerAlarmManually(AlarmType type, string roomId)
        {
            var agent = PersonStateManager.Instance != null ? PersonStateManager.Instance.DefaultAgent : PersonStateController.Instance;
            CheckAndTriggerAlarm(agent, type, roomId);
        }

        /// <summary>
        /// 兼容旧接口（不传 Agent）
        /// </summary>
        public void TriggerAlarmManually(AlarmType type)
        {
            var agent = PersonStateManager.Instance != null ? PersonStateManager.Instance.DefaultAgent : PersonStateController.Instance;
            var roomId = agent != null && agent.Status != null && !string.IsNullOrEmpty(agent.Status.currentRoomId)
                ? agent.Status.currentRoomId
                : "Unknown";
            CheckAndTriggerAlarm(agent, type, roomId);
        }

        /// <summary>
        /// 重置告警冷却时间（用于测试）
        /// </summary>
        public void ResetAlarmCooldown()
        {
            _lastAlarmTime.Clear();
            _lastAlarmRoom.Clear();
        }
    }
}


