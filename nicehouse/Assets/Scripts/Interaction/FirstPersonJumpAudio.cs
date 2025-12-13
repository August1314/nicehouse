using UnityEngine;

namespace NiceHouse.Interaction
{
    /// <summary>
    /// 第一人称跳跃音效：按下空格键时播放 jump.wav 的 0.17~0.21 秒片段。
    /// 不修改跳跃逻辑，仅监听输入并播放音效。
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class FirstPersonJumpAudio : MonoBehaviour
    {
        [Header("Source Audio")]
        [Tooltip("包含跳跃音效的音频（如 jump.wav）")]
        public AudioClip combinedClip;

        [Header("Slice Points (seconds)")]
        public float sliceBegin = 0.17f;
        public float sliceEnd = 0.21f;

        [Header("Settings")]
        [Tooltip("检测跳跃的按键（默认 Space）")]
        public KeyCode jumpKey = KeyCode.Space;
        [Tooltip("播放音量")]
        public float volume = 1f;
        [Tooltip("播放音高")]
        public float pitch = 1f;

        private AudioSource _audio;
        private AudioClip _slice;

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.loop = false;
            BuildClip();
        }

        private void OnEnable()
        {
            if (_slice == null && combinedClip != null)
            {
                BuildClip();
            }
        }

        private void OnValidate()
        {
            BuildClip();
        }

        private void Update()
        {
            if (_slice == null) return;

            if (Input.GetKeyDown(jumpKey))
            {
                _audio.volume = volume;
                _audio.pitch = pitch;
                _audio.PlayOneShot(_slice);
            }
        }

        private void BuildClip()
        {
            _slice = CreateSubClip(combinedClip, sliceBegin, sliceEnd);
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

            var clip = AudioClip.Create($"{src.name}_slice_{beginSec:F2}_{endSec:F2}", lengthSamples, channels, src.frequency, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
