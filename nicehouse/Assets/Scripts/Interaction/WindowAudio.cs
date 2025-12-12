using UnityEngine;

namespace NiceHouse.Interaction
{
    /// <summary>
    /// Plays split open/close sounds for a window using a single combined clip.
    /// Attach to window object (e.g., LivingRoomWindowLeft).
    /// You can:
    /// - Call PlayOpen/PlayClose manually（动画事件/交互脚本）
    /// - 或勾选 followController 让它自动监听 WindowController 的开关状态
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class WindowAudio : MonoBehaviour
    {
        [Header("Controller Follow (可选)")]
        [Tooltip("要监听的 WindowController。不填则自动查找本节点/父子节点。")]
        public NiceHouse.EnvironmentControl.WindowController windowController;
        [Tooltip("自动根据 WindowController 开/关状态播放音效")]
        public bool followController = true;

        [Header("Source Audio")]
        [Tooltip("Combined clip containing open and close sounds (e.g., window.wav).")]
        public AudioClip combinedClip;

        [Header("Slice Points (seconds)")]
        [Tooltip("Open sound begin time.")]
        public float openBegin = 0.65f;
        [Tooltip("Open sound end time.")]
        public float openEnd = 0.667f;
        [Tooltip("Close sound begin time.")]
        public float closeBegin = 0.05f;
        [Tooltip("Close sound end time.")]
        public float closeEnd = 0.222f;

        [Header("Volumes")]
        public float openVolume = 1f;
        public float closeVolume = 1f;

        private AudioSource _audio;
        private AudioClip _openClip;
        private AudioClip _closeClip;
        private bool _lastIsOn;

        private void Awake()
        {
            if (windowController == null)
            {
                windowController = GetComponent<NiceHouse.EnvironmentControl.WindowController>() ??
                                   GetComponentInParent<NiceHouse.EnvironmentControl.WindowController>() ??
                                   GetComponentInChildren<NiceHouse.EnvironmentControl.WindowController>();
            }

            _audio = GetComponent<AudioSource>();
            _audio.playOnAwake = false;
            BuildClips();
        }

        private void OnEnable()
        {
            if ((_openClip == null || _closeClip == null) && combinedClip != null)
            {
                BuildClips();
            }

            _lastIsOn = windowController != null && windowController.IsOn;
        }

        private void OnValidate()
        {
            if (combinedClip != null)
            {
                BuildClips();
            }
        }

        private void Update()
        {
            if (!followController || windowController == null) return;

            bool isOn = windowController.IsOn;
            if (isOn != _lastIsOn)
            {
                if (isOn)
                {
                    PlayOpen();
                }
                else
                {
                    PlayClose();
                }

                _lastIsOn = isOn;
            }
        }

        /// <summary>Play window open sound.</summary>
        public void PlayOpen()
        {
            PlayClip(_openClip, openVolume);
        }

        /// <summary>Play window close sound.</summary>
        public void PlayClose()
        {
            PlayClip(_closeClip, closeVolume);
        }

        private void PlayClip(AudioClip clip, float volume)
        {
            if (clip == null) return;
            _audio.volume = volume;
            _audio.PlayOneShot(clip);
        }

        private void BuildClips()
        {
            if (combinedClip == null) return;

            _openClip = CreateSubClip(combinedClip, openBegin, openEnd);
            _closeClip = CreateSubClip(combinedClip, closeBegin, closeEnd);

            if (_openClip == null)
            {
                Debug.LogWarning($"[WindowAudio] Open slice invalid. Check times {openBegin}-{openEnd} within clip length {combinedClip.length:F3}s", this);
            }
            if (_closeClip == null)
            {
                Debug.LogWarning($"[WindowAudio] Close slice invalid. Check times {closeBegin}-{closeEnd} within clip length {combinedClip.length:F3}s", this);
            }
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

            var clip = AudioClip.Create($"{src.name}_slice_{beginSec:F3}_{endSec:F3}", lengthSamples, channels, src.frequency, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
