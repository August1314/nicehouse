using UnityEngine;
using System.Collections.Generic;
using NiceHouse.Data;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 温度影响系统
    /// 计算设备（如空调）对房间温度的影响
    /// </summary>
    public class TemperatureInfluenceSystem : MonoBehaviour
    {
        public static TemperatureInfluenceSystem Instance { get; private set; }

        [Header("设备影响参数")]
        [Tooltip("空调制冷/制热影响系数（温度变化速度，度/秒）")]
        public float acInfluenceRate = 0.5f;
        
        [Tooltip("风扇降温影响系数（较小）")]
        public float fanCoolingRate = 0.1f;

        [Header("房间间热传导")]
        [Tooltip("相邻房间热传导系数（0-1，0=不传导，1=完全传导）")]
        public float heatTransferRate = 0.05f;
        
        [Tooltip("是否启用房间间热传导")]
        public bool enableHeatTransfer = true;

        [Header("更新设置")]
        [Tooltip("温度更新间隔（秒）")]
        public float updateInterval = 0.1f;

        [Header("初始化设置")]
        [Tooltip("初始温度值（当房间温度未初始化时使用）")]
        public float initialTemperature = 24f;

        [Header("调试")]
        [Tooltip("是否输出调试日志")]
        public bool enableDebugLog = false;

        private float _timer;
        private Dictionary<string, float> _lastTemperatures = new Dictionary<string, float>();
        private bool _initialized = false;

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
            // 延迟初始化，确保其他系统已经准备好
            Invoke(nameof(InitializeTemperatures), 0.1f);
        }

        /// <summary>
        /// 初始化所有房间的温度（如果未初始化）
        /// 从 EnvironmentDataSimulator 的配置中读取每个房间的基础温度
        /// </summary>
        private void InitializeTemperatures()
        {
            if (RoomManager.Instance == null || EnvironmentDataStore.Instance == null)
            {
                return;
            }

            // 尝试从 EnvironmentDataSimulator 获取房间配置
            var simulator = FindObjectOfType<EnvironmentDataSimulator>();
            Dictionary<string, float> roomBaseTemperatures = new Dictionary<string, float>();

            if (simulator != null && simulator.roomConfigs != null)
            {
                foreach (var config in simulator.roomConfigs)
                {
                    if (!string.IsNullOrEmpty(config.roomId))
                    {
                        roomBaseTemperatures[config.roomId] = config.baseTemperature;
                    }
                }
            }

            var allRooms = RoomManager.Instance.GetAllRooms();
            foreach (var room in allRooms.Values)
            {
                var data = EnvironmentDataStore.Instance.GetOrCreateRoomData(room.roomId);
                
                // 如果温度接近 0（未初始化），设置初始温度
                if (Mathf.Abs(data.temperature) < 0.1f)
                {
                    // 优先使用 EnvironmentDataSimulator 配置的基础温度
                    float initTemp = initialTemperature;
                    if (roomBaseTemperatures.TryGetValue(room.roomId, out float baseTemp))
                    {
                        initTemp = baseTemp;
                    }
                    
                    data.temperature = initTemp;
                    _lastTemperatures[room.roomId] = initTemp;
                    
                    if (enableDebugLog)
                    {
                        Debug.Log($"[TemperatureInfluence] Initialized {room.roomId} temperature to {initTemp}°C " +
                                 (roomBaseTemperatures.ContainsKey(room.roomId) ? "(from simulator config)" : "(default)"));
                    }
                }
            }

            _initialized = true;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;

            if (RoomManager.Instance == null || EnvironmentDataStore.Instance == null)
            {
                return;
            }

            // 计算所有房间的温度影响
            var allRooms = RoomManager.Instance.GetAllRooms();
            foreach (var room in allRooms.Values)
            {
                CalculateTemperatureInfluence(room.roomId);
            }
        }

        /// <summary>
        /// 计算单个房间的温度影响
        /// </summary>
        private void CalculateTemperatureInfluence(string roomId)
        {
            if (!EnvironmentDataStore.Instance.TryGetRoomData(roomId, out var env))
            {
                return;
            }

            // 如果还未初始化，先初始化温度
            if (!_initialized)
            {
                if (Mathf.Abs(env.temperature) < 0.1f)
                {
                    // 尝试从 EnvironmentDataSimulator 获取该房间的基础温度
                    float initTemp = initialTemperature;
                    var simulator = FindObjectOfType<EnvironmentDataSimulator>();
                    if (simulator != null && simulator.roomConfigs != null)
                    {
                        foreach (var config in simulator.roomConfigs)
                        {
                            if (config.roomId == roomId)
                            {
                                initTemp = config.baseTemperature;
                                break;
                            }
                        }
                    }
                    
                    env.temperature = initTemp;
                    _lastTemperatures[roomId] = initTemp;
                }
            }

            float temperatureChange = 0f;

            // 1. 设备影响（空调、风扇等）
            if (DeviceManager.Instance != null)
            {
                var devices = DeviceManager.Instance.GetDevicesInRoom(roomId);
                foreach (var device in devices)
                {
                    if (device.type == NiceHouse.Data.DeviceType.AirConditioner)
                    {
                        var acController = device.GetComponent<AirConditionerController>();
                        if (acController != null && acController.IsOn)
                        {
                            // 空调影响：向目标温度靠近
                            float tempDiff = env.temperature - acController.targetTemperature;
                            float influence = -tempDiff * acInfluenceRate * updateInterval;
                            temperatureChange += influence;

                            if (enableDebugLog)
                            {
                                Debug.Log($"[TemperatureInfluence] AC {device.deviceId} in {roomId}: " +
                                         $"temp={env.temperature:F2}°C, target={acController.targetTemperature}°C, " +
                                         $"change={influence:F3}°C");
                            }
                        }
                    }
                    else if (device.type == NiceHouse.Data.DeviceType.Fan)
                    {
                        var fanController = device.GetComponent<FanController>();
                        if (fanController != null && fanController.IsOn)
                        {
                            // 风扇影响：轻微降温
                            float influence = -fanCoolingRate * updateInterval;
                            temperatureChange += influence;
                        }
                    }
                }
            }

            // 2. 房间间热传导（简化模型）
            if (enableHeatTransfer)
            {
                float heatTransferChange = CalculateHeatTransfer(roomId, env.temperature);
                temperatureChange += heatTransferChange;
            }

            // 3. 应用温度变化
            if (Mathf.Abs(temperatureChange) > 0.001f)
            {
                env.temperature += temperatureChange;
                
                // 记录温度变化（用于调试）
                if (_lastTemperatures.ContainsKey(roomId))
                {
                    float delta = env.temperature - _lastTemperatures[roomId];
                    if (enableDebugLog && Mathf.Abs(delta) > 0.01f)
                    {
                        Debug.Log($"[TemperatureInfluence] {roomId} temperature changed: " +
                                 $"{_lastTemperatures[roomId]:F2}°C -> {env.temperature:F2}°C " +
                                 $"(delta: {delta:+.3f}°C)");
                    }
                }
                _lastTemperatures[roomId] = env.temperature;
            }
        }

        /// <summary>
        /// 计算房间间热传导
        /// </summary>
        private float CalculateHeatTransfer(string roomId, float currentTemp)
        {
            if (RoomManager.Instance == null || EnvironmentDataStore.Instance == null)
            {
                return 0f;
            }

            float totalTransfer = 0f;
            var allRooms = RoomManager.Instance.GetAllRooms();

            // 简化：所有房间都会相互影响（可以通过配置优化）
            foreach (var otherRoom in allRooms.Values)
            {
                if (otherRoom.roomId == roomId) continue;

                if (EnvironmentDataStore.Instance.TryGetRoomData(otherRoom.roomId, out var otherEnv))
                {
                    float tempDiff = otherEnv.temperature - currentTemp;
                    float transfer = tempDiff * heatTransferRate * updateInterval;
                    totalTransfer += transfer;
                }
            }

            return totalTransfer;
        }

        /// <summary>
        /// 手动设置温度（用于测试/调试）
        /// </summary>
        public void SetRoomTemperature(string roomId, float temperature)
        {
            if (EnvironmentDataStore.Instance != null)
            {
                var data = EnvironmentDataStore.Instance.GetOrCreateRoomData(roomId);
                data.temperature = temperature;
                _lastTemperatures[roomId] = temperature;
                
                if (enableDebugLog)
                {
                    Debug.Log($"[TemperatureInfluence] Manually set {roomId} temperature to {temperature}°C");
                }
            }
        }

        /// <summary>
        /// 获取房间温度变化率（用于调试）
        /// </summary>
        public float GetTemperatureChangeRate(string roomId)
        {
            if (!_lastTemperatures.ContainsKey(roomId))
            {
                return 0f;
            }

            if (EnvironmentDataStore.Instance != null &&
                EnvironmentDataStore.Instance.TryGetRoomData(roomId, out var env))
            {
                float lastTemp = _lastTemperatures[roomId];
                return (env.temperature - lastTemp) / updateInterval;
            }

            return 0f;
        }
    }
}

