using UnityEngine;
using TMPro;

namespace NiceHouse.UI
{
    /// <summary>
    /// UI设置验证器 - 帮助检查环境数值设置面板的配置是否正确
    /// </summary>
    public class EnvironmentValueSetterValidator : MonoBehaviour
    {
        [Header("要验证的UI组件")]
        public Canvas canvas;
        public RectTransform valueSetterPanel;
        public TextMeshProUGUI titleText;
        public TMP_Dropdown roomDropdown;
        public TMP_InputField temperatureInput;
        public TMP_InputField pm25Input;
        public TMP_InputField smokeInput;
        public TMP_InputField energyInput;
        public UnityEngine.UI.Button applyButton;
        public UnityEngine.UI.Button resetButton;

        [Header("验证结果")]
        [TextArea(10, 20)]
        public string validationResults;

        private void OnValidate()
        {
            ValidateUI();
        }

        private void Start()
        {
            ValidateUI();
        }

        [ContextMenu("验证UI设置")]
        public void ValidateUI()
        {
            System.Text.StringBuilder results = new System.Text.StringBuilder();
            results.AppendLine("=== 环境数值设置面板UI验证结果 ===\n");

            // 验证Canvas
            if (canvas != null)
            {
                results.AppendLine("✅ Canvas检查:");
                results.AppendLine($"   - Render Mode: {canvas.renderMode} (期望: ScreenSpaceOverlay)");
                results.AppendLine($"   - Sort Order: {canvas.sortingOrder} (建议: ≥100)");
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    results.AppendLine("   ⚠️  警告: Render Mode 不是 ScreenSpaceOverlay");
            }
            else
            {
                results.AppendLine("❌ Canvas: 未设置");
            }

            // 验证面板
            if (valueSetterPanel != null)
            {
                results.AppendLine("\n✅ ValueSetterPanel检查:");
                results.AppendLine($"   - Anchors Min: ({valueSetterPanel.anchorMin.x:F1}, {valueSetterPanel.anchorMin.y:F1}) (期望: 1.0, 1.0)");
                results.AppendLine($"   - Anchors Max: ({valueSetterPanel.anchorMax.x:F1}, {valueSetterPanel.anchorMax.y:F1}) (期望: 1.0, 1.0)");
                results.AppendLine($"   - Pivot: ({valueSetterPanel.pivot.x:F1}, {valueSetterPanel.pivot.y:F1}) (期望: 1.0, 1.0)");
                results.AppendLine($"   - Position: ({valueSetterPanel.anchoredPosition.x:F1}, {valueSetterPanel.anchoredPosition.y:F1}) (期望: 0.0, 0.0)");
                results.AppendLine($"   - Size: {valueSetterPanel.sizeDelta.x:F0}×{valueSetterPanel.sizeDelta.y:F0} (期望: 280×200)");

                if (valueSetterPanel.anchorMin != new Vector2(1, 1) || valueSetterPanel.anchorMax != new Vector2(1, 1))
                    results.AppendLine("   ⚠️  警告: Anchors 未正确设置为右上角");
                if (valueSetterPanel.pivot != new Vector2(1, 1))
                    results.AppendLine("   ⚠️  警告: Pivot 未正确设置为右上角");
            }
            else
            {
                results.AppendLine("\n❌ ValueSetterPanel: 未设置");
            }

            // 验证标题文本
            if (titleText != null)
            {
                results.AppendLine("\n✅ TitleText检查:");
                results.AppendLine($"   - Text: \"{titleText.text}\" (期望: \"环境设置\")");
                results.AppendLine($"   - Anchors Min: ({titleText.rectTransform.anchorMin.x:F1}, {titleText.rectTransform.anchorMin.y:F1}) (期望: 0.0, 1.0)");
                results.AppendLine($"   - Anchors Max: ({titleText.rectTransform.anchorMax.x:F1}, {titleText.rectTransform.anchorMax.y:F1}) (期望: 1.0, 1.0)");
                results.AppendLine($"   - Pivot: ({titleText.rectTransform.pivot.x:F1}, {titleText.rectTransform.pivot.y:F1}) (期望: 0.5, 1.0)");
                results.AppendLine($"   - Position: ({titleText.rectTransform.anchoredPosition.x:F1}, {titleText.rectTransform.anchoredPosition.y:F1}) (期望: 0.0, 0.0)");
                results.AppendLine($"   - Size: {titleText.rectTransform.sizeDelta.x:F0}×{titleText.rectTransform.sizeDelta.y:F0} (期望: 260×30)");

                if (titleText.text != "环境设置")
                    results.AppendLine("   ⚠️  警告: 标题文本不是\"环境设置\"");
                if (titleText.rectTransform.pivot != new Vector2(0.5f, 1.0f))
                    results.AppendLine("   ⚠️  警告: Pivot 未正确设置，可能导致标题位置不对");
            }
            else
            {
                results.AppendLine("\n❌ TitleText: 未设置");
            }

            // 验证其他组件
            ValidateComponent(roomDropdown, "RoomDropdown", results);
            ValidateComponent(temperatureInput, "TemperatureInput", results);
            ValidateComponent(pm25Input, "PM25Input", results);
            ValidateComponent(smokeInput, "SmokeInput", results);
            ValidateComponent(energyInput, "EnergyInput", results);
            ValidateComponent(applyButton, "ApplyButton", results);
            ValidateComponent(resetButton, "ResetButton", results);

            results.AppendLine("\n=== 验证完成 ===");
            results.AppendLine("如果有警告或错误，请参考 EnvironmentValueSetter_Troubleshooting.md 进行修复");

            validationResults = results.ToString();
            Debug.Log(validationResults);
        }

        private void ValidateComponent(Component component, string componentName, System.Text.StringBuilder results)
        {
            if (component != null)
            {
                results.AppendLine($"\n✅ {componentName}: 已设置");
            }
            else
            {
                results.AppendLine($"\n❌ {componentName}: 未设置");
            }
        }
    }
}