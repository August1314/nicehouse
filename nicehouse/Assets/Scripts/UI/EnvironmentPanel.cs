using UnityEngine;
using TMPro;
using NiceHouse.Data;

namespace NiceHouse.UI
{
    /// <summary>
    /// 在屏幕上展示某个房间的环境数据（温度 / 湿度 / PM2.5），以及某个设备的能耗。
    /// 需要在 Inspector 中把 Text 组件拖进来。
    /// </summary>
    public class EnvironmentPanel : MonoBehaviour
    {
        [Header("要绑定的数据 ID")]
        public string roomId = "LivingRoom01";
        public string deviceId = "Windows01";

        [Header("UI Text 引用（TextMeshProUGUI）")]
        public TextMeshProUGUI roomNameText;
        public TextMeshProUGUI temperatureText;
        public TextMeshProUGUI humidityText;
        public TextMeshProUGUI pm25Text;
        public TextMeshProUGUI energyText;

        [Header("刷新间隔（秒）")]
        public float updateInterval = 0.5f;

        private float _timer;

        private void Start()
        {
            if (roomNameText != null)
            {
                roomNameText.text = roomId;
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;

            UpdateEnvironment();
            UpdateEnergy();
        }

        private void UpdateEnvironment()
        {
            if (EnvironmentDataStore.Instance == null) return;

            if (EnvironmentDataStore.Instance.TryGetRoomData(roomId, out var data))
            {
                if (temperatureText != null)
                    temperatureText.text = $"{data.temperature:F1} °C";

                if (humidityText != null)
                    humidityText.text = $"{data.humidity:F1} %";

                if (pm25Text != null)
                    pm25Text.text = $"{data.pm25:F1} μg/m³";
            }
        }

        private void UpdateEnergy()
        {
            if (EnergyManager.Instance == null) return;

            float kwh = EnergyManager.Instance.GetDeviceDailyConsumption(deviceId);
            if (energyText != null)
            {
                energyText.text = $"{kwh:F3} kWh";
            }
        }
    }
}


