using System.Collections.Generic;
using UnityEngine;

namespace NiceHouse.Data
{
    /// <summary>
    /// 单个设备的能耗数据。
    /// </summary>
    public class EnergyData
    {
        public float currentPower;     // 当前功率（W）
        public float dailyConsumption; // 当天累计（kWh）
    }

    /// <summary>
    /// 简单能耗管理器，根据设备开关状态与功率，积累用电量。
    /// 具体设备脚本可在 OnEnable/OnDisable 或自定义回调中通知本类。
    /// </summary>
    public class EnergyManager : MonoBehaviour
    {
        public static EnergyManager Instance { get; private set; }

        [System.Serializable]
        public class DeviceEnergyConfig
        {
            public string deviceId;
            public float ratedPower = 1000f; // 额定功率（W）
        }

        [Tooltip("各设备的额定功率配置")]
        public List<DeviceEnergyConfig> deviceConfigs = new List<DeviceEnergyConfig>();

        private readonly Dictionary<string, EnergyData> _deviceEnergy = new Dictionary<string, EnergyData>();
        private readonly HashSet<string> _activeDevices = new HashSet<string>();
        private readonly Dictionary<string, float> _deviceRatedPowers = new Dictionary<string, float>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // 注意：DontDestroyOnLoad 只能用于根 GameObject，如果挂载在子对象上会失败

            _deviceRatedPowers.Clear();
            foreach (var cfg in deviceConfigs)
            {
                if (string.IsNullOrEmpty(cfg.deviceId)) continue;
                _deviceRatedPowers[cfg.deviceId] = cfg.ratedPower;
            }
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            foreach (var deviceId in _activeDevices)
            {
                var data = GetOrCreateEnergyData(deviceId);
                var power = GetRatedPower(deviceId);
                data.currentPower = power;

                // W * s -> Wh -> kWh
                data.dailyConsumption += power * deltaTime / 3600f / 1000f;
            }
        }

        private EnergyData GetOrCreateEnergyData(string deviceId)
        {
            if (!_deviceEnergy.TryGetValue(deviceId, out var data))
            {
                data = new EnergyData();
                _deviceEnergy[deviceId] = data;
            }

            return data;
        }

        private float GetRatedPower(string deviceId)
        {
            return _deviceRatedPowers.TryGetValue(deviceId, out var p) ? p : 0f;
        }

        /// <summary>
        /// 外部调用：某设备开始耗电（例如空调开启）。
        /// </summary>
        public void StartConsume(string deviceId)
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                _activeDevices.Add(deviceId);
            }
        }

        /// <summary>
        /// 外部调用：某设备停止耗电（例如空调关闭）。
        /// </summary>
        public void StopConsume(string deviceId)
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                _activeDevices.Remove(deviceId);
                if (_deviceEnergy.TryGetValue(deviceId, out var data))
                {
                    data.currentPower = 0f;
                }
            }
        }

        /// <summary>
        /// 获取某设备当天累计用电量（kWh）。
        /// </summary>
        public float GetDeviceDailyConsumption(string deviceId)
        {
            return _deviceEnergy.TryGetValue(deviceId, out var data) ? data.dailyConsumption : 0f;
        }
    }
}


