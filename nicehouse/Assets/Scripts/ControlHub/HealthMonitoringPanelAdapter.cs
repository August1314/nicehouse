using UnityEngine;
using NiceHouse.HealthMonitoring;
using NiceHouse.Data;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 健康监测面板的 Control Hub 适配器，实现 IControlPanelModule 接口。
    /// </summary>
    public class HealthMonitoringPanelAdapter : MonoBehaviour, IControlPanelModule
    {
        [Header("模块配置")]
        [SerializeField] private string moduleId = "Health";
        [SerializeField] private string displayName = "Health Monitoring";
        [SerializeField] private Sprite icon;
        [SerializeField] private Color accentColor = new Color(0.9f, 0.2f, 0.3f); // 红色

        [Header("面板引用")]
        [SerializeField] private HealthMonitoringPanel targetPanel;
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
                    Debug.LogWarning($"[HealthMonitoringPanelAdapter] Panel Root appears to be a container ({panelRoot.name}). Using targetPanel GameObject instead.", this);
                    panelRoot = targetPanel.gameObject;
                }
            }
        }

        public ControlHubModuleSnapshot BuildSnapshot()
        {
            if (HealthDataStore.Instance != null && HealthDataStore.Instance.Current != null)
            {
                var health = HealthDataStore.Instance.Current;
                
                // 判断健康状态是否异常
                bool isHealthy = IsHealthNormal(health);
                float severity = isHealthy ? 0f : 0.7f;

                string statusText = isHealthy ? "Normal" : "Abnormal";
                string heartRateStatus = GetHeartRateStatus(health.heartRate);
                string sleepStage = GetSleepStageName(health.sleepStage);

                return new ControlHubModuleSnapshot
                {
                    headline = "Vital Signs",
                    value = $"Heart Rate: {health.heartRate} bpm",
                    hint = $"{statusText} | {heartRateStatus} | {sleepStage}",
                    severity01 = severity,
                    hasAlert = !isHealthy,
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
                Debug.LogWarning($"[HealthMonitoringPanelAdapter] ShowPanel called but both panelRoot and targetPanel are null!", this);
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

        private bool IsHealthNormal(VitalSignsData health)
        {
            if (HealthMonitoringController.Instance == null) return true;

            bool heartRateOK = health.heartRate >= HealthMonitoringController.Instance.heartRateMin &&
                               health.heartRate <= HealthMonitoringController.Instance.heartRateMax;
            bool respirationOK = health.respirationRate >= HealthMonitoringController.Instance.respirationRateMin &&
                                health.respirationRate <= HealthMonitoringController.Instance.respirationRateMax;
            bool movementOK = health.bodyMovement >= HealthMonitoringController.Instance.bodyMovementMin;

            return heartRateOK && respirationOK && movementOK;
        }

        private string GetHeartRateStatus(int heartRate)
        {
            if (HealthMonitoringController.Instance == null) return "Normal";

            if (heartRate < HealthMonitoringController.Instance.heartRateMin)
                return "Low";
            if (heartRate > HealthMonitoringController.Instance.heartRateMax)
                return "High";
            return "Normal";
        }

        private string GetSleepStageName(int stage)
        {
            return stage switch
            {
                0 => "Awake",
                1 => "Light Sleep",
                2 => "Deep Sleep",
                _ => "Unknown"
            };
        }
    }
}

