using UnityEngine;

namespace NiceHouse.Data
{
    /// <summary>
    /// 安全数据阈值配置（烟雾/燃气报警）。
    /// </summary>
    [CreateAssetMenu(menuName = "NiceHouse/SafetyDataThresholds", fileName = "SafetyDataThresholds")]
    public class SafetyDataThresholds : ScriptableObject
    {
        [Header("阈值设置")]
        [Tooltip("烟雾浓度报警阈值（0-100）")]
        public float smokeAlarmThreshold = 70f;

        [Tooltip("燃气浓度报警阈值（0-100）")]
        public float gasAlarmThreshold = 40f;

        [Header("抖动缓冲")]
        [Tooltip("报警阈值下的回落缓冲，用于避免频繁进出阈值抖动")]
        public float hysteresis = 3f;

        /// <summary>
        /// 获取烟雾触发阈值。
        /// </summary>
        public float GetSmokeThreshold() => Mathf.Clamp(smokeAlarmThreshold, 0f, 100f);

        /// <summary>
        /// 获取燃气触发阈值。
        /// </summary>
        public float GetGasThreshold() => Mathf.Clamp(gasAlarmThreshold, 0f, 100f);
    }
}

