using UnityEngine;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 自动根据 RectTransform 调整 HubEntry 的 BoxCollider，并检查必备组件配置。
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(BoxCollider))]
    public class HubEntryConfigurator : MonoBehaviour
    {
        [Header("Collider Settings")]
        [Tooltip("Z 方向厚度（米），用于射线检测。")]
        [Min(0.001f)]
        public float colliderDepth = 0.01f;

        [Tooltip("额外的尺寸扩展（米）。")]
        public Vector2 padding = Vector2.zero;

        private RectTransform _rectTransform;
        private BoxCollider _collider;

        private void Reset()
        {
            SyncComponents();
            UpdateCollider();
        }

        private void OnValidate()
        {
            SyncComponents();
            UpdateCollider();
            VerifySetup();
        }

        [ContextMenu("Update Collider From RectTransform")]
        public void UpdateCollider()
        {
            if (_rectTransform == null || _collider == null) return;

            // 计算世界空间尺寸
            var rect = _rectTransform.rect;
            var lossy = _rectTransform.lossyScale;

            var worldWidth = rect.width * lossy.x + padding.x;
            var worldHeight = rect.height * lossy.y + padding.y;

            if (worldWidth <= 0f || worldHeight <= 0f)
            {
                return;
            }

            _collider.size = new Vector3(worldWidth, worldHeight, colliderDepth);
            _collider.center = Vector3.zero;
            _collider.isTrigger = true;
        }

        private void SyncComponents()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if (_collider == null)
            {
                _collider = GetComponent<BoxCollider>();
            }
        }

        private void VerifySetup()
        {
            if (!Application.isPlaying)
            {
                if (GetComponent<ControlHubEntryWidget>() == null)
                {
                    Debug.LogWarning($"{name} 缺少 ControlHubEntryWidget 组件，无法响应射线点击。", this);
                }

                if (gameObject.layer == 0) // Default
                {
                    Debug.LogWarning($"{name} 仍在 Default Layer，记得切换到 FPRaycastInteractor 的 Interactable Layer。", this);
                }
            }
        }
    }
}




