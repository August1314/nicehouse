using UnityEngine;
using NiceHouse.Data;
using System.Linq;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 求助按钮控制器
    /// 挂载在求助按钮GameObject上，用于在UI面板中控制求助按钮
    /// 与 InteractableHelpButton 脚本协同工作
    /// </summary>
    public class HelpButtonController : BaseDeviceController
    {
        [Header("求助按钮脚本")]
        [Tooltip("要控制的 InteractableHelpButton 脚本（如不指定则自动查找）")]
        public NiceHouse.Interaction.InteractableHelpButton interactableHelpButton;

        [Header("报警灯设置")]
        [Tooltip("要触发的报警灯（留空则自动查找全局报警灯）")]
        public FlashingLight targetAlarmLight;

        protected override void Awake()
        {
            base.Awake();
            
            // 自动查找 InteractableHelpButton 组件
            if (interactableHelpButton == null)
            {
                interactableHelpButton = GetComponent<NiceHouse.Interaction.InteractableHelpButton>();
                if (interactableHelpButton == null)
                {
                    interactableHelpButton = GetComponentInParent<NiceHouse.Interaction.InteractableHelpButton>();
                }
                if (interactableHelpButton == null)
                {
                    interactableHelpButton = GetComponentInChildren<NiceHouse.Interaction.InteractableHelpButton>();
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
        /// 开启求助按钮（触发报警）
        /// </summary>
        public override void TurnOn()
        {
            if (currentState == DeviceState.On || currentState == DeviceState.Running)
            {
                return;
            }

            base.TurnOn();
            currentState = DeviceState.On;

            // 触发紧急呼叫告警
            TriggerEmergencyAlarm();

            // 触发报警灯闪烁
            if (targetAlarmLight != null)
            {
                targetAlarmLight.TriggerFlash(force: true);
            }

            Debug.Log($"[HelpButtonController] {deviceDef?.deviceId} turned ON (alarm triggered)");
        }

        /// <summary>
        /// 关闭求助按钮（停止报警）
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
                    if (alarm.type == AlarmType.EmergencyCall ||
                        alarm.type == AlarmType.Smoke ||
                        alarm.type == AlarmType.GasLeak ||
                        alarm.type == AlarmType.TemperatureHigh ||
                        alarm.type == AlarmType.TemperatureLow ||
                        alarm.type == AlarmType.Fall)
                    {
                        AlarmManager.Instance.MarkHandled(alarm);
                    }
                }
            }

            Debug.Log($"[HelpButtonController] {deviceDef?.deviceId} turned OFF (alarm stopped)");
        }

        /// <summary>
        /// 触发紧急呼叫告警
        /// </summary>
        private void TriggerEmergencyAlarm()
        {
            string targetRoomId = deviceDef != null && !string.IsNullOrEmpty(deviceDef.roomId) 
                ? deviceDef.roomId 
                : "Unknown";

            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.AddAlarm(AlarmType.EmergencyCall, targetRoomId);
            }
        }

        /// <summary>
        /// 更新状态以反映实际报警状态
        /// </summary>
        private void Update()
        {
            // 检查是否有未处理的紧急呼叫告警
            bool hasActiveAlarm = IsHelpButtonActive();

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

        /// <summary>
        /// 检查求助按钮是否处于激活状态
        /// </summary>
        private bool IsHelpButtonActive()
        {
            // 检查是否正在手动闪烁
            if (targetAlarmLight != null && targetAlarmLight.IsManuallyFlashing)
            {
                return true;
            }

            // 检查是否有未处理的紧急呼叫告警
            if (AlarmManager.Instance != null)
            {
                var unhandledAlarms = AlarmManager.Instance.GetUnhandledAlarms();
                foreach (var alarm in unhandledAlarms)
                {
                    if (alarm.type == AlarmType.EmergencyCall)
                    {
                        // 检查是否是当前按钮的告警（通过房间ID匹配）
                        if (deviceDef != null && 
                            !string.IsNullOrEmpty(deviceDef.roomId) &&
                            string.Equals(alarm.roomId, deviceDef.roomId, System.StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 重写 IsOn 属性以反映实际状态
        /// </summary>
        public override bool IsOn => IsHelpButtonActive();
    }
}

