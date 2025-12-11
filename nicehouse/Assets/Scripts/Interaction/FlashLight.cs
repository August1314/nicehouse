using UnityEngine;
using NiceHouse.Data;

/// <summary>
/// 警报灯闪烁脚本
/// 支持手动点击触发，同时与告警系统协同工作
/// 当告警系统控制时，自动暂停；无告警时，可以手动触发闪烁
/// </summary>
public class FlashingLight : MonoBehaviour
{
    [Header("闪烁参数")]
    [Tooltip("最小亮度")]
    public float minIntensity = 0.5f;
    
    [Tooltip("最大亮度")]
    public float maxIntensity = 3.0f;
    
    [Tooltip("闪烁速度（每多少秒切换一次）")]
    public float flashRate = 0.2f;

    [Header("告警协同")]
    [Tooltip("是否检查告警状态（告警时自动暂停）")]
    public bool checkAlarmStatus = true;
    
    [Tooltip("是否作为全局报警灯（检查所有房间的告警，而不仅仅是特定房间）")]
    public bool isGlobalAlarmLight = true;
    
    [Tooltip("设备所在房间ID（仅当 isGlobalAlarmLight=false 时使用，留空则自动从DeviceDefinition获取）")]
    public string roomId;

    [Header("手动控制")]
    [Tooltip("是否启用手动点击触发")]
    public bool enableManualTrigger = true;
    
    [Tooltip("手动触发持续时间（秒），0表示持续闪烁直到再次点击")]
    public float manualFlashDuration = 5f;

    [Header("启动设置")]
    [Tooltip("启动延迟（秒），避免启动时立即检测到残留告警")]
    public float startDelay = 1f;

    private Light lightComponent;
    private DeviceDefinition deviceDefinition;
    private bool isManuallyFlashing = false;
    private float manualFlashEndTime = float.MaxValue; // 初始化为最大值，避免立即过期
    private bool isInitialized = false;
    
    /// <summary>
    /// 是否正在手动闪烁（供外部检查，避免告警系统覆盖）
    /// </summary>
    public bool IsManuallyFlashing => isManuallyFlashing;
    private Color originalColor;
    private float originalIntensity;
    private bool wasControlledByAlarm = false;

    void Start()
    {
        // 获取附加到此游戏对象的 Light 组件
        lightComponent = GetComponent<Light>();

        // 检查 Light 组件是否存在
        if (lightComponent == null)
        {
            Debug.LogError("FlashingLight script requires a Light component on the same GameObject.");
            enabled = false;
            return;
        }

        // 保存原始颜色和强度（在修改之前保存）
        originalColor = lightComponent.color;
        originalIntensity = lightComponent.intensity;
        
        // 确保启动时灯光是关闭状态
        // 检查是否有 LightController，如果有则让它控制，否则我们直接关闭灯光
        var lightController = GetComponent<NiceHouse.EnvironmentControl.LightController>();
        if (lightController == null)
        {
            // 如果没有 LightController，确保灯光是关闭状态
            // 如果颜色是红色，可能是之前闪烁留下的，重置
            if (lightComponent.color == Color.red || lightComponent.intensity > 1f)
            {
                // 尝试恢复为白色或原始颜色（如果原始颜色不是红色）
                if (originalColor == Color.red)
                {
                    // 如果原始颜色就是红色，改为白色
                    originalColor = Color.white;
                }
                lightComponent.color = originalColor;
                lightComponent.intensity = Mathf.Min(originalIntensity, 1f); // 确保强度不超过1
            }
            
            // 明确关闭灯光（无论之前是什么状态）
            lightComponent.enabled = false;
        }
        else
        {
            // 如果有 LightController，让它控制，但我们仍然需要确保颜色不是红色（如果是残留状态）
            if (lightComponent.color == Color.red)
            {
                // 如果颜色是红色，可能是之前闪烁留下的，重置为白色
                lightComponent.color = Color.white;
                originalColor = Color.white;
            }
        }

        // 尝试获取 DeviceDefinition 以获取房间ID
        deviceDefinition = GetComponent<DeviceDefinition>();
        if (deviceDefinition != null && string.IsNullOrEmpty(roomId))
        {
            roomId = deviceDefinition.roomId;
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

        // 延迟初始化，避免启动时立即检测到残留告警
        if (startDelay > 0f)
        {
            Invoke(nameof(EnableAlarmDetection), startDelay);
        }
        else
        {
            isInitialized = true;
        }
    }

    /// <summary>
    /// 启用告警检测（延迟调用）
    /// </summary>
    private void EnableAlarmDetection()
    {
        isInitialized = true;
    }

    void Update()
    {
        if (lightComponent == null) return;
        
        // 如果还未初始化，确保灯光是关闭状态（避免启动时显示红色）
        if (!isInitialized)
        {
            // 持续关闭灯光，无论之前是什么状态
            lightComponent.enabled = false;
            // 如果灯光是红色或强度较高，可能是残留状态，重置它
            if (lightComponent.color == Color.red || lightComponent.intensity > 1f)
            {
                lightComponent.color = originalColor;
                lightComponent.intensity = originalIntensity;
            }
            return;
        }

        // 如果正在手动闪烁，优先执行手动闪烁（即使告警系统正在控制）
        if (isManuallyFlashing)
        {
            // 检查手动触发是否过期（只有当 manualFlashDuration > 0 时才检查）
            if (manualFlashDuration > 0f && Time.time >= manualFlashEndTime)
            {
                StopManualFlash();
                return;
            }

            // 执行闪烁
            bool shouldBeOn = (Time.time % flashRate < flashRate / 2f);
            if (shouldBeOn)
        {
            lightComponent.intensity = maxIntensity;
                lightComponent.enabled = true; // 确保灯光启用
            }
            else
            {
                lightComponent.intensity = minIntensity;
                // 注意：不关闭灯光，只是降低强度，这样闪烁效果更明显
            }
            
            return; // 手动闪烁时，不检查告警系统控制
        }

        // 检查是否被告警系统控制
        bool isControlledByAlarm = IsControlledByAlarmSystem();
        
        if (isControlledByAlarm)
        {
            // 告警系统正在控制，暂停闪烁
            wasControlledByAlarm = true;
            return;
        }

        // 如果之前被告警系统控制，现在恢复了，重置状态
        if (wasControlledByAlarm && !isControlledByAlarm)
        {
            wasControlledByAlarm = false;
            // 恢复原始状态（如果告警系统改变了颜色）
            if (lightComponent.color != originalColor)
            {
                lightComponent.color = originalColor;
                lightComponent.intensity = originalIntensity;
            }
        }

        // 没有手动触发，保持原始状态（关闭灯光）
        lightComponent.enabled = false;
        lightComponent.intensity = originalIntensity;
        lightComponent.color = originalColor;
    }

    /// <summary>
    /// 检查是否被告警系统控制
    /// </summary>
    private bool IsControlledByAlarmSystem()
    {
        if (!checkAlarmStatus) return false;

        // 检查是否有未处理的告警
        if (AlarmManager.Instance != null)
        {
            var unhandledAlarms = AlarmManager.Instance.GetUnhandledAlarms();
            foreach (var alarm in unhandledAlarms)
            {
                // 如果是全局报警灯，检查所有房间的告警
                // 如果不是全局报警灯，只检查当前房间的告警
                bool shouldCheck = isGlobalAlarmLight || 
                    (!string.IsNullOrEmpty(roomId) && 
                     string.Equals(alarm.roomId, roomId, System.StringComparison.OrdinalIgnoreCase));
                
                if (shouldCheck)
                {
                    // 检查是否是需要灯光响应的告警类型
                    if (alarm.type == AlarmType.Smoke ||
                        alarm.type == AlarmType.GasLeak ||
                        alarm.type == AlarmType.TemperatureHigh ||
                        alarm.type == AlarmType.TemperatureLow ||
                        alarm.type == AlarmType.Fall ||
                        alarm.type == AlarmType.EmergencyCall)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 手动触发闪烁（可通过点击事件调用）
    /// </summary>
    /// <param name="force">是否强制触发（即使告警系统正在控制）</param>
    public void TriggerFlash(bool force = false)
    {
        if (lightComponent == null)
        {
            Debug.LogError($"[FlashingLight] {gameObject.name} Light 组件为 null，无法触发闪烁");
            return;
        }

        if (!enableManualTrigger) 
        {
            Debug.LogWarning($"[FlashingLight] {gameObject.name} 手动触发已禁用");
            return;
        }
        
        // 检查是否被告警系统控制（除非强制触发）
        bool isControlledByAlarm = IsControlledByAlarmSystem();
        if (!force && isControlledByAlarm)
        {
            Debug.Log($"[FlashingLight] {gameObject.name} 当前正被告警系统控制，手动触发将被忽略（使用 force=true 可强制触发）");
            return;
        }
        
        if (isManuallyFlashing)
        {
            // 如果正在闪烁，再次点击则停止
            StopManualFlash();
        }
        else
        {
            // 开始手动闪烁
            isManuallyFlashing = true;
            if (manualFlashDuration > 0f)
            {
                manualFlashEndTime = Time.time + manualFlashDuration;
            }
            else
            {
                // 如果 manualFlashDuration 为 0，设置为最大值，表示持续闪烁直到手动停止
                manualFlashEndTime = float.MaxValue;
            }
            
            // 确保灯光是红色的
            lightComponent.color = Color.red;
            
            // 只在调试模式下输出详细日志
            if (enableManualTrigger)
            {
                Debug.Log($"[FlashingLight] {gameObject.name} 手动闪烁已触发" + (force ? " (强制)" : ""));
            }
        }
    }

    /// <summary>
    /// 停止手动闪烁
    /// </summary>
    public void StopManualFlash()
    {
        if (!isManuallyFlashing) return;
        
        isManuallyFlashing = false;
        manualFlashEndTime = float.MaxValue; // 重置为最大值
        
        // 恢复原始状态
        lightComponent.color = originalColor;
        lightComponent.intensity = originalIntensity;
    }

    /// <summary>
    /// 切换闪烁状态（点击时调用）
    /// </summary>
    public void ToggleFlash()
    {
        TriggerFlash();
    }

    // 兼容旧接口：OnMouseDown 用于测试
    // 注意：如果 GameObject 上有 InteractableAlarmLight 组件，应该使用那个组件处理点击，而不是这个方法
    private void OnMouseDown()
    {
        // 检查是否有 InteractableAlarmLight 组件，如果有则跳过（避免重复触发）
        if (GetComponent<NiceHouse.Interaction.InteractableAlarmLight>() != null)
        {
            return; // 让 InteractableAlarmLight 处理点击
        }
        
        if (enableManualTrigger)
        {
            TriggerFlash();
        }
    }
}