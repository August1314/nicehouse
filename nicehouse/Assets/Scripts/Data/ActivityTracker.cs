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

        private readonly Dictionary<string, ActivityData> _roomActivity = new Dictionary<string, ActivityData>();

        private string _currentRoomId;
        private float _enterRoomTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // 监听数字人状态变化
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

        private void OnPersonStateChanged(PersonState newState, string roomId)
        {
            // 当数字人进入新房间时，记录离开旧房间的时间
            if (!string.IsNullOrEmpty(_currentRoomId) && _currentRoomId != roomId)
            {
                OnPersonLeaveRoom(_currentRoomId, Time.time - _enterRoomTime);
            }

            // 记录进入新房间
            if (!string.IsNullOrEmpty(roomId))
            {
                OnPersonEnterRoom(roomId);
            }
        }

        /// <summary>
        /// 数字人进入房间时调用。
        /// </summary>
        public void OnPersonEnterRoom(string roomId)
        {
            if (string.IsNullOrEmpty(roomId)) return;

            _currentRoomId = roomId;
            _enterRoomTime = Time.time;

            if (!_roomActivity.TryGetValue(roomId, out var data))
            {
                data = new ActivityData();
                _roomActivity[roomId] = data;
            }

            data.visitCount++;
        }

        /// <summary>
        /// 数字人离开房间时调用。
        /// </summary>
        /// <param name="roomId">房间ID</param>
        /// <param name="stayTime">停留时间（秒）</param>
        public void OnPersonLeaveRoom(string roomId, float stayTime)
        {
            if (string.IsNullOrEmpty(roomId)) return;

            if (_roomActivity.TryGetValue(roomId, out var data))
            {
                data.totalStayTime += stayTime;
            }
        }

        /// <summary>
        /// 获取房间活动数据。
        /// </summary>
        public ActivityData GetRoomActivity(string roomId)
        {
            return _roomActivity.TryGetValue(roomId, out var data) ? data : new ActivityData();
        }

        /// <summary>
        /// 获取所有房间的活动数据。
        /// </summary>
        public IReadOnlyDictionary<string, ActivityData> GetAllRoomActivity() => _roomActivity;

        /// <summary>
        /// 重置所有活动数据。
        /// </summary>
        public void ResetAll()
        {
            _roomActivity.Clear();
            _currentRoomId = string.Empty;
            _enterRoomTime = 0f;
        }
    }
}

