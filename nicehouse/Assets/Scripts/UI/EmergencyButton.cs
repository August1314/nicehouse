using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.UI
{
    /// <summary>
    /// 3D/2D 交互紧急按钮：调用紧急告警。
    /// 可通过 UI Button OnClick、OnMouseDown、触发器等方式调用 TriggerEmergency。
    /// </summary>
    public class EmergencyButton : MonoBehaviour
    {
        [Tooltip("告警使用的房间ID；留空则使用 Unknown")]
        public string roomId = "LivingRoom01";

        [Tooltip("是否尝试使用 DataDashboard 的当前房间")]
        public bool useDashboardRoom = true;

        [Tooltip("可选：引用 DataDashboard，用于获取 currentRoomId")]
        public DataDashboard dashboard;

        /// <summary>
        /// 对外触发紧急告警（供 OnClick / OnMouseDown / OnTriggerEnter 等调用）。
        /// </summary>
        public void TriggerEmergency()
        {
            string targetRoom = GetTargetRoomId();

            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.AddAlarm(AlarmType.EmergencyCall, targetRoom);
                Debug.Log($"[EmergencyButton] EmergencyCall in {targetRoom}");
            }
            else
            {
                Debug.LogWarning("[EmergencyButton] AlarmManager.Instance is null, alarm skipped.");
            }
        }

        // 示例：在 3D 物体上可直接点击触发（需要 Collider 可被射线命中）
        private void OnMouseDown()
        {
            TriggerEmergency();
        }

        private string GetTargetRoomId()
        {
            if (useDashboardRoom && dashboard != null)
            {
                if (!string.IsNullOrEmpty(dashboard.currentRoomId))
                {
                    return dashboard.currentRoomId;
                }
            }

            return string.IsNullOrEmpty(roomId) ? "Unknown" : roomId;
        }
    }
}

