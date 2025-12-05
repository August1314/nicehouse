using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
        [Tooltip("If false, interaction works like UI buttons (no cursor lock required). If true, requires cursor lock for FPS-style interaction.")]
        [SerializeField] private bool requireCursorLock = false;

        private IRaycastInteractable _current;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponentInChildren<Camera>();
            }
        }

        [Header("Debug")]
        [Tooltip("Enable debug logging for raycast hits")]
        public bool enableDebugLog = false;
        
        [Tooltip("Always log when cursor is not locked (helps diagnose issues)")]
        public bool logCursorLockStatus = false;

        private void Update()
        {
            if (targetCamera == null)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning("[FPRaycastInteractor] Target camera is null!");
                }
                return;
            }
            
            // Determine ray origin and direction based on cursor lock state
            Ray ray;
            bool isCursorLocked = Cursor.lockState == CursorLockMode.Locked;
            
            if (requireCursorLock && !isCursorLocked)
            {
                // Only log if explicitly enabled (not by default)
                if (enableDebugLog && Time.frameCount % 60 == 0)
                {
                    Debug.Log("[FPRaycastInteractor] Cursor not locked, skipping raycast. Lock cursor to enable interaction.");
                }
                UpdateHoverTarget(null);
                return;
            }
            
            // If cursor is locked, use camera forward (FPS style)
            // If cursor is not locked, use mouse position (UI button style)
            if (isCursorLocked)
            {
                ray = new Ray(targetCamera.transform.position, targetCamera.transform.forward);
            }
            else
            {
                // Use mouse position to create ray (like UI buttons)
                ray = targetCamera.ScreenPointToRay(Input.mousePosition);
            }
            
            // Draw raycast line in Scene view for debugging
            if (enableDebugLog)
            {
                Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red);
            }

            if (Physics.Raycast(ray, out var hit, maxDistance, interactableLayers, QueryTriggerInteraction.Ignore))
            {
                if (enableDebugLog)
                {
                    Debug.Log($"[FPRaycastInteractor] Hit: {hit.collider.name} at distance {hit.distance:F2}m");
                }
                
                var interactable = ResolveInteractable(hit.collider);
                
                if (enableDebugLog)
                {
                    if (interactable != null)
                    {
                        Debug.Log($"[FPRaycastInteractor] Found interactable: {interactable.GetType().Name} on {hit.collider.name}");
                    }
                    else
                    {
                        Debug.Log($"[FPRaycastInteractor] No interactable found on {hit.collider.name}");
                    }
                }
                
                UpdateHoverTarget(interactable);

                // Check for click - only process if not clicking on UI
                if (interactable != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        // Check if mouse is over UI element
                        bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                        
                        if (enableDebugLog)
                        {
                            Debug.Log($"[FPRaycastInteractor] Mouse button down detected. IsOverUI: {isOverUI}, Interactable: {interactable.GetType().Name}");
                        }
                        
                        if (!isOverUI)
                        {
                            if (enableDebugLog)
                            {
                                Debug.Log($"[FPRaycastInteractor] Click detected, calling OnRaycastClick on {interactable.GetType().Name}");
                            }
                            interactable.OnRaycastClick(this);
                        }
                        else
                        {
                            if (enableDebugLog)
                            {
                                Debug.Log("[FPRaycastInteractor] Click blocked by UI element");
                            }
                        }
                    }
                }
            }
            else
            {
                if (enableDebugLog && Time.frameCount % 60 == 0)
                {
                    Debug.Log("[FPRaycastInteractor] No raycast hit");
                }
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


