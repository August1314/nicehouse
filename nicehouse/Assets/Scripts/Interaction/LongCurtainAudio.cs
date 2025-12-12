using UnityEngine;
using NiceHouse.EnvironmentControl;

namespace NiceHouse.Interaction
{
    /// <summary>
    /// 长窗帘音效：开/关都播放同一段。
    /// 切片：long_curtain.wav 的 1.43s~2.01s。
    /// 可选自动跟随 CurtainController 状态，或手动调用 PlayOpen/PlayClose。
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class LongCurtainAudio : MonoBehaviour
    {
        [Header("Controller Follow (可选)")]
        [Tooltip("要跟随的 CurtainController。不填则自动查找本/父/子节点。")]
        public CurtainController curtainController;
        [Tooltip("自动根据 CurtainController IsOn 播放开/关音效")]
        public bool followController = true;

        [Header("Source Audio")]
        [Tooltip("长窗帘音频（含开关）。默认 long_curtain.wav")]
        public AudioClip combinedClip;

        [Header("Slice Points (seconds)")]
        public float sliceBegin = 1.43f;
        public float sliceEnd = 2.01f;

        [Header("Volumes")]
        public float openVolume = 1f;
        public float closeVolume = 1f;

        [Header("Loop Control")]
        [Tooltip("Max duration (seconds) to keep looping during a single open/close action.")]
        public float loopMaxDuration = 2f;

        private AudioSource _audio;
        private AudioClip _slice;
        private bool _lastIsOn;
        private bool _loopingDuringMotion;
        private float _loopEndTime;

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
            _audio.loop = false; // will manually loop for open/close if needed
            BuildClip();
        }

        private void OnEnable()
        {
            if (_slice == null && combinedClip != null) BuildClip();
            _lastIsOn = curtainController != null && curtainController.IsOn;
        }

        private void OnValidate()
        {
            BuildClip();
        }

        private void Update()
        {
            if (!followController || curtainController == null) return;
            bool isOn = curtainController.IsOn;
            if (isOn != _lastIsOn)
            {
                if (isOn) PlayOpen(loop: true);
                else PlayClose(loop: true);
                _lastIsOn = isOn;
            }

            // If looping during motion, stop after max duration
            if (_loopingDuringMotion && _audio != null && _slice != null)
            {
                if (Time.time >= _loopEndTime)
                {
                    StopLoop();
                    return;
                }
            }
        }

        public void PlayOpen(bool loop = false)
        {
            PlayClip(_slice, openVolume, loop);
        }

        public void PlayClose(bool loop = false)
        {
            PlayClip(_slice, closeVolume, loop);
        }

        private void PlayClip(AudioClip clip, float volume, bool loop)
        {
            if (clip == null) return;
            _audio.volume = volume;
            if (loop)
            {
                _loopingDuringMotion = true;
                _loopEndTime = Time.time + Mathf.Max(0.05f, loopMaxDuration);
                _audio.clip = clip;
                _audio.time = 0f;
                _audio.loop = true; // rely on AudioSource looping to avoid gaps
                _audio.Play();
            }
            else
            {
                StopLoop();
                _audio.PlayOneShot(clip);
            }
        }

        private void StopLoop()
        {
            if (_loopingDuringMotion && _audio != null)
            {
                _audio.Stop();
            }
            _loopingDuringMotion = false;
            _loopEndTime = 0f;
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
