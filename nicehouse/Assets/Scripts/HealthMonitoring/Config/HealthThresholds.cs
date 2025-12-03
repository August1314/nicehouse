using UnityEngine;

namespace NiceHouse.HealthMonitoring
{
    /// <summary>
    /// 健康阈值配置
    /// ScriptableObject，用于配置健康监测的阈值参数
    /// </summary>
    [CreateAssetMenu(fileName = "HealthThresholds", menuName = "NiceHouse/Health Monitoring/Health Thresholds")]
    public class HealthThresholds : ScriptableObject
    {
        [Header("心率阈值（bpm）")]
        [Tooltip("心率最小值")]
        public int heartRateMin = 60;

        [Tooltip("心率最大值")]
        public int heartRateMax = 100;

        [Header("呼吸率阈值（次/分钟）")]
        [Tooltip("呼吸率最小值")]
        public int respirationRateMin = 12;

        [Tooltip("呼吸率最大值")]
        public int respirationRateMax = 20;

        [Header("体动阈值")]
        [Tooltip("体动强度最小值")]
        [Range(0f, 1f)]
        public float bodyMovementMin = 0.1f;

        [Header("异常持续时间阈值（秒）")]
        [Tooltip("心率/呼吸率异常持续时间阈值")]
        public float abnormalDurationThreshold = 30f;

        [Tooltip("无体动持续时间阈值（秒），默认30分钟")]
        public float noMovementDurationThreshold = 1800f; // 30分钟

        [Header("告警设置")]
        [Tooltip("告警冷却时间（秒）")]
        public float alarmCooldown = 60f;

        /// <summary>
        /// 应用配置到 HealthMonitoringController
        /// </summary>
        public void ApplyToController(HealthMonitoringController controller)
        {
            if (controller == null) return;

            controller.heartRateMin = heartRateMin;
            controller.heartRateMax = heartRateMax;
            controller.respirationRateMin = respirationRateMin;
            controller.respirationRateMax = respirationRateMax;
            controller.bodyMovementMin = bodyMovementMin;
            controller.abnormalDurationThreshold = abnormalDurationThreshold;
            controller.noMovementDurationThreshold = noMovementDurationThreshold;
            controller.alarmCooldown = alarmCooldown;
        }
    }
}

