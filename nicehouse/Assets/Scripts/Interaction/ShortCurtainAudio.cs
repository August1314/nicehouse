using UnityEngine;
using NiceHouse.EnvironmentControl;

namespace NiceHouse.Interaction
{
    /// <summary>
    /// 短窗帘音效：开/关用不同切片。
    /// 开：short_curtain.wav 1.03~1.49s，关：5.7~6.3s。
    /// 可选自动跟随 CurtainController 状态，或手动调用 PlayOpen/PlayClose。
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ShortCurtainAudio : MonoBehaviour
    {
        [Header("Controller Follow (可选)")]
        [Tooltip("要跟随的 CurtainController。不填则自动查找本/父/子节点。")]
        public CurtainController curtainController;
        [Tooltip("自动根据 CurtainController IsOn 播放开/关音效")]
        public bool followController = true;

        [Header("Source Audio")]
        [Tooltip("短窗帘音频（含开关）。默认 short_curtain.wav")]
        public AudioClip combinedClip;

        [Header("Slice Points (seconds)")]
        public float openBegin = 1.03f;
        public float openEnd = 1.49f;
        public float closeBegin = 5.7f;
        public float closeEnd = 6.3f;

        [Header("Volumes")]
        public float openVolume = 1f;
        public float closeVolume = 1f;

        private AudioSource _audio;
        private AudioClip _openClip;
        private AudioClip _closeClip;
        private bool _lastIsOn;

        private void Awake()
        {
            if (curtainController == null)
            {
                curtainController = GetComponent<CurtainController>() ??
                                    GetComponentInParent<CurtainController>() ??
                                    GetComponentInChildren<CurtainController>();
            }

            _audio = GetComponent<AudioSource>();
            _audio.playOnAwake = false;
            BuildClips();
        }

        private void OnEnable()
        {
            if ((_openClip == null || _closeClip == null) && combinedClip != null) BuildClips();
            _lastIsOn = curtainController != null && curtainController.IsOn;
        }

        private void OnValidate()
        {
            BuildClips();
        }

        private void Update()
        {
            if (!followController || curtainController == null) return;
            bool isOn = curtainController.IsOn;
            if (isOn != _lastIsOn)
            {
                if (isOn) PlayOpen();
                else PlayClose();
                _lastIsOn = isOn;
            }
        }

        public void PlayOpen()
        {
            PlayClip(_openClip, openVolume);
        }

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
            _openClip = CreateSubClip(combinedClip, openBegin, openEnd);
            _closeClip = CreateSubClip(combinedClip, closeBegin, closeEnd);
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
