using UnityEngine;
using NiceHouse.ControlHub;
using NiceHouse.Interaction;

namespace NiceHouse.Interaction
{
    /// <summary>
    /// 原有的门交互脚本：射线点击后简单旋转门到 0/90 度。
    /// 仅新增可选的 DoorAudio 播放，不改原旋转逻辑。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableDoor : MonoBehaviour, IRaycastInteractable
    {
        [Header("门控制器")]
        [Tooltip("要控制的门控制器（如不指定则自动查找）")]
        public DoorController doorController;

        [Tooltip("门开关音效（可选）")]
        public DoorAudio doorAudio;

        [Header("交互设置")]
        [Tooltip("交互距离限制（0 表示无限制）")]
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
            // 自动查找DoorController（保持原行为，仅用于兼容）
            if (doorController == null)
            {
                doorController = GetComponent<DoorController>();
                if (doorController == null)
                {
                    doorController = GetComponentInParent<DoorController>();
                }
            }

            // 自动查找DoorAudio（新增，可选）
            if (doorAudio == null)
            {
                doorAudio = GetComponent<DoorAudio>() ?? GetComponentInParent<DoorAudio>();
            }

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
            ToggleDoor();
        }

        public void OnHoverEnter(FPRaycastInteractor interactor)
        {
            if (enableHighlight && !_isHighlighted)
            {
                SetHighlight(true);
            }
        }

        public void OnHoverExit(FPRaycastInteractor interactor)
        {
            if (_isHighlighted)
            {
                SetHighlight(false);
            }
        }

        private void SetHighlight(bool highlight)
        {
            _isHighlighted = highlight;

            if (_renderer != null && _originalMaterial != null)
            {
                if (highlight)
                {
                    if (_originalMaterial.HasProperty("_BaseColor"))
                        _originalMaterial.SetColor("_BaseColor", highlightColor);
                    else if (_originalMaterial.HasProperty("_Color"))
                        _originalMaterial.SetColor("_Color", highlightColor);
                }
                else
                {
                    if (_originalMaterial.HasProperty("_BaseColor"))
                        _originalMaterial.SetColor("_BaseColor", _originalColor);
                    else if (_originalMaterial.HasProperty("_Color"))
                        _originalMaterial.SetColor("_Color", _originalColor);
                }
            }
        }

        public void ToggleDoor()
        {
            // 保留原有的简单旋转逻辑（0/90 切换）
            var rotation = transform.rotation;
            var targetAngleIsOpen = rotation.eulerAngles.y == 0f; // 当前0则即将开门
            var targetAngle = targetAngleIsOpen ? 90f : 0f;
            transform.rotation = Quaternion.Euler(rotation.eulerAngles.x, targetAngle, rotation.eulerAngles.z);

            // 播放音效（不影响原逻辑）
            if (doorAudio != null)
            {
                if (targetAngleIsOpen)
                    doorAudio.PlayOpen();
                else
                    doorAudio.PlayClose();
            }
        }

        public float CurrentDoorAngle => transform.rotation.eulerAngles.y;

        public string HoverHint => doorController != null
            ? (doorController.IsDoorOpen ? closeHint : openHint)
            : $"门角度 {CurrentDoorAngle:F1}°";
    }
}
