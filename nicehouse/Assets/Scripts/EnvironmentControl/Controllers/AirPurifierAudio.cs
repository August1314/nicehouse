using System.Collections;
using UnityEngine;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 空气净化器音频控制
    /// 复用空调的分段播放逻辑：从一段合成音频里切出 start/loop/stop 三段，
    /// 根据 AirPurifierController 的开关状态播放。
    /// </summary>
    [RequireComponent(typeof(AirPurifierController))]
    public class AirPurifierAudio : MonoBehaviour
    {
        [Header("Source Audio")]
        [Tooltip("包含 start/loop/stop 的合成音频")]
        public AudioClip combinedClip;

        [Header("Slice Points (seconds)")]
        [Tooltip("Start 段起始时间")]
        public float startBegin = 0f;
        [Tooltip("Start 段结束时间（建议短一些避免空白）")]
        public float startEnd = 2f;
        [Tooltip("Loop 首次进入点（可用于预卷）")]
        public float firstLoopBegin = 2f;
        [Tooltip("Loop 段起始时间")]
        public float loopSliceBegin = 2.2f;
        [Tooltip("Loop 段结束时间")]
        public float loopSliceEnd = 8f;
        [Tooltip("Stop 段起始时间（到音频末尾结束）")]
        public float stopBegin = 8.2f;

        [Header("Playback Timing")]
        [Tooltip("Start 播放结束后多久进入 Loop（秒），0 为立即")]
        public float loopStartDelay = 0f;
        [Tooltip("Loop 回卷时的偏移量（秒），避免每圈都从攻击段开始")]
        public float loopReplayOffset = 0f;
        [Tooltip("Loop 末尾提前多少秒触发回卷（防止溢出）")]
        public float loopTailGuard = 0.05f;

        [Header("Volumes")]
        public float startVolume = 1f;
        public float loopVolume = 0.7f;
        public float stopVolume = 1f;

        [Header("3D Settings")]
        [Tooltip("启用 3D 衰减")]
        public bool enable3DAttenuation = true;
        [Tooltip("满音量半径")]
        public float minDistance = 2f;
        [Tooltip("衰减到几乎静音的距离")]
        public float maxDistance = 12f;
        [Range(0f, 1f)]
        [Tooltip("0=2D, 1=3D")]
        public float spatialBlend = 1f;

        private AirPurifierController _controller;
        private AudioSource _sfxSource;   // start / stop
        private AudioSource _loopSource;  // loop

        private AudioClip _startClip;
        private AudioClip _loopClip;
        private AudioClip _stopClip;

        private Coroutine _loopDelayRoutine;
        private bool _lastIsOn;
        private float _loopSliceLocalOffset;

        private void Awake()
        {
            _controller = GetComponent<AirPurifierController>();

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            _loopSource = gameObject.AddComponent<AudioSource>();
            _loopSource.playOnAwake = false;
            _loopSource.loop = true;

            Apply3DSettings(_sfxSource);
            Apply3DSettings(_loopSource);

            BuildClips();
        }

        private void OnEnable()
        {
            _lastIsOn = _controller != null && _controller.IsOn;
            if (_lastIsOn)
            {
                StartLoopImmediate();
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _loopSource?.Stop();
        }

        private void Update()
        {
            if (_controller == null) return;

            bool isOn = _controller.IsOn;
            if (isOn == _lastIsOn) return;

            if (isOn)
            {
                HandleTurnOn();
            }
            else
            {
                HandleTurnOff();
            }

            _lastIsOn = isOn;
        }

        private void LateUpdate()
        {
            // Loop 末尾提前回卷，避免回到攻击段
            if (_loopClip != null && _loopSource != null && _loopSource.clip == _loopClip && _loopSource.isPlaying)
            {
                float guard = Mathf.Max(0.01f, loopTailGuard);
                if (_loopSource.time >= _loopClip.length - guard)
                {
                    float baseOffset = _loopSliceLocalOffset;
                    float offset = Mathf.Clamp(baseOffset + loopReplayOffset, 0f, _loopClip.length - 0.01f);
                    _loopSource.time = offset;
                }
            }
        }

        private void HandleTurnOn()
        {
            if (_loopDelayRoutine != null)
            {
                StopCoroutine(_loopDelayRoutine);
                _loopDelayRoutine = null;
            }

            if (_startClip != null)
            {
                _sfxSource.volume = startVolume;
                _sfxSource.PlayOneShot(_startClip);
                _loopDelayRoutine = StartCoroutine(StartLoopAfter(loopStartDelay));
            }
            else
            {
                StartLoopImmediate();
            }
        }

        private void HandleTurnOff()
        {
            if (_loopDelayRoutine != null)
            {
                StopCoroutine(_loopDelayRoutine);
                _loopDelayRoutine = null;
            }

            _loopSource.Stop();

            if (_stopClip != null)
            {
                _sfxSource.volume = stopVolume;
                _sfxSource.PlayOneShot(_stopClip);
            }
        }

        private IEnumerator StartLoopAfter(float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            StartLoopImmediate();
            _loopDelayRoutine = null;
        }

        private void StartLoopImmediate()
        {
            if (_loopClip == null) return;
            _loopSource.volume = loopVolume;
            _loopSource.clip = _loopClip;
            if (!_loopSource.isPlaying)
            {
                _loopSource.time = 0f;
                _loopSource.Play();
            }
        }

        private void BuildClips()
        {
            if (combinedClip == null) return;

            _startClip = CreateSubClip(combinedClip, startBegin, startEnd);

            float loopClipBegin = Mathf.Min(firstLoopBegin, loopSliceBegin);
            _loopSliceLocalOffset = Mathf.Max(0f, loopSliceBegin - loopClipBegin);
            _loopClip = CreateSubClip(combinedClip, loopClipBegin, loopSliceEnd);

            _stopClip = CreateSubClip(combinedClip, stopBegin, combinedClip.length);
        }

        private static AudioClip CreateSubClip(AudioClip src, float beginSec, float endSec)
        {
            if (src == null) return null;
            if (endSec <= beginSec) return null;

            int startSample = Mathf.Max(0, Mathf.FloorToInt(beginSec * src.frequency));
            int endSample = Mathf.Min(src.samples, Mathf.FloorToInt(endSec * src.frequency));
            int lengthSamples = endSample - startSample;
            if (lengthSamples <= 0) return null;

            int channels = src.channels;
            float[] data = new float[lengthSamples * channels];
            src.GetData(data, startSample);

            var clip = AudioClip.Create($"{src.name}_slice_{beginSec:F1}_{endSec:F1}", lengthSamples, channels, src.frequency, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void Apply3DSettings(AudioSource source)
        {
            if (source == null) return;
            if (!enable3DAttenuation)
            {
                source.spatialBlend = 0f; // 2D
                return;
            }

            source.spatialBlend = Mathf.Clamp01(spatialBlend);
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = Mathf.Max(0.01f, minDistance);
            source.maxDistance = Mathf.Max(source.minDistance + 0.01f, maxDistance);
            source.dopplerLevel = 0f;
        }
    }
}

