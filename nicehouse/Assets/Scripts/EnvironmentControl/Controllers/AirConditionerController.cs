using UnityEngine;
using System.Collections;
using NiceHouse.Data;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 空调控制器
    /// 挂载在空调GameObject上
    /// </summary>
    public class AirConditionerController : BaseDeviceController
    {
        [Header("动画组件")]
        [Tooltip("导风板Transform（需手动指定）")]
        public Transform ventBlade;
        
        [Tooltip("扫风角度（度）")]
        public float sweepAngle = 30f;
        
        [Tooltip("扫风速度（度/秒）")]
        public float sweepSpeed = 60f;
        
        [Header("状态")]
        [Tooltip("目标温度（℃）")]
        public float targetTemperature = 24f;
        
        private float currentAngle = 0f;
        private bool isSweeping = false;
        private Coroutine sweepCoroutine;

        /// <summary>
        /// 开启空调
        /// </summary>
        public override void TurnOn()
        {
            base.TurnOn();
            currentState = DeviceState.Running;
            isSweeping = true;
            
            // 开始扫风动画
            if (ventBlade != null)
            {
                if (sweepCoroutine != null)
                {
                    StopCoroutine(sweepCoroutine);
                }
                sweepCoroutine = StartCoroutine(SweepAnimation());
            }
            
            Debug.Log($"[AirConditioner] {deviceDef?.deviceId} turned ON, target temp: {targetTemperature}°C");
        }

        /// <summary>
        /// 关闭空调
        /// </summary>
        public override void TurnOff()
        {
            base.TurnOff();
            isSweeping = false;
            
            if (sweepCoroutine != null)
            {
                StopCoroutine(sweepCoroutine);
                sweepCoroutine = null;
            }
            
            Debug.Log($"[AirConditioner] {deviceDef?.deviceId} turned OFF");
        }

        /// <summary>
        /// 设置目标温度
        /// </summary>
        public void SetTargetTemperature(float temp)
        {
            targetTemperature = temp;
            Debug.Log($"[AirConditioner] {deviceDef?.deviceId} target temperature set to {temp}°C");
        }

        /// <summary>
        /// 导风板扫风动画
        /// </summary>
        private IEnumerator SweepAnimation()
        {
            float direction = 1f;
            while (isSweeping)
            {
                currentAngle += direction * sweepSpeed * Time.deltaTime;
                
                if (currentAngle >= sweepAngle)
                {
                    currentAngle = sweepAngle;
                    direction = -1f;
                }
                else if (currentAngle <= -sweepAngle)
                {
                    currentAngle = -sweepAngle;
                    direction = 1f;
                }
                
                if (ventBlade != null)
                {
                    ventBlade.localRotation = Quaternion.Euler(0, currentAngle, 0);
                }
                
                yield return null;
            }
        }
    }
}

