using UnityEngine;
using NiceHouse.ControlHub;
using NiceHouse.Data;
using System.Linq;

namespace NiceHouse.Interaction
{
    /// <summary>
    /// 可交互求助按钮组件
    /// 挂载在求助按钮模型上，点击时触发报警灯闪烁和紧急呼叫告警
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableHelpButton : MonoBehaviour, IRaycastInteractable
    {
        [Header("报警灯设置")]
        [Tooltip("要触发的报警灯（留空则自动查找全局报警灯）")]
        public FlashingLight targetAlarmLight;
        
        [Tooltip("是否自动查找全局报警灯")]
        public bool autoFindGlobalAlarmLight = true;

        [Header("告警设置")]
        [Tooltip("点击时是否触发紧急呼叫告警")]
        public bool triggerEmergencyAlarm = true;
        
        [Tooltip("告警使用的房间ID（留空则从DeviceDefinition获取）")]
        public string roomId;

        [Header("交互设置")]
        [Tooltip("交互距离限制（0表示无限制）")]
        public float interactionDistance = 0f;
        
        [Tooltip("点击时的提示文字")]
        public string clickHint = "触发紧急呼叫";

        [Header("视觉反馈")]
        [Tooltip("悬停时的高亮颜色")]
        public Color highlightColor = new Color(1f, 0.5f, 0.5f, 1f);
        
        [Tooltip("是否启用悬停高亮")]
        public bool enableHighlight = true;

        private Renderer _renderer;
        private Material _originalMaterial;
        private Color _originalColor;
        private bool _isHighlighted;
        private DeviceDefinition _deviceDefinition;

        private void Awake()
        {
            // 检查是否有旧的 EmergencyButton 脚本，如果有则禁用它的 OnMouseDown（避免冲突）
            var emergencyButton = GetComponent<NiceHouse.UI.EmergencyButton>();
            if (emergencyButton != null)
            {
                Debug.LogWarning($"[InteractableHelpButton] {gameObject.name} 上同时存在 EmergencyButton 和 InteractableHelpButton 脚本。建议移除 EmergencyButton 脚本，只使用 InteractableHelpButton。");
                // 不自动禁用，让用户手动移除，但给出警告
            }

            // 获取 DeviceDefinition
            _deviceDefinition = GetComponent<DeviceDefinition>();
            if (_deviceDefinition != null && string.IsNullOrEmpty(roomId))
            {
                roomId = _deviceDefinition.roomId;
            }

            // 如果没有指定房间ID，尝试从父对象获取
            if (string.IsNullOrEmpty(roomId))
            {
                var parentDevice = GetComponentInParent<DeviceDefinition>();
                if (parentDevice != null)
                {
                    roomId = parentDevice.roomId;
                }
            }

            // 如果没有指定报警灯，尝试自动查找
            if (targetAlarmLight == null && autoFindGlobalAlarmLight)
            {
                FindGlobalAlarmLight();
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
                Debug.LogWarning($"[InteractableHelpButton] {gameObject.name} 缺少 Collider 组件，已自动添加 BoxCollider");
                gameObject.AddComponent<BoxCollider>();
            }
        }

        /// <summary>
        /// 自动查找全局报警灯
        /// </summary>
        private void FindGlobalAlarmLight()
        {
            var flashingLights = FindObjectsOfType<FlashingLight>();
            foreach (var flashingLight in flashingLights)
            {
                if (flashingLight.isGlobalAlarmLight)
                {
                    targetAlarmLight = flashingLight;
                    Debug.Log($"[InteractableHelpButton] 找到全局报警灯: {flashingLight.gameObject.name}");
                    return;
                }
            }
            
            Debug.LogWarning($"[InteractableHelpButton] 未找到全局报警灯，请手动指定或确保报警灯上挂载了 FlashingLight 且 isGlobalAlarmLight=true");
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
            // 如果报警灯未找到，尝试重新查找
            if (targetAlarmLight == null && autoFindGlobalAlarmLight)
            {
                FindGlobalAlarmLight();
            }

            if (targetAlarmLight == null)
            {
                Debug.LogError($"[InteractableHelpButton] {gameObject.name} 没有找到报警灯！请检查：1) 报警灯上是否有 FlashingLight 组件 2) FlashingLight 的 isGlobalAlarmLight 是否为 true");
                return;
            }

            // 检查报警灯是否正在闪烁（手动或告警系统控制）
            // 注意：对于 EmergencyCall 告警，只检查其他房间的告警，不检查自己房间的（避免误判）
            bool isFlashing = targetAlarmLight.IsManuallyFlashing;
            
            if (!isFlashing && AlarmManager.Instance != null)
            {
                var unhandledAlarms = AlarmManager.Instance.GetUnhandledAlarms();
                foreach (var alarm in unhandledAlarms)
                {
                    // 对于 EmergencyCall，只检查其他房间的告警
                    if (alarm.type == AlarmType.EmergencyCall)
                    {
                        // 如果告警的房间ID和当前按钮的房间ID不一致，才认为是其他告警
                        string currentRoomId = string.IsNullOrEmpty(roomId) ? 
                            (_deviceDefinition != null ? _deviceDefinition.roomId : null) : roomId;
                        if (!string.IsNullOrEmpty(currentRoomId) && 
                            !string.Equals(alarm.roomId, currentRoomId, System.StringComparison.OrdinalIgnoreCase))
                        {
                            isFlashing = true;
                            break;
                        }
                    }
                    else if (alarm.type == AlarmType.Smoke || 
                             alarm.type == AlarmType.GasLeak || 
                             alarm.type == AlarmType.TemperatureHigh || 
                             alarm.type == AlarmType.TemperatureLow ||
                             alarm.type == AlarmType.Fall)
                    {
                        // 其他类型的告警，都认为是正在闪烁
                        isFlashing = true;
                        break;
                    }
                }
            }

            if (isFlashing)
            {
                // 如果正在闪烁，停止闪烁并清除告警
                Debug.Log($"[InteractableHelpButton] {gameObject.name} 检测到报警灯正在闪烁，停止报警");
                
                // 停止手动闪烁
                if (targetAlarmLight.IsManuallyFlashing)
                {
                    targetAlarmLight.TriggerFlash(force: true); // 再次点击停止
                }
                
                // 停止告警系统的闪烁协程
                if (NiceHouse.SmartMonitoring.AlarmResponseHelper.Instance != null)
                {
                    NiceHouse.SmartMonitoring.AlarmResponseHelper.Instance.StopGlobalAlarmLight();
                }
                
                // 标记所有相关告警为已处理
                if (AlarmManager.Instance != null)
                {
                    var unhandledAlarms = AlarmManager.Instance.GetUnhandledAlarms().ToList();
                    foreach (var alarm in unhandledAlarms)
                    {
                        if (alarm.type == AlarmType.EmergencyCall || 
                            alarm.type == AlarmType.Smoke || 
                            alarm.type == AlarmType.GasLeak || 
                            alarm.type == AlarmType.TemperatureHigh || 
                            alarm.type == AlarmType.TemperatureLow ||
                            alarm.type == AlarmType.Fall)
                        {
                            AlarmManager.Instance.MarkHandled(alarm);
                        }
                    }
                }
            }
            else
            {
                // 如果没有闪烁，触发紧急呼叫告警
                if (triggerEmergencyAlarm)
                {
                    TriggerEmergencyAlarm();
                }

                // 强制触发报警灯闪烁（确保即使告警系统未响应也能闪烁）
                Debug.Log($"[InteractableHelpButton] {gameObject.name} 触发紧急呼叫报警");
                targetAlarmLight.TriggerFlash(force: true);
            }
        }

        /// <summary>
        /// 触发紧急呼叫告警
        /// </summary>
        private void TriggerEmergencyAlarm()
        {
            string targetRoomId = string.IsNullOrEmpty(roomId) ? "Unknown" : roomId;

            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.AddAlarm(AlarmType.EmergencyCall, targetRoomId);
                Debug.Log($"[InteractableHelpButton] 触发紧急呼叫告警: {targetRoomId}");
            }
            else
            {
                Debug.LogWarning("[InteractableHelpButton] AlarmManager.Instance is null, 无法触发告警");
            }
        }

        /// <summary>
        /// 悬停提示文字
        /// </summary>
        public string HoverHint
        {
            get
            {
                return clickHint;
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

