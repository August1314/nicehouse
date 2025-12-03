using System.Collections.Generic;
using UnityEngine;

namespace NiceHouse.Data
{
    /// <summary>
    /// 简单的环境数据模拟器，用于演示温度 / 湿度 / PM2.5 的动态变化。
    /// 可在 Inspector 中配置不同房间的基础值与波动范围。
    /// </summary>
    public class EnvironmentDataSimulator : MonoBehaviour
    {
        [System.Serializable]
        public class RoomEnvironmentConfig
        {
            public string roomId;
            public float baseTemperature = 24f;
            public float temperatureAmplitude = 2f;
            public float baseHumidity = 50f;
            public float humidityAmplitude = 10f;
            public float basePm25 = 35f;
            public float pm25Amplitude = 20f;
        }

        [Tooltip("数据更新间隔（秒）")]
        public float updateInterval = 1f;

        [Tooltip("每个房间的环境配置")]
        public List<RoomEnvironmentConfig> roomConfigs = new List<RoomEnvironmentConfig>();

        private float _timer;

        private void Awake()
        {
            // 如果还没有在 Inspector 中手动配置，则根据房间 ID 填充一份默认配置
            if (roomConfigs == null || roomConfigs.Count == 0)
            {
                InitializeDefaultConfigs();
            }
        }

        private void InitializeDefaultConfigs()
        {
            roomConfigs = new List<RoomEnvironmentConfig>();

            // 这些 ID 来自 RoomDefinition 的默认命名：
            // LivingRoom01、Kitchen01、Study01、Bathroom01、BedRoom01、Corridor01

            roomConfigs.Add(new RoomEnvironmentConfig
            {
                roomId = "LivingRoom01",
                baseTemperature = 25f,
                temperatureAmplitude = 1.5f,
                baseHumidity = 50f,
                humidityAmplitude = 8f,
                basePm25 = 35f,
                pm25Amplitude = 15f
            });

            roomConfigs.Add(new RoomEnvironmentConfig
            {
                roomId = "Kitchen01",
                baseTemperature = 26.5f,
                temperatureAmplitude = 2.0f,
                baseHumidity = 55f,
                humidityAmplitude = 10f,
                basePm25 = 45f,
                pm25Amplitude = 20f
            });

            roomConfigs.Add(new RoomEnvironmentConfig
            {
                roomId = "Study01",
                baseTemperature = 24f,
                temperatureAmplitude = 1.0f,
                baseHumidity = 45f,
                humidityAmplitude = 6f,
                basePm25 = 30f,
                pm25Amplitude = 10f
            });

            roomConfigs.Add(new RoomEnvironmentConfig
            {
                roomId = "Bathroom01",
                baseTemperature = 23f,
                temperatureAmplitude = 1.5f,
                baseHumidity = 60f,
                humidityAmplitude = 12f,
                basePm25 = 25f,
                pm25Amplitude = 8f
            });

            roomConfigs.Add(new RoomEnvironmentConfig
            {
                roomId = "BedRoom01",
                baseTemperature = 22f,
                temperatureAmplitude = 1.0f,
                baseHumidity = 50f,
                humidityAmplitude = 8f,
                basePm25 = 20f,
                pm25Amplitude = 8f
            });

            roomConfigs.Add(new RoomEnvironmentConfig
            {
                roomId = "Corridor01",
                baseTemperature = 23.5f,
                temperatureAmplitude = 1.0f,
                baseHumidity = 48f,
                humidityAmplitude = 6f,
                basePm25 = 28f,
                pm25Amplitude = 10f
            });
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;

            foreach (var cfg in roomConfigs)
            {
                if (string.IsNullOrEmpty(cfg.roomId)) continue;

                var data = EnvironmentDataStore.Instance.GetOrCreateRoomData(cfg.roomId);

                var t = Time.time;
                data.temperature = cfg.baseTemperature +
                                    Mathf.Sin(t * 0.1f) * cfg.temperatureAmplitude;
                data.humidity = cfg.baseHumidity +
                                 Mathf.Sin(t * 0.07f) * cfg.humidityAmplitude;
                data.pm25 = Mathf.Max(0f,
                    cfg.basePm25 + Mathf.Sin(t * 0.09f) * cfg.pm25Amplitude);

                // PM10 可以简化为 PM2.5 的某个比例或偏移
                data.pm10 = data.pm25 * 1.2f;
            }
        }
    }
}


