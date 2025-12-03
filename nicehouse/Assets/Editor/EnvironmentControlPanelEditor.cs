using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using NiceHouse.EnvironmentControl;

namespace NiceHouse.Editor
{
    /// <summary>
    /// EnvironmentControlPanel 的 Inspector 扩展
    /// 在 Inspector 中添加一个明显的"调整布局"按钮
    /// </summary>
    [CustomEditor(typeof(EnvironmentControlPanel))]
    public class EnvironmentControlPanelEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // 绘制默认的 Inspector
            DrawDefaultInspector();

            // 添加分隔线
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // 添加一个明显的按钮
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("点击下面的按钮自动调整面板布局（大小、位置、间距等）", MessageType.Info);
            
            // 大按钮样式
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.padding = new RectOffset(10, 10, 10, 10);
            buttonStyle.fixedHeight = 40;

            if (GUILayout.Button("🔧 自动调整布局", buttonStyle))
            {
                AdjustLayout((EnvironmentControlPanel)target);
            }

            EditorGUILayout.Space(5);
        }

        /// <summary>
        /// 调整布局的核心方法
        /// </summary>
        private void AdjustLayout(EnvironmentControlPanel panel)
        {
            if (panel == null)
            {
                EditorUtility.DisplayDialog("错误", "EnvironmentControlPanel 组件为空！", "确定");
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

            // 5. 调整状态文本大小
            AdjustStatusTextSizes(panelObj);

            // 6. 调整环境数据文本和模式文本的高度
            AdjustDataTextSizes(panelObj);

            // 7. 调整文本大小
            AdjustTextSizes(panelObj);

            // 8. 调整 Canvas Scaler（可选，让 UI 更大）
            AdjustCanvasScaler(panelObj);

            EditorUtility.DisplayDialog("完成", "EnvironmentControlPanel 布局调整完成！\n\n调整内容：\n- 面板大小：400x（全高度）\n- 位置：右上角\n- 纵向间距：1（最小）\n- 内边距：5（最小）\n- Row 高度：35（固定）\n- 按钮高度：28\n- 状态文本高度：30\n- PM/环境数据文本高度：35\n- ModeText 高度：40\n- 字体大小已优化（20-36）\n- Canvas 缩放已优化\n\n如果设备显示 N/A，请查看：\ndocs/environment-control-troubleshooting.md", "确定");
            Debug.Log("[EnvironmentControlPanelEditor] 布局调整完成！");
        }

        /// <summary>
        /// 调整 Panel 的 RectTransform
        /// </summary>
        private void AdjustPanelRectTransform(RectTransform rect)
        {
            // 设置锚点到右下角
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);  // 改为全高度，让面板可以占满右侧
            rect.pivot = new Vector2(1f, 1f);     // 改为右上角对齐

            // 设置大小（增加高度，让内容能完整显示）
            rect.sizeDelta = new Vector2(400f, 0f);  // 高度设为0，让锚点自动填充

            // 设置位置（距离右边缘和顶部各 20 像素）
            rect.anchoredPosition = new Vector2(-20f, -20f);

            EditorUtility.SetDirty(rect);
            Debug.Log("[EnvironmentControlPanelEditor] 已调整 Panel 的 RectTransform");
        }

        /// <summary>
        /// 配置 VerticalLayoutGroup
        /// </summary>
        private void ConfigureVerticalLayoutGroup(GameObject panelObj)
        {
            VerticalLayoutGroup vlg = panelObj.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = panelObj.AddComponent<VerticalLayoutGroup>();
                Undo.RegisterCreatedObjectUndo(vlg, "添加 VerticalLayoutGroup");
            }

            // 减小间距和内边距，让内容更紧凑
            vlg.spacing = 1f;  // 进一步减小到 1
            vlg.padding = new RectOffset(5, 5, 5, 5);  // 进一步减小到 5
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;  // 不控制子元素高度
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;  // 不强制扩展高度，让子元素使用自己的高度

            EditorUtility.SetDirty(vlg);
            Debug.Log("[EnvironmentControlPanelEditor] 已配置 VerticalLayoutGroup（间距已减小）");
        }

        /// <summary>
        /// 为每个 Row 配置 HorizontalLayoutGroup
        /// </summary>
        private void ConfigureRowLayoutGroups(GameObject panelObj)
        {
            string[] rowNames = { "AirConditionerRow", "AirPurifierRow", "FanRow", "FreshAirRow" };

            foreach (string rowName in rowNames)
            {
                Transform rowTransform = panelObj.transform.Find(rowName);
                if (rowTransform == null)
                {
                    Debug.LogWarning($"[EnvironmentControlPanelEditor] 未找到 {rowName}，跳过");
                    continue;
                }

                GameObject rowObj = rowTransform.gameObject;
                HorizontalLayoutGroup hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
                if (hlg == null)
                {
                    hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
                    Undo.RegisterCreatedObjectUndo(hlg, $"添加 HorizontalLayoutGroup 到 {rowName}");
                }

                hlg.spacing = 5f;  // 进一步减小到 5
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;  // 不强制扩展高度

                // 调整 Row 的 RectTransform 高度，让它更紧凑
                RectTransform rowRect = rowObj.GetComponent<RectTransform>();
                if (rowRect != null)
                {
                    // 设置 Row 高度为固定值，不让它自动扩展
                    rowRect.sizeDelta = new Vector2(rowRect.sizeDelta.x, 35f);  // 固定高度 35
                    EditorUtility.SetDirty(rowRect);
                }

                EditorUtility.SetDirty(hlg);
                Debug.Log($"[EnvironmentControlPanelEditor] 已配置 {rowName} 的 HorizontalLayoutGroup 和高度");
            }
        }

        /// <summary>
        /// 调整按钮大小
        /// </summary>
        private void AdjustButtonSizes(GameObject panelObj)
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
                    Debug.LogWarning($"[EnvironmentControlPanelEditor] 未找到 {buttonName}，跳过");
                    continue;
                }

                RectTransform buttonRect = buttonTransform.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.sizeDelta = new Vector2(160f, 28f);  // 减小按钮高度从 32 到 28
                    EditorUtility.SetDirty(buttonRect);
                    Debug.Log($"[EnvironmentControlPanelEditor] 已调整 {buttonName} 大小");
                }
            }
        }

        /// <summary>
        /// 调整状态文本的大小（高度）
        /// </summary>
        private void AdjustStatusTextSizes(GameObject panelObj)
        {
            string[] statusTextNames = { "AirConditionerStatusText", "AirPurifierStatusText", "FanStatusText", "FreshAirStatusText" };

            foreach (string statusTextName in statusTextNames)
            {
                Transform statusTextTransform = panelObj.transform.Find(statusTextName);
                if (statusTextTransform == null)
                {
                    // 尝试在 Row 下查找
                    foreach (string rowName in new[] { "AirConditionerRow", "AirPurifierRow", "FanRow", "FreshAirRow" })
                    {
                        Transform rowTransform = panelObj.transform.Find(rowName);
                        if (rowTransform != null)
                        {
                            statusTextTransform = rowTransform.Find(statusTextName);
                            if (statusTextTransform != null) break;
                        }
                    }
                }

                if (statusTextTransform == null)
                {
                    Debug.LogWarning($"[EnvironmentControlPanelEditor] 未找到 {statusTextName}，跳过");
                    continue;
                }

                RectTransform statusTextRect = statusTextTransform.GetComponent<RectTransform>();
                if (statusTextRect != null)
                {
                    // 设置状态文本高度，让它更紧凑
                    statusTextRect.sizeDelta = new Vector2(statusTextRect.sizeDelta.x, 30f);  // 固定高度 30
                    EditorUtility.SetDirty(statusTextRect);
                    Debug.Log($"[EnvironmentControlPanelEditor] 已调整 {statusTextName} 高度");
                }
            }
        }

        /// <summary>
        /// 调整环境数据文本和模式文本的高度
        /// </summary>
        private void AdjustDataTextSizes(GameObject panelObj)
        {
            // 调整 PM10、PM2.5、Temperature、Humidity 文本的高度
            string[] dataTextNames = { "Pm10Text", "Pm25Text", "TemperatureText", "HumidityText", "ModeText" };

            foreach (string textName in dataTextNames)
            {
                Transform textTransform = panelObj.transform.Find(textName);
                if (textTransform == null)
                {
                    Debug.LogWarning($"[EnvironmentControlPanelEditor] 未找到 {textName}，跳过");
                    continue;
                }

                RectTransform textRect = textTransform.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    // 根据文本类型设置不同的高度
                    if (textName.Contains("Mode"))
                    {
                        textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, 40f);  // ModeText 高度 40
                    }
                    else
                    {
                        textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, 35f);  // 其他数据文本高度 35
                    }
                    EditorUtility.SetDirty(textRect);
                    Debug.Log($"[EnvironmentControlPanelEditor] 已调整 {textName} 高度");
                }
            }
        }

        /// <summary>
        /// 调整文本大小
        /// </summary>
        private void AdjustTextSizes(GameObject panelObj)
        {
            TMPro.TextMeshProUGUI[] texts = panelObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);

            foreach (TMPro.TextMeshProUGUI text in texts)
            {
                if (text == null) continue;

                // 根据文本类型设置不同的字体大小（使用更大的值）
                string textName = text.gameObject.name.ToLower();
                
                // 按钮文本：中等
                if (textName.Contains("button") || (text.transform.parent != null && text.transform.parent.name.ToLower().Contains("button")))
                {
                    text.fontSize = 20;
                }
                // 状态文本：中等偏大
                else if (textName.Contains("status"))
                {
                    text.fontSize = 28;
                }
                // 模式文本：大
                else if (textName.Contains("mode"))
                {
                    text.fontSize = 32;
                }
                // 环境数据文本（PM10, PM2.5, Temperature, Humidity）：大
                else if (textName.Contains("temp") || textName.Contains("humid") || textName.Contains("pm") || textName.Contains("pm10") || textName.Contains("pm25"))
                {
                    text.fontSize = 30;
                }
                // 房间名称：大
                else if (textName.Contains("room"))
                {
                    text.fontSize = 36;
                }
                // 其他文本：默认中等尺寸
                else
                {
                    text.fontSize = 24;
                }

                // 确保启用自动大小（如果用户想要）
                text.enableAutoSizing = true;
                text.fontSizeMin = text.fontSize * 0.5f;
                text.fontSizeMax = text.fontSize * 1.5f;

                EditorUtility.SetDirty(text);
            }

            Debug.Log($"[EnvironmentControlPanelEditor] 已调整 {texts.Length} 个文本组件的字体大小");
        }

        /// <summary>
        /// 调整 Canvas Scaler 以优化 UI 显示
        /// </summary>
        private void AdjustCanvasScaler(GameObject panelObj)
        {
            // 查找 Canvas
            Canvas canvas = panelObj.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[EnvironmentControlPanelEditor] 未找到 Canvas，跳过 Canvas Scaler 调整");
                return;
            }

            UnityEngine.UI.CanvasScaler scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler != null)
            {
                // 如果 Scale Factor 太小，调整为 1.0 或更大
                if (scaler.scaleFactor < 1.0f)
                {
                    scaler.scaleFactor = 1.0f;
                    EditorUtility.SetDirty(scaler);
                    Debug.Log("[EnvironmentControlPanelEditor] 已调整 Canvas Scaler 的 Scale Factor 为 1.0");
                }
            }
        }
    }
}

