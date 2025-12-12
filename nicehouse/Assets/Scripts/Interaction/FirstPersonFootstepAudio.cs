using UnityEngine;

namespace NiceHouse.Interaction
{
    /// <summary>
    /// 第一人称行走音效：按 WSAD 移动时循环播放 walking.wav 的 0.6~1.5s 片段。
    /// 不修改移动逻辑，只监听输入与地面状态。
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class FirstPersonFootstepAudio : MonoBehaviour
    {
        [Header("Source Audio")]
        [Tooltip("包含行走音效的音频（如 walking.wav）")]
        public AudioClip combinedClip;

        [Header("Slice Points (seconds)")]
        public float sliceBegin = 0.6f;
        public float sliceEnd = 1.8f;

        [Header("Movement Detection")]
        [Tooltip("输入向量平方大于此值视为在移动")]
        public float moveThresholdSqr = 0.01f; // ~0.1 magnitude
        [Tooltip("是否需要角色在地面上才播放脚步声")]
        public bool requireGrounded = true;
        [Tooltip("可选：绑定 CharacterController 以读取 isGrounded")]
        public CharacterController characterController;

        [Header("Audio Settings")]
        public float volume = 1f;
        public float pitch = 1f;
        [Tooltip("额外的播放速度系数，>1 更快，<1 更慢")]
        public float speedMultiplier = 1f;

        private AudioSource _audio;
        private AudioClip _loopClip;
        private bool _isLooping;

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.loop = true;

            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            BuildClip();
        }

        private void OnEnable()
        {
            if (_loopClip == null && combinedClip != null)
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
            if (_loopClip == null) return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            bool moving = (h * h + v * v) > moveThresholdSqr;

            bool groundedOk = true;
            if (requireGrounded && characterController != null)
            {
                groundedOk = characterController.isGrounded;
            }

            bool shouldPlay = moving && groundedOk;

            if (shouldPlay && !_isLooping)
            {
                StartLoop();
            }
            else if (!shouldPlay && _isLooping)
            {
                StopLoop();
            }
        }

        private void StartLoop()
        {
            _audio.clip = _loopClip;
            _audio.volume = volume;
            _audio.pitch = pitch * speedMultiplier;
            _audio.time = 0f;
            _audio.loop = true;
            _audio.Play();
            _isLooping = true;
        }

        private void StopLoop()
        {
            _audio.Stop();
            _isLooping = false;
        }

        private void BuildClip()
        {
            _loopClip = CreateSubClip(combinedClip, sliceBegin, sliceEnd);
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
