using System.Collections.Generic;
using NiceHouse.LayeredVisualization.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NiceHouse.LayeredVisualization
{
    /// <summary>
    /// 图层切换 UI，负责控制多个 FloorHeatmapGrid 实例的启用状态，并同步图例。
    /// </summary>
    public class LayerPanelUI : MonoBehaviour
    {
        [System.Serializable]
        public class LayerConfig
        {
            public string displayName;
            public Toggle toggle;
            public FloorHeatmapGrid grid;
            public bool activateOnStart;
        }

        [Header("Layer Entries")]
        public List<LayerConfig> layers = new List<LayerConfig>();

        [Header("UI References")]
        public LegendPanelUI legendPanel;
        public TextMeshProUGUI activeLayerLabel;

        [Tooltip("是否在 Start 时自动激活第一项")]
        public bool autoActivateFirstLayer = true;

        private int _activeIndex = -1;

        private void Start()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                var index = i;
                var config = layers[i];
                if (config.toggle != null)
                {
                    config.toggle.onValueChanged.AddListener(isOn =>
                    {
                        if (isOn)
                        {
                            ActivateLayer(index);
                        }
                        else if (_activeIndex == index)
                        {
                            // 防止全部关闭：保持当前层开启
                            config.toggle.isOn = true;
                        }
                    });
                }
            }

            if (autoActivateFirstLayer && layers.Count > 0)
            {
                int defaultIndex = layers.FindIndex(l => l.activateOnStart);
                if (defaultIndex < 0) defaultIndex = 0;

                ActivateLayer(defaultIndex);
                if (layers[defaultIndex].toggle != null)
                {
                    layers[defaultIndex].toggle.isOn = true;
                }
            }
        }

        private void ActivateLayer(int index)
        {
            if (index < 0 || index >= layers.Count) return;
            if (_activeIndex == index) return;
            _activeIndex = index;

            for (int i = 0; i < layers.Count; i++)
            {
                bool shouldEnable = i == index;
                var layer = layers[i];
                if (layer.grid != null)
                {
                    layer.grid.gameObject.SetActive(shouldEnable);
                }

                if (layer.toggle != null && layer.toggle.isOn != shouldEnable)
                {
                    layer.toggle.SetIsOnWithoutNotify(shouldEnable);
                }
            }

            var activeLayer = layers[index];
            if (activeLayerLabel != null)
            {
                activeLayerLabel.text = activeLayer.displayName;
            }

            if (legendPanel != null && activeLayer.grid != null)
            {
                legendPanel.UpdateLegend(
                    activeLayer.displayName,
                    activeLayer.grid.GetMetricUnit(),
                    activeLayer.grid.minValue,
                    activeLayer.grid.maxValue,
                    activeLayer.grid.colorGradient);
            }
        }
    }
}


