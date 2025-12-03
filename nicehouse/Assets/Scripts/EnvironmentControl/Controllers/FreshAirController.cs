using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 新风系统控制器
    /// 挂载在新风系统GameObject上
    /// </summary>
    public class FreshAirController : BaseDeviceController
    {
        [Header("状态指示")]
        [Tooltip("运行状态指示器（可选，例如灯光或粒子特效）")]
        public GameObject statusIndicator;

        /// <summary>
        /// 开启新风系统
        /// </summary>
        public override void TurnOn()
        {
            base.TurnOn();
            currentState = DeviceState.Running;
            
            if (statusIndicator != null)
            {
                statusIndicator.SetActive(true);
            }
            
            Debug.Log($"[FreshAir] {deviceDef?.deviceId} turned ON");
        }

        /// <summary>
        /// 关闭新风系统
        /// </summary>
        public override void TurnOff()
        {
            base.TurnOff();
            
            if (statusIndicator != null)
            {
                statusIndicator.SetActive(false);
            }
            
            Debug.Log($"[FreshAir] {deviceDef?.deviceId} turned OFF");
        }
    }
}

