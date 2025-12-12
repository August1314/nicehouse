using UnityEngine;
using System.Collections;
using NiceHouse.Data;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 环境智控核心控制器
    /// 负责环境数据监测、阈值判断、自动联动
    /// </summary>
    public class EnvironmentController : MonoBehaviour
    {
        public static EnvironmentController Instance { get; private set; }
        
        [Header("配置")]
        [Tooltip("环境阈值配置（ScriptableObject）")]
        public EnvironmentThresholds thresholds;
        
        [Header("控制参数")]
        [Tooltip("环境数据检查间隔（秒）")]
        public float checkInterval = 1f;
        
        [Tooltip("自动模式（开启后会自动联动设备）")]
        public bool autoMode = true;

        [Header("空气净化效果")]
        [Tooltip("单台净化器的PM2.5净化速率（μg/m³ 每分钟）")]
        public float pm25CleanRatePerMinute = 480f;
        
        [Tooltip("净化后的最低PM2.5值（避免降到0以下）")]
        public float pm25Floor = 5f;

        [Tooltip("单扇开启窗户的PM2.5换气净化速率（μg/m³ 每分钟）")]
        public float pm25VentRatePerWindowPerMinute = 20f;
        
        [Header("调试")]
        [Tooltip("是否输出调试日志")]
        public bool enableDebugLog = false;

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
            if (thresholds == null)
            {
                Debug.LogWarning("[EnvironmentController] EnvironmentThresholds not assigned! Please create and assign a threshold configuration.");
            }
            
            // 延迟启动，确保所有 Manager 都已初始化
            StartCoroutine(DelayedStartMonitoring());
        }

        private System.Collections.IEnumerator DelayedStartMonitoring()
        {
            // 等待一帧，确保所有 Awake 和 Start 都已执行
            yield return null;

            if (RoomManager.Instance == null)
            {
                Debug.LogError("[EnvironmentController] RoomManager.Instance is null after initialization!");
            }
            else
            {
                Debug.Log("[EnvironmentController] RoomManager.Instance is ready");
            }

            StartCoroutine(MonitorEnvironment());
        }

        /// <summary>
        /// 持续监测环境数据
        /// </summary>
        private IEnumerator MonitorEnvironment()
        {
            while (true)
            {
                if (autoMode && thresholds != null)
                {
                    CheckAllRooms();
                }
                yield return new WaitForSeconds(checkInterval);
            }
        }

        /// <summary>
        /// 检查所有房间的环境数据
        /// </summary>
        private void CheckAllRooms()
        {
            if (RoomManager.Instance == null)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning("[EnvironmentController] RoomManager.Instance is null");
                }
                return;
            }

            var allRooms = RoomManager.Instance.GetAllRooms();
            foreach (var room in allRooms.Values)
            {
                CheckRoomEnvironment(room.roomId);
            }
        }

        /// <summary>
        /// 检查单个房间的环境数据并触发联动
        /// </summary>
        private void CheckRoomEnvironment(string roomId)
        {
            if (EnvironmentDataStore.Instance == null)
            {
                return;
            }

            if (!EnvironmentDataStore.Instance.TryGetRoomData(roomId, out var env))
            {
                return;
            }

            // PM2.5超标检查
            if (env.pm25 > thresholds.pm25Threshold)
            {
                TriggerAirPurification(roomId);
            }

            // PM10超标检查
            if (env.pm10 > thresholds.pm10Threshold)
            {
                TriggerAirPurification(roomId);
            }

            // 温度检查
            if (env.temperature > thresholds.temperatureHighThreshold)
            {
                TriggerCooling(roomId);
                TriggerTemperatureAlarm(roomId, true, env.temperature); // 触发高温告警
            }
            else if (env.temperature < thresholds.temperatureLowThreshold)
            {
                TriggerHeating(roomId);
                TriggerTemperatureAlarm(roomId, false, env.temperature); // 触发低温告警
            }

            // 湿度检查
            if (env.humidity > thresholds.humidityHighThreshold || 
                env.humidity < thresholds.humidityLowThreshold)
            {
                TriggerHumidityControl(roomId);
            }

            ApplyAirPurifierEffect(roomId, env);
        }

        /// <summary>
        /// 触发空气净化联动
        /// </summary>
        private void TriggerAirPurification(string roomId)
        {
            if (DeviceManager.Instance == null)
            {
                return;
            }

            var devices = DeviceManager.Instance.GetDevicesInRoom(roomId);
            bool purifierActivated = false;
            bool freshAirActivated = false;

            foreach (var device in devices)
            {
                if (device.type == NiceHouse.Data.DeviceType.AirPurifier)
                {
                    var controller = device.GetComponent<AirPurifierController>();
                    if (controller != null && !controller.IsOn)
                    {
                        controller.TurnOn();
                        purifierActivated = true;
                        if (enableDebugLog)
                        {
                            float pm25Value = EnvironmentDataStore.Instance.TryGetRoomData(roomId, out var e) ? e.pm25 : 0f;
                            Debug.Log($"[EnvironmentController] Auto-activated AirPurifier in {roomId} (PM2.5: {pm25Value})");
                        }
                    }
                }
                else if (device.type == NiceHouse.Data.DeviceType.FreshAirSystem)
                {
                    var controller = device.GetComponent<FreshAirController>();
                    if (controller != null && !controller.IsOn)
                    {
                        controller.TurnOn();
                        freshAirActivated = true;
                        if (enableDebugLog)
                        {
                            Debug.Log($"[EnvironmentController] Auto-activated FreshAirSystem in {roomId}");
                        }
                    }
                }
            }

            // 记录告警（如果设备被激活）
            if (purifierActivated || freshAirActivated)
            {
                if (AlarmManager.Instance != null)
                {
                    AlarmManager.Instance.AddAlarm(AlarmType.Smoke, roomId);
                }
            }
        }

        /// <summary>
        /// 当房间存在运行中的净化器/开启窗户时，对PM2.5施加衰减模型
        /// </summary>
        private void ApplyAirPurifierEffect(string roomId, RoomEnvironmentData env)
        {
            if (DeviceManager.Instance == null || env == null)
            {
                return;
            }

            var devices = DeviceManager.Instance.GetDevicesInRoom(roomId);
            int activePurifierCount = 0;
            int openWindowCount = 0;

            foreach (var device in devices)
            {
                if (device.type == NiceHouse.Data.DeviceType.AirPurifier)
                {
                    var controller = device.GetComponent<AirPurifierController>();
                    if (controller != null && controller.IsOn)
                    {
                        activePurifierCount++;
                    }
                }
                else if (device.type == NiceHouse.Data.DeviceType.Window)
                {
                    var window = device.GetComponent<WindowController>();
                    if (window != null && window.IsOn)
                    {
                        openWindowCount++;
                    }
                }
            }

            if (activePurifierCount == 0 && openWindowCount == 0)
            {
                return;
            }

            // 线性衰减模型：净化器 + 开窗通风叠加
            float purifierRatePerSecond = pm25CleanRatePerMinute / 60f;
            float ventRatePerSecond = pm25VentRatePerWindowPerMinute / 60f;

            float delta = (purifierRatePerSecond * activePurifierCount +
                           ventRatePerSecond * openWindowCount) * checkInterval;

            env.pm25 = Mathf.Max(pm25Floor, env.pm25 - delta);
            // 简单保持 PM10 与 PM2.5 的比例
            env.pm10 = env.pm25 * 1.2f;
        }

        /// <summary>
        /// 触发制冷联动
        /// </summary>
        private void TriggerCooling(string roomId)
        {
            if (DeviceManager.Instance == null)
            {
                return;
            }

            var devices = DeviceManager.Instance.GetDevicesInRoom(roomId);
            foreach (var device in devices)
            {
                if (device.type == NiceHouse.Data.DeviceType.AirConditioner)
                {
                    var controller = device.GetComponent<AirConditionerController>();
                    if (controller != null && !controller.IsOn)
                    {
                        controller.TurnOn();
                        controller.SetTargetTemperature(thresholds.targetTemperature);
                        if (enableDebugLog)
                        {
                            Debug.Log($"[EnvironmentController] Auto-activated AirConditioner (Cooling) in {roomId}, target: {thresholds.targetTemperature}°C");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 触发制热联动
        /// </summary>
        private void TriggerHeating(string roomId)
        {
            if (DeviceManager.Instance == null)
            {
                return;
            }

            var devices = DeviceManager.Instance.GetDevicesInRoom(roomId);
            foreach (var device in devices)
            {
                if (device.type == NiceHouse.Data.DeviceType.AirConditioner)
                {
                    var controller = device.GetComponent<AirConditionerController>();
                    if (controller != null && !controller.IsOn)
                    {
                        controller.TurnOn();
                        controller.SetTargetTemperature(22f);  // 制热目标温度
                        if (enableDebugLog)
                        {
                            Debug.Log($"[EnvironmentController] Auto-activated AirConditioner (Heating) in {roomId}, target: 22°C");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 触发温度告警
        /// </summary>
        private void TriggerTemperatureAlarm(string roomId, bool isHigh, float temperature)
        {
            if (AlarmManager.Instance == null)
            {
                return;
            }

            // 使用冷却机制避免频繁告警
            string alarmKey = $"Temperature{(isHigh ? "High" : "Low")}:{roomId}";
            float cooldown = 60f; // 60秒冷却时间
            float now = Time.time;
            
            // 检查冷却时间（使用静态字典存储，避免每次创建新实例）
            if (!_temperatureAlarmCooldown.TryGetValue(alarmKey, out var lastTime) || (now - lastTime) >= cooldown)
            {
                AlarmType alarmType = isHigh ? AlarmType.TemperatureHigh : AlarmType.TemperatureLow;
                AlarmManager.Instance.AddAlarm(alarmType, roomId);
                _temperatureAlarmCooldown[alarmKey] = now;
                
                if (enableDebugLog)
                {
                    string tempStatus = isHigh ? "high" : "low";
                    Debug.Log($"[EnvironmentController] Temperature {tempStatus} alarm triggered in {roomId} (temperature: {temperature:F1}°C)");
                }
            }
        }

        // 温度告警冷却字典
        private static readonly System.Collections.Generic.Dictionary<string, float> _temperatureAlarmCooldown = 
            new System.Collections.Generic.Dictionary<string, float>();

        /// <summary>
        /// 触发湿度控制联动
        /// </summary>
        private void TriggerHumidityControl(string roomId)
        {
            if (DeviceManager.Instance == null)
            {
                return;
            }

            if (!EnvironmentDataStore.Instance.TryGetRoomData(roomId, out var env))
            {
                return;
            }

            // 根据实际设备类型实现
            // 例如：开启加湿器、除湿器等
            // 这里暂时只记录日志
            if (enableDebugLog)
            {
                Debug.Log($"[EnvironmentController] Humidity control needed in {roomId} (current: {env.humidity}%)");
            }
        }

        /// <summary>
        /// 切换自动/手动模式
        /// </summary>
        public void SetAutoMode(bool enabled)
        {
            autoMode = enabled;
            Debug.Log($"[EnvironmentController] Auto mode {(enabled ? "ENABLED" : "DISABLED")}");
        }

        /// <summary>
        /// 手动触发设备控制（用于UI按钮）
        /// </summary>
        public void ManualControlDevice(string roomId, NiceHouse.Data.DeviceType deviceType, bool turnOn)
        {
            if (DeviceManager.Instance == null)
            {
                return;
            }

            var devices = DeviceManager.Instance.GetDevicesInRoom(roomId);
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
                        case NiceHouse.Data.DeviceType.Window:
                            controller = device.GetComponent<WindowController>() 
                                         ?? device.GetComponentInChildren<WindowController>(includeInactive: true)
                                         ?? device.GetComponentInParent<WindowController>();
                            break;
                        case NiceHouse.Data.DeviceType.Curtain:
                            controller = device.GetComponent<CurtainController>()
                                         ?? device.GetComponentInChildren<CurtainController>(includeInactive: true)
                                         ?? device.GetComponentInParent<CurtainController>();
                            break;
                        case NiceHouse.Data.DeviceType.Light:
                            // 优先查找 AlarmLightController，如果没有则查找 LightController
                            controller = device.GetComponent<AlarmLightController>();
                            if (controller == null)
                            {
                            controller = device.GetComponent<LightController>();
                            }
                            break;
                        case NiceHouse.Data.DeviceType.SmokeSensor:
                            controller = device.GetComponent<SmokeDetectorController>();
                            break;
                        case NiceHouse.Data.DeviceType.HelpButton:
                            controller = device.GetComponent<HelpButtonController>();
                            break;
                    }

                    if (controller != null)
                    {
                        if (turnOn)
                        {
                            controller.TurnOn();
                        }
                        else
                        {
                            controller.TurnOff();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 通过设备ID直接控制设备（支持所有设备类型）
        /// </summary>
        public void ManualControlDeviceById(string deviceId, bool turnOn)
        {
            if (DeviceManager.Instance == null)
            {
                return;
            }

            if (!DeviceManager.Instance.TryGetDevice(deviceId, out var device))
            {
                Debug.LogWarning($"[EnvironmentController] Device not found: {deviceId}");
                return;
            }

            var controller = device.GetComponent<BaseDeviceController>();
            if (controller != null)
            {
                if (turnOn)
                {
                    controller.TurnOn();
                }
                else
                {
                    controller.TurnOff();
                }
            }
            else
            {
                Debug.LogWarning($"[EnvironmentController] Device {deviceId} does not have a BaseDeviceController");
            }
        }
    }
}

