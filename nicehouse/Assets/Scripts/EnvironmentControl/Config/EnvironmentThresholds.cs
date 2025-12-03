using UnityEngine;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 环境阈值配置
    /// 在Project窗口右键 Create > NiceHouse > EnvironmentThresholds 创建
    /// </summary>
    [CreateAssetMenu(fileName = "EnvironmentThresholds", menuName = "NiceHouse/EnvironmentThresholds")]
    public class EnvironmentThresholds : ScriptableObject
    {
        [Header("PM2.5/PM10阈值")]
        [Tooltip("PM2.5超标阈值（μg/m³）")]
        public float pm25Threshold = 75f;
        
        [Tooltip("PM10超标阈值（μg/m³）")]
        public float pm10Threshold = 150f;
        
        [Header("温度阈值")]
        [Tooltip("温度过高阈值（℃）")]
        public float temperatureHighThreshold = 28f;
        
        [Tooltip("温度过低阈值（℃）")]
        public float temperatureLowThreshold = 18f;
        
        [Tooltip("自动模式目标温度（℃）")]
        public float targetTemperature = 24f;
        
        [Header("湿度阈值")]
        [Tooltip("湿度过高阈值（%）")]
        public float humidityHighThreshold = 70f;
        
        [Tooltip("湿度过低阈值（%）")]
        public float humidityLowThreshold = 30f;
    }
}

