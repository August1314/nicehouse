using UnityEngine;
using NiceHouse.UI;
using NiceHouse.Data;
using NiceHouse.HealthMonitoring;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 数据仪表盘的 Control Hub 适配器，实现 IControlPanelModule 接口。
    /// </summary>
    public class DataDashboardAdapter : MonoBehaviour, IControlPanelModule
    {
        [Header("模块配置")]
        [SerializeField] private string moduleId = "Dashboard";
        [SerializeField] private string displayName = "Data Dashboard";
        [SerializeField] private Sprite icon;
        [SerializeField] private Color accentColor = new Color(0.5f, 0.8f, 0.3f); // 绿色

        [Header("面板引用")]
        [SerializeField] private DataDashboard targetPanel;
        [SerializeField] private GameObject panelRoot;

        [Header("状态来源")]
        [SerializeField] private string currentRoomId = "LivingRoom01";

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
                    Debug.LogWarning($"[DataDashboardAdapter] Panel Root appears to be a container ({panelRoot.name}). Using targetPanel GameObject instead.", this);
                    panelRoot = targetPanel.gameObject;
                }
            }
        }

        public ControlHubModuleSnapshot BuildSnapshot()
        {
            // 汇总多个数据源的状态
            int alertCount = 0;
            float maxSeverity = 0f;

            // 检查告警
            if (AlarmManager.Instance != null)
            {
                var recentAlarms = AlarmManager.Instance.GetRecentAlarms(10);
                foreach (var alarm in recentAlarms)
                {
                    if (!alarm.handled)
                    {
                        alertCount++;
                        maxSeverity = Mathf.Max(maxSeverity, 0.8f);
                    }
                }
            }

            // 检查环境数据
            if (EnvironmentDataStore.Instance != null &&
                EnvironmentDataStore.Instance.TryGetRoomData(currentRoomId, out var env))
            {
                if (env.pm25 > 75f) maxSeverity = Mathf.Max(maxSeverity, 1f);
                else if (env.pm25 > 35f) maxSeverity = Mathf.Max(maxSeverity, 0.5f);
            }

            // 检查健康数据
            if (HealthDataStore.Instance != null && HealthDataStore.Instance.Current != null)
            {
                var health = HealthDataStore.Instance.Current;
                if (HealthMonitoringController.Instance != null)
                {
                    bool heartRateOK = health.heartRate >= HealthMonitoringController.Instance.heartRateMin &&
                                       health.heartRate <= HealthMonitoringController.Instance.heartRateMax;
                    if (!heartRateOK) maxSeverity = Mathf.Max(maxSeverity, 0.7f);
                }
            }

            string summary = alertCount > 0 ? $"{alertCount} alerts" : "Normal";

            return new ControlHubModuleSnapshot
            {
                headline = "Status",
                value = summary,
                hint = $"Room: {currentRoomId} | Data Summary",
                severity01 = maxSeverity,
                hasAlert = alertCount > 0 || maxSeverity > 0.5f,
                accentColor = accentColor
            };
        }

        private bool _isShowing = false;
        
        public void ShowPanel()
        {
            // 防止重复调用
            if (_isShowing)
            {
                return;
            }
            
            GameObject targetObj = null;
            if (panelRoot != null)
            {
                targetObj = panelRoot;
            }
            else if (targetPanel != null)
            {
                targetObj = targetPanel.gameObject;
            }
            
            if (targetObj != null)
            {
                // 如果已经激活，直接返回
                if (targetObj.activeSelf)
                {
                    _isShowing = true;
                    return;
                }
                
                // 检查是否有多个同名 GameObject，并强制隐藏所有其他实例
                var allDashboards = FindObjectsOfType<DataDashboard>(true); // 包括未激活的
                
                if (allDashboards.Length > 1)
                {
                    Debug.LogWarning($"[DataDashboardAdapter] Found {allDashboards.Length} DataDashboard instances! Hiding all except the target one.", this);
                    foreach (var db in allDashboards)
                    {
                        if (db.gameObject != targetObj)
                        {
                            db.gameObject.SetActive(false);
                        }
                    }
                }
                
                targetObj.SetActive(true);
                _isShowing = true;
            }
            else
            {
                Debug.LogWarning($"[DataDashboardAdapter] ShowPanel called but both panelRoot and targetPanel are null!", this);
            }
        }

        public void HidePanel()
        {
            GameObject targetObj = null;
            if (panelRoot != null)
            {
                targetObj = panelRoot;
            }
            else if (targetPanel != null)
            {
                targetObj = targetPanel.gameObject;
            }
            
            if (targetObj != null)
            {
                // 先隐藏目标对象
                targetObj.SetActive(false);
                
                // 然后强制隐藏所有 DataDashboard 实例（包括未激活的），确保没有残留
                var allDashboards = FindObjectsOfType<DataDashboard>(true);
                foreach (var db in allDashboards)
                {
                    if (db.gameObject.activeSelf)
                    {
                        db.gameObject.SetActive(false);
                    }
                }
                
                _isShowing = false;
            }
        }
    }
}

