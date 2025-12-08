using UnityEngine;
using NiceHouse.EnvironmentControl;
using NiceHouse.Data;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 环境智控面板的 Control Hub 适配器，实现 IControlPanelModule 接口。
    /// </summary>
    public class EnvironmentControlPanelAdapter : MonoBehaviour, IControlPanelModule
    {
        [Header("模块配置")]
        [SerializeField] private string moduleId = "Environment";
        [SerializeField] private string displayName = "Environment Control";
        [SerializeField] private Sprite icon;
        [SerializeField] private Color accentColor = new Color(0.1f, 0.8f, 1f); // 蓝色

        [Header("面板引用")]
        [SerializeField] private EnvironmentControlPanel targetPanel;
        [SerializeField] private GameObject panelRoot;

        [Header("状态来源")]
        [SerializeField] private string currentRoomId = "LivingRoom01";

        public string ModuleId => moduleId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public Color AccentColor => accentColor;

        private void Awake()
        {
            // 如果没有手动指定 panelRoot，尝试从 targetPanel 获取
            if (panelRoot == null && targetPanel != null)
            {
                panelRoot = targetPanel.gameObject;
            }
            
            // 确保 panelRoot 指向的是面板 GameObject，而不是容器
            // 如果 panelRoot 是 EmbeddedRoot 或类似的容器，应该使用 targetPanel.gameObject
            if (panelRoot != null && targetPanel != null)
            {
                // 如果 panelRoot 的名字包含 "Root" 或 "Container"，说明可能是容器，应该用 targetPanel
                if (panelRoot.name.Contains("Root") || panelRoot.name.Contains("Container") || 
                    panelRoot.name.Contains("Embedded"))
                {
                    Debug.LogWarning($"[EnvironmentControlPanelAdapter] Panel Root appears to be a container ({panelRoot.name}). Using targetPanel GameObject instead.", this);
                    panelRoot = targetPanel.gameObject;
                }
            }
        }

        public ControlHubModuleSnapshot BuildSnapshot()
        {
            // 从环境数据存储获取实时状态
            if (EnvironmentDataStore.Instance != null &&
                EnvironmentDataStore.Instance.TryGetRoomData(currentRoomId, out var env))
            {
                // 计算严重程度（基于 PM2.5）
                float severity = 0f;
                bool hasAlert = false;

                if (env.pm25 > 75f)
                {
                    severity = 1f;
                    hasAlert = true;
                }
                else if (env.pm25 > 35f)
                {
                    severity = 0.5f;
                }

                // Temperature status
                string tempStatus = env.temperature < 18f ? "Cold" : 
                                   env.temperature > 28f ? "Hot" : "Comfortable";

                return new ControlHubModuleSnapshot
                {
                    headline = "Environment Status",
                    value = $"{env.temperature:F1}°C / PM2.5: {env.pm25:F1}",
                    hint = $"Humidity {env.humidity:F1}% | {tempStatus}",
                    severity01 = severity,
                    hasAlert = hasAlert,
                    accentColor = accentColor
                };
            }

            return ControlHubModuleSnapshot.Default(displayName, accentColor);
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
                
                targetObj.SetActive(true);
                _isShowing = true;
            }
            else
            {
                Debug.LogWarning($"[EnvironmentControlPanelAdapter] ShowPanel called but both panelRoot and targetPanel are null!", this);
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
                targetObj.SetActive(false);
                _isShowing = false;
            }
        }
    }
}

