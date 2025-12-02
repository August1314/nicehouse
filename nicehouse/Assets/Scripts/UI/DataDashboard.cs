using UnityEngine;
using TMPro;
using NiceHouse.Data;
using System.Linq;

namespace NiceHouse.UI
{
    /// <summary>
    /// 数据底座综合仪表盘，显示所有数据模块的状态。
    /// </summary>
    public class DataDashboard : MonoBehaviour
    {
        [Header("房间选择")]
        [Tooltip("当前显示的房间ID")]
        public string currentRoomId = "LivingRoom01";

        [Header("环境数据")]
        public TextMeshProUGUI roomNameText;
        public TextMeshProUGUI temperatureText;
        public TextMeshProUGUI humidityText;
        public TextMeshProUGUI pm25Text;

        [Header("能耗数据")]
        public TextMeshProUGUI energyText;

        [Header("数字人状态")]
        public TextMeshProUGUI personStateText;
        public TextMeshProUGUI personRoomText;
        public TextMeshProUGUI stateDurationText;

        [Header("健康数据")]
        public TextMeshProUGUI heartRateText;
        public TextMeshProUGUI respirationText;
        public TextMeshProUGUI bodyMovementText;
        public TextMeshProUGUI sleepStageText;

        [Header("活动追踪")]
        public TextMeshProUGUI visitCountText;
        public TextMeshProUGUI stayTimeText;

        [Header("安全数据")]
        public TextMeshProUGUI smokeLevelText;
        public TextMeshProUGUI gasLevelText;

        [Header("告警信息")]
        public TextMeshProUGUI alarmListText;

        [Header("更新设置")]
        [Tooltip("更新间隔（秒）")]
        public float updateInterval = 0.5f;

        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;

            UpdateAllData();
        }

        private void UpdateAllData()
        {
            UpdateEnvironmentData();
            UpdateEnergyData();
            UpdatePersonState();
            UpdateHealthData();
            UpdateActivityTracking();
            UpdateSafetyData();
            UpdateAlarmList();
        }

        private void UpdateEnvironmentData()
        {
            if (roomNameText != null)
            {
                roomNameText.text = $"<size=24><b>{currentRoomId}</b></size>";
            }

            if (EnvironmentDataStore.Instance != null &&
                EnvironmentDataStore.Instance.TryGetRoomData(currentRoomId, out var env))
            {
                if (temperatureText != null)
                {
                    // 根据温度设置颜色
                    string color = GetTemperatureColor(env.temperature);
                    temperatureText.text = $"<color=#CCCCCC>Temp:</color> <color={color}><b>{env.temperature:F1}</b></color><color=#CCCCCC>°C</color>";
                }
                if (humidityText != null)
                    humidityText.text = $"<color=#CCCCCC>Humidity:</color> <b>{env.humidity:F1}</b><color=#CCCCCC>%</color>";
                if (pm25Text != null)
                {
                    // 根据PM2.5设置颜色
                    string pmColor = GetPM25Color(env.pm25);
                    pm25Text.text = $"<color=#CCCCCC>PM2.5:</color> <color={pmColor}><b>{env.pm25:F1}</b></color> <color=#CCCCCC>μg/m³</color>";
                }
            }
            else
            {
                if (temperatureText != null) temperatureText.text = "<color=#CCCCCC>Temp:</color> <color=#888888>--</color>";
                if (humidityText != null) humidityText.text = "<color=#CCCCCC>Humidity:</color> <color=#888888>--</color>";
                if (pm25Text != null) pm25Text.text = "<color=#CCCCCC>PM2.5:</color> <color=#888888>--</color>";
            }
        }

        private void UpdateEnergyData()
        {
            if (energyText == null) return;

            if (EnergyManager.Instance != null)
            {
                // 计算当前房间所有设备的累计能耗
                float totalEnergy = 0f;
                if (DeviceManager.Instance != null)
                {
                    var devices = DeviceManager.Instance.GetDevicesInRoom(currentRoomId);
                    foreach (var device in devices)
                    {
                        totalEnergy += EnergyManager.Instance.GetDeviceDailyConsumption(device.deviceId);
                    }
                }
                energyText.text = $"<color=#CCCCCC>Energy:</color> <b>{totalEnergy:F4}</b> <color=#CCCCCC>kWh</color>";
            }
            else
            {
                energyText.text = "<color=#CCCCCC>Energy:</color> <color=#888888>--</color>";
            }
        }

        private void UpdatePersonState()
        {
            if (PersonStateController.Instance != null && PersonStateController.Instance.Status != null)
            {
                var status = PersonStateController.Instance.Status;
                
                if (personStateText != null)
                {
                    string stateColor = GetStateColor(status.state);
                    personStateText.text = $"<color=#CCCCCC>State:</color> <color={stateColor}><b>{GetStateName(status.state)}</b></color>";
                }
                if (personRoomText != null)
                    personRoomText.text = $"<color=#CCCCCC>Room:</color> <b>{status.currentRoomId}</b>";
                if (stateDurationText != null)
                    stateDurationText.text = $"<color=#CCCCCC>Duration:</color> <b>{status.stateDuration:F1}</b><color=#CCCCCC>s</color>";
            }
            else
            {
                if (personStateText != null) personStateText.text = "<color=#CCCCCC>State:</color> <color=#888888>--</color>";
                if (personRoomText != null) personRoomText.text = "<color=#CCCCCC>Room:</color> <color=#888888>--</color>";
                if (stateDurationText != null) stateDurationText.text = "<color=#CCCCCC>Duration:</color> <color=#888888>--</color>";
            }
        }

        private void UpdateHealthData()
        {
            if (HealthDataStore.Instance != null && HealthDataStore.Instance.Current != null)
            {
                var health = HealthDataStore.Instance.Current;
                
                if (heartRateText != null)
                {
                    string hrColor = GetHeartRateColor(health.heartRate);
                    heartRateText.text = $"<color=#CCCCCC>Heart Rate:</color> <color={hrColor}><b>{health.heartRate}</b></color> <color=#CCCCCC>bpm</color>";
                }
                if (respirationText != null)
                    respirationText.text = $"<color=#CCCCCC>Respiration:</color> <b>{health.respirationRate}</b> <color=#CCCCCC>/min</color>";
                if (bodyMovementText != null)
                    bodyMovementText.text = $"<color=#CCCCCC>Body Movement:</color> <b>{health.bodyMovement:F2}</b>";
                if (sleepStageText != null)
                {
                    string sleepColor = GetSleepStageColor(health.sleepStage);
                    sleepStageText.text = $"<color=#CCCCCC>Sleep Stage:</color> <color={sleepColor}><b>{GetSleepStageName(health.sleepStage)}</b></color>";
                }
            }
            else
            {
                if (heartRateText != null) heartRateText.text = "<color=#CCCCCC>Heart Rate:</color> <color=#888888>--</color>";
                if (respirationText != null) respirationText.text = "<color=#CCCCCC>Respiration:</color> <color=#888888>--</color>";
                if (bodyMovementText != null) bodyMovementText.text = "<color=#CCCCCC>Body Movement:</color> <color=#888888>--</color>";
                if (sleepStageText != null) sleepStageText.text = "<color=#CCCCCC>Sleep Stage:</color> <color=#888888>--</color>";
            }
        }

        private void UpdateActivityTracking()
        {
            if (ActivityTracker.Instance != null)
            {
                var activity = ActivityTracker.Instance.GetRoomActivity(currentRoomId);
                
                if (visitCountText != null)
                    visitCountText.text = $"<color=#CCCCCC>Visits:</color> <b>{activity.visitCount}</b>";
                if (stayTimeText != null)
                    stayTimeText.text = $"<color=#CCCCCC>Stay Time:</color> <b>{activity.totalStayTime:F1}</b><color=#CCCCCC>s</color>";
            }
            else
            {
                if (visitCountText != null) visitCountText.text = "<color=#CCCCCC>Visits:</color> <color=#888888>--</color>";
                if (stayTimeText != null) stayTimeText.text = "<color=#CCCCCC>Stay Time:</color> <color=#888888>--</color>";
            }
        }

        private void UpdateSafetyData()
        {
            if (SafetyDataStore.Instance != null &&
                SafetyDataStore.Instance.TryGetRoomSafety(currentRoomId, out var safety))
            {
                if (smokeLevelText != null)
                {
                    string smokeColor = safety.smokeLevel > 50f ? "#FF4444" : "#88FF88";
                    smokeLevelText.text = $"<color=#CCCCCC>Smoke:</color> <color={smokeColor}><b>{safety.smokeLevel:F1}</b></color>";
                }
                if (gasLevelText != null)
                {
                    string gasColor = safety.gasLevel > 30f ? "#FF4444" : "#88FF88";
                    gasLevelText.text = $"<color=#CCCCCC>Gas:</color> <color={gasColor}><b>{safety.gasLevel:F1}</b></color>";
                }
            }
            else
            {
                if (smokeLevelText != null) smokeLevelText.text = "<color=#CCCCCC>Smoke:</color> <color=#888888>--</color>";
                if (gasLevelText != null) gasLevelText.text = "<color=#CCCCCC>Gas:</color> <color=#888888>--</color>";
            }
        }

        private void UpdateAlarmList()
        {
            if (alarmListText == null) return;

            if (AlarmManager.Instance != null)
            {
                var recentAlarms = AlarmManager.Instance.GetRecentAlarms(5);
                if (recentAlarms.Any())
                {
                    var alarmLines = recentAlarms.Select(a => 
                    {
                        string color = a.handled ? "#888888" : "#FF4444";
                        string status = a.handled ? "[Handled]" : "[Active]";
                        return $"<color={color}>• {a.type}</color> <color=#CCCCCC>({a.roomId})</color> <color=#888888>{a.time:HH:mm:ss}</color> <color={color}>{status}</color>";
                    });
                    alarmListText.text = "<color=#CCCCCC><size=16><b>Recent Alarms:</b></size></color>\n" + string.Join("\n", alarmLines);
                }
                else
                {
                    alarmListText.text = "<color=#CCCCCC><size=16><b>Recent Alarms:</b></size></color>\n<color=#888888>None</color>";
                }
            }
            else
            {
                alarmListText.text = "<color=#CCCCCC>Recent Alarms:</color> <color=#888888>--</color>";
            }
        }

        private string GetStateName(PersonState state)
        {
            return state switch
            {
                PersonState.Idle => "Idle",
                PersonState.Walking => "Walking",
                PersonState.Sitting => "Sitting",
                PersonState.Bathing => "Bathing",
                PersonState.Sleeping => "Sleeping",
                PersonState.Fallen => "Fallen",
                PersonState.OutOfBed => "OutOfBed",
                _ => "Unknown"
            };
        }

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

        // 颜色辅助方法
        private string GetTemperatureColor(float temp)
        {
            if (temp < 18f) return "#4A90E2";      // 蓝色（冷）
            if (temp < 22f) return "#7ED321";      // 绿色（舒适）
            if (temp < 26f) return "#F5A623";       // 橙色（温暖）
            return "#D0021B";                      // 红色（热）
        }

        private string GetPM25Color(float pm25)
        {
            if (pm25 < 35f) return "#7ED321";      // 绿色（良好）
            if (pm25 < 75f) return "#F5A623";      // 橙色（中等）
            return "#D0021B";                      // 红色（差）
        }

        private string GetStateColor(PersonState state)
        {
            return state switch
            {
                PersonState.Idle => "#CCCCCC",
                PersonState.Walking => "#7ED321",
                PersonState.Sitting => "#F5A623",
                PersonState.Bathing => "#4A90E2",
                PersonState.Sleeping => "#9013FE",
                PersonState.Fallen => "#D0021B",
                PersonState.OutOfBed => "#D0021B",
                _ => "#CCCCCC"
            };
        }

        private string GetHeartRateColor(int hr)
        {
            if (hr < 60) return "#4A90E2";         // 蓝色（偏低）
            if (hr <= 100) return "#7ED321";       // 绿色（正常）
            return "#D0021B";                      // 红色（偏高）
        }

        private string GetSleepStageColor(int stage)
        {
            return stage switch
            {
                0 => "#CCCCCC",                    // 灰色（清醒）
                1 => "#4A90E2",                    // 蓝色（浅睡）
                2 => "#9013FE",                    // 紫色（深睡）
                _ => "#CCCCCC"
            };
        }
    }
}

