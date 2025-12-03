using System.Collections.Generic;
using System.IO;
using NiceHouse.LayeredVisualization;
using NiceHouse.LayeredVisualization.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NiceHouse.EditorTools
{
    /// <summary>
    /// 一键生成分层可视化 UI 预制体（Layer Panel + Legend Panel）。
    /// 菜单：Tools/Layered Visualization/Generate UI Prefabs
    /// </summary>
    public static class LayeredVisualizationUIGenerator
    {
        private const string PrefabFolder = "Assets/Prefabs/LayeredVisualization";

        [MenuItem("Tools/Layered Visualization/Generate UI Prefabs")]
        public static void Generate()
        {
            Directory.CreateDirectory(PrefabFolder);

            CreateLayerPanelPrefab(Path.Combine(PrefabFolder, "LayerPanel.prefab"));
            CreateLegendPanelPrefab(Path.Combine(PrefabFolder, "LegendPanel.prefab"));

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Layered Visualization", "LayerPanel & LegendPanel 预制体生成完成。", "OK");
        }

        private static void CreateLayerPanelPrefab(string path)
        {
            var root = CreateUIRoot("LayerPanel", new Vector2(260f, 200f));

            var image = root.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.5f);

            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 12f;

            var fitter = root.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Header
            var header = CreateTMPText("Header", root.transform, "图层选择", 24, FontStyles.Bold);
            header.alignment = TextAlignmentOptions.Left;
            header.color = Color.white;

            // Active layer label
            var activeLabel = CreateTMPText("ActiveLayerLabel", root.transform, "温度", 18, FontStyles.Bold);
            activeLabel.color = new Color(0.88f, 0.9f, 1f, 1f);

            // Toggle container
            var toggleContainer = new GameObject("ToggleContainer", typeof(RectTransform));
            toggleContainer.transform.SetParent(root.transform, false);
            var toggleLayout = toggleContainer.AddComponent<VerticalLayoutGroup>();
            toggleLayout.spacing = 8f;
            toggleLayout.childControlHeight = true;
            toggleLayout.childControlWidth = true;

            // Create toggles
            var toggles = new List<ToggleConfig>
            {
                new("温度", true),
                new("湿度"),
                new("PM2.5")
            };

            foreach (var config in toggles)
            {
                config.Toggle = CreateToggle(toggleContainer.transform, config.Label);
                if (config.IsDefault)
                {
                    config.Toggle.isOn = true;
                }
            }

            var panel = root.AddComponent<LayerPanelUI>();
            panel.activeLayerLabel = activeLabel;
            panel.autoActivateFirstLayer = false;
            panel.layers = new List<LayerPanelUI.LayerConfig>
            {
                new LayerPanelUI.LayerConfig
                {
                    displayName = "温度",
                    toggle = toggles[0].Toggle,
                    activateOnStart = true
                },
                new LayerPanelUI.LayerConfig
                {
                    displayName = "湿度",
                    toggle = toggles[1].Toggle,
                    activateOnStart = false
                },
                new LayerPanelUI.LayerConfig
                {
                    displayName = "PM2.5",
                    toggle = toggles[2].Toggle,
                    activateOnStart = false
                }
            };

            SavePrefab(root, path);
        }

        private static void CreateLegendPanelPrefab(string path)
        {
            var root = CreateUIRoot("LegendPanel", new Vector2(260f, 140f));

            var image = root.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.45f);

            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 8f;

            var fitter = root.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var title = CreateTMPText("Title", root.transform, "温度 (℃)", 20, FontStyles.Bold);
            title.color = Color.white;

            var gradientGO = new GameObject("GradientImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            gradientGO.transform.SetParent(root.transform, false);
            var gradientRect = gradientGO.GetComponent<RectTransform>();
            gradientRect.sizeDelta = new Vector2(220f, 20f);

            var rangeText = CreateTMPText("Range", root.transform, "22.0 - 28.0 ℃", 16, FontStyles.Normal);
            rangeText.color = new Color(0.85f, 0.9f, 1f, 1f);

            var legend = root.AddComponent<LegendPanelUI>();
            legend.titleText = title;
            legend.rangeText = rangeText;
            legend.gradientImage = gradientGO.GetComponent<Image>();

            SavePrefab(root, path);
        }

        private static GameObject CreateUIRoot(string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            return go;
        }

        private static TextMeshProUGUI CreateTMPText(string name, Transform parent, string text, int fontSize, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            var defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null)
            {
                tmp.font = defaultFont;
            }

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(220f, fontSize + 10);

            return tmp;
        }

        private static Toggle CreateToggle(Transform parent, string label)
        {
            var toggleGO = new GameObject(label + "Toggle", typeof(RectTransform));
            toggleGO.transform.SetParent(parent, false);

            var background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            background.transform.SetParent(toggleGO.transform, false);
            var bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.5f);
            bgRect.anchorMax = new Vector2(0f, 0.5f);
            bgRect.sizeDelta = new Vector2(20f, 20f);
            bgRect.anchoredPosition = new Vector2(10f, 0f);

            var bgImage = background.GetComponent<Image>();
            bgImage.color = new Color(1f, 1f, 1f, 0.7f);

            var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkmark.transform.SetParent(background.transform, false);
            var checkRect = checkmark.GetComponent<RectTransform>();
            checkRect.sizeDelta = new Vector2(16f, 16f);

            var checkImage = checkmark.GetComponent<Image>();
            checkImage.color = new Color(0.2f, 0.8f, 1f, 1f);

            var labelGO = CreateTMPText("Label", toggleGO.transform, label, 18, FontStyles.Normal);
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(40f, 0f);

            var toggle = toggleGO.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            toggle.isOn = false;

            var layout = toggleGO.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.padding = new RectOffset(0, 0, 5, 5);

            return toggle;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private class ToggleConfig
        {
            public string Label;
            public bool IsDefault;
            public Toggle Toggle;

            public ToggleConfig(string label, bool isDefault = false)
            {
                Label = label;
                IsDefault = isDefault;
            }
        }
    }
}


