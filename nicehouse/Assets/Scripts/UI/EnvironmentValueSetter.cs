using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NiceHouse.Data;

namespace NiceHouse.UI
{
    /// <summary>
    /// 环境数值设置面板
    /// 允许用户手动设置环境温度、PM2.5、烟雾浓度、电力能耗
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
        public TMP_InputField smokeInput; // 烟雾浓度（从SafetyDataStore）
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

                // 从SafetyDataStore获取烟雾浓度
                if (smokeInput != null)
                {
                    if (SafetyDataStore.Instance != null &&
                        SafetyDataStore.Instance.TryGetRoomSafety(currentRoomId, out var safetyData))
                    {
                        smokeInput.text = safetyData.smokeLevel.ToString("F1");
                    }
                    else
                    {
                        smokeInput.text = "0.0";
                    }
                }
            }

            // 总能耗（可设置的值）
            if (energyInput != null && EnvironmentDataStore.Instance.TryGetRoomData(currentRoomId, out var envDataForEnergy))
            {
                energyInput.text = envDataForEnergy.energy.ToString("F2");
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

            // 解析并设置烟雾浓度
            if (float.TryParse(smokeInput.text, out float smoke))
            {
                if (SafetyDataStore.Instance != null)
                {
                    var safetyData = SafetyDataStore.Instance.GetOrCreateRoomSafety(currentRoomId);
                    safetyData.smokeLevel = smoke;
                }
            }

            // 解析并设置电力能耗
            if (float.TryParse(energyInput.text, out float energy))
            {
                envData.energy = energy;
            }

            Debug.Log($"Applied values for room {currentRoomId}: Temp={envData.temperature}, PM2.5={envData.pm25}, Smoke={smoke}, Energy={envData.energy}");
        }

        private void ResetToCurrentValues()
        {
            UpdateUIFromData();
        }
    }
}