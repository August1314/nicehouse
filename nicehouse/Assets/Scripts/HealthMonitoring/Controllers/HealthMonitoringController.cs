using System.Collections.Generic;
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
        private readonly Dictionary<string, float> _lastAlarmTimeByPerson = new Dictionary<string, float>();

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
            // 多人模式优先：遍历 Registry
            if (HealthDataRegistry.Instance != null && PersonStateManager.Instance != null)
            {
                foreach (var agent in PersonStateManager.Instance.GetAllAgents())
                {
                    if (agent == null) continue;
                    var pid = string.IsNullOrEmpty(agent.PersonId) ? "Unknown" : agent.PersonId;
                    var health = HealthDataRegistry.Instance.GetOrCreate(pid);
                    if (health == null) continue;
                    CheckHeartRate(health.heartRate, pid, agent);
                    CheckRespirationRate(health.respirationRate, pid, agent);
                    CheckBodyMovement(health.bodyMovement, pid, agent);
                }
                return;
            }

            // 兼容单人模式
            if (HealthDataStore.Instance == null)
            {
                Debug.LogWarning("[HealthMonitoringController] HealthDataStore.Instance is null!");
                return;
            }

            var single = HealthDataStore.Instance.Current;
            if (single == null)
            {
                Debug.LogWarning("[HealthMonitoringController] HealthDataStore.Current is null!");
                return;
            }

            var defaultAgent = PersonStateManager.Instance != null
                ? PersonStateManager.Instance.DefaultAgent
                : PersonStateController.Instance;
            var singlePid = defaultAgent != null ? defaultAgent.PersonId : "Unknown";
            CheckHeartRate(single.heartRate, singlePid, defaultAgent);
            CheckRespirationRate(single.respirationRate, singlePid, defaultAgent);
            CheckBodyMovement(single.bodyMovement, singlePid, defaultAgent);
        }

        /// <summary>
        /// 检查心率
        /// </summary>
        private void CheckHeartRate(int heartRate, string personId, PersonStateController agent)
        {
            bool isAbnormal = heartRate < heartRateMin || heartRate > heartRateMax;

            if (isAbnormal)
            {
                _heartRateAbnormalTime += checkInterval;
                if (_heartRateAbnormalTime >= abnormalDurationThreshold)
                {
                    string message = $"Heart rate abnormal: {heartRate} bpm (normal: {heartRateMin}-{heartRateMax})";
                    Debug.Log($"[HealthMonitoringController] Heart rate abnormal detected: {heartRate} bpm, abnormal time: {_heartRateAbnormalTime:F1}s");
                    TriggerHealthAlarm(message, personId, agent);
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
        private void CheckRespirationRate(int respirationRate, string personId, PersonStateController agent)
        {
            bool isAbnormal = respirationRate < respirationRateMin || respirationRate > respirationRateMax;

            if (isAbnormal)
            {
                _respirationAbnormalTime += checkInterval;
                if (_respirationAbnormalTime >= abnormalDurationThreshold)
                {
                    string message = $"Respiration rate abnormal: {respirationRate} /min (normal: {respirationRateMin}-{respirationRateMax})";
                    TriggerHealthAlarm(message, personId, agent);
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
        private void CheckBodyMovement(float bodyMovement, string personId, PersonStateController agent)
        {
            if (bodyMovement < bodyMovementMin)
            {
                _noMovementTime += checkInterval;
                if (_noMovementTime >= noMovementDurationThreshold)
                {
                    string message = $"No body movement detected for extended period ({_noMovementTime / 60f:F1} minutes)";
                    TriggerHealthAlarm(message, personId, agent);
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
        private void TriggerHealthAlarm(string message, string personId, PersonStateController agent)
        {
            // 测试模式下跳过冷却时间检查
            if (!testMode)
            {
                // 按人冷却
                var pidKey = string.IsNullOrEmpty(personId) ? "Unknown" : personId;
                if (_lastAlarmTimeByPerson.TryGetValue(pidKey, out var last))
                {
                    float timeSinceLastAlarm = Time.time - last;
                    if (timeSinceLastAlarm < alarmCooldown)
                    {
                        Debug.Log($"[HealthMonitoringController] Alarm in cooldown for {pidKey}, remaining: {alarmCooldown - timeSinceLastAlarm:F1}s");
                        return; // 还在冷却期内，不触发告警
                    }
                }
            }

            // 获取房间与人物信息
            string roomId = "Unknown";
            if (agent != null && agent.Status != null)
            {
                roomId = string.IsNullOrEmpty(agent.Status.currentRoomId) ? "Unknown" : agent.Status.currentRoomId;
            }

            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.AddAlarm(AlarmType.HealthAbnormal, roomId, personId);
                var pidKey = string.IsNullOrEmpty(personId) ? "Unknown" : personId;
                _lastAlarmTimeByPerson[pidKey] = Time.time;

                Debug.Log($"[HealthMonitoringController] Health alarm triggered: {message} in {roomId} person={personId}");
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
            var agent = PersonStateManager.Instance != null
                ? PersonStateManager.Instance.DefaultAgent
                : PersonStateController.Instance;
            var personId = agent != null ? agent.PersonId : "Unknown";
            TriggerHealthAlarm(message, personId, agent);
        }
    }
}

