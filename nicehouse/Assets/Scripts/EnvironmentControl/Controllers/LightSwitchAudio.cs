using UnityEngine;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// Plays on/off switch sounds for a LightController using a combined audio clip.
    /// Attach to the same GameObject as LightController.
    /// </summary>
    [RequireComponent(typeof(LightController))]
    public class LightSwitchAudio : MonoBehaviour
    {
        [Header("Source Audio")]
        [Tooltip("Combined clip containing both on and off sounds.")]
        public AudioClip combinedClip;

        [Header("Slice Points (seconds)")]
        [Tooltip("On sound begins at this time.")]
        public float onBegin = 0.63f;
        [Tooltip("Off sound begins at this time.")]
        public float offBegin = 1.87f;

        [Header("Volumes")]
        public float onVolume = 1f;
        public float offVolume = 1f;

        private LightController _light;
        private AudioSource _audio;
        private AudioClip _onClip;
        private AudioClip _offClip;
        private bool _lastIsOn;

        private void Awake()
        {
            _light = GetComponent<LightController>();
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;

            BuildClips();
        }

        private void OnEnable()
        {
            _lastIsOn = _light != null && _light.IsLightOn;
        }

        private void Update()
        {
            if (_light == null) return;

            bool isOn = _light.IsLightOn;
            if (isOn == _lastIsOn) return;

            if (isOn)
            {
                PlayClip(_onClip, onVolume);
            }
            else
            {
                PlayClip(_offClip, offVolume);
            }

            _lastIsOn = isOn;
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

            // On: from onBegin to offBegin (or clip end if offBegin not set)
            float onEnd = offBegin > onBegin ? offBegin : combinedClip.length;
            _onClip = CreateSubClip(combinedClip, onBegin, onEnd);

            // Off: from offBegin to end
            _offClip = CreateSubClip(combinedClip, offBegin, combinedClip.length);
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
