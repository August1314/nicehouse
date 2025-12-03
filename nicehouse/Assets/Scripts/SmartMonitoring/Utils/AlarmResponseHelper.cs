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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

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
            }

            // 显示Console消息
            if (showConsoleMessage)
            {
                string message = GetAlarmMessage(type, roomId);
                Debug.Log($"[AlarmResponse] {message}"); // 使用 Log 而不是 LogWarning，因为这是正常的告警信息
            }
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
            // 跌倒、坠床等紧急情况使用紧急音效
            if (type == AlarmType.Fall)
            {
                return emergencyAlarmSound != null ? emergencyAlarmSound : normalAlarmSound;
            }

            return normalAlarmSound;
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
            Color flashColor = (type == AlarmType.Fall) ? Color.red : Color.yellow;

            float elapsed = 0f;
            bool isOn = true;

            while (flashDuration <= 0f || elapsed < flashDuration)
            {
                // 切换灯光状态
                for (int i = 0; i < lights.Count; i++)
                {
                    if (lights[i] != null)
                    {
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
                _ => type.ToString()
            };

            return $"{typeName} - Room: {roomId}";
        }
    }
}

