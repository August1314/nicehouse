using UnityEngine;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 汇总面板模块的状态快照，用于入口卡片展示。
    /// </summary>
    [System.Serializable]
    public struct ControlHubModuleSnapshot
    {
        public string headline;
        public string value;
        public string hint;
        public float severity01;
        public bool hasAlert;
        public Color accentColor;

        public static ControlHubModuleSnapshot Default(string title, Color color)
        {
            return new ControlHubModuleSnapshot
            {
                headline = title,
                value = "--",
                hint = "等待数据...",
                severity01 = 0f,
                hasAlert = false,
                accentColor = color
            };
        }
    }

    /// <summary>
    /// 提供模块摘要、图标和 Show/Hide 控制的接口。
    /// </summary>
    public interface IControlPanelModule
    {
        string ModuleId { get; }
        string DisplayName { get; }
        Sprite Icon { get; }
        Color AccentColor { get; }

        /// <summary>
        /// 返回用于入口卡片展示的最新状态快照。
        /// </summary>
        ControlHubModuleSnapshot BuildSnapshot();

        /// <summary>
        /// 当用户在控制面板上激活该模块时调用。
        /// </summary>
        void ShowPanel();

        /// <summary>
        /// 当用户关闭或切换到其他模块时调用。
        /// </summary>
        void HidePanel();
    }
}


