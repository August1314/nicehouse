using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NiceHouse.Data;

namespace NiceHouse.HealthMonitoring
{
    /// <summary>
    /// 健康监测UI面板
    /// 显示数字人生命体征数据、健康状态、异常告警
    /// </summary>
    public class HealthMonitoringPanel : MonoBehaviour
    {
        [Header("生命体征显示")]
        public TextMeshProUGUI heartRateText;
        public TextMeshProUGUI respirationRateText;
        public TextMeshProUGUI bodyMovementText;
        public TextMeshProUGUI sleepStageText;

        [Header("体动强度进度条（可选）")]
        public Image bodyMovementProgressBar;

        [Header("健康状态指示")]
        public TextMeshProUGUI healthStatusText;

        [Header("更新设置")]
        [Tooltip("更新间隔（秒）")]
        public float updateInterval = 0.5f;

        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;

            UpdateHealthData();
        }

        /// <summary>
        /// 更新健康数据显示
        /// </summary>
        private void UpdateHealthData()
        {
            if (HealthDataStore.Instance == null) return;

            var health = HealthDataStore.Instance.Current;
            if (health == null) return;

            UpdateHeartRate(health.heartRate);
            UpdateRespirationRate(health.respirationRate);
            UpdateBodyMovement(health.bodyMovement);
            UpdateSleepStage(health.sleepStage);
            UpdateHealthStatus();
        }

        /// <summary>
        /// 更新心率显示
        /// </summary>
        private void UpdateHeartRate(int heartRate)
        {
            if (heartRateText == null) return;

            string color = GetHeartRateColor(heartRate);
            string status = GetHeartRateStatus(heartRate);
            heartRateText.text = $"<color=#CCCCCC>Heart Rate:</color> <color={color}><b>{heartRate}</b></color> <color=#CCCCCC>bpm</color> <color={color}>({status})</color>";
        }

        /// <summary>
        /// 更新呼吸率显示
        /// </summary>
        private void UpdateRespirationRate(int respirationRate)
        {
            if (respirationRateText == null) return;

            string color = GetRespirationRateColor(respirationRate);
            string status = GetRespirationRateStatus(respirationRate);
            respirationRateText.text = $"<color=#CCCCCC>Respiration:</color> <color={color}><b>{respirationRate}</b></color> <color=#CCCCCC>/min</color> <color={color}>({status})</color>";
        }

        /// <summary>
        /// 更新体动显示
        /// </summary>
        private void UpdateBodyMovement(float bodyMovement)
        {
            if (bodyMovementText != null)
            {
                bodyMovementText.text = $"<color=#CCCCCC>Body Movement:</color> <b>{bodyMovement:F2}</b>";
            }

            // 更新进度条
            if (bodyMovementProgressBar != null)
            {
                bodyMovementProgressBar.fillAmount = Mathf.Clamp01(bodyMovement);
                
                // 根据体动强度设置颜色
                Color barColor = bodyMovement < 0.1f ? Color.red : 
                                 bodyMovement < 0.3f ? Color.yellow : 
                                 Color.green;
                bodyMovementProgressBar.color = barColor;
            }
        }

        /// <summary>
        /// 更新睡眠阶段显示
        /// </summary>
        private void UpdateSleepStage(int sleepStage)
        {
            if (sleepStageText == null) return;

            string stageName = GetSleepStageName(sleepStage);
            string color = GetSleepStageColor(sleepStage);
            sleepStageText.text = $"<color=#CCCCCC>Sleep Stage:</color> <color={color}><b>{stageName}</b></color>";
        }

        /// <summary>
        /// 更新健康状态总览
        /// </summary>
        private void UpdateHealthStatus()
        {
            if (healthStatusText == null) return;

            if (HealthDataStore.Instance == null || HealthDataStore.Instance.Current == null)
            {
                healthStatusText.text = "<color=#CCCCCC>Status:</color> <color=#888888>No Data</color>";
                return;
            }

            var health = HealthDataStore.Instance.Current;
            bool isHealthy = IsHealthNormal(health);

            if (isHealthy)
            {
                healthStatusText.text = "<color=#CCCCCC>Status:</color> <color=#00FF00><b>Healthy</b></color>";
            }
            else
            {
                healthStatusText.text = "<color=#CCCCCC>Status:</color> <color=#FF0000><b>Abnormal</b></color>";
            }
        }

        /// <summary>
        /// 判断健康状态是否正常
        /// </summary>
        private bool IsHealthNormal(VitalSignsData health)
        {
            if (HealthMonitoringController.Instance == null) return true;

            bool heartRateOK = health.heartRate >= HealthMonitoringController.Instance.heartRateMin &&
                               health.heartRate <= HealthMonitoringController.Instance.heartRateMax;
            bool respirationOK = health.respirationRate >= HealthMonitoringController.Instance.respirationRateMin &&
                                health.respirationRate <= HealthMonitoringController.Instance.respirationRateMax;
            bool movementOK = health.bodyMovement >= HealthMonitoringController.Instance.bodyMovementMin;

            return heartRateOK && respirationOK && movementOK;
        }

        /// <summary>
        /// 获取心率颜色
        /// </summary>
        private string GetHeartRateColor(int heartRate)
        {
            if (HealthMonitoringController.Instance == null) return "#CCCCCC";

            if (heartRate < HealthMonitoringController.Instance.heartRateMin ||
                heartRate > HealthMonitoringController.Instance.heartRateMax)
            {
                return "#FF0000"; // 红色：异常
            }
            return "#00FF00"; // 绿色：正常
        }

        /// <summary>
        /// 获取心率状态文本
        /// </summary>
        private string GetHeartRateStatus(int heartRate)
        {
            if (HealthMonitoringController.Instance == null) return "Normal";

            if (heartRate < HealthMonitoringController.Instance.heartRateMin)
                return "Low";
            if (heartRate > HealthMonitoringController.Instance.heartRateMax)
                return "High";
            return "Normal";
        }

        /// <summary>
        /// 获取呼吸率颜色
        /// </summary>
        private string GetRespirationRateColor(int respirationRate)
        {
            if (HealthMonitoringController.Instance == null) return "#CCCCCC";

            if (respirationRate < HealthMonitoringController.Instance.respirationRateMin ||
                respirationRate > HealthMonitoringController.Instance.respirationRateMax)
            {
                return "#FF0000"; // 红色：异常
            }
            return "#00FF00"; // 绿色：正常
        }

        /// <summary>
        /// 获取呼吸率状态文本
        /// </summary>
        private string GetRespirationRateStatus(int respirationRate)
        {
            if (HealthMonitoringController.Instance == null) return "Normal";

            if (respirationRate < HealthMonitoringController.Instance.respirationRateMin)
                return "Low";
            if (respirationRate > HealthMonitoringController.Instance.respirationRateMax)
                return "High";
            return "Normal";
        }

        /// <summary>
        /// 获取睡眠阶段名称
        /// </summary>
        private string GetSleepStageName(int stage)
        {
            return stage switch
            {
                0 => "Awake",
                1 => "Light Sleep",
                2 => "Deep Sleep",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// 获取睡眠阶段颜色
        /// </summary>
        private string GetSleepStageColor(int stage)
        {
            return stage switch
            {
                0 => "#CCCCCC", // 灰色：清醒
                1 => "#4A90E2", // 蓝色：浅睡
                2 => "#9013FE", // 紫色：深睡
                _ => "#CCCCCC"
            };
        }
    }
}

