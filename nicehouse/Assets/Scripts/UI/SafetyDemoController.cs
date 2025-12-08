using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.UI
{
    /// <summary>
    /// 烟雾/燃气演示控制器：用于 UI 按钮一键触发/恢复安全数据。
    /// 将方法绑定到 Button OnClick，即可在课堂演示中快速制造告警。
    /// </summary>
    public class SafetyDemoController : MonoBehaviour
    {
        [Header("目标房间")]
        [Tooltip("手动指定的房间ID")]
        public string roomId = "LivingRoom01";

        [Tooltip("若勾选，则优先使用 DataDashboard 的当前房间")]
        public bool useDashboardRoom = true;

        [Tooltip("可选：引用 DataDashboard 以获取当前房间")]
        public DataDashboard dashboard;

        [Header("演示数值")]
        [Tooltip("触发烟雾超标时设置的值")]
        public float smokeHighValue = 85f;

        [Tooltip("触发燃气超标时设置的值")]
        public float gasHighValue = 65f;

        [Tooltip("恢复时设置的安全值")]
        public float safeValue = 0f;

        /// <summary>
        /// 一键烟雾超标。
        /// </summary>
        public void TriggerSmokeHigh()
        {
            string targetRoom = GetTargetRoomId();
            if (SafetyDataStore.Instance != null)
            {
                SafetyDataStore.Instance.SetSmokeLevel(targetRoom, smokeHighValue);
                Debug.Log($"[SafetyDemo] Set smoke to {smokeHighValue} in {targetRoom}");
            }
        }

        /// <summary>
        /// 一键燃气超标。
        /// </summary>
        public void TriggerGasHigh()
        {
            string targetRoom = GetTargetRoomId();
            if (SafetyDataStore.Instance != null)
            {
                SafetyDataStore.Instance.SetGasLevel(targetRoom, gasHighValue);
                Debug.Log($"[SafetyDemo] Set gas to {gasHighValue} in {targetRoom}");
            }
        }

        /// <summary>
        /// 恢复安全值。
        /// </summary>
        public void ResetSafetyValues()
        {
            string targetRoom = GetTargetRoomId();
            if (SafetyDataStore.Instance != null)
            {
                SafetyDataStore.Instance.SetSmokeLevel(targetRoom, safeValue);
                SafetyDataStore.Instance.SetGasLevel(targetRoom, safeValue);
                Debug.Log($"[SafetyDemo] Reset safety values to {safeValue} in {targetRoom}");
            }
        }

        private string GetTargetRoomId()
        {
            if (useDashboardRoom && dashboard != null)
            {
                return dashboard.currentRoomId;
            }

            return string.IsNullOrEmpty(roomId) ? "LivingRoom01" : roomId;
        }
    }
}

