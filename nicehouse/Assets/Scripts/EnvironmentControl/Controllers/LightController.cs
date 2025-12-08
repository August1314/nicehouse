using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 灯光控制器
    /// 挂载在灯光GameObject上，控制灯光的开关
    /// </summary>
    public class LightController : BaseDeviceController
    {
        [Header("灯光组件")]
        [Tooltip("要控制的Light组件（如不指定则自动查找子物体中的Light）")]
        public Light targetLight;
        
        [Tooltip("灯光开启时的强度")]
        public float onIntensity = 1f;
        
        [Tooltip("灯光关闭时的强度（通常为0）")]
        public float offIntensity = 0f;

        [Header("发光材质（可选）")]
        [Tooltip("灯罩或灯泡的Renderer，用于切换发光效果")]
        public Renderer emissiveRenderer;
        
        [Tooltip("发光材质的Emission颜色属性名")]
        public string emissionColorProperty = "_EmissionColor";
        
        [Tooltip("开灯时的发光颜色")]
        public Color emissionOnColor = Color.white * 2f;
        
        [Tooltip("关灯时的发光颜色")]
        public Color emissionOffColor = Color.black;

        [Header("状态")]
        [SerializeField]
        private bool isLightOn = false;

        protected override void Awake()
        {
            base.Awake();
            
            // 自动查找Light组件
            if (targetLight == null)
            {
                targetLight = GetComponentInChildren<Light>();
            }
            
            // 初始化灯光状态
            UpdateLightVisual();
        }

        /// <summary>
        /// 开灯
        /// </summary>
        public override void TurnOn()
        {
            if (isLightOn) return;
            
            base.TurnOn();
            isLightOn = true;
            currentState = DeviceState.On;
            
            UpdateLightVisual();
            
            Debug.Log($"[Light] {deviceDef?.deviceId} turned ON");
        }

        /// <summary>
        /// 关灯
        /// </summary>
        public override void TurnOff()
        {
            if (!isLightOn) return;
            
            base.TurnOff();
            isLightOn = false;
            currentState = DeviceState.Off;
            
            UpdateLightVisual();
            
            Debug.Log($"[Light] {deviceDef?.deviceId} turned OFF");
        }

        /// <summary>
        /// 切换灯光状态
        /// </summary>
        public void Toggle()
        {
            if (isLightOn)
            {
                TurnOff();
            }
            else
            {
                TurnOn();
            }
        }

        /// <summary>
        /// 更新灯光视觉效果
        /// </summary>
        private void UpdateLightVisual()
        {
            // 更新Light组件
            if (targetLight != null)
            {
                targetLight.intensity = isLightOn ? onIntensity : offIntensity;
                targetLight.enabled = isLightOn;
            }
            
            // 更新发光材质
            if (emissiveRenderer != null)
            {
                var material = emissiveRenderer.material;
                if (material != null)
                {
                    Color emissionColor = isLightOn ? emissionOnColor : emissionOffColor;
                    material.SetColor(emissionColorProperty, emissionColor);
                    
                    // 确保启用Emission
                    if (isLightOn)
                    {
                        material.EnableKeyword("_EMISSION");
                    }
                    else
                    {
                        material.DisableKeyword("_EMISSION");
                    }
                }
            }
        }

        /// <summary>
        /// 设置灯光强度（开灯状态下）
        /// </summary>
        public void SetIntensity(float intensity)
        {
            onIntensity = intensity;
            if (isLightOn && targetLight != null)
            {
                targetLight.intensity = intensity;
            }
        }

        /// <summary>
        /// 获取当前灯光是否开启
        /// </summary>
        public bool IsLightOn => isLightOn;
    }
}
