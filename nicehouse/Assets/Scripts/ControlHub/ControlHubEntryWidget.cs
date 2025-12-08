using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 控制中枢面板中的单个入口卡片，负责展示状态并响应按钮点击。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ControlHubEntryWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private Image accentBar;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Transform visualRoot;

        [Header("Feedback")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float animationSpeed = 8f;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clickClip;

        private IControlPanelModule _module;
        private Action<string> _onSelected;
        private Vector3 _initialScale;
        private bool _isHovering;
        private Button _button;

        private void Awake()
        {
            if (visualRoot == null)
            {
                visualRoot = transform;
            }
            _initialScale = visualRoot.localScale;
            _button = GetComponent<Button>();

            if (_button != null)
            {
                _button.onClick.AddListener(HandleButtonClicked);
            }

            // 自动查找 Text 组件（如果 Inspector 中没有指定）
            AutoFindTextComponents();
        }

        private void AutoFindTextComponents()
        {
            // 自动查找 titleText
            if (titleText == null)
            {
                titleText = GetComponentInChildren<TMP_Text>();
                // 如果找到多个，尝试查找包含 "Title" 或 "Name" 的
                if (titleText == null)
                {
                    var allTexts = GetComponentsInChildren<TMP_Text>();
                    foreach (var text in allTexts)
                    {
                        if (text.name.Contains("Title") || text.name.Contains("Name") || 
                            text.name.Contains("title") || text.name.Contains("name"))
                        {
                            titleText = text;
                            break;
                        }
                    }
                    // 如果还是没找到，使用第一个
                    if (titleText == null && allTexts.Length > 0)
                    {
                        titleText = allTexts[0];
                    }
                }
            }

            // 自动查找 valueText（第二个 Text 组件）
            if (valueText == null)
            {
                var allTexts = GetComponentsInChildren<TMP_Text>();
                int foundCount = 0;
                foreach (var text in allTexts)
                {
                    if (text == titleText) continue;
                    if (text.name.Contains("Value") || text.name.Contains("value") ||
                        text.name.Contains("Status") || text.name.Contains("status"))
                    {
                        valueText = text;
                        break;
                    }
                    if (foundCount == 0)
                    {
                        valueText = text;
                    }
                    foundCount++;
                }
            }

            // 自动查找 hintText（第三个 Text 组件）
            if (hintText == null)
            {
                var allTexts = GetComponentsInChildren<TMP_Text>();
                int foundCount = 0;
                foreach (var text in allTexts)
                {
                    if (text == titleText || text == valueText) continue;
                    if (text.name.Contains("Hint") || text.name.Contains("hint") ||
                        text.name.Contains("Subtitle") || text.name.Contains("subtitle"))
                    {
                        hintText = text;
                        break;
                    }
                    if (foundCount == 0)
                    {
                        hintText = text;
                    }
                    foundCount++;
                }
            }
        }

        private void Update()
        {
            var targetScale = _isHovering ? _initialScale * hoverScale : _initialScale;
            visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, targetScale, Time.deltaTime * animationSpeed);
        }

        public void Bind(IControlPanelModule module, Action<string> onSelected)
        {
            _module = module;
            _onSelected = onSelected;

            if (titleText != null)
            {
                titleText.text = module.DisplayName;
            }

            if (iconImage != null)
            {
                iconImage.sprite = module.Icon;
                iconImage.enabled = module.Icon != null;
            }

            UpdateSnapshot(module.BuildSnapshot());
        }

        public void UpdateSnapshot(ControlHubModuleSnapshot snapshot)
        {
            if (valueText != null)
            {
                valueText.text = snapshot.value;
            }

            if (hintText != null)
            {
                hintText.text = snapshot.hint;
            }

            if (accentBar != null)
            {
                accentBar.color = snapshot.accentColor;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = snapshot.hasAlert ? 1f : 0.9f;
            }
        }

        private void HandleButtonClicked()
        {
            if (_module == null)
            {
                Debug.LogWarning("[ControlHubEntryWidget] Button clicked but no module is bound.", this);
                return;
            }

            if (clickClip != null)
            {
                if (audioSource == null)
                {
                    audioSource = GetComponent<AudioSource>();
                }

                audioSource?.PlayOneShot(clickClip);
            }

            if (_onSelected == null)
            {
                Debug.LogWarning($"[ControlHubEntryWidget] Button clicked for module '{_module.ModuleId}' but no selection callback is assigned.", this);
                return;
            }

            _onSelected.Invoke(_module.ModuleId);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
        }
    }
}


