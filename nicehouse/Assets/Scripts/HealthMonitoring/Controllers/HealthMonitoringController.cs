using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.HealthMonitoring
{
    /// <summary>
    /// 健康监测控制器
    /// 监测数字人生命体征，检测异常（心率、呼吸率、体动）并触发告警
    /// </summary>
    public class HealthMonitoringController : MonoBehaviour
    {
        public static HealthMonitoringController Instance { get; private set; }

        [Header("健康阈值")]
        [Tooltip("心率最小值（bpm）")]
        public int heartRateMin = 60;

        [Tooltip("心率最大值（bpm）")]
        public int heartRateMax = 100;

        [Tooltip("呼吸率最小值（次/分钟）")]
        public int respirationRateMin = 12;

        [Tooltip("呼吸率最大值（次/分钟）")]
        public int respirationRateMax = 20;

        [Tooltip("体动强度最小值")]
        public float bodyMovementMin = 0.1f;

        [Header("异常持续时间阈值（秒）")]
        [Tooltip("心率/呼吸率异常持续时间阈值")]
        public float abnormalDurationThreshold = 30f;

        [Tooltip("无体动持续时间阈值（秒），默认30分钟")]
        public float noMovementDurationThreshold = 1800f; // 30分钟

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
        private float _heartRateAbnormalTime = 0f;
        private float _respirationAbnormalTime = 0f;
        private float _noMovementTime = 0f;
        private float _lastAlarmTime = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Debug.Log("[HealthMonitoringController] Initialized");
        }

        private void Update()
        {
            if (!enableMonitoring) return;

            _timer += Time.deltaTime;
            if (_timer < checkInterval) return;
            _timer = 0f;

            CheckHealthStatus();
        }

        /// <summary>
        /// 检查健康状态
        /// </summary>
        private void CheckHealthStatus()
        {
            if (HealthDataStore.Instance == null)
            {
                Debug.LogWarning("[HealthMonitoringController] HealthDataStore.Instance is null!");
                return;
            }

            var health = HealthDataStore.Instance.Current;
            if (health == null)
            {
                Debug.LogWarning("[HealthMonitoringController] HealthDataStore.Current is null!");
                return;
            }

            CheckHeartRate(health.heartRate);
            CheckRespirationRate(health.respirationRate);
            CheckBodyMovement(health.bodyMovement);
        }

        /// <summary>
        /// 检查心率
        /// </summary>
        private void CheckHeartRate(int heartRate)
        {
            bool isAbnormal = heartRate < heartRateMin || heartRate > heartRateMax;

            if (isAbnormal)
            {
                _heartRateAbnormalTime += checkInterval;
                if (_heartRateAbnormalTime >= abnormalDurationThreshold)
                {
                    string message = $"Heart rate abnormal: {heartRate} bpm (normal: {heartRateMin}-{heartRateMax})";
                    Debug.Log($"[HealthMonitoringController] Heart rate abnormal detected: {heartRate} bpm, abnormal time: {_heartRateAbnormalTime:F1}s");
                    TriggerHealthAlarm(message);
                    _heartRateAbnormalTime = 0f; // 重置计时器
                }
            }
            else
            {
                if (_heartRateAbnormalTime > 0f)
                {
                    Debug.Log($"[HealthMonitoringController] Heart rate returned to normal: {heartRate} bpm");
                }
                _heartRateAbnormalTime = 0f;
            }
        }

        /// <summary>
        /// 检查呼吸率
        /// </summary>
        private void CheckRespirationRate(int respirationRate)
        {
            bool isAbnormal = respirationRate < respirationRateMin || respirationRate > respirationRateMax;

            if (isAbnormal)
            {
                _respirationAbnormalTime += checkInterval;
                if (_respirationAbnormalTime >= abnormalDurationThreshold)
                {
                    string message = $"Respiration rate abnormal: {respirationRate} /min (normal: {respirationRateMin}-{respirationRateMax})";
                    TriggerHealthAlarm(message);
                    _respirationAbnormalTime = 0f; // 重置计时器
                }
            }
            else
            {
                _respirationAbnormalTime = 0f;
            }
        }

        /// <summary>
        /// 检查体动
        /// </summary>
        private void CheckBodyMovement(float bodyMovement)
        {
            if (bodyMovement < bodyMovementMin)
            {
                _noMovementTime += checkInterval;
                if (_noMovementTime >= noMovementDurationThreshold)
                {
                    string message = $"No body movement detected for extended period ({_noMovementTime / 60f:F1} minutes)";
                    TriggerHealthAlarm(message);
                    _noMovementTime = 0f; // 重置计时器
                }
            }
            else
            {
                _noMovementTime = 0f;
            }
        }

        /// <summary>
        /// 触发健康异常告警
        /// </summary>
        private void TriggerHealthAlarm(string message)
        {
            // 测试模式下跳过冷却时间检查
            if (!testMode)
            {
                // 检查告警冷却时间
                float timeSinceLastAlarm = Time.time - _lastAlarmTime;
                if (timeSinceLastAlarm < alarmCooldown)
                {
                    Debug.Log($"[HealthMonitoringController] Alarm in cooldown, time remaining: {alarmCooldown - timeSinceLastAlarm:F1}s");
                    return; // 还在冷却期内，不触发告警
                }
            }

            // 获取房间ID，如果为空则使用默认值
            string roomId = "Unknown";
            if (PersonStateController.Instance != null && PersonStateController.Instance.Status != null)
            {
                roomId = PersonStateController.Instance.Status.currentRoomId;
                if (string.IsNullOrEmpty(roomId))
                {
                    roomId = "Unknown";
                }
            }

            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.AddAlarm(AlarmType.HealthAbnormal, roomId);
                _lastAlarmTime = Time.time;

                Debug.Log($"[HealthMonitoringController] Health alarm triggered: {message} in {roomId}");
            }
            else
            {
                Debug.LogError("[HealthMonitoringController] AlarmManager.Instance is null!");
            }
        }

        /// <summary>
        /// 重置异常计时器（用于测试）
        /// </summary>
        public void ResetAbnormalTimers()
        {
            _heartRateAbnormalTime = 0f;
            _respirationAbnormalTime = 0f;
            _noMovementTime = 0f;
        }

        /// <summary>
        /// 手动触发告警（用于测试）
        /// </summary>
        public void TriggerAlarmManually(string message = "Manual health alarm")
        {
            TriggerHealthAlarm(message);
        }
    }
}

