using UnityEngine;
using NiceHouse.Data;
using NiceHouse.EnvironmentControl;
using NiceHouse.ControlHub;

namespace NiceHouse.Interaction
{
    /// <summary>
    /// Allows devices to be interacted with via mouse click (toggle control)
    /// Implements IRaycastInteractable interface, supports first-person raycast clicking
    /// </summary>
    [RequireComponent(typeof(DeviceDefinition))]
    [RequireComponent(typeof(Collider))]
    public class DeviceInteractable : MonoBehaviour, IRaycastInteractable
    {
        [Header("Interaction Settings")]
        [Tooltip("Hint text displayed on hover")]
        public string hoverHint = "Click to toggle";

        [Tooltip("Sound played on click (optional)")]
        public AudioClip clickSound;

        [Tooltip("Whether to show debug logs")]
        public bool enableDebugLog = false;

        private DeviceDefinition _deviceDef;
        private BaseDeviceController _controller;
        private AudioSource _audioSource;
        private bool _isHovering;

        private void Awake()
        {
            _deviceDef = GetComponent<DeviceDefinition>();
            if (_deviceDef == null)
            {
                Debug.LogError($"[DeviceInteractable] DeviceDefinition not found on {gameObject.name}");
            }

            // Try to get controller
            _controller = GetComponent<BaseDeviceController>();
            if (_controller == null)
            {
                Debug.LogWarning($"[DeviceInteractable] BaseDeviceController not found on {gameObject.name}. Device cannot be controlled.");
            }

            // Setup audio source (if sound needs to be played)
            if (clickSound != null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                    _audioSource.playOnAwake = false;
                }
            }

            // Ensure Collider is not a trigger (for physics collision detection)
            var collider = GetComponent<Collider>();
            if (collider != null && collider.isTrigger)
            {
                Debug.LogWarning($"[DeviceInteractable] Collider on {gameObject.name} is set as Trigger. Raycast may not work correctly.");
            }
        }

        /// <summary>
        /// Implements IRaycastInteractable interface
        /// </summary>
        public string HoverHint => GetHoverHint();

        public void OnHoverEnter(FPRaycastInteractor interactor)
        {
            if (_isHovering) return;
            _isHovering = true;

            if (enableDebugLog)
            {
                Debug.Log($"[DeviceInteractable] Hover Enter: {gameObject.name}");
            }

            // Can add hover effects here (e.g., highlight, glow, etc.)
            // E.g.: change material color, show hint, etc.
        }

        public void OnHoverExit(FPRaycastInteractor interactor)
        {
            if (!_isHovering) return;
            _isHovering = false;

            if (enableDebugLog)
            {
                Debug.Log($"[DeviceInteractable] Hover Exit: {gameObject.name}");
            }

            // Restore original state
        }

        public void OnRaycastClick(FPRaycastInteractor interactor)
        {
            if (_controller == null)
            {
                Debug.LogWarning($"[DeviceInteractable] Cannot control {gameObject.name}: Controller not found.");
                return;
            }

            // Play click sound
            if (_audioSource != null && clickSound != null)
            {
                _audioSource.PlayOneShot(clickSound);
            }

            // Toggle device state
            if (_controller.IsOn)
            {
                _controller.TurnOff();
                if (enableDebugLog)
                {
                    Debug.Log($"[DeviceInteractable] Turned OFF: {_deviceDef?.deviceId}");
                }
            }
            else
            {
                _controller.TurnOn();
                if (enableDebugLog)
                {
                    Debug.Log($"[DeviceInteractable] Turned ON: {_deviceDef?.deviceId}");
                }

                // If it's an air conditioner, can set target temperature
                if (_controller is AirConditionerController acController)
                {
                    // Use default target temperature or get from EnvironmentController
                    if (EnvironmentController.Instance != null && 
                        EnvironmentController.Instance.thresholds != null)
                    {
                        acController.SetTargetTemperature(
                            EnvironmentController.Instance.thresholds.targetTemperature
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Get hover hint text
        /// </summary>
        private string GetHoverHint()
        {
            if (string.IsNullOrEmpty(hoverHint))
            {
                // Generate default hint based on device type
                if (_deviceDef != null)
                {
                    string deviceName = _deviceDef.type switch
                    {
                        NiceHouse.Data.DeviceType.AirConditioner => "Air Conditioner",
                        NiceHouse.Data.DeviceType.Fan => "Fan",
                        NiceHouse.Data.DeviceType.AirPurifier => "Air Purifier",
                        NiceHouse.Data.DeviceType.FreshAirSystem => "Fresh Air System",
                        NiceHouse.Data.DeviceType.Light => "Light",
                        _ => "Device"
                    };

                    string state = _controller != null && _controller.IsOn ? "Turn Off" : "Turn On";
                    return $"Click to {state} {deviceName}";
                }

                return "Click to interact";
            }

            return hoverHint;
        }
    }
}

