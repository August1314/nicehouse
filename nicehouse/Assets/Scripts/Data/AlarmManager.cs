using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NiceHouse.Data
{
    /// <summary>
    /// 告警类型枚举。
    /// </summary>
    public enum AlarmType
    {
        Smoke,           // 烟雾
        GasLeak,         // 燃气泄漏
        Fall,            // 跌倒
        LongSitting,     // 久坐
        LongBathing,     // 久浴
        HealthAbnormal,  // 健康异常
        EmergencyCall    // 一键呼叫
    }

    /// <summary>
    /// 告警记录。
    /// </summary>
    [System.Serializable]
    public class AlarmRecord
    {
        public AlarmType type;
        public string roomId;
        public System.DateTime time;
        public bool handled;

        public AlarmRecord(AlarmType type, string roomId)
        {
            this.type = type;
            this.roomId = roomId;
            this.time = System.DateTime.Now;
            this.handled = false;
        }
    }

    /// <summary>
    /// 统一告警管理器，负责记录和管理所有告警事件。
    /// </summary>
    public class AlarmManager : MonoBehaviour
    {
        public static AlarmManager Instance { get; private set; }

        [Tooltip("最大保存告警记录数")]
        public int maxRecords = 100;

        [Tooltip("告警事件，参数：告警记录")]
        public System.Action<AlarmRecord> OnAlarmAdded;

        private readonly List<AlarmRecord> _records = new List<AlarmRecord>();

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

        /// <summary>
        /// 添加告警记录。
        /// </summary>
        /// <param name="type">告警类型</param>
        /// <param name="roomId">房间ID</param>
        public void AddAlarm(AlarmType type, string roomId)
        {
            var record = new AlarmRecord(type, roomId);
            _records.Add(record);

            // 限制记录数量
            if (_records.Count > maxRecords)
            {
                _records.RemoveAt(0);
            }

            Debug.Log($"[Alarm] {type} in {roomId} at {record.time:HH:mm:ss}");

            OnAlarmAdded?.Invoke(record);
        }

        /// <summary>
        /// 获取最近的告警记录。
        /// </summary>
        /// <param name="count">返回数量</param>
        /// <returns>告警记录列表（按时间倒序）</returns>
        public IEnumerable<AlarmRecord> GetRecentAlarms(int count)
        {
            return _records.OrderByDescending(r => r.time).Take(count);
        }

        /// <summary>
        /// 获取所有未处理的告警。
        /// </summary>
        public IEnumerable<AlarmRecord> GetUnhandledAlarms()
        {
            return _records.Where(r => !r.handled).OrderByDescending(r => r.time);
        }

        /// <summary>
        /// 标记告警为已处理。
        /// </summary>
        public void MarkHandled(AlarmRecord record)
        {
            if (record != null)
            {
                record.handled = true;
            }
        }

        /// <summary>
        /// 获取所有告警记录。
        /// </summary>
        public IReadOnlyList<AlarmRecord> GetAllAlarms() => _records;

        /// <summary>
        /// 清空所有告警记录。
        /// </summary>
        public void ClearAll()
        {
            _records.Clear();
        }
    }
}

