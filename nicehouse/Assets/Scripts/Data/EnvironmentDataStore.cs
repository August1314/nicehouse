using System.Collections.Generic;
using UnityEngine;

namespace NiceHouse.Data
{
    /// <summary>
    /// 单个房间的环境数据。
    /// </summary>
    public class RoomEnvironmentData
    {
        public float temperature;
        public float humidity;
        public float pm25;
        public float pm10;
    }

    /// <summary>
    /// 负责存储和提供房间环境数据访问接口。
    /// </summary>
    public class EnvironmentDataStore : MonoBehaviour
    {
        public static EnvironmentDataStore Instance { get; private set; }

        private readonly Dictionary<string, RoomEnvironmentData> _roomEnvData =
            new Dictionary<string, RoomEnvironmentData>();

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
        /// 获取或创建房间环境数据。如果数据不存在，会自动创建新的数据对象。
        /// </summary>
        /// <param name="roomId">房间ID</param>
        /// <returns>房间环境数据对象</returns>
        public RoomEnvironmentData GetOrCreateRoomData(string roomId)
        {
            if (!_roomEnvData.TryGetValue(roomId, out var data))
            {
                data = new RoomEnvironmentData();
                _roomEnvData[roomId] = data;
            }

            return data;
        }

        /// <summary>
        /// 尝试获取房间环境数据。
        /// </summary>
        /// <param name="roomId">房间ID</param>
        /// <param name="data">如果找到，返回环境数据；否则为 null</param>
        /// <returns>如果找到数据返回 true，否则返回 false</returns>
        public bool TryGetRoomData(string roomId, out RoomEnvironmentData data)
        {
            return _roomEnvData.TryGetValue(roomId, out data);
        }

        /// <summary>
        /// 获取所有房间环境数据的只读字典。
        /// </summary>
        /// <returns>房间ID到环境数据的映射</returns>
        public IReadOnlyDictionary<string, RoomEnvironmentData> GetAllRoomData() => _roomEnvData;
    }
}


