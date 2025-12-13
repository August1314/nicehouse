using UnityEngine;
using NiceHouse.Data;
using System.Collections;
using System.Collections.Generic;

namespace NiceHouse.SmartMonitoring
{
    /// <summary>
    /// 告警响应辅助类
    /// 负责告警触发时的UI提示、音效播放、灯光闪烁等响应
    /// </summary>
    public class AlarmResponseHelper : MonoBehaviour
    {
        public static AlarmResponseHelper Instance { get; private set; }

        [Header("UI提示")]
        [Tooltip("告警弹窗预制体（可选）")]
        public GameObject alarmPopupPrefab;

        [Tooltip("UI Canvas（用于实例化弹窗）")]
        public Transform uiCanvas;

        [Header("音效")]
        [Tooltip("普通告警音效")]
        public AudioClip normalAlarmSound;

        [Tooltip("紧急告警音效")]
        public AudioClip emergencyAlarmSound;

        [Tooltip("音效播放器")]
        public AudioSource audioSource;

        [Header("灯光控制")]
        [Tooltip("是否启用灯光闪烁")]
        public bool enableLightFlash = true;

        [Tooltip("灯光闪烁间隔（秒）")]
        public float flashInterval = 0.5f;

        [Tooltip("灯光闪烁持续时间（秒），0表示持续闪烁直到告警处理")]
        public float flashDuration = 0f;

        [Header("告警消息显示")]
        [Tooltip("是否在Console显示告警消息")]
        public bool showConsoleMessage = true;

        private Dictionary<string, Coroutine> _roomFlashCoroutines = new Dictionary<string, Coroutine>();
        private Dictionary<string, List<Light>> _roomLights = new Dictionary<string, List<Light>>();
        private Coroutine _globalAlarmLightCoroutine;
        private List<Light> _globalAlarmLights;
        private readonly HashSet<string> _alarmOpenedWindows = new HashSet<string>();

        [Header("启动设置")]
        [Tooltip("启动时停止所有灯光闪烁（避免残留告警导致启动时闪烁）")]
        public bool stopAllFlashOnStart = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // 订阅告警事件（在 Awake 先订阅，避免 Start 前触发的告警漏掉）
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.OnAlarmAdded += OnAlarmAddedHandler;
            }

            // 如果没有指定AudioSource，尝试获取或创建
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }

        private void Start()
        {
            // 启动时停止所有灯光闪烁，避免残留告警导致启动时闪烁
            if (stopAllFlashOnStart)
            {
                StopAllLightFlash();
            }

            // 订阅告警事件
            if (AlarmManager.Instance != null)
            {
                // 避免重复订阅
                AlarmManager.Instance.OnAlarmAdded -= OnAlarmAddedHandler;
                AlarmManager.Instance.OnAlarmAdded += OnAlarmAddedHandler;
            }
        }

        private void OnDestroy()
        {
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.OnAlarmAdded -= OnAlarmAddedHandler;
            }
        }

        /// <summary>
        /// 响应告警
        /// </summary>
        public void RespondToAlarm(AlarmType type, string roomId)
        {
            // 显示UI提示
            ShowAlarmPopup(type, roomId);

            // 播放音效
            PlayAlarmSound(type);

            // 闪烁灯光
            if (enableLightFlash)
            {
                FlashRoomLights(roomId, type);
                FlashGlobalAlarmLight(type); // 同时闪烁全局报警灯
            }

            // 显示Console消息
            if (showConsoleMessage)
            {
                string message = GetAlarmMessage(type, roomId);
                Debug.Log($"[AlarmResponse] {message}"); // 使用 Log 而不是 LogWarning，因为这是正常的告警信息
            }

            // 告警联动：烟雾/燃气 → 自动开窗
            HandleWindowAutoOpen(type, roomId);
        }

        /// <summary>
        /// 告警事件回调：用于触发 RespondToAlarm
        /// </summary>
        private void OnAlarmAddedHandler(AlarmRecord record)
        {
            if (record == null) return;
            RespondToAlarm(record.type, record.roomId);
        }

        /// <summary>
        /// 显示告警弹窗
        /// </summary>
        private void ShowAlarmPopup(AlarmType type, string roomId)
        {
            if (alarmPopupPrefab == null || uiCanvas == null) return;

            try
            {
                GameObject popup = Instantiate(alarmPopupPrefab, uiCanvas);
                // 可以在这里设置弹窗内容
                // 例如：popup.GetComponent<AlarmPopup>().SetAlarm(type, roomId);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[AlarmResponseHelper] Failed to show alarm popup: {e.Message}");
            }
        }

        /// <summary>
        /// 播放告警音效
        /// </summary>
        private void PlayAlarmSound(AlarmType type)
        {
            if (audioSource == null) return;

            AudioClip clip = GetAlarmSound(type);
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// 获取告警音效
        /// </summary>
        private AudioClip GetAlarmSound(AlarmType type)
        {
            switch (type)
            {
                case AlarmType.Fall:
                case AlarmType.Smoke:
                case AlarmType.GasLeak:
                case AlarmType.TemperatureHigh:
                case AlarmType.TemperatureLow:
                    return emergencyAlarmSound != null ? emergencyAlarmSound : normalAlarmSound;
                default:
                    return normalAlarmSound;
            }
        }

        /// <summary>
        /// 闪烁房间灯光
        /// </summary>
        private void FlashRoomLights(string roomId, AlarmType type)
        {
            if (DeviceManager.Instance == null) return;

            // 停止该房间之前的闪烁协程
            if (_roomFlashCoroutines.ContainsKey(roomId))
            {
                if (_roomFlashCoroutines[roomId] != null)
                {
                    StopCoroutine(_roomFlashCoroutines[roomId]);
                }
                _roomFlashCoroutines.Remove(roomId);
            }

            // 获取房间内的灯光设备
            List<Light> lights = GetRoomLights(roomId);
            if (lights.Count == 0) return;

            // 启动闪烁协程
            Coroutine flashCoroutine = StartCoroutine(FlashLightsCoroutine(lights, roomId, type));
            _roomFlashCoroutines[roomId] = flashCoroutine;
        }

        /// <summary>
        /// 闪烁全局报警灯（任何房间告警时都会闪烁）
        /// </summary>
        private void FlashGlobalAlarmLight(AlarmType type)
        {
            // 获取全局报警灯
            List<Light> globalLights = GetGlobalAlarmLights();
            if (globalLights.Count == 0) return;

            // 停止之前的全局报警灯闪烁协程
            if (_globalAlarmLightCoroutine != null)
            {
                StopCoroutine(_globalAlarmLightCoroutine);
            }

            // 启动闪烁协程
            _globalAlarmLightCoroutine = StartCoroutine(FlashLightsCoroutine(globalLights, "Global", type));
        }

        /// <summary>
        /// 获取全局报警灯（所有带有 FlashingLight 且 isGlobalAlarmLight=true 的灯光）
        /// </summary>
        private List<Light> GetGlobalAlarmLights()
        {
            // 如果已缓存，直接返回
            if (_globalAlarmLights != null && _globalAlarmLights.Count > 0)
            {
                return _globalAlarmLights;
            }

            _globalAlarmLights = new List<Light>();

            // 查找所有带有 FlashingLight 组件的物体
            var flashingLights = FindObjectsOfType<FlashingLight>();
            foreach (var flashingLight in flashingLights)
            {
                // 检查是否是全局报警灯
                if (flashingLight.isGlobalAlarmLight)
                {
                    Light light = flashingLight.GetComponent<Light>();
                    if (light != null)
                    {
                        _globalAlarmLights.Add(light);
                    }
                }
            }

            return _globalAlarmLights;
        }

        /// <summary>
        /// 获取房间内的灯光设备
        /// </summary>
        private List<Light> GetRoomLights(string roomId)
        {
            // 如果已缓存，直接返回
            if (_roomLights.ContainsKey(roomId))
            {
                return _roomLights[roomId];
            }

            List<Light> lights = new List<Light>();

            if (DeviceManager.Instance != null)
            {
                var devices = DeviceManager.Instance.GetDevicesInRoom(roomId);
                foreach (var device in devices)
                {
                    if (device.type == NiceHouse.Data.DeviceType.Light)
                    {
                        // 尝试获取Light组件（可能是Unity Light或自定义组件）
                        Light light = device.GetComponent<Light>();
                        if (light != null)
                        {
                            lights.Add(light);
                        }
                    }
                }
            }

            // 缓存结果
            _roomLights[roomId] = lights;
            return lights;
        }

        /// <summary>
        /// 灯光闪烁协程
        /// </summary>
        private IEnumerator FlashLightsCoroutine(List<Light> lights, string roomId, AlarmType type)
        {
            if (lights.Count == 0) yield break;

            // 保存原始强度
            float[] originalIntensities = new float[lights.Count];
            Color[] originalColors = new Color[lights.Count];
            for (int i = 0; i < lights.Count; i++)
            {
                originalIntensities[i] = lights[i].intensity;
                originalColors[i] = lights[i].color;
            }

            // 确定闪烁颜色（紧急告警用红色，普通告警用黄色）
            Color flashColor = Color.yellow;
            if (type == AlarmType.Fall)
            {
                flashColor = Color.red;
            }
            else if (type == AlarmType.Smoke)
            {
                flashColor = new Color(0.9f, 0.3f, 0.2f); // 烟雾偏红
            }
            else if (type == AlarmType.GasLeak)
            {
                flashColor = new Color(1f, 0.7f, 0.2f); // 燃气偏橙
            }
            else if (type == AlarmType.TemperatureHigh)
            {
                flashColor = new Color(1f, 0.4f, 0f); // 高温偏橙红
            }
            else if (type == AlarmType.TemperatureLow)
            {
                flashColor = new Color(0.2f, 0.4f, 1f); // 低温偏蓝
            }

            float elapsed = 0f;
            bool isOn = true;

            while (flashDuration <= 0f || elapsed < flashDuration)
            {
                // 切换灯光状态
                for (int i = 0; i < lights.Count; i++)
                {
                    if (lights[i] != null)
                    {
                        // 检查是否有 FlashingLight 且正在手动闪烁，如果是则跳过（让手动闪烁优先）
                        var flashingLight = lights[i].GetComponent<FlashingLight>();
                        if (flashingLight != null && flashingLight.IsManuallyFlashing)
                        {
                            continue; // 跳过这个灯光，让 FlashingLight 自己控制
                        }

                        if (isOn)
                        {
                            lights[i].intensity = originalIntensities[i] * 1.5f;
                            lights[i].color = flashColor;
                        }
                        else
                        {
                            lights[i].intensity = originalIntensities[i];
                            lights[i].color = originalColors[i];
                        }
                    }
                }

                isOn = !isOn;
                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;
            }

            // 恢复原始状态
            for (int i = 0; i < lights.Count; i++)
            {
                if (lights[i] != null)
                {
                    lights[i].intensity = originalIntensities[i];
                    lights[i].color = originalColors[i];
                }
            }

            // 清理协程记录
            if (_roomFlashCoroutines.ContainsKey(roomId))
            {
                _roomFlashCoroutines.Remove(roomId);
            }
        }

        /// <summary>
        /// 当房间出现烟雾/燃气告警时自动打开窗户
        /// </summary>
        /// <summary>
        /// 当出现需通风的告警时自动打开窗户（可静态调用兜底，跨房间）
        /// </summary>
        public static void TryAutoOpenWindows(AlarmType type, string roomId)
        {
            if (Instance != null)
            {
                Instance.HandleWindowAutoOpen(type, roomId);
            }
        }

        /// <summary>
        /// 当出现需通风的告警时自动打开窗户（遍历全局所有窗户）
        /// </summary>
        public void HandleWindowAutoOpen(AlarmType type, string roomId)
        {
            // 这里拓展为烟雾、燃气、紧急呼叫都尝试开窗
            if (type != AlarmType.Smoke && type != AlarmType.GasLeak && type != AlarmType.EmergencyCall)
            {
                return;
            }

            if (DeviceManager.Instance == null)
            {
                return;
            }

            var devices = DeviceManager.Instance.GetAllDevices().Values;
            bool openedAny = false;
            int candidateCount = 0;
            foreach (var device in devices)
            {
                if (device.type == NiceHouse.Data.DeviceType.Window)
                {
                    candidateCount++;
                    // 兼容挂在子物体/父物体的控制器
                    var window = device.GetComponent<NiceHouse.EnvironmentControl.WindowController>()
                                 ?? device.GetComponentInChildren<NiceHouse.EnvironmentControl.WindowController>(includeInactive: true)
                                 ?? device.GetComponentInParent<NiceHouse.EnvironmentControl.WindowController>();
                    if (window != null && !window.IsOn)
                    {
                        window.TurnOn();
                        openedAny = true;
                    }
                }
            }

            if (openedAny)
            {
                // 记录“全局打开”，使用特殊 key 以便后续关闭全部由告警打开的窗
                _alarmOpenedWindows.Add("*global*");
                Debug.Log($"[AlarmResponseHelper] Auto-opened ALL windows due to {type}, candidates={candidateCount}");
            }
            else
            {
                Debug.LogWarning($"[AlarmResponseHelper] No windows opened for {type}. candidates={candidateCount}, DeviceManager null? {DeviceManager.Instance == null}");
            }
        }

        /// <summary>
        /// 告警被标记处理后，可调用此方法关闭因告警打开的窗户
        /// （支持按房间或全局，外部在标记 handled 时调用，以免影响手动开窗）
        /// </summary>
        public void CloseWindowsForRoomIfOpenedByAlarm(string roomId)
        {
            bool isGlobal = string.IsNullOrEmpty(roomId) || roomId == "*global*";

            if (!isGlobal && !_alarmOpenedWindows.Contains(roomId) && !_alarmOpenedWindows.Contains("*global*")) return;
            if (DeviceManager.Instance == null) return;

            IEnumerable<DeviceDefinition> devices = isGlobal
                ? DeviceManager.Instance.GetAllDevices().Values
                : DeviceManager.Instance.GetDevicesInRoom(roomId);

            foreach (var device in devices)
            {
                if (device.type == NiceHouse.Data.DeviceType.Window)
                {
                    var window = device.GetComponent<NiceHouse.EnvironmentControl.WindowController>()
                                 ?? device.GetComponentInChildren<NiceHouse.EnvironmentControl.WindowController>(includeInactive: true)
                                 ?? device.GetComponentInParent<NiceHouse.EnvironmentControl.WindowController>();
                    if (window != null && window.IsOn)
                    {
                        window.TurnOff();
                    }
                }
            }

            if (isGlobal)
            {
                _alarmOpenedWindows.Remove("*global*");
            }
            else
            {
                _alarmOpenedWindows.Remove(roomId);
            }
        }

        /// <summary>
        /// 在告警被外部标记 handled 时调用，批量关闭因告警打开的窗户
        /// </summary>
        public void CloseWindowsForHandledAlarms(IEnumerable<AlarmRecord> handledAlarms)
        {
            if (handledAlarms == null) return;
            foreach (var alarm in handledAlarms)
            {
                CloseWindowsForRoomIfOpenedByAlarm(alarm.roomId);
            }
        }

        /// <summary>
        /// 停止房间灯光闪烁
        /// </summary>
        public void StopRoomLightFlash(string roomId)
        {
            if (_roomFlashCoroutines.ContainsKey(roomId))
            {
                if (_roomFlashCoroutines[roomId] != null)
                {
                    StopCoroutine(_roomFlashCoroutines[roomId]);
                }
                _roomFlashCoroutines.Remove(roomId);
            }

            // 如果此前有按房间记录的告警开窗，顺便关闭
            CloseWindowsForRoomIfOpenedByAlarm(roomId);
        }

        /// <summary>
        /// 停止全局报警灯闪烁
        /// </summary>
        public void StopGlobalAlarmLight()
        {
            if (_globalAlarmLightCoroutine != null)
            {
                StopCoroutine(_globalAlarmLightCoroutine);
                _globalAlarmLightCoroutine = null;
            }

            // 恢复全局报警灯的原始状态
            if (_globalAlarmLights != null)
            {
                foreach (var light in _globalAlarmLights)
                {
                    if (light != null)
                    {
                        // 尝试从 FlashingLight 获取原始状态
                        var flashingLight = light.GetComponent<FlashingLight>();
                        if (flashingLight != null)
                        {
                            // FlashingLight 会在 Update 中自动恢复
                        }
                    }
                }
            }

            // 全局报警灯停闪时，关闭因告警全局打开的窗
            CloseWindowsForRoomIfOpenedByAlarm("*global*");
        }

        /// <summary>
        /// 停止所有灯光闪烁（包括房间灯光和全局报警灯）
        /// </summary>
        public void StopAllLightFlash()
        {
            // 停止所有房间灯光
            foreach (var coroutine in _roomFlashCoroutines.Values)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
            _roomFlashCoroutines.Clear();

            // 停止全局报警灯
            StopGlobalAlarmLight();

             // 一并关闭所有因告警打开的窗
             CloseWindowsForRoomIfOpenedByAlarm("*global*");
        }

        /// <summary>
        /// 获取告警消息文本
        /// </summary>
        private string GetAlarmMessage(AlarmType type, string roomId)
        {
            string typeName = type switch
            {
                AlarmType.LongSitting => "Long Sitting",
                AlarmType.LongBathing => "Long Bathing",
                AlarmType.Fall => "Fall/OutOfBed",
                AlarmType.Smoke => "Smoke",
                AlarmType.GasLeak => "Gas Leak",
                AlarmType.HealthAbnormal => "Health Abnormal",
                AlarmType.EmergencyCall => "Emergency Call",
                AlarmType.TemperatureHigh => "Temperature High",
                AlarmType.TemperatureLow => "Temperature Low",
                _ => type.ToString()
            };

            return $"{typeName} - Room: {roomId}";
        }
    }
}

