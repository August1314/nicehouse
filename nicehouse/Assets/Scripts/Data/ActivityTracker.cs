using System.Collections.Generic;
using UnityEngine;

namespace NiceHouse.Data
{
    /// <summary>
    /// 房间活动数据（用于热力图）。
    /// </summary>
    [System.Serializable]
    public class ActivityData
    {
        public int visitCount;        // 访问次数
        public float totalStayTime;   // 累计停留时间（秒）
    }

    /// <summary>
    /// 活动追踪器，记录数字人在各房间的活动频次和停留时间。
    /// </summary>
    public class ActivityTracker : MonoBehaviour
    {
        public static ActivityTracker Instance { get; private set; }

        private readonly Dictionary<string, Dictionary<string, ActivityData>> _activityByPersonAndRoom =
            new Dictionary<string, Dictionary<string, ActivityData>>();

        private readonly Dictionary<string, (string roomId, float enterTime)> _agentStay =
            new Dictionary<string, (string roomId, float enterTime)>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // 注意：DontDestroyOnLoad 只能用于根 GameObject，如果挂载在子对象上会失败
        }

        private void Start()
        {
            // 监听多 Agent 状态变化
            if (PersonStateManager.Instance != null)
            {
                PersonStateManager.Instance.OnAnyStateChanged += OnPersonStateChanged;
            }
            else if (PersonStateController.Instance != null)
            {
                // 兼容单实例
                PersonStateController.Instance.OnStateChanged += OnLegacyPersonStateChanged;
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
        }

        private void OnPersonStateChanged(PersonStateController agent, PersonState newState, string roomId)
        {
            if (agent == null) return;
            string personId = string.IsNullOrEmpty(agent.PersonId) ? "Unknown" : agent.PersonId;
            HandleRoomChange(personId, roomId);
        }

        private void OnLegacyPersonStateChanged(PersonState newState, string roomId)
        {
            HandleRoomChange("Default", roomId);
        }

        private void HandleRoomChange(string personId, string roomId)
        {
            if (!_agentStay.TryGetValue(personId, out var stay))
            {
                stay = (roomId: string.Empty, enterTime: 0f);
            }

            // 离开旧房间
            if (!string.IsNullOrEmpty(stay.roomId) && stay.roomId != roomId)
            {
                OnPersonLeaveRoom(personId, stay.roomId, Time.time - stay.enterTime);
            }

            // 进入新房间
            if (!string.IsNullOrEmpty(roomId))
            {
                OnPersonEnterRoom(personId, roomId);
            }
        }

        /// <summary>
        /// 数字人进入房间时调用。
        /// </summary>
        public void OnPersonEnterRoom(string personId, string roomId)
        {
            if (string.IsNullOrEmpty(roomId)) return;
            if (string.IsNullOrEmpty(personId)) personId = "Unknown";

            _agentStay[personId] = (roomId, Time.time);

            var dict = GetOrCreatePersonMap(personId);
            if (!dict.TryGetValue(roomId, out var data))
            {
                data = new ActivityData();
                dict[roomId] = data;
            }

            data.visitCount++;
        }

        /// <summary>
        /// 兼容旧接口：未指定 personId 时使用 Unknown。
        /// </summary>
        public void OnPersonEnterRoom(string roomId)
        {
            OnPersonEnterRoom("Unknown", roomId);
        }

        /// <summary>
        /// 数字人离开房间时调用。
        /// </summary>
        /// <param name="roomId">房间ID</param>
        /// <param name="stayTime">停留时间（秒）</param>
        public void OnPersonLeaveRoom(string personId, string roomId, float stayTime)
        {
            if (string.IsNullOrEmpty(roomId)) return;
            if (string.IsNullOrEmpty(personId)) personId = "Unknown";

            var dict = GetOrCreatePersonMap(personId);
            if (dict.TryGetValue(roomId, out var data))
            {
                data.totalStayTime += stayTime;
            }
        }

        /// <summary>
        /// 兼容旧接口：未指定 personId 时使用 Unknown。
        /// </summary>
        public void OnPersonLeaveRoom(string roomId, float stayTime)
        {
            OnPersonLeaveRoom("Unknown", roomId, stayTime);
        }

        /// <summary>
        /// 获取房间活动数据。
        /// </summary>
        public ActivityData GetRoomActivity(string roomId)
        {
            var total = new ActivityData();
            foreach (var person in _activityByPersonAndRoom.Values)
            {
                if (person.TryGetValue(roomId, out var data))
                {
                    total.visitCount += data.visitCount;
                    total.totalStayTime += data.totalStayTime;
                }
            }
            return total;
        }

        /// <summary>
        /// 获取指定人的房间活动数据。
        /// </summary>
        public ActivityData GetRoomActivity(string personId, string roomId)
        {
            if (string.IsNullOrEmpty(personId) || string.IsNullOrEmpty(roomId)) return new ActivityData();
            return _activityByPersonAndRoom.TryGetValue(personId, out var dict) && dict.TryGetValue(roomId, out var data)
                ? data
                : new ActivityData();
        }

        /// <summary>
        /// 获取所有房间的活动数据。
        /// </summary>
        public IReadOnlyDictionary<string, ActivityData> GetAllRoomActivity()
        {
            var merged = new Dictionary<string, ActivityData>();
            foreach (var person in _activityByPersonAndRoom.Values)
            {
                foreach (var kv in person)
                {
                    if (!merged.TryGetValue(kv.Key, out var data))
                    {
                        data = new ActivityData();
                        merged[kv.Key] = data;
                    }

                    data.visitCount += kv.Value.visitCount;
                    data.totalStayTime += kv.Value.totalStayTime;
                }
            }
            return merged;
        }

        /// <summary>
        /// 获取指定人的全部活动数据。
        /// </summary>
        public IReadOnlyDictionary<string, ActivityData> GetAllRoomActivity(string personId)
        {
            if (string.IsNullOrEmpty(personId)) return new Dictionary<string, ActivityData>();
            if (_activityByPersonAndRoom.TryGetValue(personId, out var dict))
            {
                return dict;
            }
            return new Dictionary<string, ActivityData>();
        }

        /// <summary>
        /// 重置所有活动数据。
        /// </summary>
        public void ResetAll()
        {
            _activityByPersonAndRoom.Clear();
            _agentStay.Clear();
        }

        private Dictionary<string, ActivityData> GetOrCreatePersonMap(string personId)
        {
            if (!_activityByPersonAndRoom.TryGetValue(personId, out var dict))
            {
                dict = new Dictionary<string, ActivityData>();
                _activityByPersonAndRoom[personId] = dict;
            }
            return dict;
        }
    }
}


