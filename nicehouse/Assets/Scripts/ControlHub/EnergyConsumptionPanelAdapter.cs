using UnityEngine;
using NiceHouse.UI;
using NiceHouse.Data;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 能源消耗面板的 Control Hub 适配器，实现 IControlPanelModule 接口。
    /// </summary>
    public class EnergyConsumptionPanelAdapter : MonoBehaviour, IControlPanelModule
    {
        [Header("Module Configuration")]
        [SerializeField] private string moduleId = "Energy";
        [SerializeField] private string displayName = "Energy Consumption";
        [SerializeField] private Sprite icon;
        [SerializeField] private Color accentColor = new Color(1f, 0.84f, 0f); // 金色/黄色

        [Header("Panel References")]
        [SerializeField] private EnergyConsumptionPanel targetPanel;
        [SerializeField] private GameObject panelRoot;

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
            if (panelRoot != null && targetPanel != null)
            {
                if (panelRoot.name.Contains("Root") || panelRoot.name.Contains("Container") || 
                    panelRoot.name.Contains("Embedded"))
                {
                    Debug.LogWarning($"[EnergyConsumptionPanelAdapter] Panel Root appears to be a container ({panelRoot.name}). Using targetPanel GameObject instead.", this);
                    panelRoot = targetPanel.gameObject;
                }
            }
        }

        public ControlHubModuleSnapshot BuildSnapshot()
        {
            // 从能源管理器获取实时状态
            if (EnergyManager.Instance != null)
            {
                float totalConsumption = EnergyManager.Instance.GetTotalDailyConsumption();
                float totalPower = EnergyManager.Instance.GetTotalCurrentPower();

                // 计算严重程度（基于总功率，功率越高越严重）
                float severity = 0f;
                bool hasAlert = false;

                // 假设总功率超过 5000W 为警告
                if (totalPower > 5000f)
                {
                    severity = 1f;
                    hasAlert = true;
                }
                else if (totalPower > 3000f)
                {
                    severity = 0.5f;
                }

                return new ControlHubModuleSnapshot
                {
                    headline = "Energy Consumption",
                    value = $"{totalConsumption:F3} kWh",
                    hint = $"Current Power: {totalPower:F1} W",
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
                Debug.LogWarning($"[EnergyConsumptionPanelAdapter] ShowPanel called but both panelRoot and targetPanel are null!", this);
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

