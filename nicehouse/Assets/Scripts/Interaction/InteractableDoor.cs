using UnityEngine;
using NiceHouse.ControlHub;
using NiceHouse.Interaction;

namespace NiceHouse.Interaction
{
    [RequireComponent(typeof(Collider))]
    public class InteractableDoor : MonoBehaviour, IRaycastInteractable
    {
        [Header("门控制器")]
        [Tooltip("要控制的门控制器（如不指定则自动查找）")]
        public DoorController doorController;

        [Header("无控制器时的开门角度")]
        [Tooltip("未挂 DoorController 时，开门相对当前初始角度的偏移")]
        public float openAngleOffset = 90f;

        [Header("交互设置")]
        [Tooltip("交互距离限制（0表示无限制）")]
        public float interactionDistance = 0f;

        [Tooltip("开门时的提示文字")]
        public string closeHint = "关门";

        [Tooltip("关门时的提示文字")]
        public string openHint = "开门";

        [Header("视觉反馈")]
        [Tooltip("悬停时的高亮颜色")]
        public Color highlightColor = new Color(1f, 1f, 0.5f, 1f);

        [Tooltip("是否启用悬停高亮")]
        public bool enableHighlight = true;

        private Renderer _renderer;
        private Material _originalMaterial;
        private Color _originalColor;
        private bool _isHighlighted;
        private Vector3 _initialLocalEuler;
        private bool _isOpen;

        private void Awake()
        {
            // 自动查找DoorController
            if (doorController == null)
            {
                doorController = GetComponent<DoorController>();
                if (doorController == null)
                {
                    doorController = GetComponentInParent<DoorController>();
                }
            }

            _initialLocalEuler = transform.localEulerAngles;
            _isOpen = false;

            // 确保有Collider
            var collider = GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogWarning($"[InteractableDoor] {gameObject.name} 缺少 Collider 组件，已自动添加 BoxCollider");
                gameObject.AddComponent<BoxCollider>();
            }
        }

        private void Start()
        {
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
        }

        public void OnRaycastClick(FPRaycastInteractor interactor)
        {
            Debug.Log($"[InteractableDoor] {gameObject.name} 被点击");
            ToggleDoor();
        }

        public void OnHoverEnter(FPRaycastInteractor interactor)
        {
            if (enableHighlight && !_isHighlighted)
            {
                SetHighlight(true);
            }

            Debug.Log($"[InteractableDoor] {gameObject.name} 悬停进入");
        }

        public void OnHoverExit(FPRaycastInteractor interactor)
        {
            if (_isHighlighted)
            {
                SetHighlight(false);
            }

            Debug.Log($"[InteractableDoor] {gameObject.name} 悬停离开");
        }

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

        public void ToggleDoor()
        {
            // 优先使用 DoorController（带插值动画且保证局部旋转）
            if (doorController != null)
            {
                doorController.Toggle();
                _isOpen = doorController.IsDoorOpen;
                return;
            }

            // 无控制器时，直接在局部 Y 轴上以初始角度为基准旋转，不改动 X/Z
            _isOpen = !_isOpen;
            float targetY = _initialLocalEuler.y + (_isOpen ? openAngleOffset : 0f);
            transform.localRotation = Quaternion.Euler(_initialLocalEuler.x, targetY, _initialLocalEuler.z);
        }

        public float CurrentDoorAngle
        {
            get
            {
                return transform.localRotation.eulerAngles.y;
            }
        }

        public string HoverHint
        {
            get
            {
                return $"门角度: {CurrentDoorAngle:F1}°";
            }
        }
    }
}
