using UnityEngine;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 一个可快速配置的模块适配器，负责在控制中枢面板上展示元数据并控制一个已有的 UI 根节点。
    /// </summary>
    public class SimpleControlPanelModule : MonoBehaviour, IControlPanelModule
    {
        [SerializeField] private string moduleId = "Environment";
        [SerializeField] private string displayName = "Environment Control";
        [SerializeField] private Sprite icon;
        [SerializeField] private Color accentColor = new Color(0.1f, 0.8f, 1f);
        [SerializeField] private GameObject panelRoot;
        [TextArea]
        [SerializeField] private string summaryHeadline = "室内状态";
        [SerializeField] private string summaryValue = "--";
        [SerializeField] private string summaryHint = "等待数据...";
        [Range(0f, 1f)]
        [SerializeField] private float severity;
        [SerializeField] private bool hasAlert;

        public string ModuleId => moduleId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public Color AccentColor => accentColor;

        public ControlHubModuleSnapshot BuildSnapshot()
        {
            return new ControlHubModuleSnapshot
            {
                headline = summaryHeadline,
                value = summaryValue,
                hint = summaryHint,
                severity01 = severity,
                hasAlert = hasAlert,
                accentColor = accentColor
            };
        }

        public void ShowPanel()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        public void HidePanel()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        /// <summary>
        /// 供外部脚本动态更新卡片摘要。
        /// </summary>
        public void UpdateSummary(string headline, string value, string hint, float severityValue, bool alert)
        {
            summaryHeadline = headline;
            summaryValue = value;
            summaryHint = hint;
            severity = Mathf.Clamp01(severityValue);
            hasAlert = alert;
        }

        private void OnValidate()
        {
            severity = Mathf.Clamp01(severity);
        }
    }
}


