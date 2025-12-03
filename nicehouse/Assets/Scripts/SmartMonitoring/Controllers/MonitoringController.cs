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
            // 订阅状态变化事件
            if (PersonStateController.Instance != null)
            {
                PersonStateController.Instance.OnStateChanged += OnPersonStateChanged;
            }
            else
            {
                Debug.LogWarning("[MonitoringController] PersonStateController.Instance is null!");
            }

            // 订阅告警管理器事件
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.OnAlarmAdded += OnAlarmAdded;
            }
        }

        private void OnDestroy()
        {
            if (PersonStateController.Instance != null)
            {
                PersonStateController.Instance.OnStateChanged -= OnPersonStateChanged;
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

        /// <summary>
        /// 数字人状态变化事件处理
        /// </summary>
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

        /// <summary>
        /// 检查并触发告警（带去重逻辑）
        /// </summary>
        private void CheckAndTriggerAlarm(AlarmType type, string roomId)
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

            // 触发告警
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.AddAlarm(type, roomId);
                _lastAlarmTime[type] = Time.time;
                _lastAlarmRoom[type] = roomId;

                Debug.Log($"[MonitoringController] Alarm triggered: {type} in {roomId}");
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
            CheckAndTriggerAlarm(type, roomId);
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

