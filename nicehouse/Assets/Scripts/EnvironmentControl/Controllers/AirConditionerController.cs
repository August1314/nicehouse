using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NiceHouse.Data;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 空调控制器
    /// 挂载在空调GameObject上
    /// </summary>
    public class AirConditionerController : BaseDeviceController
    {
        public enum SweepAxis
        {
            Yaw,   // 左右摆
            Pitch, // 上下翻
            Roll   // 前后翻
        }

        [Header("动画组件")]
        [Tooltip("导风板Transform（需手动指定）")]
        public Transform ventBlade;
        
        [Tooltip("导风板组（支持多个扇叶/导风板）")]
        public List<Transform> ventBlades = new List<Transform>();
        
        [Tooltip("每个导风板的轴向（可选）。为空时使用全局 Sweep Axis。长度与 Vent Blades 对齐。")]
        public List<SweepAxis> ventBladeAxes = new List<SweepAxis>();
        
        [Header("扫风角度设置")]
        [Tooltip("开始角度（度）。如果启用自定义角度，则使用此值")]
        public float startAngle = -30f;
        
        [Tooltip("结束角度（度）。如果启用自定义角度，则使用此值")]
        public float endAngle = 30f;
        
        [Tooltip("是否使用自定义开始/结束角度（true）还是使用对称角度（false）")]
        public bool useCustomAngles = false;
        
        [Tooltip("扫风角度（度）。当 Use Custom Angles 为 false 时使用，表示从 -sweepAngle 到 +sweepAngle")]
        public float sweepAngle = 30f;
        
        [Tooltip("扫风速度（度/秒）")]
        public float sweepSpeed = 60f;
        
        [Tooltip("扫风绕的轴：Yaw=左右摆，Pitch=上下翻，Roll=前后翻")]
        public SweepAxis sweepAxis = SweepAxis.Yaw;
        
        [Header("状态")]
        [Tooltip("目标温度（℃）")]
        public float targetTemperature = 24f;
        
        private float currentAngle = 0f;
        private bool isSweeping = false;
        private Coroutine sweepCoroutine;
        
        /// <summary>
        /// 获取实际使用的开始角度
        /// </summary>
        private float GetStartAngle()
        {
            return useCustomAngles ? startAngle : -sweepAngle;
        }
        
        /// <summary>
        /// 获取实际使用的结束角度
        /// </summary>
        private float GetEndAngle()
        {
            return useCustomAngles ? endAngle : sweepAngle;
        }

        /// <summary>
        /// 开启空调
        /// </summary>
        public override void TurnOn()
        {
            base.TurnOn();
            currentState = DeviceState.Running;
            isSweeping = true;
            
            // 开始扫风动画
            if (ventBlade != null || (ventBlades != null && ventBlades.Count > 0))
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
            float start = GetStartAngle();
            float end = GetEndAngle();
            
            // 初始化角度
            currentAngle = start;
            
            // 根据开始和结束角度的关系确定初始方向
            // 如果 start < end，从 start 向 end 移动（正方向）
            // 如果 start > end，从 start 向 end 移动（负方向）
            float direction = start < end ? 1f : -1f;
            
            while (isSweeping)
            {
                float deltaAngle = direction * sweepSpeed * Time.deltaTime;
                float nextAngle = currentAngle + deltaAngle;
                
                // 检查是否超出范围
                if (start < end)
                {
                    // 正常情况：start < end（例如：-30 到 30）
                    if (direction > 0f && nextAngle >= end)
                    {
                        currentAngle = end;
                        direction = -1f; // 反向
                    }
                    else if (direction < 0f && nextAngle <= start)
                    {
                        currentAngle = start;
                        direction = 1f; // 正向
                    }
                    else
                    {
                        currentAngle = nextAngle;
                    }
                }
                else
                {
                    // 反向情况：start > end（例如：0 到 -60）
                    if (direction < 0f && nextAngle <= end)
                    {
                        currentAngle = end;
                        direction = 1f; // 正向
                    }
                    else if (direction > 0f && nextAngle >= start)
                    {
                        currentAngle = start;
                        direction = -1f; // 反向
                    }
                    else
                    {
                        currentAngle = nextAngle;
                    }
                }
                
                ApplySweepRotation(currentAngle);
                
                yield return null;
            }
        }

        /// <summary>
        /// 将当前角度应用到所有导风板（支持单个或多个）
        /// </summary>
        private void ApplySweepRotation(float angle)
        {
            if (ventBlade != null)
            {
                ventBlade.localRotation = GetSweepRotation(angle, sweepAxis);
            }

            if (ventBlades != null && ventBlades.Count > 0)
            {
                for (int i = 0; i < ventBlades.Count; i++)
                {
                    if (ventBlades[i] != null)
                    {
                        // 如果配置了单独轴向，则使用单独轴向；否则使用全局 sweepAxis
                        var axis = (ventBladeAxes != null && i < ventBladeAxes.Count)
                            ? ventBladeAxes[i]
                            : sweepAxis;
                        ventBlades[i].localRotation = GetSweepRotation(angle, axis);
                    }
                }
            }
        }

        /// <summary>
        /// 根据轴向计算旋转
        /// </summary>
        private Quaternion GetSweepRotation(float angle, SweepAxis axis)
        {
            switch (axis)
            {
                case SweepAxis.Pitch:
                    return Quaternion.Euler(angle, 0, 0);
                case SweepAxis.Roll:
                    return Quaternion.Euler(0, 0, angle);
                default: // Yaw
                    return Quaternion.Euler(0, angle, 0);
            }
        }
    }
}

