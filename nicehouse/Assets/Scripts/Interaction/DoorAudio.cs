using UnityEngine;

namespace NiceHouse.Interaction
{
    /// <summary>
    /// Plays split open/close sounds for a door using a combined audio clip.
    /// Attach to the door GameObject (Door.001) and call PlayOpen/PlayClose when the door toggles.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class DoorAudio : MonoBehaviour
    {
        [Header("Source Audio")]
        [Tooltip("Combined clip containing open and close sounds (e.g., door_open_off.m4a).")]
        public AudioClip combinedClip;

        [Header("Slice Points (seconds)")]
        [Tooltip("Open sound begin time.")]
        public float openBegin = 12.2f;
        [Tooltip("Open sound end time.")]
        public float openEnd = 12.5f;
        [Tooltip("Close sound begin time.")]
        public float closeBegin = 17.8f;
        [Tooltip("Close sound end time.")]
        public float closeEnd = 18.4f;

        [Header("Volumes")]
        public float openVolume = 1f;
        public float closeVolume = 1f;

        private AudioSource _audio;
        private AudioClip _openClip;
        private AudioClip _closeClip;

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
            _audio.playOnAwake = false;
            BuildClips();
        }

        private void OnEnable()
        {
            // In case the clip was assigned after Awake
            if ((_openClip == null || _closeClip == null) && combinedClip != null)
            {
                BuildClips();
            }
        }

        private void OnValidate()
        {
            // Re-slice in editor when values change
            if (combinedClip != null)
            {
                BuildClips();
            }
        }

        /// <summary>Play door open sound.</summary>
        public void PlayOpen()
        {
            PlayClip(_openClip, openVolume);
        }

        /// <summary>Play door close sound.</summary>
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
                Debug.LogWarning($"[DoorAudio] Open slice invalid. Check times {openBegin}-{openEnd} within clip length {combinedClip.length:F2}s", this);
            }
            if (_closeClip == null)
            {
                Debug.LogWarning($"[DoorAudio] Close slice invalid. Check times {closeBegin}-{closeEnd} within clip length {combinedClip.length:F2}s", this);
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

            var clip = AudioClip.Create($"{src.name}_slice_{beginSec:F2}_{endSec:F2}", lengthSamples, channels, src.frequency, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
