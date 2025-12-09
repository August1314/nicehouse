using UnityEngine;

namespace NiceHouse.SmartMonitoring
{
    /// <summary>
    /// Siren player for the alarm light. Can auto-follow a Light's on/off state
    /// (e.g., smoke_detector toggles the Light) or be controlled manually.
    /// Attach to the alarm GameObject (e.g., Bec-Alarma-Rosu-High-Poly) alongside the flashing light.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AlarmSiren : MonoBehaviour
    {
        [Tooltip("Looping siren clip (e.g., Assets/audio/alarm/alarm.wav).")]
        public AudioClip sirenClip;

        [Tooltip("Playback volume for the siren.")]
        [Range(0f, 1f)] public float volume = 1f;

        [Header("Light Follow (optional)")]
        [Tooltip("If set, siren follows this light's enabled/intensity state.")]
        public Light targetLight;
        [Tooltip("Start siren when light intensity is above this threshold.")]
        public float intensityThreshold = 0.05f;
        [Tooltip("Automatically sync siren with target light state.")]
        public bool followLight = true;

        private AudioSource _audio;
        private bool _isPlaying;
        private bool _lastLightOn;

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.loop = true;
        }

        private void OnEnable()
        {
            _isPlaying = false;
            if (followLight && targetLight != null)
            {
                _lastLightOn = IsLightOn();
                if (_lastLightOn)
                {
                    StartSiren();
                }
            }
            else
            {
                StartSiren();
            }
        }

        private void OnDisable()
        {
            StopSiren();
        }

        private void Update()
        {
            if (!followLight || targetLight == null) return;

            bool lightOn = IsLightOn();
            if (lightOn != _lastLightOn)
            {
                if (lightOn)
                {
                    StartSiren();
                }
                else
                {
                    StopSiren();
                }

                _lastLightOn = lightOn;
            }
        }

        /// <summary>Manually start siren (can be called by smoke_detector).</summary>
        public void StartSiren()
        {
            if (_isPlaying || sirenClip == null) return;
            _audio.clip = sirenClip;
            _audio.volume = volume;
            _audio.Play();
            _isPlaying = true;
        }

        /// <summary>Manually stop siren.</summary>
        public void StopSiren()
        {
            if (!_isPlaying) return;
            _audio.Stop();
            _isPlaying = false;
        }

        private bool IsLightOn()
        {
            return targetLight != null && targetLight.enabled && targetLight.intensity > intensityThreshold;
        }
    }
}
