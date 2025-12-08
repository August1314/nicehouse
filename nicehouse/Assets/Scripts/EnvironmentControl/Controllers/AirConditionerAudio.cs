using System.Collections;
using UnityEngine;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// Plays start/loop/stop sounds for an AirConditionerController based on its on/off state.
    /// Source clip is a combined file; this script slices it at runtime to separate clips.
    /// </summary>
    [RequireComponent(typeof(AirConditionerController))]
    public class AirConditionerAudio : MonoBehaviour
    {
        [Header("Source Audio")]
        [Tooltip("Combined audio clip that contains start, loop and stop in one file.")]
        public AudioClip combinedClip;

        [Header("Slice Points (seconds)")]
        [Tooltip("Start segment begins at this time.")]
        public float startBegin = 1.3f;
        [Tooltip("Start segment ends at this time (keep it short to avoid dead air).")]
        public float startEnd = 2.8f;
        [Tooltip("Loop audio is sliced from this time in the source clip.")]
        public float loopSliceBegin = 13f;
        [Tooltip("Loop audio ends at this time in the source clip.")]
        public float loopSliceEnd = 25f;
        [Tooltip("Stop segment begins at this time and continues to clip end.")]
        public float stopBegin = 26.5f;

        [Header("Playback Timing")]
        [Tooltip("Delay (seconds) after start sfx before loop begins. Set 0 for immediate.")]
        public float loopStartDelay = 0.6f;

        [Header("Volumes")]
        public float startVolume = 1f;
        public float loopVolume = 0.6f;
        public float stopVolume = 1f;

        private AirConditionerController _controller;
        private AudioSource _sfxSource;   // start/stop
        private AudioSource _loopSource;  // looping bed

        private AudioClip _startClip;
        private AudioClip _loopClip;
        private AudioClip _stopClip;

        private Coroutine _loopDelayRoutine;
        private bool _lastIsOn;

        private void Awake()
        {
            _controller = GetComponent<AirConditionerController>();

            // Prepare audio sources
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _loopSource = gameObject.AddComponent<AudioSource>();
            _loopSource.playOnAwake = false;
            _loopSource.loop = true;

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

        private void HandleTurnOn()
        {
            // Cancel any pending loop start, then play start and queue loop
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
                _loopSource.Play();
            }
        }

        private void BuildClips()
        {
            if (combinedClip == null) return;

            _startClip = CreateSubClip(combinedClip, startBegin, startEnd, false);
            _loopClip = CreateSubClip(combinedClip, loopSliceBegin, loopSliceEnd, true);
            _stopClip = CreateSubClip(combinedClip, stopBegin, combinedClip.length, false);
        }

        private static AudioClip CreateSubClip(AudioClip src, float beginSec, float endSec, bool loop)
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
            // AudioSource controls looping; AudioClip no longer exposes wrapMode in newer Unity versions.
            return clip;
        }
    }
}
