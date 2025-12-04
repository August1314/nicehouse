using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NiceHouse.Data;
using System.Linq;
using System.Collections.Generic;

namespace NiceHouse.SmartMonitoring
{
    /// <summary>
    /// 智能监护UI面板
    /// 显示数字人状态、告警列表、控制按钮
    /// </summary>
    public class MonitoringPanel : MonoBehaviour
    {
        [Header("状态显示")]
        public TextMeshProUGUI stateText;
        public TextMeshProUGUI roomText;
        public TextMeshProUGUI durationText;

        [Header("状态切换按钮")]
        public Button idleButton;
        public Button walkingButton;
        public Button sittingButton;
        public Button bathingButton;
        public Button sleepingButton;
        public Button fallenButton;
        public Button outOfBedButton;

        [Header("房间选择")]
        public TMP_Dropdown roomDropdown;

        [Header("告警列表")]
        public Transform alarmListContent;
        public GameObject alarmItemPrefab;

        [Header("告警设置")]
        public TMP_InputField longSittingThresholdInput;
        public TMP_InputField longBathingThresholdInput;
        public Toggle enableMonitoringToggle;
        public Button applySettingsButton;

        [Header("更新设置")]
        [Tooltip("更新间隔（秒）")]
        public float updateInterval = 0.5f;

        private float _timer;
        private PersonStateSimulator _stateSimulator;

        private void Start()
        {
            // 查找或创建 PersonStateSimulator
            _stateSimulator = FindObjectOfType<PersonStateSimulator>();
            if (_stateSimulator == null)
            {
                GameObject simulatorObj = new GameObject("PersonStateSimulator");
                _stateSimulator = simulatorObj.AddComponent<PersonStateSimulator>();
            }

            // 绑定状态切换按钮
            if (idleButton != null) idleButton.onClick.AddListener(() => ChangeState(PersonState.Idle));
            if (walkingButton != null) walkingButton.onClick.AddListener(() => ChangeState(PersonState.Walking));
            if (sittingButton != null) sittingButton.onClick.AddListener(() => ChangeState(PersonState.Sitting));
            if (bathingButton != null) bathingButton.onClick.AddListener(() => ChangeState(PersonState.Bathing));
            if (sleepingButton != null) sleepingButton.onClick.AddListener(() => ChangeState(PersonState.Sleeping));
            if (fallenButton != null) fallenButton.onClick.AddListener(() => ChangeState(PersonState.Fallen));
            if (outOfBedButton != null) outOfBedButton.onClick.AddListener(() => ChangeState(PersonState.OutOfBed));

            // 初始化房间下拉菜单
            InitializeRoomDropdown();

            // 初始化告警设置
            InitializeAlarmSettings();

            // 订阅告警事件
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.OnAlarmAdded += OnAlarmAdded;
            }

            UpdateUI();
        }

        private void OnDestroy()
        {
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.OnAlarmAdded -= OnAlarmAdded;
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;

            UpdateUI();
        }

        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            UpdatePersonState();
            UpdateAlarmList();
        }

        /// <summary>
        /// 更新数字人状态显示
        /// </summary>
        private void UpdatePersonState()
        {
            if (PersonStateController.Instance == null) return;

            var status = PersonStateController.Instance.Status;
            if (status == null) return;

            if (stateText != null)
            {
                string stateName = GetStateName(status.state);
                string stateColor = GetStateColor(status.state);
                stateText.text = $"<color=#CCCCCC>State:</color> <color={stateColor}><b>{stateName}</b></color>";
            }

            if (roomText != null)
            {
                roomText.text = $"<color=#CCCCCC>Room:</color> <b>{status.currentRoomId}</b>";
            }

            if (durationText != null)
            {
                int minutes = Mathf.FloorToInt(status.stateDuration / 60f);
                int seconds = Mathf.FloorToInt(status.stateDuration % 60f);
                durationText.text = $"<color=#CCCCCC>Duration:</color> <b>{minutes:D2}:{seconds:D2}</b>";
            }
        }

        /// <summary>
        /// 更新告警列表
        /// </summary>
        private void UpdateAlarmList()
        {
            if (AlarmManager.Instance == null || alarmListContent == null) return;

            // 清除旧列表项
            foreach (Transform child in alarmListContent)
            {
                Destroy(child.gameObject);
            }

            // 获取最近10条告警
            var alarms = AlarmManager.Instance.GetRecentAlarms(10);

            foreach (var alarm in alarms)
            {
                CreateAlarmItem(alarm);
            }
        }

        /// <summary>
        /// 创建告警列表项
        /// </summary>
        private void CreateAlarmItem(AlarmRecord alarm)
        {
            if (alarmItemPrefab == null) return;

            GameObject item = Instantiate(alarmItemPrefab, alarmListContent);
            
            // 查找文本组件并设置内容
            TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                string textName = text.gameObject.name.ToLower();
                
                if (textName.Contains("type") || textName.Contains("alarm"))
                {
                    text.text = GetAlarmTypeName(alarm.type);
                }
                else if (textName.Contains("room"))
                {
                    text.text = alarm.roomId;
                }
                else if (textName.Contains("time"))
                {
                    text.text = alarm.time.ToString("HH:mm:ss");
                }
                else if (textName.Contains("status") || textName.Contains("handled"))
                {
                    text.text = alarm.handled ? "Handled" : "Unhandled";
                    text.color = alarm.handled ? Color.gray : Color.red;
                }
            }

            // 查找"已处理"按钮
            Button handleButton = item.GetComponentInChildren<Button>();
            if (handleButton != null && !alarm.handled)
            {
                handleButton.onClick.AddListener(() => MarkAlarmHandled(alarm));
            }
            else if (handleButton != null && alarm.handled)
            {
                handleButton.interactable = false;
            }
        }

        /// <summary>
        /// 改变数字人状态
        /// </summary>
        private void ChangeState(PersonState newState)
        {
            if (_stateSimulator == null) return;

            string roomId = GetSelectedRoomId();
            _stateSimulator.ChangeState(newState, roomId);
        }

        /// <summary>
        /// 获取选中的房间ID
        /// </summary>
        private string GetSelectedRoomId()
        {
            if (roomDropdown != null && roomDropdown.options.Count > 0)
            {
                int index = roomDropdown.value;
                if (index >= 0 && index < roomDropdown.options.Count)
                {
                    return roomDropdown.options[index].text;
                }
            }

            return _stateSimulator != null ? _stateSimulator.currentRoomId : "LivingRoom01";
        }

        /// <summary>
        /// 初始化房间下拉菜单
        /// </summary>
        private void InitializeRoomDropdown()
        {
            if (roomDropdown == null) return;

            roomDropdown.ClearOptions();

            if (RoomManager.Instance != null)
            {
                var rooms = RoomManager.Instance.GetAllRooms();
                List<string> roomNames = new List<string>();
                foreach (var room in rooms)
                {
                    roomNames.Add(room.Key); // 使用 Key 获取 roomId
                }
                roomDropdown.AddOptions(roomNames);
            }
            else
            {
                // 默认房间列表
                roomDropdown.AddOptions(new List<string> { "LivingRoom01", "BedRoom01", "Kitchen01", "BathRoom01" });
            }

            // 监听房间选择变化
            roomDropdown.onValueChanged.AddListener(OnRoomSelected);
        }

        /// <summary>
        /// 房间选择变化处理
        /// </summary>
        private void OnRoomSelected(int index)
        {
            if (_stateSimulator != null && roomDropdown != null)
            {
                string roomId = roomDropdown.options[index].text;
                _stateSimulator.SetCurrentRoom(roomId);
            }
        }

        /// <summary>
        /// 初始化告警设置
        /// </summary>
        private void InitializeAlarmSettings()
        {
            if (MonitoringController.Instance != null)
            {
                if (longSittingThresholdInput != null)
                {
                    longSittingThresholdInput.text = MonitoringController.Instance.longSittingThreshold.ToString();
                }

                if (longBathingThresholdInput != null)
                {
                    longBathingThresholdInput.text = MonitoringController.Instance.longBathingThreshold.ToString();
                }

                if (enableMonitoringToggle != null)
                {
                    enableMonitoringToggle.isOn = MonitoringController.Instance.enableMonitoring;
                }
            }

            if (applySettingsButton != null)
            {
                applySettingsButton.onClick.AddListener(ApplyAlarmSettings);
            }
        }

        /// <summary>
        /// 应用告警设置
        /// </summary>
        private void ApplyAlarmSettings()
        {
            if (MonitoringController.Instance == null) return;

            if (longSittingThresholdInput != null)
            {
                if (float.TryParse(longSittingThresholdInput.text, out float value))
                {
                    MonitoringController.Instance.longSittingThreshold = value;
                }
            }

            if (longBathingThresholdInput != null)
            {
                if (float.TryParse(longBathingThresholdInput.text, out float value))
                {
                    MonitoringController.Instance.longBathingThreshold = value;
                }
            }

            if (enableMonitoringToggle != null)
            {
                MonitoringController.Instance.enableMonitoring = enableMonitoringToggle.isOn;
            }

            Debug.Log("[MonitoringPanel] Alarm settings applied");
        }

        /// <summary>
        /// 告警添加事件处理
        /// </summary>
        private void OnAlarmAdded(AlarmRecord record)
        {
            // 告警添加时更新列表
            UpdateAlarmList();
        }

        /// <summary>
        /// 标记告警为已处理
        /// </summary>
        private void MarkAlarmHandled(AlarmRecord record)
        {
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.MarkHandled(record);
                UpdateAlarmList();
            }
        }

        /// <summary>
        /// 获取状态名称
        /// </summary>
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
                _ => state.ToString()
            };
        }

        /// <summary>
        /// 获取状态颜色
        /// </summary>
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

        /// <summary>
        /// 获取告警类型名称
        /// </summary>
        private string GetAlarmTypeName(AlarmType type)
        {
            return type switch
            {
                AlarmType.LongSitting => "Long Sitting",
                AlarmType.LongBathing => "Long Bathing",
                AlarmType.Fall => "Fall/OutOfBed",
                AlarmType.Smoke => "Smoke",
                AlarmType.GasLeak => "Gas Leak",
                AlarmType.HealthAbnormal => "Health Abnormal",
                AlarmType.EmergencyCall => "Emergency Call",
                _ => type.ToString()
            };
        }
    }
}

