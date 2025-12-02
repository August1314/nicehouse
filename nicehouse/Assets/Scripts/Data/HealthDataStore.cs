using UnityEngine;

namespace NiceHouse.Data
{
    /// <summary>
    /// 生命体征数据。
    /// </summary>
    [System.Serializable]
    public class VitalSignsData
    {
        public int heartRate;        // 心率（bpm）
        public int respirationRate;  // 呼吸率（次/分钟）
        public float bodyMovement;   // 体动强度（0-1）
        public int sleepStage;       // 睡眠阶段：0-清醒，1-浅睡，2-深睡
    }

    /// <summary>
    /// 健康数据存储与模拟器。
    /// </summary>
    public class HealthDataStore : MonoBehaviour
    {
        public static HealthDataStore Instance { get; private set; }

        [Tooltip("数据更新间隔（秒）")]
        public float updateInterval = 1f;

        [Tooltip("基础心率（bpm）")]
        public int baseHeartRate = 72;

        [Tooltip("心率波动范围（bpm）")]
        public int heartRateAmplitude = 10;

        [Tooltip("基础呼吸率（次/分钟）")]
        public int baseRespirationRate = 16;

        [Tooltip("呼吸率波动范围")]
        public int respirationAmplitude = 4;

        public VitalSignsData Current { get; private set; }

        private float _timer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 初始化数据
            Current = new VitalSignsData
            {
                heartRate = baseHeartRate,
                respirationRate = baseRespirationRate,
                bodyMovement = 0.1f,
                sleepStage = 0
            };
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;

            SimulateNextStep();
        }

        /// <summary>
        /// 模拟下一步健康数据变化。
        /// </summary>
        public void SimulateNextStep()
        {
            if (Current == null)
            {
                Current = new VitalSignsData();
            }

            var t = Time.time;

            // 心率：基础值 + 正弦波动
            Current.heartRate = Mathf.RoundToInt(
                baseHeartRate + Mathf.Sin(t * 0.1f) * heartRateAmplitude
            );

            // 呼吸率：基础值 + 正弦波动
            Current.respirationRate = Mathf.RoundToInt(
                baseRespirationRate + Mathf.Sin(t * 0.15f) * respirationAmplitude
            );

            // 体动强度：根据数字人状态决定
            if (PersonStateController.Instance != null)
            {
                var state = PersonStateController.Instance.Status?.state ?? PersonState.Idle;
                Current.bodyMovement = state switch
                {
                    PersonState.Walking => 0.8f + Random.Range(-0.1f, 0.1f),
                    PersonState.Sitting => 0.2f + Random.Range(-0.1f, 0.1f),
                    PersonState.Sleeping => 0.05f + Random.Range(-0.02f, 0.02f),
                    PersonState.Fallen => 0.9f + Random.Range(-0.1f, 0.1f),
                    _ => 0.3f + Random.Range(-0.1f, 0.1f)
                };
            }
            else
            {
                Current.bodyMovement = 0.3f + Mathf.Sin(t * 0.2f) * 0.2f;
            }

            // 睡眠阶段：根据数字人状态
            if (PersonStateController.Instance != null)
            {
                var state = PersonStateController.Instance.Status?.state ?? PersonState.Idle;
                Current.sleepStage = state switch
                {
                    PersonState.Sleeping => Random.value > 0.5f ? 1 : 2, // 浅睡或深睡
                    _ => 0 // 清醒
                };
            }
            else
            {
                Current.sleepStage = 0;
            }
        }
    }
}

