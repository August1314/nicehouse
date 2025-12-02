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


