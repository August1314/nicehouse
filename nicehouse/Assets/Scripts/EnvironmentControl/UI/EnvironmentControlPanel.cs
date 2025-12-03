using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NiceHouse.Data;
using NiceHouse.EnvironmentControl;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 环境智控UI面板
    /// 显示环境数据、设备控制按钮、模式切换
    /// </summary>
    public class EnvironmentControlPanel : MonoBehaviour
    {
        [Header("房间选择")]
        [Tooltip("当前显示的房间ID")]
        public string currentRoomId = "LivingRoom01";

        [Header("环境数据显示")]
        public TextMeshProUGUI roomNameText;
        public TextMeshProUGUI temperatureText;
        public TextMeshProUGUI humidityText;
        public TextMeshProUGUI pm25Text;
        public TextMeshProUGUI pm10Text;

        [Header("设备控制按钮")]
        public Button airConditionerButton;
        public TextMeshProUGUI airConditionerStatusText;
        
        public Button airPurifierButton;
        public TextMeshProUGUI airPurifierStatusText;
        
        public Button fanButton;
        public TextMeshProUGUI fanStatusText;
        
        public Button freshAirButton;
        public TextMeshProUGUI freshAirStatusText;

        [Header("模式切换")]
        public Toggle autoModeToggle;
        public TextMeshProUGUI modeText;

        [Header("更新设置")]
        [Tooltip("更新间隔（秒）")]
        public float updateInterval = 0.5f;

        private float _timer;
        private EnvironmentController envController;

        private void Start()
        {
            envController = EnvironmentController.Instance;
            
            // 绑定按钮事件
            if (airConditionerButton != null)
            {
                airConditionerButton.onClick.AddListener(() => ToggleDevice(NiceHouse.Data.DeviceType.AirConditioner));
            }
            
            if (airPurifierButton != null)
            {
                airPurifierButton.onClick.AddListener(() => ToggleDevice(NiceHouse.Data.DeviceType.AirPurifier));
            }
            
            if (fanButton != null)
            {
                fanButton.onClick.AddListener(() => ToggleDevice(NiceHouse.Data.DeviceType.Fan));
            }
            
            if (freshAirButton != null)
            {
                freshAirButton.onClick.AddListener(() => ToggleDevice(NiceHouse.Data.DeviceType.FreshAirSystem));
            }

            // 绑定模式切换
            if (autoModeToggle != null)
            {
                autoModeToggle.isOn = envController != null && envController.autoMode;
                autoModeToggle.onValueChanged.AddListener(OnModeChanged);
            }

            UpdateAllUI();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;

            UpdateAllUI();
        }

        private void UpdateAllUI()
        {
            UpdateEnvironmentData();
            UpdateDeviceStatus();
            UpdateModeDisplay();
        }

        /// <summary>
        /// 更新环境数据显示
        /// </summary>
        private void UpdateEnvironmentData()
        {
            if (roomNameText != null)
            {
                if (RoomManager.Instance != null && 
                    RoomManager.Instance.TryGetRoom(currentRoomId, out var room))
                {
                    roomNameText.text = $"<size=36><b>{room.displayName}</b></size>";
                }
                else
                {
                    roomNameText.text = $"<size=36><b>{currentRoomId}</b></size>";
                }
            }

            if (EnvironmentDataStore.Instance != null &&
                EnvironmentDataStore.Instance.TryGetRoomData(currentRoomId, out var env))
            {
                // 温度显示（带颜色编码）
                if (temperatureText != null)
                {
                    string color = GetTemperatureColor(env.temperature);
                    temperatureText.text = $"<size=30><color=#CCCCCC>Temperature:</color> <color={color}><b>{env.temperature:F1}</b></color><color=#CCCCCC>°C</color></size>";
                }

                // 湿度显示
                if (humidityText != null)
                {
                    humidityText.text = $"<size=30><color=#CCCCCC>Humidity:</color> <b>{env.humidity:F1}</b><color=#CCCCCC>%</color></size>";
                }

                // PM2.5显示（带颜色编码）
                if (pm25Text != null)
                {
                    string pmColor = GetPM25Color(env.pm25);
                    pm25Text.text = $"<size=30><color=#CCCCCC>PM2.5:</color> <color={pmColor}><b>{env.pm25:F1}</b></color> <color=#CCCCCC>μg/m³</color></size>";
                }

                // PM10显示
                if (pm10Text != null)
                {
                    string pm10Color = GetPM10Color(env.pm10);
                    pm10Text.text = $"<size=30><color=#CCCCCC>PM10:</color> <color={pm10Color}><b>{env.pm10:F1}</b></color> <color=#CCCCCC>μg/m³</color></size>";
                }
            }
            else
            {
                if (temperatureText != null) temperatureText.text = "Temperature: N/A";
                if (humidityText != null) humidityText.text = "Humidity: N/A";
                if (pm25Text != null) pm25Text.text = "PM2.5: N/A";
                if (pm10Text != null) pm10Text.text = "PM10: N/A";
            }
        }

        /// <summary>
        /// 更新设备状态显示
        /// </summary>
        private void UpdateDeviceStatus()
        {
            if (DeviceManager.Instance == null) return;

            var devices = DeviceManager.Instance.GetDevicesInRoom(currentRoomId);
            
            // 空调状态
            UpdateDeviceStatusText(devices, NiceHouse.Data.DeviceType.AirConditioner, airConditionerStatusText);
            
            // 净化器状态
            UpdateDeviceStatusText(devices, NiceHouse.Data.DeviceType.AirPurifier, airPurifierStatusText);
            
            // 风扇状态
            UpdateDeviceStatusText(devices, NiceHouse.Data.DeviceType.Fan, fanStatusText);
            
            // 新风系统状态
            UpdateDeviceStatusText(devices, NiceHouse.Data.DeviceType.FreshAirSystem, freshAirStatusText);
        }

        /// <summary>
        /// 更新单个设备的状态文本
        /// </summary>
        private void UpdateDeviceStatusText(System.Collections.Generic.IReadOnlyList<DeviceDefinition> devices, 
            NiceHouse.Data.DeviceType deviceType, TextMeshProUGUI statusText)
        {
            if (statusText == null) return;

            BaseDeviceController controller = null;
            foreach (var device in devices)
            {
                if (device.type == deviceType)
                {
                    switch (deviceType)
                    {
                        case NiceHouse.Data.DeviceType.AirConditioner:
                            controller = device.GetComponent<AirConditionerController>();
                            break;
                        case NiceHouse.Data.DeviceType.AirPurifier:
                            controller = device.GetComponent<AirPurifierController>();
                            break;
                        case NiceHouse.Data.DeviceType.Fan:
                            controller = device.GetComponent<FanController>();
                            break;
                        case NiceHouse.Data.DeviceType.FreshAirSystem:
                            controller = device.GetComponent<FreshAirController>();
                            break;
                    }
                    break;
                }
            }

            if (controller != null)
            {
                if (controller.IsOn)
                {
                    statusText.text = "<size=28><color=#00FF00><b>ON</b></color></size>";
                }
                else
                {
                    statusText.text = "<size=28><color=#888888>OFF</color></size>";
                }
            }
            else
            {
                statusText.text = "<size=28><color=#FF0000>N/A</color></size>";
            }
        }

        /// <summary>
        /// 更新模式显示
        /// </summary>
        private void UpdateModeDisplay()
        {
            if (modeText != null && envController != null)
            {
                if (envController.autoMode)
                {
                    modeText.text = "<size=32><color=#00FF00><b>AUTO</b></color></size>";
                }
                else
                {
                    modeText.text = "<size=32><color=#FFAA00><b>MANUAL</b></color></size>";
                }
            }
        }

        /// <summary>
        /// 切换设备开关状态
        /// </summary>
        private void ToggleDevice(NiceHouse.Data.DeviceType deviceType)
        {
            if (envController == null) return;

            // 获取当前设备状态
            bool isOn = IsDeviceOn(deviceType);
            
            // 切换状态
            envController.ManualControlDevice(currentRoomId, deviceType, !isOn);
        }

        /// <summary>
        /// 检查设备是否开启
        /// </summary>
        private bool IsDeviceOn(NiceHouse.Data.DeviceType deviceType)
        {
            if (DeviceManager.Instance == null) return false;

            var devices = DeviceManager.Instance.GetDevicesInRoom(currentRoomId);
            foreach (var device in devices)
            {
                if (device.type == deviceType)
                {
                    BaseDeviceController controller = null;
                    switch (deviceType)
                    {
                        case NiceHouse.Data.DeviceType.AirConditioner:
                            controller = device.GetComponent<AirConditionerController>();
                            break;
                        case NiceHouse.Data.DeviceType.AirPurifier:
                            controller = device.GetComponent<AirPurifierController>();
                            break;
                        case NiceHouse.Data.DeviceType.Fan:
                            controller = device.GetComponent<FanController>();
                            break;
                        case NiceHouse.Data.DeviceType.FreshAirSystem:
                            controller = device.GetComponent<FreshAirController>();
                            break;
                    }
                    
                    return controller != null && controller.IsOn;
                }
            }
            
            return false;
        }

        /// <summary>
        /// 模式切换回调
        /// </summary>
        private void OnModeChanged(bool isAuto)
        {
            if (envController != null)
            {
                envController.SetAutoMode(isAuto);
            }
        }

        /// <summary>
        /// 根据温度获取颜色
        /// </summary>
        private string GetTemperatureColor(float temp)
        {
            if (temp < 18f) return "#4A90E2";      // 蓝色（冷）
            if (temp < 24f) return "#7ED321";       // 绿色（舒适）
            if (temp < 28f) return "#F5A623";       // 橙色（热）
            return "#D0021B";                        // 红色（很热）
        }

        /// <summary>
        /// 根据PM2.5获取颜色
        /// </summary>
        private string GetPM25Color(float pm25)
        {
            if (pm25 < 35f) return "#7ED321";       // 绿色（优）
            if (pm25 < 75f) return "#F5A623";       // 橙色（良）
            return "#D0021B";                        // 红色（超标）
        }

        /// <summary>
        /// 根据PM10获取颜色
        /// </summary>
        private string GetPM10Color(float pm10)
        {
            if (pm10 < 50f) return "#7ED321";       // 绿色（优）
            if (pm10 < 150f) return "#F5A623";      // 橙色（良）
            return "#D0021B";                       // 红色（超标）
        }
    }
}

