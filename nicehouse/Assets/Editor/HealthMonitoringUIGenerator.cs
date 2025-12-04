using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using NiceHouse.HealthMonitoring;

namespace NiceHouse.Editor
{
    /// <summary>
    /// HealthMonitoringPanel UI 自动生成工具
    /// 使用方法：在 Unity 菜单栏选择 "NiceHouse/UI/生成 HealthMonitoringPanel UI"
    /// </summary>
    public class HealthMonitoringUIGenerator : EditorWindow
    {
        [MenuItem("NiceHouse/UI/生成 HealthMonitoringPanel UI")]
        public static void GenerateUI()
        {
            // 查找或创建 HealthMonitoringPanel
            HealthMonitoringPanel panel = FindObjectOfType<HealthMonitoringPanel>();
            if (panel == null)
            {
                // 查找 Canvas
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    EditorUtility.DisplayDialog("Error", "Canvas not found!\nPlease create a Canvas first.", "OK");
                    return;
                }

                // 创建 HealthMonitoringPanel GameObject
                GameObject panelObj = new GameObject("HealthMonitoringPanel");
                panelObj.transform.SetParent(canvas.transform, false);
                panel = panelObj.AddComponent<HealthMonitoringPanel>();

                // 添加 Panel 组件
                Image panelImage = panelObj.AddComponent<Image>();
                panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

                // 设置 RectTransform
                RectTransform panelRect = panelObj.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0f);
                panelRect.anchorMax = new Vector2(1f, 0.5f);
                panelRect.sizeDelta = Vector2.zero;
                panelRect.anchoredPosition = Vector2.zero;

                Undo.RegisterCreatedObjectUndo(panelObj, "创建 HealthMonitoringPanel");
            }

            GameObject panelGameObject = panel.gameObject;
            Undo.RegisterCompleteObjectUndo(panelGameObject, "生成 HealthMonitoringPanel UI");

            // 添加 VerticalLayoutGroup
            VerticalLayoutGroup vlg = panelGameObject.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = panelGameObject.AddComponent<VerticalLayoutGroup>();
                Undo.RegisterCreatedObjectUndo(vlg, "添加 VerticalLayoutGroup");
            }
            vlg.spacing = 10f;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // 1. 创建健康状态显示区域
            CreateHealthStatusSection(panelGameObject, panel);

            // 2. 创建生命体征显示区域
            CreateVitalSignsSection(panelGameObject, panel);

            // 3. 创建体动进度条区域
            CreateBodyMovementSection(panelGameObject, panel);

            // 4. 创建 HealthMonitoringController GameObject
            CreateHealthMonitoringController();

            EditorUtility.DisplayDialog("Complete", "HealthMonitoringPanel UI generation complete!\n\nCreated:\n- Health status display\n- Vital signs display (Heart Rate, Respiration, Sleep Stage)\n- Body movement progress bar\n- HealthMonitoringController GameObject\n\nPlease check if script field bindings are correct.", "OK");
            Debug.Log("[HealthMonitoringUIGenerator] UI 生成完成！");
        }

        /// <summary>
        /// 创建健康状态显示区域
        /// </summary>
        private static void CreateHealthStatusSection(GameObject parent, HealthMonitoringPanel panel)
        {
            // HealthStatusText
            if (panel.healthStatusText == null)
            {
                GameObject statusText = CreateTextMeshPro(parent, "HealthStatusText", "Status: Healthy", 24);
                panel.healthStatusText = statusText.GetComponent<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// 创建生命体征显示区域
        /// </summary>
        private static void CreateVitalSignsSection(GameObject parent, HealthMonitoringPanel panel)
        {
            // HeartRateText
            if (panel.heartRateText == null)
            {
                GameObject heartRateText = CreateTextMeshPro(parent, "HeartRateText", "Heart Rate: 72 bpm (Normal)", 20);
                panel.heartRateText = heartRateText.GetComponent<TextMeshProUGUI>();
            }

            // RespirationRateText
            if (panel.respirationRateText == null)
            {
                GameObject respirationText = CreateTextMeshPro(parent, "RespirationRateText", "Respiration: 16 /min (Normal)", 20);
                panel.respirationRateText = respirationText.GetComponent<TextMeshProUGUI>();
            }

            // SleepStageText
            if (panel.sleepStageText == null)
            {
                GameObject sleepStageText = CreateTextMeshPro(parent, "SleepStageText", "Sleep Stage: Awake", 20);
                panel.sleepStageText = sleepStageText.GetComponent<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// 创建体动进度条区域
        /// </summary>
        private static void CreateBodyMovementSection(GameObject parent, HealthMonitoringPanel panel)
        {
            // BodyMovementText
            if (panel.bodyMovementText == null)
            {
                GameObject bodyMovementText = CreateTextMeshPro(parent, "BodyMovementText", "Body Movement: 0.30", 20);
                panel.bodyMovementText = bodyMovementText.GetComponent<TextMeshProUGUI>();
            }

            // BodyMovementProgressBar
            if (panel.bodyMovementProgressBar == null)
            {
                GameObject progressBarContainer = new GameObject("BodyMovementProgressBar");
                progressBarContainer.transform.SetParent(parent.transform, false);
                RectTransform containerRect = progressBarContainer.AddComponent<RectTransform>();
                containerRect.sizeDelta = new Vector2(0, 25);

                // Background
                GameObject background = new GameObject("Background");
                background.transform.SetParent(progressBarContainer.transform, false);
                RectTransform bgRect = background.AddComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;
                bgRect.anchoredPosition = Vector2.zero;

                Image bgImage = background.AddComponent<Image>();
                bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

                // Fill
                GameObject fill = new GameObject("Fill");
                fill.transform.SetParent(background.transform, false);
                RectTransform fillRect = fill.AddComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = new Vector2(0.5f, 1f); // 初始填充50%
                fillRect.sizeDelta = Vector2.zero;
                fillRect.anchoredPosition = Vector2.zero;

                Image fillImage = fill.AddComponent<Image>();
                fillImage.color = Color.green;
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;

                panel.bodyMovementProgressBar = fillImage;

                Undo.RegisterCreatedObjectUndo(progressBarContainer, "创建体动进度条");
            }
        }

        /// <summary>
        /// 创建 HealthMonitoringController GameObject
        /// </summary>
        private static void CreateHealthMonitoringController()
        {
            // 检查是否已存在
            HealthMonitoringController controller = FindObjectOfType<HealthMonitoringController>();
            if (controller != null)
            {
                Debug.Log("[HealthMonitoringUIGenerator] HealthMonitoringController already exists");
                return;
            }

            // 创建 GameObject
            GameObject controllerObj = new GameObject("HealthMonitoringController");
            controller = controllerObj.AddComponent<HealthMonitoringController>();

            Undo.RegisterCreatedObjectUndo(controllerObj, "创建 HealthMonitoringController");
            Debug.Log("[HealthMonitoringUIGenerator] HealthMonitoringController created");
        }

        /// <summary>
        /// 创建 TextMeshPro 文本
        /// </summary>
        private static GameObject CreateTextMeshPro(GameObject parent, string name, string text, int fontSize = 18)
        {
            // 检查是否已存在
            Transform existing = parent.transform.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, fontSize + 10);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;

            Undo.RegisterCreatedObjectUndo(textObj, $"创建 {name}");
            return textObj;
        }
    }
}

