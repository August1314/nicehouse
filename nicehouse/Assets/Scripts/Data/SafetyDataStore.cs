using System.Collections.Generic;
using UnityEngine;

namespace NiceHouse.Data
{
    /// <summary>
    /// 房间安全数据（烟雾、燃气浓度）。
    /// </summary>
    [System.Serializable]
    public class SafetyData
    {
        public float smokeLevel;  // 烟雾浓度（0-100）
        public float gasLevel;    // 燃气浓度（0-100）
    }

    /// <summary>
    /// 安全数据存储，管理各房间的烟雾和燃气浓度。
    /// </summary>
    public class SafetyDataStore : MonoBehaviour
    {
        public static SafetyDataStore Instance { get; private set; }

        [Tooltip("数据更新间隔（秒）")]
        public float updateInterval = 1f;

        [Header("生命周期")]
        [Tooltip("场景切换时保持不销毁")]
        public bool dontDestroyOnLoad = true;

        [Header("阈值与告警设置")]
        [Tooltip("安全阈值配置（可选）。未配置时使用默认值。")]
        public SafetyDataThresholds thresholds;

        [Tooltip("是否启用烟雾/燃气阈值检测并触发告警")]
        public bool enableAlarmDetection = true;

        [Tooltip("同一房间同一类型告警的最小间隔（秒）")]
        public float alarmCooldown = 60f;

        [Tooltip("启用调试日志")]
        public bool enableDebugLog = false;

        private readonly Dictionary<string, SafetyData> _roomSafety = new Dictionary<string, SafetyData>();
        private readonly Dictionary<string, float> _lastAlarmTime = new Dictionary<string, float>();

        private float _timer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (dontDestroyOnLoad && transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            // 注意：DontDestroyOnLoad 只能用于根 GameObject，如果挂载在子对象上会失败
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;

            // 模拟各房间安全数据（正常情况下浓度很低）
            if (RoomManager.Instance != null)
            {
                foreach (var room in RoomManager.Instance.GetAllRooms().Values)
                {
                    var data = GetOrCreateRoomSafety(room.roomId);
                    // 正常情况下烟雾和燃气浓度都很低
                    data.smokeLevel = Mathf.Max(0f, data.smokeLevel + Random.Range(-0.5f, 0.5f));
                    data.gasLevel = Mathf.Max(0f, data.gasLevel + Random.Range(-0.2f, 0.2f));

                    if (enableAlarmDetection)
                    {
                        CheckThresholdAndTrigger(room.roomId, data);
                    }
                }
            }
        }

        /// <summary>
        /// 获取或创建房间安全数据。
        /// </summary>
        public SafetyData GetOrCreateRoomSafety(string roomId)
        {
            if (!_roomSafety.TryGetValue(roomId, out var data))
            {
                data = new SafetyData { smokeLevel = 0f, gasLevel = 0f };
                _roomSafety[roomId] = data;
            }
            return data;
        }

        /// <summary>
        /// 获取房间安全数据。
        /// </summary>
        public bool TryGetRoomSafety(string roomId, out SafetyData data)
        {
            return _roomSafety.TryGetValue(roomId, out data);
        }

        /// <summary>
        /// 设置房间烟雾浓度（用于模拟火灾等场景）。
        /// </summary>
        public void SetSmokeLevel(string roomId, float level)
        {
            var data = GetOrCreateRoomSafety(roomId);
            data.smokeLevel = Mathf.Clamp(level, 0f, 100f);
        }

        /// <summary>
        /// 设置房间燃气浓度（用于模拟燃气泄漏等场景）。
        /// </summary>
        public void SetGasLevel(string roomId, float level)
        {
            var data = GetOrCreateRoomSafety(roomId);
            data.gasLevel = Mathf.Clamp(level, 0f, 100f);
        }

        /// <summary>
        /// 获取所有房间的安全数据。
        /// </summary>
        public IReadOnlyDictionary<string, SafetyData> GetAllRoomSafety() => _roomSafety;

        /// <summary>
        /// 检查阈值并触发告警。
        /// </summary>
        private void CheckThresholdAndTrigger(string roomId, SafetyData data)
        {
            float now = Time.time;
            float smokeThreshold = thresholds != null ? thresholds.GetSmokeThreshold() : 70f;
            float gasThreshold = thresholds != null ? thresholds.GetGasThreshold() : 40f;

            if (data.smokeLevel >= smokeThreshold)
            {
                TryTriggerAlarm(AlarmType.Smoke, roomId, now);
            }

            if (data.gasLevel >= gasThreshold)
            {
                TryTriggerAlarm(AlarmType.GasLeak, roomId, now);
            }
        }

        /// <summary>
        /// 触发告警（带冷却去重）。
        /// </summary>
        private void TryTriggerAlarm(AlarmType type, string roomId, float now)
        {
            string key = $"{type}:{roomId}";

            if (_lastAlarmTime.TryGetValue(key, out var lastTime))
            {
                if (now - lastTime < alarmCooldown)
                {
                    return; // 冷却中
                }
            }

            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.AddAlarm(type, roomId);
                _lastAlarmTime[key] = now;

                if (enableDebugLog)
                {
                    Debug.Log($"[SafetyDataStore] Alarm triggered: {type} in {roomId} (cooldown {alarmCooldown}s)");
                }
            }
            else
            {
                Debug.LogWarning("[SafetyDataStore] AlarmManager.Instance is null, alarm skipped.");
            }
        }
    }
}
