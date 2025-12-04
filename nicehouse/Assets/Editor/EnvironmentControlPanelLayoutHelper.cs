using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using NiceHouse.EnvironmentControl;

namespace NiceHouse.Editor
{
    /// <summary>
    /// EnvironmentControlPanel 布局自动调整工具
    /// 使用方法：在 Unity 菜单栏选择 "NiceHouse/UI/调整 EnvironmentControlPanel 布局"
    /// </summary>
    public class EnvironmentControlPanelLayoutHelper : EditorWindow
    {
        [MenuItem("NiceHouse/UI/调整 EnvironmentControlPanel 布局")]
        public static void AdjustLayout()
        {
            // 查找 EnvironmentControlPanel
            EnvironmentControlPanel panel = FindObjectOfType<EnvironmentControlPanel>();
            if (panel == null)
            {
                EditorUtility.DisplayDialog("错误", "未找到 EnvironmentControlPanel！\n请确保场景中存在该组件。", "确定");
                return;
            }

            GameObject panelObj = panel.gameObject;
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            if (panelRect == null)
            {
                EditorUtility.DisplayDialog("错误", "EnvironmentControlPanel 没有 RectTransform 组件！", "确定");
                return;
            }

            Undo.RegisterCompleteObjectUndo(panelObj, "调整 EnvironmentControlPanel 布局");

            // 1. 调整 Panel 的 RectTransform
            AdjustPanelRectTransform(panelRect);

            // 2. 配置 VerticalLayoutGroup
            ConfigureVerticalLayoutGroup(panelObj);

            // 3. 为每个 Row 配置 HorizontalLayoutGroup
            ConfigureRowLayoutGroups(panelObj);

            // 4. 调整按钮大小
            AdjustButtonSizes(panelObj);

            // 5. 调整文本大小（可选）
            AdjustTextSizes(panelObj);

            EditorUtility.DisplayDialog("完成", "EnvironmentControlPanel 布局调整完成！", "确定");
            Debug.Log("[EnvironmentControlPanelLayoutHelper] 布局调整完成！");
        }

        /// <summary>
        /// 调整 Panel 的 RectTransform（大小、位置、锚点）
        /// </summary>
        private static void AdjustPanelRectTransform(RectTransform rect)
        {
            // 设置锚点到右下角
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0f);

            // 设置大小
            rect.sizeDelta = new Vector2(400f, 500f);

            // 设置位置（距离右边缘和底部各 20 像素）
            rect.anchoredPosition = new Vector2(-20f, 20f);

            Debug.Log("[EnvironmentControlPanelLayoutHelper] 已调整 Panel 的 RectTransform");
        }

        /// <summary>
        /// 配置 VerticalLayoutGroup
        /// </summary>
        private static void ConfigureVerticalLayoutGroup(GameObject panelObj)
        {
            VerticalLayoutGroup vlg = panelObj.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = panelObj.AddComponent<VerticalLayoutGroup>();
                Undo.RegisterCreatedObjectUndo(vlg, "添加 VerticalLayoutGroup");
            }

            vlg.spacing = 10f;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            EditorUtility.SetDirty(vlg);
            Debug.Log("[EnvironmentControlPanelLayoutHelper] 已配置 VerticalLayoutGroup");
        }

        /// <summary>
        /// 为每个 Row 配置 HorizontalLayoutGroup
        /// </summary>
        private static void ConfigureRowLayoutGroups(GameObject panelObj)
        {
            string[] rowNames = { "AirConditionerRow", "AirPurifierRow", "FanRow", "FreshAirRow" };

            foreach (string rowName in rowNames)
            {
                Transform rowTransform = panelObj.transform.Find(rowName);
                if (rowTransform == null)
                {
                    Debug.LogWarning($"[EnvironmentControlPanelLayoutHelper] 未找到 {rowName}，跳过");
                    continue;
                }

                GameObject rowObj = rowTransform.gameObject;
                HorizontalLayoutGroup hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
                if (hlg == null)
                {
                    hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
                    Undo.RegisterCreatedObjectUndo(hlg, $"添加 HorizontalLayoutGroup 到 {rowName}");
                }

                hlg.spacing = 10f;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;

                EditorUtility.SetDirty(hlg);
                Debug.Log($"[EnvironmentControlPanelLayoutHelper] 已配置 {rowName} 的 HorizontalLayoutGroup");
            }
        }

        /// <summary>
        /// 调整按钮大小
        /// </summary>
        private static void AdjustButtonSizes(GameObject panelObj)
        {
            string[] buttonNames = { "AirConditionerButton", "AirPurifierButton", "FanButton", "FreshAirButton" };

            foreach (string buttonName in buttonNames)
            {
                Transform buttonTransform = panelObj.transform.Find(buttonName);
                if (buttonTransform == null)
                {
                    // 尝试在 Row 下查找
                    foreach (string rowName in new[] { "AirConditionerRow", "AirPurifierRow", "FanRow", "FreshAirRow" })
                    {
                        Transform rowTransform = panelObj.transform.Find(rowName);
                        if (rowTransform != null)
                        {
                            buttonTransform = rowTransform.Find(buttonName);
                            if (buttonTransform != null) break;
                        }
                    }
                }

                if (buttonTransform == null)
                {
                    Debug.LogWarning($"[EnvironmentControlPanelLayoutHelper] 未找到 {buttonName}，跳过");
                    continue;
                }

                RectTransform buttonRect = buttonTransform.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.sizeDelta = new Vector2(160f, 32f);
                    EditorUtility.SetDirty(buttonRect);
                    Debug.Log($"[EnvironmentControlPanelLayoutHelper] 已调整 {buttonName} 大小");
                }
            }
        }

        /// <summary>
        /// 调整文本大小（移除过大的 size 标签，设置合理的字体大小）
        /// </summary>
        private static void AdjustTextSizes(GameObject panelObj)
        {
            // 查找所有 TextMeshProUGUI 组件
            TextMeshProUGUI[] texts = panelObj.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (TextMeshProUGUI text in texts)
            {
                if (text == null) continue;

                // 设置合理的字体大小
                if (text.fontSize > 20)
                {
                    text.fontSize = 16;
                    EditorUtility.SetDirty(text);
                }

                // 移除过大的 size 标签（在运行时由脚本处理，这里只调整默认字体大小）
            }

            Debug.Log($"[EnvironmentControlPanelLayoutHelper] 已调整 {texts.Length} 个文本组件的字体大小");
        }

        /// <summary>
        /// 显示帮助窗口（可选）
        /// </summary>
        [MenuItem("NiceHouse/UI/EnvironmentControlPanel 布局帮助")]
        public static void ShowHelp()
        {
            string helpText = @"EnvironmentControlPanel 布局调整工具

功能：
1. 调整 Panel 大小：400x500，定位到右下角
2. 配置 VerticalLayoutGroup：间距 10，内边距 10
3. 为每个 Row 添加 HorizontalLayoutGroup：间距 10
4. 调整按钮大小：160x32
5. 调整文本字体大小：最大 16

使用方法：
1. 确保场景中存在 EnvironmentControlPanel GameObject
2. 在菜单栏选择 'NiceHouse/UI/调整 EnvironmentControlPanel 布局'
3. 工具会自动完成所有调整

注意：
- 调整前建议先保存场景
- 如果 Row 结构不同，可能需要手动调整";

            EditorUtility.DisplayDialog("EnvironmentControlPanel 布局帮助", helpText, "确定");
        }
    }
}

