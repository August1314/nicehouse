using UnityEngine;
using NiceHouse.SmartMonitoring;
using NiceHouse.Data;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 智能监护面板的 Control Hub 适配器，实现 IControlPanelModule 接口。
    /// </summary>
    public class MonitoringPanelAdapter : MonoBehaviour, IControlPanelModule
    {
        [Header("模块配置")]
        [SerializeField] private string moduleId = "Monitoring";
        [SerializeField] private string displayName = "Smart Monitoring";
        [SerializeField] private Sprite icon;
        [SerializeField] private Color accentColor = new Color(0.9f, 0.5f, 0.1f); // 橙色

        [Header("面板引用")]
        [SerializeField] private MonitoringPanel targetPanel;
        [SerializeField] private GameObject panelRoot;

        public string ModuleId => moduleId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public Color AccentColor => accentColor;

        private void Awake()
        {
            if (panelRoot == null && targetPanel != null)
            {
                panelRoot = targetPanel.gameObject;
            }
            
            // 确保 panelRoot 指向的是面板 GameObject，而不是容器
            if (panelRoot != null && targetPanel != null)
            {
                if (panelRoot.name.Contains("Root") || panelRoot.name.Contains("Container") || 
                    panelRoot.name.Contains("Embedded"))
                {
                    Debug.LogWarning($"[MonitoringPanelAdapter] Panel Root appears to be a container ({panelRoot.name}). Using targetPanel GameObject instead.", this);
                    panelRoot = targetPanel.gameObject;
                }
            }
        }

        public ControlHubModuleSnapshot BuildSnapshot()
        {
            // 从 PersonStateController 获取数字人状态
            if (PersonStateController.Instance != null && PersonStateController.Instance.Status != null)
            {
                var status = PersonStateController.Instance.Status;
                string stateName = GetStateName(status.state);
                
                // 检查是否有告警
                bool hasAlert = false;
                float severity = 0f;

                if (AlarmManager.Instance != null)
                {
                    var recentAlarms = AlarmManager.Instance.GetRecentAlarms(5);
                    foreach (var alarm in recentAlarms)
                    {
                        if (!alarm.handled)
                        {
                            hasAlert = true;
                            severity = 0.8f;
                            break;
                        }
                    }
                }

                // 危险状态
                if (status.state == PersonState.Fallen || status.state == PersonState.OutOfBed)
                {
                    hasAlert = true;
                    severity = 1f;
                }

                return new ControlHubModuleSnapshot
                {
                    headline = "Digital Person Status",
                    value = stateName,
                    hint = $"Room: {status.currentRoomId} | Duration: {(int)status.stateDuration}s",
                    severity01 = severity,
                    hasAlert = hasAlert,
                    accentColor = accentColor
                };
            }

            return ControlHubModuleSnapshot.Default(displayName, accentColor);
        }

        public void ShowPanel()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
            else if (targetPanel != null)
            {
                targetPanel.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[MonitoringPanelAdapter] ShowPanel called but both panelRoot and targetPanel are null!", this);
            }
        }

        public void HidePanel()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
            else if (targetPanel != null)
            {
                targetPanel.gameObject.SetActive(false);
            }
        }

        private string GetStateName(PersonState state)
        {
            return state switch
            {
                PersonState.Idle => "Idle",
                PersonState.Walking => "Walking",
                PersonState.Sitting => "Sitting",
                PersonState.Bathing => "Bathing",
                PersonState.Sleeping => "Sleeping",
                PersonState.Fallen => "Fallen",
                PersonState.OutOfBed => "Out of Bed",
                _ => state.ToString()
            };
        }
    }
}

