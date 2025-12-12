using UnityEngine;

namespace NiceHouse.Interaction
{
    /// <summary>
    /// Plays curtain open/close sounds. Can follow a curtain controller (optional) or be triggered manually.
    /// For long curtains (Curtains, Curtains.001): use same slice for open/close (1.43~2.01s of long_curtain.wav).
    /// For short curtains (e.g., Zebra Curtains.002): open 1.03~1.49s, close 5.7~6.3s (short_curtain.wav).
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class CurtainAudio : MonoBehaviour
    {
        [Header("Controller Follow (optional)")]
        [Tooltip("Curtain controller to follow (Animator or other Component). Leave empty for manual calls.")]
        public Component curtainController;

        [Tooltip("Name of bool parameter (Animator) or bool field/property to reflect open state.")]
        public string openStateProperty = "IsOpen";

        [Tooltip("Auto-follow controller and play sounds on state change.")]
        public bool followController = false;

        [Header("Clips")]
        public AudioClip longCurtainClip;
        public AudioClip shortCurtainClip;

        [Header("Long Curtain Slice (both open/close)")]
        public float longBegin = 1.43f;
        public float longEnd = 2.01f;

        [Header("Short Curtain Slices")]
        public float shortOpenBegin = 1.03f;
        public float shortOpenEnd = 1.49f;
        public float shortCloseBegin = 5.7f;
        public float shortCloseEnd = 6.3f;

        [Header("Volumes")]
        public float openVolume = 1f;
        public float closeVolume = 1f;

        private AudioSource _audio;
        private AudioClip _longClipSlice;
        private AudioClip _shortOpenClip;
        private AudioClip _shortCloseClip;
        private bool _lastIsOpen;

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
            _audio.playOnAwake = false;
            BuildClips();
        }

        private void OnEnable()
        {
            _lastIsOpen = GetControllerIsOpen();
        }

        private void OnValidate()
        {
            BuildClips();
        }

        private void Update()
        {
            if (!followController) return;
            bool isOpen = GetControllerIsOpen();
            if (isOpen != _lastIsOpen)
            {
                if (isOpen) PlayOpen();
                else PlayClose();
                _lastIsOpen = isOpen;
            }
        }

        /// <summary>Play open sound for the curtain.</summary>
        public void PlayOpen(bool isLongCurtain = false)
        {
            if (isLongCurtain)
            {
                PlayClip(_longClipSlice, openVolume);
            }
            else
            {
                PlayClip(_shortOpenClip, openVolume);
            }
        }

        /// <summary>Play close sound for the curtain.</summary>
        public void PlayClose(bool isLongCurtain = false)
        {
            if (isLongCurtain)
            {
                PlayClip(_longClipSlice, closeVolume);
            }
            else
            {
                PlayClip(_shortCloseClip, closeVolume);
            }
        }

        private void PlayClip(AudioClip clip, float volume)
        {
            if (clip == null) return;
            _audio.volume = volume;
            _audio.PlayOneShot(clip);
        }

        private void BuildClips()
        {
            _longClipSlice = CreateSubClip(longCurtainClip, longBegin, longEnd);
            _shortOpenClip = CreateSubClip(shortCurtainClip, shortOpenBegin, shortOpenEnd);
            _shortCloseClip = CreateSubClip(shortCurtainClip, shortCloseBegin, shortCloseEnd);
        }

        private bool GetControllerIsOpen()
        {
            if (curtainController == null) return false;

            // Animator parameter
            var anim = curtainController as Animator;
            if (anim != null && !string.IsNullOrEmpty(openStateProperty))
            {
                return anim.GetBool(openStateProperty);
            }

            // Reflection: bool property/field named openStateProperty
            var t = curtainController.GetType();
            if (!string.IsNullOrEmpty(openStateProperty))
            {
                var prop = t.GetProperty(openStateProperty, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null && prop.PropertyType == typeof(bool))
                {
                    return (bool)prop.GetValue(curtainController);
                }

                var field = t.GetField(openStateProperty, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null && field.FieldType == typeof(bool))
                {
                    return (bool)field.GetValue(curtainController);
                }
            }

            return false;
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
