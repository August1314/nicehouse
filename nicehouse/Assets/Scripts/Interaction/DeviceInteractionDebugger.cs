using UnityEngine;
using NiceHouse.ControlHub;

namespace NiceHouse.Interaction
{
    /// <summary>
    /// Debug script to help diagnose mouse click interaction issues
    /// Add this to Player GameObject to see detailed raycast information
    /// </summary>
    public class DeviceInteractionDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [Tooltip("Enable detailed debug logging")]
        public bool enableDebugLog = true;
        
        [Tooltip("Draw raycast line in Scene view")]
        public bool drawRaycastLine = true;
        
        [Tooltip("Raycast distance")]
        public float maxDistance = 10f;
        
        [Header("References")]
        [Tooltip("Camera to use for raycast (auto-found if null)")]
        public Camera targetCamera;
        
        private FPRaycastInteractor _interactor;
        
        private void Awake()
        {
            // Try to find FPRaycastInteractor
            _interactor = GetComponent<FPRaycastInteractor>();
            if (_interactor == null)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning("[DeviceInteractionDebugger] FPRaycastInteractor not found on this GameObject. Please add it to enable mouse interaction.");
                }
            }
            
            // Auto-find camera if not assigned
            if (targetCamera == null)
            {
                targetCamera = GetComponentInChildren<Camera>();
                if (targetCamera == null)
                {
                    targetCamera = Camera.main;
                }
            }
        }
        
        private void Update()
        {
            if (targetCamera == null)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning("[DeviceInteractionDebugger] Target camera is null!");
                }
                return;
            }
            
            // Check cursor lock state
            bool cursorLocked = Cursor.lockState == CursorLockMode.Locked;
            
            // Perform raycast
            var ray = new Ray(targetCamera.transform.position, targetCamera.transform.forward);
            
            if (drawRaycastLine)
            {
                Color rayColor = cursorLocked ? Color.green : Color.red;
                Debug.DrawRay(ray.origin, ray.direction * maxDistance, rayColor);
            }
            
            if (Physics.Raycast(ray, out var hit, maxDistance))
            {
                if (enableDebugLog)
                {
                    Debug.Log($"[DeviceInteractionDebugger] Raycast HIT: {hit.collider.name} at distance {hit.distance:F2}m");
                }
                
                // Check for DeviceInteractable
                var interactable = hit.collider.GetComponent<DeviceInteractable>();
                if (interactable != null)
                {
                    if (enableDebugLog)
                    {
                        Debug.Log($"[DeviceInteractionDebugger] ✓ Found DeviceInteractable on {hit.collider.name}");
                    }
                    
                    if (!cursorLocked)
                    {
                        Debug.LogWarning($"[DeviceInteractionDebugger] ⚠ Cursor is NOT locked! FPRaycastInteractor requires cursor lock to work.");
                    }
                    
                    if (_interactor == null)
                    {
                        Debug.LogWarning($"[DeviceInteractionDebugger] ⚠ FPRaycastInteractor not found! Mouse clicks will not work.");
                    }
                }
                else
                {
                    if (enableDebugLog)
                    {
                        Debug.Log($"[DeviceInteractionDebugger] ✗ No DeviceInteractable found on {hit.collider.name}");
                    }
                }
            }
            else
            {
                if (enableDebugLog && Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
                {
                    Debug.Log("[DeviceInteractionDebugger] No raycast hit");
                }
            }
            
            // Display status on screen
            if (enableDebugLog)
            {
                DisplayStatus(cursorLocked);
            }
        }
        
        private void DisplayStatus(bool cursorLocked)
        {
            // This would require OnGUI, but for now just use Debug.Log
            // You can add OnGUI if you want on-screen display
        }
        
        private void OnGUI()
        {
            if (!enableDebugLog) return;
            
            GUIStyle style = new GUIStyle();
            style.fontSize = 16;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperLeft;
            
            float y = 10f;
            float lineHeight = 25f;
            
            GUI.Label(new Rect(10, y, 500, 30), $"Cursor Locked: {(Cursor.lockState == CursorLockMode.Locked ? "YES ✓" : "NO ✗")}", style);
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 500, 30), $"FPRaycastInteractor: {(_interactor != null ? "Found ✓" : "Missing ✗")}", style);
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 500, 30), $"Camera: {(targetCamera != null ? targetCamera.name : "None")}", style);
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 500, 30), "Press ESC to toggle cursor lock", style);
        }
    }
}

