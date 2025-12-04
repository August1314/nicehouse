using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 风扇控制器
    /// 挂载在风扇GameObject上
    /// </summary>
    public class FanController : BaseDeviceController
    {
        [Header("动画组件")]
        [Tooltip("扇叶Transform（需手动指定）")]
        public Transform fanBlade;
        
        [Tooltip("旋转速度（度/秒）")]
        public float rotationSpeed = 360f;
        
        private bool isRotating = false;

        /// <summary>
        /// 开启风扇
        /// </summary>
        public override void TurnOn()
        {
            base.TurnOn();
            currentState = DeviceState.Running;
            isRotating = true;
            
            Debug.Log($"[Fan] {deviceDef?.deviceId} turned ON");
        }

        /// <summary>
        /// 关闭风扇
        /// </summary>
        public override void TurnOff()
        {
            base.TurnOff();
            isRotating = false;
            
            Debug.Log($"[Fan] {deviceDef?.deviceId} turned OFF");
        }

        private void Update()
        {
            if (isRotating && fanBlade != null)
            {
                // 扇叶绕Z轴旋转
                fanBlade.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            }
        }
    }
}

