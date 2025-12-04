using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 设备状态枚举
    /// </summary>
    public enum DeviceState
    {
        Off,        // 关闭
        On,         // 开启
        Running,    // 运行中
        Error       // 故障
    }

    /// <summary>
    /// 设备控制器基类
    /// 所有设备控制器都应继承此类
    /// </summary>
    [RequireComponent(typeof(DeviceDefinition))]
    public abstract class BaseDeviceController : MonoBehaviour
    {
        protected DeviceDefinition deviceDef;
        protected DeviceState currentState = DeviceState.Off;

        protected virtual void Awake()
        {
            deviceDef = GetComponent<DeviceDefinition>();
            if (deviceDef == null)
            {
                Debug.LogError($"[{GetType().Name}] DeviceDefinition component not found on {gameObject.name}");
            }
        }

        /// <summary>
        /// 开启设备
        /// </summary>
        public virtual void TurnOn()
        {
            if (currentState == DeviceState.On || currentState == DeviceState.Running)
            {
                return;
            }

            currentState = DeviceState.On;
            
            if (deviceDef != null && !string.IsNullOrEmpty(deviceDef.deviceId))
            {
                EnergyManager.Instance.StartConsume(deviceDef.deviceId);
            }
        }

        /// <summary>
        /// 关闭设备
        /// </summary>
        public virtual void TurnOff()
        {
            if (currentState == DeviceState.Off)
            {
                return;
            }

            currentState = DeviceState.Off;
            
            if (deviceDef != null && !string.IsNullOrEmpty(deviceDef.deviceId))
            {
                EnergyManager.Instance.StopConsume(deviceDef.deviceId);
            }
        }

        /// <summary>
        /// 设备是否开启
        /// </summary>
        public bool IsOn => currentState == DeviceState.On || currentState == DeviceState.Running;

        /// <summary>
        /// 当前设备状态
        /// </summary>
        public DeviceState State => currentState;
    }
}

