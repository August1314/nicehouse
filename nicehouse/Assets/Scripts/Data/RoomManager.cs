using System.Collections.Generic;
using UnityEngine;

namespace NiceHouse.Data
{
    /// <summary>
    /// 负责在场景启动时收集所有房间定义，并提供查询接口。
    /// </summary>
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        // roomId -> RoomDefinition
        private readonly Dictionary<string, RoomDefinition> _roomsById =
            new Dictionary<string, RoomDefinition>();

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            // 确保在 Start 时也初始化（防止 Awake 执行顺序问题）
            if (Instance == null)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            // 如果 Instance 已存在且不是当前对象，销毁当前对象
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[RoomManager] Duplicate instance detected on {gameObject.name}, destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            // 如果 Instance 为 null，设置为当前对象
            if (Instance == null)
            {
                Instance = this;
            }

            // 注意：DontDestroyOnLoad 只能用于根 GameObject，如果挂载在子对象上会失败

            _roomsById.Clear();
            var rooms = FindObjectsOfType<RoomDefinition>();
            foreach (var room in rooms)
            {
                if (string.IsNullOrEmpty(room.roomId))
                {
                    Debug.LogWarning($"RoomDefinition on {room.name} has empty roomId.");
                    continue;
                }

                if (_roomsById.ContainsKey(room.roomId))
                {
                    Debug.LogWarning($"Duplicate roomId detected: {room.roomId}");
                    continue;
                }

                _roomsById.Add(room.roomId, room);
            }

            Debug.Log($"[RoomManager] Initialized with {_roomsById.Count} rooms");
        }

        /// <summary>
        /// 尝试获取指定ID的房间定义。
        /// </summary>
        /// <param name="roomId">房间唯一ID，例如 "LivingRoom01"</param>
        /// <param name="room">如果找到，返回房间定义；否则为 null</param>
        /// <returns>如果找到房间返回 true，否则返回 false</returns>
        public bool TryGetRoom(string roomId, out RoomDefinition room)
        {
            return _roomsById.TryGetValue(roomId, out room);
        }

        /// <summary>
        /// 获取所有房间定义的只读字典。
        /// </summary>
        /// <returns>房间ID到房间定义的映射</returns>
        public IReadOnlyDictionary<string, RoomDefinition> GetAllRooms() => _roomsById;
    }
}


