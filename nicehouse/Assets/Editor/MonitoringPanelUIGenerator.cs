using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using NiceHouse.SmartMonitoring;

namespace NiceHouse.Editor
{
    /// <summary>
    /// MonitoringPanel UI 自动生成工具
    /// 使用方法：在 Unity 菜单栏选择 "NiceHouse/UI/生成 MonitoringPanel UI"
    /// </summary>
    public class MonitoringPanelUIGenerator : EditorWindow
    {
        [MenuItem("NiceHouse/UI/生成 MonitoringPanel UI")]
        public static void GenerateUI()
        {
            // 查找或创建 MonitoringPanel
            MonitoringPanel panel = FindObjectOfType<MonitoringPanel>();
            if (panel == null)
            {
                // 查找 Canvas
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    EditorUtility.DisplayDialog("Error", "Canvas not found!\nPlease create a Canvas first.", "OK");
                    return;
                }

                // 创建 MonitoringPanel GameObject
                GameObject panelObj = new GameObject("MonitoringPanel");
                panelObj.transform.SetParent(canvas.transform, false);
                panel = panelObj.AddComponent<MonitoringPanel>();

                // 添加 Panel 组件
                Image panelImage = panelObj.AddComponent<Image>();
                panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

                // 设置 RectTransform
                RectTransform panelRect = panelObj.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0f, 0f);
                panelRect.anchorMax = new Vector2(0.5f, 1f);
                panelRect.sizeDelta = Vector2.zero;
                panelRect.anchoredPosition = Vector2.zero;

                Undo.RegisterCreatedObjectUndo(panelObj, "创建 MonitoringPanel");
            }

            GameObject panelGameObject = panel.gameObject;
            Undo.RegisterCompleteObjectUndo(panelGameObject, "生成 MonitoringPanel UI");

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

            // 1. 创建状态显示区域
            CreateStateDisplaySection(panelGameObject, panel);

            // 2. 创建状态切换按钮区域
            CreateStateButtonsSection(panelGameObject, panel);

            // 3. 创建房间选择区域
            CreateRoomSelectionSection(panelGameObject, panel);

            // 4. 创建告警列表区域
            CreateAlarmListSection(panelGameObject, panel);

            // 5. 创建告警设置区域
            CreateAlarmSettingsSection(panelGameObject, panel);

            // 6. 创建告警项预制体
            CreateAlarmItemPrefab();

            EditorUtility.DisplayDialog("Complete", "MonitoringPanel UI generation complete!\n\nCreated:\n- State display texts (3)\n- State switch buttons (7)\n- Room dropdown\n- Alarm list scroll view\n- Alarm settings inputs and buttons\n- Alarm item prefab\n\nPlease check if script field bindings are correct.", "OK");
            Debug.Log("[MonitoringPanelUIGenerator] UI 生成完成！");
        }

        /// <summary>
        /// 创建状态显示区域
        /// </summary>
        private static void CreateStateDisplaySection(GameObject parent, MonitoringPanel panel)
        {
            // StateText
            if (panel.stateText == null)
            {
                GameObject stateText = CreateTextMeshPro(parent, "StateText", "State: Idle", 20);
                panel.stateText = stateText.GetComponent<TextMeshProUGUI>();
            }

            // RoomText
            if (panel.roomText == null)
            {
                GameObject roomText = CreateTextMeshPro(parent, "RoomText", "Room: LivingRoom01", 18);
                panel.roomText = roomText.GetComponent<TextMeshProUGUI>();
            }

            // DurationText
            if (panel.durationText == null)
            {
                GameObject durationText = CreateTextMeshPro(parent, "DurationText", "Duration: 00:00", 18);
                panel.durationText = durationText.GetComponent<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// 创建状态切换按钮区域
        /// </summary>
        private static void CreateStateButtonsSection(GameObject parent, MonitoringPanel panel)
        {
            // 检查是否已存在按钮容器
            Transform existingContainer = parent.transform.Find("StateButtonsContainer");
            GameObject container;
            
            if (existingContainer != null)
            {
                container = existingContainer.gameObject;
            }
            else
            {
                // 创建容器
                container = new GameObject("StateButtonsContainer");
                container.transform.SetParent(parent.transform, false);
                RectTransform containerRect = container.AddComponent<RectTransform>();
                containerRect.sizeDelta = new Vector2(0, 40);

                HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 5f;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;

                Undo.RegisterCreatedObjectUndo(container, "创建状态按钮容器");
            }

            // 创建按钮（如果不存在）
            if (panel.idleButton == null)
                panel.idleButton = CreateButton(container, "IdleButton", "Idle").GetComponent<Button>();
            if (panel.walkingButton == null)
                panel.walkingButton = CreateButton(container, "WalkingButton", "Walking").GetComponent<Button>();
            if (panel.sittingButton == null)
                panel.sittingButton = CreateButton(container, "SittingButton", "Sitting").GetComponent<Button>();
            if (panel.bathingButton == null)
                panel.bathingButton = CreateButton(container, "BathingButton", "Bathing").GetComponent<Button>();
            if (panel.sleepingButton == null)
                panel.sleepingButton = CreateButton(container, "SleepingButton", "Sleeping").GetComponent<Button>();
            if (panel.fallenButton == null)
                panel.fallenButton = CreateButton(container, "FallenButton", "Fallen").GetComponent<Button>();
            if (panel.outOfBedButton == null)
                panel.outOfBedButton = CreateButton(container, "OutOfBedButton", "OutOfBed").GetComponent<Button>();
        }

        /// <summary>
        /// 创建房间选择区域
        /// </summary>
        private static void CreateRoomSelectionSection(GameObject parent, MonitoringPanel panel)
        {
            if (panel.roomDropdown == null)
            {
                GameObject dropdown = CreateDropdown(parent, "RoomDropdown");
                panel.roomDropdown = dropdown.GetComponent<TMP_Dropdown>();

                // 添加默认选项
                TMP_Dropdown dropdownComponent = dropdown.GetComponent<TMP_Dropdown>();
                dropdownComponent.options.Clear();
                dropdownComponent.options.Add(new TMP_Dropdown.OptionData("LivingRoom01"));
                dropdownComponent.options.Add(new TMP_Dropdown.OptionData("BedRoom01"));
                dropdownComponent.options.Add(new TMP_Dropdown.OptionData("Kitchen01"));
                dropdownComponent.options.Add(new TMP_Dropdown.OptionData("BathRoom01"));
            }
        }

        /// <summary>
        /// 创建告警列表区域
        /// </summary>
        private static void CreateAlarmListSection(GameObject parent, MonitoringPanel panel)
        {
            // 检查是否已存在
            Transform existingScrollView = parent.transform.Find("AlarmListScrollView");
            GameObject scrollView;
            
            if (existingScrollView != null)
            {
                scrollView = existingScrollView.gameObject;
                // 找到 Content
                Transform contentTransform = scrollView.transform.Find("Viewport/Content");
                if (contentTransform != null)
                {
                    panel.alarmListContent = contentTransform.GetComponent<RectTransform>();
                }
                return; // 已存在，不需要重新创建
            }

            // 创建 ScrollView
            scrollView = new GameObject("AlarmListScrollView");
            scrollView.transform.SetParent(parent.transform, false);
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.sizeDelta = new Vector2(0, 200);

            ScrollRect scrollRectComponent = scrollView.AddComponent<ScrollRect>();
            Image scrollImage = scrollView.AddComponent<Image>();
            scrollImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            // 创建 Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;

            Mask mask = viewport.AddComponent<Mask>();
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0);

            // 创建 Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            contentRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup contentVlg = content.AddComponent<VerticalLayoutGroup>();
            contentVlg.spacing = 5f;
            contentVlg.padding = new RectOffset(5, 5, 5, 5);
            contentVlg.childControlWidth = true;
            contentVlg.childControlHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 配置 ScrollRect
            scrollRectComponent.content = contentRect;
            scrollRectComponent.viewport = viewportRect;
            scrollRectComponent.horizontal = false;
            scrollRectComponent.vertical = true;

            panel.alarmListContent = contentRect;

            Undo.RegisterCreatedObjectUndo(scrollView, "创建告警列表滚动视图");
        }

        /// <summary>
        /// 创建告警设置区域
        /// </summary>
        private static void CreateAlarmSettingsSection(GameObject parent, MonitoringPanel panel)
        {
            // 久坐阈值输入框
            if (panel.longSittingThresholdInput == null)
            {
                GameObject sittingInput = CreateInputField(parent, "LongSittingThresholdInput", "30");
                panel.longSittingThresholdInput = sittingInput.GetComponent<TMP_InputField>();
                TMP_InputField sittingInputField = sittingInput.GetComponent<TMP_InputField>();
                sittingInputField.contentType = TMP_InputField.ContentType.DecimalNumber; // 允许小数输入
                sittingInputField.characterLimit = 10; // 增加字符限制以支持小数
            }

            // 久浴阈值输入框
            if (panel.longBathingThresholdInput == null)
            {
                GameObject bathingInput = CreateInputField(parent, "LongBathingThresholdInput", "20");
                panel.longBathingThresholdInput = bathingInput.GetComponent<TMP_InputField>();
                TMP_InputField bathingInputField = bathingInput.GetComponent<TMP_InputField>();
                bathingInputField.contentType = TMP_InputField.ContentType.DecimalNumber; // 允许小数输入
                bathingInputField.characterLimit = 10; // 增加字符限制以支持小数
            }

            // 监测开关
            if (panel.enableMonitoringToggle == null)
            {
                GameObject toggle = CreateToggle(parent, "EnableMonitoringToggle", "Enable Monitoring");
                panel.enableMonitoringToggle = toggle.GetComponent<Toggle>();
                Toggle toggleComponent = toggle.GetComponent<Toggle>();
                toggleComponent.isOn = true;
            }

            // 应用设置按钮
            if (panel.applySettingsButton == null)
            {
                GameObject applyButton = CreateButton(parent, "ApplySettingsButton", "Apply Settings");
                panel.applySettingsButton = applyButton.GetComponent<Button>();
            }
        }

        /// <summary>
        /// 创建告警项预制体
        /// </summary>
        private static void CreateAlarmItemPrefab()
        {
            // 检查是否已存在
            string prefabPath = "Assets/Prefabs/AlarmItemPrefab.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log("[MonitoringPanelUIGenerator] 告警项预制体已存在，跳过创建");
                return;
            }

            // 创建 Prefabs 文件夹
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // 创建预制体 GameObject
            GameObject prefab = new GameObject("AlarmItemPrefab");
            RectTransform prefabRect = prefab.AddComponent<RectTransform>();
            prefabRect.sizeDelta = new Vector2(0, 30);

            Image prefabImage = prefab.AddComponent<Image>();
            prefabImage.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

            HorizontalLayoutGroup prefabHlg = prefab.AddComponent<HorizontalLayoutGroup>();
            prefabHlg.spacing = 10f;
            prefabHlg.padding = new RectOffset(5, 5, 5, 5);
            prefabHlg.childControlWidth = false;
            prefabHlg.childControlHeight = false;

            // 创建子元素
            GameObject typeText = CreateTextMeshPro(prefab, "TypeText", "Alarm Type", 14);
            RectTransform typeRect = typeText.GetComponent<RectTransform>();
            typeRect.sizeDelta = new Vector2(120, 0);

            GameObject roomText = CreateTextMeshPro(prefab, "RoomText", "Room", 14);
            RectTransform roomRect = roomText.GetComponent<RectTransform>();
            roomRect.sizeDelta = new Vector2(100, 0);

            GameObject timeText = CreateTextMeshPro(prefab, "TimeText", "Time", 14);
            RectTransform timeRect = timeText.GetComponent<RectTransform>();
            timeRect.sizeDelta = new Vector2(80, 0);

            GameObject statusText = CreateTextMeshPro(prefab, "StatusText", "Status", 14);
            RectTransform statusRect = statusText.GetComponent<RectTransform>();
            statusRect.sizeDelta = new Vector2(60, 0);

            GameObject handleButton = CreateButton(prefab, "HandleButton", "Handle");
            RectTransform buttonRect = handleButton.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(70, 25);

            // 保存为预制体
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            DestroyImmediate(prefab);

            Debug.Log($"[MonitoringPanelUIGenerator] 告警项预制体已创建: {prefabPath}");
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

        /// <summary>
        /// 创建按钮
        /// </summary>
        private static GameObject CreateButton(GameObject parent, string name, string buttonText)
        {
            // 检查是否已存在
            Transform existing = parent.transform.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 30);

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            Button button = buttonObj.AddComponent<Button>();

            // 创建文本子对象
            GameObject textObj = new GameObject("Text (TMP)");
            textObj.transform.SetParent(buttonObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = buttonText;
            tmp.fontSize = 14;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            button.targetGraphic = image;

            Undo.RegisterCreatedObjectUndo(buttonObj, $"创建 {name}");
            return buttonObj;
        }

        /// <summary>
        /// 创建下拉菜单
        /// </summary>
        private static GameObject CreateDropdown(GameObject parent, string name)
        {
            // 检查是否已存在
            Transform existing = parent.transform.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject dropdownObj = new GameObject(name);
            dropdownObj.transform.SetParent(parent.transform, false);

            RectTransform rect = dropdownObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 30);

            Image image = dropdownObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

            // 创建 Label
            GameObject label = new GameObject("Label");
            label.transform.SetParent(dropdownObj.transform, false);
            RectTransform labelRect = label.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.offsetMin = new Vector2(10, 6);
            labelRect.offsetMax = new Vector2(-25, -7);

            TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
            labelText.text = "Select Room";
            labelText.fontSize = 14;
            labelText.color = new Color(0.196f, 0.196f, 0.196f, 0.5f);

            // 创建 Arrow
            GameObject arrow = new GameObject("Arrow");
            arrow.transform.SetParent(dropdownObj.transform, false);
            RectTransform arrowRect = arrow.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1, 0.5f);
            arrowRect.anchorMax = new Vector2(1, 0.5f);
            arrowRect.sizeDelta = new Vector2(20, 20);
            arrowRect.anchoredPosition = new Vector2(-15, 0);

            Image arrowImage = arrow.AddComponent<Image>();
            arrowImage.color = new Color(0.196f, 0.196f, 0.196f, 0.5f);

            // 创建 Template (下拉列表模板)
            GameObject template = new GameObject("Template");
            template.transform.SetParent(dropdownObj.transform, false);
            template.SetActive(false);

            RectTransform templateRect = template.AddComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1);
            templateRect.anchoredPosition = new Vector2(0, 2);
            templateRect.sizeDelta = new Vector2(0, 150);

            Image templateImage = template.AddComponent<Image>();
            templateImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            ScrollRect templateScroll = template.AddComponent<ScrollRect>();
            templateScroll.horizontal = false;
            templateScroll.vertical = true;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;

            Mask viewportMask = viewport.AddComponent<Mask>();
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0);

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 28);
            contentRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup contentVlg = content.AddComponent<VerticalLayoutGroup>();
            ContentSizeFitter contentCsf = content.AddComponent<ContentSizeFitter>();
            contentCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            templateScroll.content = contentRect;
            templateScroll.viewport = viewportRect;

            // Item
            GameObject item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            RectTransform itemRect = item.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0, 20);

            Toggle itemToggle = item.AddComponent<Toggle>();
            itemToggle.targetGraphic = item.AddComponent<Image>();

            GameObject itemLabel = new GameObject("Item Label");
            itemLabel.transform.SetParent(item.transform, false);
            RectTransform itemLabelRect = itemLabel.AddComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.sizeDelta = Vector2.zero;
            itemLabelRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI itemLabelText = itemLabel.AddComponent<TextMeshProUGUI>();
            itemLabelText.text = "Option A";
            itemLabelText.fontSize = 14;

            itemToggle.graphic = itemLabelText;

            // 配置 Dropdown
            dropdown.targetGraphic = image;
            dropdown.captionText = labelText;
            dropdown.itemText = itemLabelText;
            dropdown.options.Clear();
            dropdown.template = templateRect;
            dropdown.value = 0;

            Undo.RegisterCreatedObjectUndo(dropdownObj, $"创建 {name}");
            return dropdownObj;
        }

        /// <summary>
        /// 创建输入框
        /// </summary>
        private static GameObject CreateInputField(GameObject parent, string name, string placeholder)
        {
            // 检查是否已存在
            Transform existing = parent.transform.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent.transform, false);

            RectTransform rect = inputObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 30);

            Image image = inputObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();

            // 创建 Text Area
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.sizeDelta = Vector2.zero;
            textAreaRect.anchoredPosition = Vector2.zero;

            RectMask2D textAreaMask = textArea.AddComponent<RectMask2D>();

            // 创建 Text
            GameObject text = new GameObject("Text");
            text.transform.SetParent(textArea.transform, false);
            RectTransform textRect = text.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            textRect.offsetMin = new Vector2(10, 6);
            textRect.offsetMax = new Vector2(-10, -6);

            TextMeshProUGUI textTmp = text.AddComponent<TextMeshProUGUI>();
            textTmp.text = "";
            textTmp.fontSize = 14;
            textTmp.color = Color.white;

            // 创建 Placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.sizeDelta = Vector2.zero;
            placeholderRect.anchoredPosition = Vector2.zero;
            placeholderRect.offsetMin = new Vector2(10, 6);
            placeholderRect.offsetMax = new Vector2(-10, -6);

            TextMeshProUGUI placeholderTmp = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderTmp.text = placeholder;
            placeholderTmp.fontSize = 14;
            placeholderTmp.color = new Color(0.196f, 0.196f, 0.196f, 0.5f);
            placeholderTmp.fontStyle = FontStyles.Italic;

            // 配置 InputField
            inputField.textViewport = textAreaRect;
            inputField.textComponent = textTmp;
            inputField.placeholder = placeholderTmp;
            inputField.targetGraphic = image;

            Undo.RegisterCreatedObjectUndo(inputObj, $"创建 {name}");
            return inputObj;
        }

        /// <summary>
        /// 创建开关
        /// </summary>
        private static GameObject CreateToggle(GameObject parent, string name, string labelText)
        {
            // 检查是否已存在
            Transform existing = parent.transform.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject toggleObj = new GameObject(name);
            toggleObj.transform.SetParent(parent.transform, false);

            RectTransform rect = toggleObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 20);

            Toggle toggle = toggleObj.AddComponent<Toggle>();

            // 创建 Background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(toggleObj.transform, false);
            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(0, 0.5f);
            bgRect.sizeDelta = new Vector2(20, 20);
            bgRect.anchoredPosition = new Vector2(10, 0);

            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // 创建 Checkmark
            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(background.transform, false);
            RectTransform checkRect = checkmark.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.sizeDelta = Vector2.zero;
            checkRect.anchoredPosition = Vector2.zero;

            Image checkImage = checkmark.AddComponent<Image>();
            checkImage.color = Color.white;

            // 创建 Label
            GameObject label = new GameObject("Label");
            label.transform.SetParent(toggleObj.transform, false);
            RectTransform labelRect = label.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.offsetMin = new Vector2(35, 0);
            labelRect.offsetMax = new Vector2(0, 0);

            TextMeshProUGUI labelTmp = label.AddComponent<TextMeshProUGUI>();
            labelTmp.text = labelText;
            labelTmp.fontSize = 14;
            labelTmp.color = Color.white;

            // 配置 Toggle
            toggle.graphic = checkImage;
            toggle.targetGraphic = bgImage;

            Undo.RegisterCreatedObjectUndo(toggleObj, $"创建 {name}");
            return toggleObj;
        }
    }
}

