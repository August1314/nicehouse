using UnityEngine;
using NiceHouse.EnvironmentControl;
using NiceHouse.ControlHub;

namespace NiceHouse.Interaction
{
    /// <summary>
    /// 可交互灯光组件
    /// 挂载在灯光模型上，响应射线点击实现开关灯
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableLight : MonoBehaviour, IRaycastInteractable
    {
        [Header("灯光控制器")]
        [Tooltip("要控制的灯光控制器（如不指定则自动查找）")]
        public LightController lightController;

        [Header("交互设置")]
        [Tooltip("交互距离限制（0表示无限制）")]
        public float interactionDistance = 0f;
        
        [Tooltip("开灯时的提示文字")]
        public string turnOffHint = "关灯";
        
        [Tooltip("关灯时的提示文字")]
        public string turnOnHint = "开灯";

        [Header("视觉反馈")]
        [Tooltip("悬停时的高亮颜色")]
        public Color highlightColor = new Color(1f, 1f, 0.5f, 1f);
        
        [Tooltip("是否启用悬停高亮")]
        public bool enableHighlight = true;

        private Renderer _renderer;
        private Material _originalMaterial;
        private Color _originalColor;
        private bool _isHighlighted;

        private void Awake()
        {
            // 自动查找LightController
            if (lightController == null)
            {
                lightController = GetComponent<LightController>();
                if (lightController == null)
                {
                    lightController = GetComponentInParent<LightController>();
                }
                if (lightController == null)
                {
                    lightController = GetComponentInChildren<LightController>();
                }
            }

            // 获取Renderer用于高亮效果
            _renderer = GetComponent<Renderer>();
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<Renderer>();
            }

            if (_renderer != null)
            {
                _originalMaterial = _renderer.material;
                if (_originalMaterial.HasProperty("_BaseColor"))
                {
                    _originalColor = _originalMaterial.GetColor("_BaseColor");
                }
                else if (_originalMaterial.HasProperty("_Color"))
                {
                    _originalColor = _originalMaterial.GetColor("_Color");
                }
            }

            // 确保有Collider
            var collider = GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogWarning($"[InteractableLight] {gameObject.name} 缺少 Collider 组件，已自动添加 BoxCollider");
                gameObject.AddComponent<BoxCollider>();
            }
        }

        /// <summary>
        /// 当准星悬停进入时
        /// </summary>
        public void OnHoverEnter(FPRaycastInteractor interactor)
        {
            if (enableHighlight && !_isHighlighted)
            {
                SetHighlight(true);
            }
        }

        /// <summary>
        /// 当准星悬停离开时
        /// </summary>
        public void OnHoverExit(FPRaycastInteractor interactor)
        {
            if (_isHighlighted)
            {
                SetHighlight(false);
            }
        }

        /// <summary>
        /// 当玩家点击时
        /// </summary>
        public void OnRaycastClick(FPRaycastInteractor interactor)
        {
            if (lightController == null)
            {
                Debug.LogWarning($"[InteractableLight] {gameObject.name} 没有关联的 LightController");
                return;
            }

            // 切换灯光状态
            lightController.Toggle();
            
            Debug.Log($"[InteractableLight] {gameObject.name} 灯光已{(lightController.IsLightOn ? "开启" : "关闭")}");
        }

        /// <summary>
        /// 悬停提示文字
        /// </summary>
        public string HoverHint
        {
            get
            {
                if (lightController == null) return "灯光";
                return lightController.IsLightOn ? turnOffHint : turnOnHint;
            }
        }

        /// <summary>
        /// 设置高亮状态
        /// </summary>
        private void SetHighlight(bool highlight)
        {
            _isHighlighted = highlight;

            if (_renderer != null && _originalMaterial != null)
            {
                if (highlight)
                {
                    if (_originalMaterial.HasProperty("_BaseColor"))
                    {
                        _originalMaterial.SetColor("_BaseColor", highlightColor);
                    }
                    else if (_originalMaterial.HasProperty("_Color"))
                    {
                        _originalMaterial.SetColor("_Color", highlightColor);
                    }
                }
                else
                {
                    if (_originalMaterial.HasProperty("_BaseColor"))
                    {
                        _originalMaterial.SetColor("_BaseColor", _originalColor);
                    }
                    else if (_originalMaterial.HasProperty("_Color"))
                    {
                        _originalMaterial.SetColor("_Color", _originalColor);
                    }
                }
            }
        }

        private void OnDisable()
        {
            // 确保禁用时恢复原始颜色
            if (_isHighlighted)
            {
                SetHighlight(false);
            }
        }
    }
}
