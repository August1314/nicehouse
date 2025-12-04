using System.Collections.Generic;
using UnityEngine;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 从第一人称相机中心发射射线，用于点击墙面控制面板等 3D UI。
    /// </summary>
    public class FPRaycastInteractor : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float maxDistance = 4f;
        [SerializeField] private LayerMask interactableLayers = ~0;
        [SerializeField] private CrosshairUI crosshairUI;
        [SerializeField] private bool requireCursorLock = true;

        private IRaycastInteractable _current;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponentInChildren<Camera>();
            }
        }

        private void Update()
        {
            if (targetCamera == null) return;
            if (requireCursorLock && Cursor.lockState != CursorLockMode.Locked)
            {
                UpdateHoverTarget(null);
                return;
            }

            var ray = new Ray(targetCamera.transform.position, targetCamera.transform.forward);

            if (Physics.Raycast(ray, out var hit, maxDistance, interactableLayers, QueryTriggerInteraction.Ignore))
            {
                var interactable = ResolveInteractable(hit.collider);
                UpdateHoverTarget(interactable);

                if (interactable != null && Input.GetMouseButtonDown(0))
                {
                    interactable.OnRaycastClick(this);
                }
            }
            else
            {
                UpdateHoverTarget(null);
            }
        }

        private void UpdateHoverTarget(IRaycastInteractable next)
        {
            if (ReferenceEquals(_current, next)) return;

            _current?.OnHoverExit(this);
            _current = next;
            _current?.OnHoverEnter(this);

            crosshairUI?.SetHoverState(_current != null, _current?.HoverHint);
        }

        private IRaycastInteractable ResolveInteractable(Collider collider)
        {
            if (collider == null) return null;

            var buffer = ListPool<MonoBehaviour>.Get();
            collider.GetComponentsInParent(true, buffer);

            IRaycastInteractable interactable = null;
            foreach (var behaviour in buffer)
            {
                if (behaviour is IRaycastInteractable rayTarget)
                {
                    interactable = rayTarget;
                    break;
                }
            }

            ListPool<MonoBehaviour>.Release(buffer);
            return interactable;
        }
    }

    /// <summary>
    /// 简易 List 缓存，避免频繁 GC。
    /// </summary>
    internal static class ListPool<T>
    {
        private static readonly Stack<List<T>> Pool = new Stack<List<T>>();

        public static List<T> Get()
        {
            return Pool.Count > 0 ? Pool.Pop() : new List<T>();
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            Pool.Push(list);
        }
    }
}


