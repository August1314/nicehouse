using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NiceHouse.Data;

namespace NiceHouse.UI
{
    /// <summary>
    /// 环境数值设置面板
    /// 允许用户手动设置环境温度、PM2.5、烟雾浓度（PM10）、电力能耗
    /// 面板半透明，位于屏幕右上角
    /// </summary>
    public class EnvironmentValueSetter : MonoBehaviour
    {
        [Header("房间选择")]
        public TMP_Dropdown roomDropdown;
        public string currentRoomId = "LivingRoom01";

        [Header("数值设置")]
        public TMP_InputField temperatureInput;
        public TMP_InputField pm25Input;
        public TMP_InputField smokeInput; // PM10 作为烟雾浓度
        public TMP_InputField energyInput; // 总电力能耗

        [Header("按钮")]
        public Button applyButton;
        public Button resetButton;

        [Header("面板设置")]
        public CanvasGroup canvasGroup; // 用于半透明
        public float alpha = 0.8f; // 半透明度

        private void Start()
        {
            // 设置半透明
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }

            // 初始化房间下拉菜单
            InitializeRoomDropdown();

            // 绑定按钮事件
            if (applyButton != null)
            {
                applyButton.onClick.AddListener(ApplyValues);
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(ResetToCurrentValues);
            }

            // 初始化显示当前值
            UpdateUIFromData();

            // 设置能耗输入框为只读（显示用）
            if (energyInput != null)
            {
                energyInput.readOnly = true;
            }
        }

        private void InitializeRoomDropdown()
        {
            if (roomDropdown == null) return;

            roomDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>();

            // 从RoomManager获取所有房间ID
            if (RoomManager.Instance != null)
            {
                foreach (var roomId in RoomManager.Instance.GetAllRooms().Keys)
                {
                    options.Add(roomId);
                }
            }
            else
            {
                // 默认房间
                options.AddRange(new[] { "LivingRoom01", "Kitchen01", "Study01", "Bathroom01", "BedRoom01", "Corridor01" });
            }

            roomDropdown.AddOptions(options);

            // 设置当前房间
            int index = options.IndexOf(currentRoomId);
            if (index >= 0)
            {
                roomDropdown.value = index;
            }

            roomDropdown.onValueChanged.AddListener(OnRoomChanged);
        }

        private void OnRoomChanged(int index)
        {
            currentRoomId = roomDropdown.options[index].text;
            UpdateUIFromData();
        }

        private void UpdateUIFromData()
        {
            if (EnvironmentDataStore.Instance == null) return;

            if (EnvironmentDataStore.Instance.TryGetRoomData(currentRoomId, out var envData))
            {
                if (temperatureInput != null)
                    temperatureInput.text = envData.temperature.ToString("F1");

                if (pm25Input != null)
                    pm25Input.text = envData.pm25.ToString("F1");

                if (smokeInput != null)
                    smokeInput.text = envData.pm10.ToString("F1");
            }

            // 总能耗（所有设备的累计）
            if (EnergyManager.Instance != null && energyInput != null)
            {
                float totalConsumption = EnergyManager.Instance.GetTotalDailyConsumption();
                energyInput.text = totalConsumption.ToString("F2");
            }
        }

        private void ApplyValues()
        {
            if (EnvironmentDataStore.Instance == null) return;

            var envData = EnvironmentDataStore.Instance.GetOrCreateRoomData(currentRoomId);

            // 解析并设置温度
            if (float.TryParse(temperatureInput.text, out float temp))
            {
                envData.temperature = temp;
            }

            // 解析并设置PM2.5
            if (float.TryParse(pm25Input.text, out float pm25))
            {
                envData.pm25 = pm25;
            }

            // 解析并设置烟雾浓度（PM10）
            if (float.TryParse(smokeInput.text, out float smoke))
            {
                envData.pm10 = smoke;
            }

            // 电力能耗设置（这里暂时只显示，不设置，因为能耗是计算出来的）
            // 如果需要设置，可以考虑添加一个总能耗的字段

            Debug.Log($"Applied values for room {currentRoomId}: Temp={envData.temperature}, PM2.5={envData.pm25}, Smoke={envData.pm10}");
        }

        private void ResetToCurrentValues()
        {
            UpdateUIFromData();
        }
    }
}