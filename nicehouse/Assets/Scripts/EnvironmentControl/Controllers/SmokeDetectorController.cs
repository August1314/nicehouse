using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 烟雾探测器控制器
    /// 挂载在烟雾探测器GameObject上，用于在UI面板中控制烟雾探测器
    /// 与 InteractableSmokeDetector 脚本协同工作
    /// </summary>
    public class SmokeDetectorController : BaseDeviceController
    {
        [Header("烟雾探测器脚本")]
        [Tooltip("要控制的 InteractableSmokeDetector 脚本（如不指定则自动查找）")]
        public NiceHouse.Interaction.InteractableSmokeDetector interactableSmokeDetector;

        [Header("报警灯设置")]
        [Tooltip("要触发的报警灯（留空则自动查找全局报警灯）")]
        public FlashingLight targetAlarmLight;

        protected override void Awake()
        {
            base.Awake();
            
            // 自动查找 InteractableSmokeDetector 组件
            if (interactableSmokeDetector == null)
            {
                interactableSmokeDetector = GetComponent<NiceHouse.Interaction.InteractableSmokeDetector>();
                if (interactableSmokeDetector == null)
                {
                    interactableSmokeDetector = GetComponentInParent<NiceHouse.Interaction.InteractableSmokeDetector>();
                }
                if (interactableSmokeDetector == null)
                {
                    interactableSmokeDetector = GetComponentInChildren<NiceHouse.Interaction.InteractableSmokeDetector>();
                }
            }

            // 自动查找报警灯
            if (targetAlarmLight == null)
            {
                var flashingLights = FindObjectsOfType<FlashingLight>();
                foreach (var flashingLight in flashingLights)
                {
                    if (flashingLight.isGlobalAlarmLight)
                    {
                        targetAlarmLight = flashingLight;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 开启烟雾探测器（触发报警）
        /// </summary>
        public override void TurnOn()
        {
            if (currentState == DeviceState.On || currentState == DeviceState.Running)
            {
                return;
            }

            base.TurnOn();
            currentState = DeviceState.On;

            // 触发烟雾告警
            TriggerSmokeAlarm();

            // 触发报警灯闪烁
            if (targetAlarmLight != null)
            {
                targetAlarmLight.TriggerFlash(force: true);
            }

            Debug.Log($"[SmokeDetectorController] {deviceDef?.deviceId} turned ON (alarm triggered)");
        }

        /// <summary>
        /// 关闭烟雾探测器（停止报警）
        /// </summary>
        public override void TurnOff()
        {
            if (currentState == DeviceState.Off)
            {
                return;
            }

            base.TurnOff();
            currentState = DeviceState.Off;

            // 停止报警灯闪烁
            if (targetAlarmLight != null)
            {
                if (targetAlarmLight.IsManuallyFlashing)
                {
                    targetAlarmLight.TriggerFlash(force: true); // 停止手动闪烁
                }
            }

            // 停止告警系统的闪烁协程
            if (NiceHouse.SmartMonitoring.AlarmResponseHelper.Instance != null)
            {
                NiceHouse.SmartMonitoring.AlarmResponseHelper.Instance.StopGlobalAlarmLight();
            }

            // 标记所有相关告警为已处理
            if (AlarmManager.Instance != null)
            {
                var unhandledAlarms = AlarmManager.Instance.GetUnhandledAlarms();
                foreach (var alarm in unhandledAlarms)
                {
                    if (alarm.type == AlarmType.Smoke ||
                        alarm.type == AlarmType.GasLeak ||
                        alarm.type == AlarmType.TemperatureHigh ||
                        alarm.type == AlarmType.TemperatureLow)
                    {
                        AlarmManager.Instance.MarkHandled(alarm);
                    }
                }
            }

            Debug.Log($"[SmokeDetectorController] {deviceDef?.deviceId} turned OFF (alarm stopped)");
        }

        /// <summary>
        /// 触发烟雾告警
        /// </summary>
        private void TriggerSmokeAlarm()
        {
            string targetRoomId = deviceDef != null && !string.IsNullOrEmpty(deviceDef.roomId) 
                ? deviceDef.roomId 
                : "Unknown";

            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.AddAlarm(AlarmType.Smoke, targetRoomId);
            }
        }

        /// <summary>
        /// 更新状态以反映实际报警状态
        /// </summary>
        private void Update()
        {
            // 检查是否有未处理的烟雾告警
            bool hasActiveAlarm = false;
            if (AlarmManager.Instance != null)
            {
                var unhandledAlarms = AlarmManager.Instance.GetUnhandledAlarms();
                foreach (var alarm in unhandledAlarms)
                {
                    if (alarm.type == AlarmType.Smoke)
                    {
                        // 检查是否是当前探测器的告警（通过房间ID匹配）
                        if (deviceDef != null && 
                            !string.IsNullOrEmpty(deviceDef.roomId) &&
                            string.Equals(alarm.roomId, deviceDef.roomId, System.StringComparison.OrdinalIgnoreCase))
                        {
                            hasActiveAlarm = true;
                            break;
                        }
                    }
                }
            }

            // 同步状态
            if (hasActiveAlarm && (currentState == DeviceState.Off))
            {
                currentState = DeviceState.On;
            }
            else if (!hasActiveAlarm && (currentState == DeviceState.On || currentState == DeviceState.Running))
            {
                currentState = DeviceState.Off;
            }
        }
    }
}

