using System.Collections.Generic;
using UnityEngine;

namespace NiceHouse.Data
{
    /// <summary>
    /// 管理所有设备的查找与按房间分组。
    /// </summary>
    public class DeviceManager : MonoBehaviour
    {
        public static DeviceManager Instance { get; private set; }

        private readonly Dictionary<string, DeviceDefinition> _devicesById =
            new Dictionary<string, DeviceDefinition>();

        private readonly Dictionary<string, List<DeviceDefinition>> _devicesByRoomId =
            new Dictionary<string, List<DeviceDefinition>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _devicesById.Clear();
            _devicesByRoomId.Clear();

            var devices = FindObjectsOfType<DeviceDefinition>();
            foreach (var device in devices)
            {
                if (string.IsNullOrEmpty(device.deviceId))
                {
                    Debug.LogWarning($"DeviceDefinition on {device.name} has empty deviceId.");
                    continue;
                }

                if (_devicesById.ContainsKey(device.deviceId))
                {
                    Debug.LogWarning($"Duplicate deviceId detected: {device.deviceId}");
                    continue;
                }

                _devicesById.Add(device.deviceId, device);

                if (!_devicesByRoomId.TryGetValue(device.roomId, out var list))
                {
                    list = new List<DeviceDefinition>();
                    _devicesByRoomId[device.roomId] = list;
                }

                list.Add(device);
            }
        }

        /// <summary>
        /// 尝试获取指定ID的设备定义。
        /// </summary>
        /// <param name="deviceId">设备唯一ID，例如 "AC_LivingRoom_01"</param>
        /// <param name="device">如果找到，返回设备定义；否则为 null</param>
        /// <returns>如果找到设备返回 true，否则返回 false</returns>
        public bool TryGetDevice(string deviceId, out DeviceDefinition device)
        {
            return _devicesById.TryGetValue(deviceId, out device);
        }

        /// <summary>
        /// 获取指定房间内的所有设备。
        /// </summary>
        /// <param name="roomId">房间ID</param>
        /// <returns>设备列表，如果房间不存在或没有设备则返回空列表</returns>
        public IReadOnlyList<DeviceDefinition> GetDevicesInRoom(string roomId)
        {
            return _devicesByRoomId.TryGetValue(roomId, out var list) ? list : System.Array.Empty<DeviceDefinition>();
        }

        /// <summary>
        /// 获取所有设备定义的只读字典。
        /// </summary>
        /// <returns>设备ID到设备定义的映射</returns>
        public IReadOnlyDictionary<string, DeviceDefinition> GetAllDevices() => _devicesById;
    }
}


