using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 报警灯控制器
    /// 挂载在报警灯GameObject上，用于在UI面板中控制报警灯
    /// 与 FlashingLight 脚本协同工作
    /// </summary>
    public class AlarmLightController : BaseDeviceController
    {
        [Header("报警灯脚本")]
        [Tooltip("要控制的 FlashingLight 脚本（如不指定则自动查找）")]
        public FlashingLight flashingLight;

        protected override void Awake()
        {
            base.Awake();
            
            // 自动查找 FlashingLight 组件
            if (flashingLight == null)
            {
                flashingLight = GetComponent<FlashingLight>();
                if (flashingLight == null)
                {
                    flashingLight = GetComponentInParent<FlashingLight>();
                }
                if (flashingLight == null)
                {
                    flashingLight = GetComponentInChildren<FlashingLight>();
                }
            }

            if (flashingLight == null)
            {
                Debug.LogWarning($"[AlarmLightController] {gameObject.name} 没有找到 FlashingLight 组件");
            }
        }

        /// <summary>
        /// 开启报警灯（触发闪烁）
        /// </summary>
        public override void TurnOn()
        {
            if (currentState == DeviceState.On || currentState == DeviceState.Running)
            {
                return;
            }

            base.TurnOn();
            currentState = DeviceState.On;

            // 触发 FlashingLight 闪烁
            if (flashingLight != null)
            {
                flashingLight.TriggerFlash(force: true);
            }

            Debug.Log($"[AlarmLightController] {deviceDef?.deviceId} turned ON (flashing)");
        }

        /// <summary>
        /// 关闭报警灯（停止闪烁）
        /// </summary>
        public override void TurnOff()
        {
            if (currentState == DeviceState.Off)
            {
                return;
            }

            base.TurnOff();
            currentState = DeviceState.Off;

            // 停止 FlashingLight 闪烁
            if (flashingLight != null)
            {
                // 如果正在手动闪烁，再次调用 TriggerFlash 会停止
                if (flashingLight.IsManuallyFlashing)
                {
                    flashingLight.TriggerFlash(force: true);
                }
                else
                {
                    // 如果没有手动闪烁，但可能被告警系统控制，停止告警系统的闪烁
                    if (NiceHouse.SmartMonitoring.AlarmResponseHelper.Instance != null)
                    {
                        NiceHouse.SmartMonitoring.AlarmResponseHelper.Instance.StopGlobalAlarmLight();
                    }
                }
            }

            Debug.Log($"[AlarmLightController] {deviceDef?.deviceId} turned OFF (stopped flashing)");
        }

        /// <summary>
        /// 更新状态以反映实际闪烁状态
        /// </summary>
        private void Update()
        {
            // 更新当前状态以反映实际闪烁状态
            bool isActuallyOn = IsAlarmLightFlashing();
            
            if (isActuallyOn && (currentState == DeviceState.Off))
            {
                // 如果实际在闪烁但状态显示为关闭，更新状态（但不触发 TurnOn，避免循环）
                currentState = DeviceState.On;
            }
            else if (!isActuallyOn && (currentState == DeviceState.On || currentState == DeviceState.Running))
            {
                // 如果实际已停止但状态显示为开启，更新状态（但不触发 TurnOff，避免循环）
                currentState = DeviceState.Off;
            }
        }

        /// <summary>
        /// 检查报警灯是否正在闪烁
        /// </summary>
        private bool IsAlarmLightFlashing()
        {
            // 检查是否正在手动闪烁
            if (flashingLight != null && flashingLight.IsManuallyFlashing)
            {
                return true;
            }

            // 检查是否被告警系统控制
            if (AlarmManager.Instance != null)
            {
                var unhandledAlarms = AlarmManager.Instance.GetUnhandledAlarms();
                foreach (var alarm in unhandledAlarms)
                {
                    if (alarm.type == AlarmType.Smoke ||
                        alarm.type == AlarmType.GasLeak ||
                        alarm.type == AlarmType.TemperatureHigh ||
                        alarm.type == AlarmType.TemperatureLow ||
                        alarm.type == AlarmType.Fall ||
                        alarm.type == AlarmType.EmergencyCall)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

