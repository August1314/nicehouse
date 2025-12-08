using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NiceHouse.Data;
using NiceHouse.EnvironmentControl;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 环境智控UI面板
    /// 显示环境数据、设备控制按钮、模式切换
    /// 支持控制所有房间的所有设备
    /// </summary>
    public class EnvironmentControlPanel : MonoBehaviour
    {
        [Header("房间选择")]
        [Tooltip("房间选择下拉菜单（可选）")]
        public TMP_Dropdown roomDropdown;
        
        [Tooltip("当前显示的房间ID")]
        public string currentRoomId = "LivingRoom01";

        [Header("环境数据显示")]
        public TextMeshProUGUI roomNameText;
        public TextMeshProUGUI temperatureText;
        public TextMeshProUGUI humidityText;
        public TextMeshProUGUI pm25Text;
        public TextMeshProUGUI pm10Text;

        [Header("设备控制按钮（旧版，保留兼容性）")]
        [Tooltip("是否显示旧版设备按钮（如果为false，则隐藏这些按钮）")]
        public bool showLegacyButtons = false;
        
        public Button airConditionerButton;
        public TextMeshProUGUI airConditionerStatusText;
        
        public Button airPurifierButton;
        public TextMeshProUGUI airPurifierStatusText;
        
        public Button fanButton;
        public TextMeshProUGUI fanStatusText;
        
        public Button freshAirButton;
        public TextMeshProUGUI freshAirStatusText;

        [Header("设备列表（新版，支持所有设备）")]
        [Tooltip("设备列表容器（ScrollView 的 Content）")]
        public RectTransform deviceListContainer;
        
        [Tooltip("设备项预制体（可选，如果为空则动态创建）")]
        public GameObject deviceItemPrefab;
        
        [Tooltip("显示所有房间的设备（true）还是只显示当前房间的设备（false）")]
        public bool showAllRooms = true;
        
        [Tooltip("设备项高度")]
        public float deviceItemHeight = 50f;
        
        [Tooltip("设备名称宽度")]
        public float deviceNameWidth = 200f;
        
        [Tooltip("房间名称宽度（仅在显示所有房间时使用）")]
        public float roomNameWidth = 120f;
        
        [Tooltip("状态文本宽度")]
        public float statusWidth = 60f;
        
        [Tooltip("按钮宽度")]
        public float buttonWidth = 80f;

        [Header("模式切换")]
        public Toggle autoModeToggle;
        public TextMeshProUGUI modeText;

        [Header("更新设置")]
        [Tooltip("更新间隔（秒）")]
        public float updateInterval = 0.5f;

        private float _timer;
        private EnvironmentController envController;
        private readonly Dictionary<string, GameObject> _deviceItems = new Dictionary<string, GameObject>();

        private void OnValidate()
        {
            // 在编辑器中修改 showLegacyButtons 时立即更新显示
            if (Application.isPlaying)
            {
                UpdateLegacyButtonsVisibility();
            }
        }
        
        private void OnEnable()
        {
            // 当 GameObject 被激活时，确保 legacy 按钮状态正确
            if (Application.isPlaying)
            {
                UpdateLegacyButtonsVisibility();
            }
        }

        private void Start()
        {
            envController = EnvironmentController.Instance;
            
            // 初始化房间下拉菜单
            InitializeRoomDropdown();
            
            // 控制旧版按钮的显示/隐藏
            UpdateLegacyButtonsVisibility();
            
            // 绑定按钮事件（旧版兼容）
            if (airConditionerButton != null)
            {
                airConditionerButton.onClick.AddListener(() => ToggleDevice(NiceHouse.Data.DeviceType.AirConditioner));
            }
            
            if (airPurifierButton != null)
            {
                airPurifierButton.onClick.AddListener(() => ToggleDevice(NiceHouse.Data.DeviceType.AirPurifier));
            }
            
            if (fanButton != null)
            {
                fanButton.onClick.AddListener(() => ToggleDevice(NiceHouse.Data.DeviceType.Fan));
            }
            
            if (freshAirButton != null)
            {
                freshAirButton.onClick.AddListener(() => ToggleDevice(NiceHouse.Data.DeviceType.FreshAirSystem));
            }

            // 绑定模式切换
            if (autoModeToggle != null)
            {
                autoModeToggle.isOn = envController != null && envController.autoMode;
                autoModeToggle.onValueChanged.AddListener(OnModeChanged);
            }

            UpdateAllUI();
        }
        
        /// <summary>
        /// 更新旧版按钮的显示/隐藏状态
        /// </summary>
        private void UpdateLegacyButtonsVisibility()
        {
            bool show = showLegacyButtons;
            
            // 隐藏/显示整个 Row GameObjects（这些是场景中直接存在的）
            string[] rowNames = { "AirConditionerRow", "AirPurifierRow", "FanRow", "FreshAirRow" };
            int foundCount = 0;
            int hiddenCount = 0;
            
            foreach (string rowName in rowNames)
            {
                Transform rowTransform = null;
                
                // 先尝试直接查找
                rowTransform = transform.Find(rowName);
                
                // 如果没找到，尝试递归查找
                if (rowTransform == null)
                {
                    rowTransform = FindChildRecursive(transform, rowName);
                }
                
                // 如果还是没找到，尝试在整个场景中查找（作为最后手段）
                if (rowTransform == null)
                {
                    GameObject foundObj = GameObject.Find(rowName);
                    if (foundObj != null)
                    {
                        rowTransform = foundObj.transform;
                    }
                }
                
                if (rowTransform != null)
                {
                    foundCount++;
                    bool wasActive = rowTransform.gameObject.activeSelf;
                    rowTransform.gameObject.SetActive(show);
                    
                    if (!show && wasActive)
                    {
                        hiddenCount++;
                    }
                }
            }
            
            // 也控制单独的按钮和状态文本（如果它们不在 Row 中）
            if (airConditionerButton != null) airConditionerButton.gameObject.SetActive(show);
            if (airPurifierButton != null) airPurifierButton.gameObject.SetActive(show);
            if (fanButton != null) fanButton.gameObject.SetActive(show);
            if (freshAirButton != null) freshAirButton.gameObject.SetActive(show);
            
            if (airConditionerStatusText != null) airConditionerStatusText.gameObject.SetActive(show);
            if (airPurifierStatusText != null) airPurifierStatusText.gameObject.SetActive(show);
            if (fanStatusText != null) fanStatusText.gameObject.SetActive(show);
            if (freshAirStatusText != null) freshAirStatusText.gameObject.SetActive(show);
            
            // 调试日志
            if (!show && foundCount > 0)
            {
                Debug.Log($"[EnvironmentControlPanel] Legacy buttons hidden: Found {foundCount} rows, Hidden {hiddenCount} active rows");
            }
        }
        
        /// <summary>
        /// 递归查找子对象
        /// </summary>
        private Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    return child;
                }
                
                Transform found = FindChildRecursive(child, name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        /// <summary>
        /// 初始化房间下拉菜单
        /// </summary>
        private void InitializeRoomDropdown()
        {
            if (roomDropdown == null || RoomManager.Instance == null) return;

            roomDropdown.ClearOptions();
            var roomOptions = new List<string>();
            
            // 添加 "All" 选项作为第一个选项
            roomOptions.Add("All");
            
            var allRooms = RoomManager.Instance.GetAllRooms();
            foreach (var room in allRooms.Values)
            {
                // 优先使用英文显示名称，如果 displayName 是中文则使用 roomId
                string displayName = GetEnglishRoomName(room);
                roomOptions.Add(displayName);
            }

            roomDropdown.AddOptions(roomOptions);
            
            // 设置当前选中的房间
            // 如果 showAllRooms 为 true，默认选择 "All"
            if (showAllRooms)
            {
                roomDropdown.value = 0; // "All" 是第一个选项
            }
            else
            {
                // 查找当前房间的索引（需要 +1，因为第一个是 "All"）
                int currentIndex = roomOptions.FindIndex(r => 
                {
                    if (r == "All") return false; // 跳过 "All"
                    if (r == currentRoomId) return true;
                    if (RoomManager.Instance.TryGetRoom(currentRoomId, out var room))
                    {
                        return r == GetEnglishRoomName(room);
                    }
                    return false;
                });
                if (currentIndex >= 0)
                {
                    roomDropdown.value = currentIndex;
                }
            }

            roomDropdown.onValueChanged.AddListener(OnRoomSelected);
        }
        
        /// <summary>
        /// 获取英文房间名称（如果 displayName 是中文，则返回 roomId 或英文映射）
        /// </summary>
        private string GetEnglishRoomName(RoomDefinition room)
        {
            // 如果 displayName 为空，直接使用 roomId
            if (string.IsNullOrEmpty(room.displayName))
            {
                return room.roomId;
            }
            
            // 检查 displayName 是否包含中文字符
            if (ContainsChineseCharacters(room.displayName))
            {
                // 如果包含中文，使用 roomId 或英文映射
                return GetEnglishNameFromRoomId(room.roomId);
            }
            
            // 如果不包含中文，直接使用 displayName
            return room.displayName;
        }
        
        /// <summary>
        /// 检查字符串是否包含中文字符
        /// </summary>
        private bool ContainsChineseCharacters(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            
            foreach (char c in text)
            {
                // Unicode 范围：CJK统一汉字 (0x4E00-0x9FFF)
                if (c >= 0x4E00 && c <= 0x9FFF)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 根据 roomId 获取英文名称
        /// </summary>
        private string GetEnglishNameFromRoomId(string roomId)
        {
            // 房间ID到英文名称的映射（标准格式，查找时使用大小写不敏感匹配）
            var roomNameMap = new Dictionary<string, string>
            {
                // 标准格式的房间 ID 映射
                { "BedRoom01", "Bedroom" },
                { "Bedroom01", "Bedroom" },
                { "LivingRoom01", "Living Room" },
                { "Kitchen01", "Kitchen" },
                { "Study01", "Study" },
                { "Office", "Office" },
                { "Corridor01", "Corridor" },
                { "Bathroom01", "Bathroom" },
                { "DiningRoom01", "Dining Room" }
            };
            
            // 先尝试精确匹配
            if (roomNameMap.TryGetValue(roomId, out string englishName))
            {
                return englishName;
            }
            
            // 如果精确匹配失败，尝试大小写不敏感匹配
            foreach (var kvp in roomNameMap)
            {
                if (string.Equals(kvp.Key, roomId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }
            
            // 如果没有映射，尝试从 roomId 生成英文名称
            // 例如：BedRoom01 -> Bedroom, LivingRoom01 -> Living Room
            string name = roomId;
            
            // 移除末尾的数字
            while (name.Length > 0 && char.IsDigit(name[name.Length - 1]))
            {
                name = name.Substring(0, name.Length - 1);
            }
            
            // 在驼峰命名之间添加空格
            name = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
            
            return name;
        }

        /// <summary>
        /// 房间选择回调
        /// </summary>
        private void OnRoomSelected(int index)
        {
            if (RoomManager.Instance == null) return;

            // index 0 是 "All" 选项
            if (index == 0)
            {
                showAllRooms = true;
                // 保持 currentRoomId 不变，用于显示环境数据（默认显示第一个房间）
                UpdateAllUI();
                return;
            }

            // index >= 1 是具体房间（需要 -1 因为第一个是 "All"）
            var allRooms = RoomManager.Instance.GetAllRooms().Values.ToList();
            int roomIndex = index - 1; // 减去 "All" 选项
            
            if (roomIndex >= 0 && roomIndex < allRooms.Count)
            {
                showAllRooms = false;
                currentRoomId = allRooms[roomIndex].roomId;
                UpdateAllUI();
            }
        }

        private void Update()
        {
            // 持续检查 legacy 按钮的显示状态，确保它们保持隐藏
            if (!showLegacyButtons)
            {
                EnsureLegacyButtonsHidden();
            }
            
            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;

            UpdateAllUI();
        }
        
        /// <summary>
        /// 确保 legacy 按钮保持隐藏状态（防止其他代码重新激活它们）
        /// </summary>
        private void EnsureLegacyButtonsHidden()
        {
            string[] rowNames = { "AirConditionerRow", "AirPurifierRow", "FanRow", "FreshAirRow" };
            
            foreach (string rowName in rowNames)
            {
                Transform rowTransform = transform.Find(rowName);
                if (rowTransform == null)
                {
                    rowTransform = FindChildRecursive(transform, rowName);
                }
                if (rowTransform == null)
                {
                    GameObject foundObj = GameObject.Find(rowName);
                    if (foundObj != null)
                    {
                        rowTransform = foundObj.transform;
                    }
                }
                
                if (rowTransform != null && rowTransform.gameObject.activeSelf)
                {
                    rowTransform.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateAllUI()
        {
            UpdateEnvironmentData();
            UpdateDeviceStatus();
            UpdateModeDisplay();
            UpdateDeviceList();
        }

        /// <summary>
        /// 更新环境数据显示
        /// </summary>
        private void UpdateEnvironmentData()
        {
            if (roomNameText != null)
            {
                if (showAllRooms)
                {
                    // 显示 "All" 时，显示 "All Rooms"
                    roomNameText.text = "<size=36><b>All Rooms</b></size>";
                }
                else if (RoomManager.Instance != null && 
                    RoomManager.Instance.TryGetRoom(currentRoomId, out var room))
                {
                    roomNameText.text = $"<size=36><b>{room.displayName}</b></size>";
                }
                else
                {
                    roomNameText.text = $"<size=36><b>{currentRoomId}</b></size>";
                }
            }
            
            // 如果显示所有房间，环境数据仍然显示当前选中房间的数据（或第一个房间）
            // 如果需要显示汇总数据，可以在这里添加逻辑

            if (EnvironmentDataStore.Instance != null &&
                EnvironmentDataStore.Instance.TryGetRoomData(currentRoomId, out var env))
            {
                // 温度显示（带颜色编码）
                if (temperatureText != null)
                {
                    string color = GetTemperatureColor(env.temperature);
                    temperatureText.text = $"<size=30><color=#CCCCCC>Temperature:</color> <color={color}><b>{env.temperature:F1}</b></color><color=#CCCCCC>°C</color></size>";
                }

                // 湿度显示
                if (humidityText != null)
                {
                    humidityText.text = $"<size=30><color=#CCCCCC>Humidity:</color> <b>{env.humidity:F1}</b><color=#CCCCCC>%</color></size>";
                }

                // PM2.5显示（带颜色编码）
                if (pm25Text != null)
                {
                    string pmColor = GetPM25Color(env.pm25);
                    pm25Text.text = $"<size=30><color=#CCCCCC>PM2.5:</color> <color={pmColor}><b>{env.pm25:F1}</b></color> <color=#CCCCCC>μg/m³</color></size>";
                }

                // PM10显示
                if (pm10Text != null)
                {
                    string pm10Color = GetPM10Color(env.pm10);
                    pm10Text.text = $"<size=30><color=#CCCCCC>PM10:</color> <color={pm10Color}><b>{env.pm10:F1}</b></color> <color=#CCCCCC>μg/m³</color></size>";
                }
            }
            else
            {
                if (temperatureText != null) temperatureText.text = "Temperature: N/A";
                if (humidityText != null) humidityText.text = "Humidity: N/A";
                if (pm25Text != null) pm25Text.text = "PM2.5: N/A";
                if (pm10Text != null) pm10Text.text = "PM10: N/A";
            }
        }

        /// <summary>
        /// 更新设备状态显示
        /// </summary>
        private void UpdateDeviceStatus()
        {
            if (DeviceManager.Instance == null) return;

            var devices = DeviceManager.Instance.GetDevicesInRoom(currentRoomId);
            
            // 空调状态
            UpdateDeviceStatusText(devices, NiceHouse.Data.DeviceType.AirConditioner, airConditionerStatusText);
            
            // 净化器状态
            UpdateDeviceStatusText(devices, NiceHouse.Data.DeviceType.AirPurifier, airPurifierStatusText);
            
            // 风扇状态
            UpdateDeviceStatusText(devices, NiceHouse.Data.DeviceType.Fan, fanStatusText);
            
            // 新风系统状态
            UpdateDeviceStatusText(devices, NiceHouse.Data.DeviceType.FreshAirSystem, freshAirStatusText);
        }

        /// <summary>
        /// 更新单个设备的状态文本
        /// </summary>
        private void UpdateDeviceStatusText(System.Collections.Generic.IReadOnlyList<DeviceDefinition> devices, 
            NiceHouse.Data.DeviceType deviceType, TextMeshProUGUI statusText)
        {
            if (statusText == null) return;

            BaseDeviceController controller = null;
            foreach (var device in devices)
            {
                if (device.type == deviceType)
                {
                    switch (deviceType)
                    {
                        case NiceHouse.Data.DeviceType.AirConditioner:
                            controller = device.GetComponent<AirConditionerController>();
                            break;
                        case NiceHouse.Data.DeviceType.AirPurifier:
                            controller = device.GetComponent<AirPurifierController>();
                            break;
                        case NiceHouse.Data.DeviceType.Fan:
                            controller = device.GetComponent<FanController>();
                            break;
                        case NiceHouse.Data.DeviceType.FreshAirSystem:
                            controller = device.GetComponent<FreshAirController>();
                            break;
                    }
                    break;
                }
            }

            if (controller != null)
            {
                if (controller.IsOn)
                {
                    statusText.text = "<size=28><color=#00FF00><b>ON</b></color></size>";
                }
                else
                {
                    statusText.text = "<size=28><color=#888888>OFF</color></size>";
                }
            }
            else
            {
                statusText.text = "<size=28><color=#FF0000>N/A</color></size>";
            }
        }

        /// <summary>
        /// 更新模式显示
        /// </summary>
        private void UpdateModeDisplay()
        {
            if (modeText != null && envController != null)
            {
                if (envController.autoMode)
                {
                    modeText.text = "<size=32><color=#00FF00><b>AUTO</b></color></size>";
                }
                else
                {
                    modeText.text = "<size=32><color=#FFAA00><b>MANUAL</b></color></size>";
                }
            }
        }

        /// <summary>
        /// 切换设备开关状态
        /// </summary>
        private void ToggleDevice(NiceHouse.Data.DeviceType deviceType)
        {
            if (envController == null) return;

            // 获取当前设备状态
            bool isOn = IsDeviceOn(deviceType);
            
            // 切换状态
            envController.ManualControlDevice(currentRoomId, deviceType, !isOn);
        }

        /// <summary>
        /// 检查设备是否开启
        /// </summary>
        private bool IsDeviceOn(NiceHouse.Data.DeviceType deviceType)
        {
            if (DeviceManager.Instance == null) return false;

            var devices = DeviceManager.Instance.GetDevicesInRoom(currentRoomId);
            foreach (var device in devices)
            {
                if (device.type == deviceType)
                {
                    BaseDeviceController controller = null;
                    switch (deviceType)
                    {
                        case NiceHouse.Data.DeviceType.AirConditioner:
                            controller = device.GetComponent<AirConditionerController>();
                            break;
                        case NiceHouse.Data.DeviceType.AirPurifier:
                            controller = device.GetComponent<AirPurifierController>();
                            break;
                        case NiceHouse.Data.DeviceType.Fan:
                            controller = device.GetComponent<FanController>();
                            break;
                        case NiceHouse.Data.DeviceType.FreshAirSystem:
                            controller = device.GetComponent<FreshAirController>();
                            break;
                    }
                    
                    return controller != null && controller.IsOn;
                }
            }
            
            return false;
        }

        /// <summary>
        /// 模式切换回调
        /// </summary>
        private void OnModeChanged(bool isAuto)
        {
            if (envController != null)
            {
                envController.SetAutoMode(isAuto);
            }
        }

        /// <summary>
        /// 根据温度获取颜色
        /// </summary>
        private string GetTemperatureColor(float temp)
        {
            if (temp < 18f) return "#4A90E2";      // 蓝色（冷）
            if (temp < 24f) return "#7ED321";       // 绿色（舒适）
            if (temp < 28f) return "#F5A623";       // 橙色（热）
            return "#D0021B";                        // 红色（很热）
        }

        /// <summary>
        /// 根据PM2.5获取颜色
        /// </summary>
        private string GetPM25Color(float pm25)
        {
            if (pm25 < 35f) return "#7ED321";       // 绿色（优）
            if (pm25 < 75f) return "#F5A623";       // 橙色（良）
            return "#D0021B";                        // 红色（超标）
        }

        /// <summary>
        /// 根据PM10获取颜色
        /// </summary>
        private string GetPM10Color(float pm10)
        {
            if (pm10 < 50f) return "#7ED321";       // 绿色（优）
            if (pm10 < 150f) return "#F5A623";      // 橙色（良）
            return "#D0021B";                       // 红色（超标）
        }

        /// <summary>
        /// 更新设备列表
        /// </summary>
        private void UpdateDeviceList()
        {
            if (deviceListContainer == null || DeviceManager.Instance == null) return;

            // 获取要显示的设备
            Dictionary<string, DeviceDefinition> devicesToShow = new Dictionary<string, DeviceDefinition>();
            
            if (showAllRooms)
            {
                var allDevices = DeviceManager.Instance.GetAllDevices();
                foreach (var kvp in allDevices)
                {
                    devicesToShow[kvp.Key] = kvp.Value;
                }
            }
            else
            {
                // 先尝试精确匹配
                var devicesInRoom = DeviceManager.Instance.GetDevicesInRoom(currentRoomId);
                
                // 如果精确匹配失败，尝试大小写不敏感匹配
                if (devicesInRoom.Count == 0)
                {
                    var allDevices = DeviceManager.Instance.GetAllDevices();
                    var matchedDevices = new List<DeviceDefinition>();
                    foreach (var device in allDevices.Values)
                    {
                        if (string.Equals(device.roomId, currentRoomId, System.StringComparison.OrdinalIgnoreCase))
                        {
                            matchedDevices.Add(device);
                        }
                    }
                    if (matchedDevices.Count > 0)
                    {
                        devicesInRoom = matchedDevices;
                    }
                }
                
                foreach (var device in devicesInRoom)
                {
                    devicesToShow[device.deviceId] = device;
                }
            }

            // 移除不存在的设备项
            var deviceIdsToRemove = _deviceItems.Keys.Where(id => !devicesToShow.ContainsKey(id)).ToList();
            foreach (var deviceId in deviceIdsToRemove)
            {
                if (_deviceItems.TryGetValue(deviceId, out var item))
                {
                    Destroy(item);
                    _deviceItems.Remove(deviceId);
                }
            }

            // 更新或创建设备项
            foreach (var device in devicesToShow.Values)
            {
                if (!_deviceItems.ContainsKey(device.deviceId))
                {
                    CreateDeviceItem(device);
                }

                UpdateDeviceItem(device);
            }
        }

        /// <summary>
        /// 创建设备项
        /// </summary>
        private void CreateDeviceItem(DeviceDefinition device)
        {
            GameObject itemObj;
            
            if (deviceItemPrefab != null)
            {
                itemObj = Instantiate(deviceItemPrefab, deviceListContainer);
            }
            else
            {
                // 动态创建设备项
                itemObj = new GameObject($"DeviceItem_{device.deviceId}");
                itemObj.transform.SetParent(deviceListContainer, false);
                
                RectTransform rectTransform = itemObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(0, deviceItemHeight);
                
                // 添加水平布局
                HorizontalLayoutGroup layoutGroup = itemObj.AddComponent<HorizontalLayoutGroup>();
                layoutGroup.spacing = 10f;
                layoutGroup.padding = new RectOffset(10, 10, 5, 5);
                layoutGroup.childControlWidth = false;
                layoutGroup.childControlHeight = false;
                layoutGroup.childForceExpandWidth = false;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childAlignment = TextAnchor.MiddleLeft;

                // 设备名称文本
                GameObject nameObj = new GameObject("DeviceName");
                nameObj.transform.SetParent(itemObj.transform, false);
                RectTransform nameRect = nameObj.AddComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0f, 0.5f);
                nameRect.anchorMax = new Vector2(0f, 0.5f);
                nameRect.pivot = new Vector2(0f, 0.5f);
                nameRect.sizeDelta = new Vector2(deviceNameWidth, deviceItemHeight - 10f);
                nameRect.anchoredPosition = Vector2.zero;
                
                LayoutElement nameLayout = nameObj.AddComponent<LayoutElement>();
                nameLayout.preferredWidth = deviceNameWidth;
                nameLayout.flexibleWidth = 0f;
                
                TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
                nameText.text = device.deviceId;
                nameText.fontSize = 14f;
                nameText.color = Color.white;
                nameText.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;
                nameText.enableWordWrapping = false;
                nameText.overflowMode = TextOverflowModes.Ellipsis;
                nameText.raycastTarget = false;

                // 房间名称文本（如果显示所有房间）
                if (showAllRooms)
                {
                    GameObject roomObj = new GameObject("RoomName");
                    roomObj.transform.SetParent(itemObj.transform, false);
                    RectTransform roomRect = roomObj.AddComponent<RectTransform>();
                    roomRect.anchorMin = new Vector2(0f, 0.5f);
                    roomRect.anchorMax = new Vector2(0f, 0.5f);
                    roomRect.pivot = new Vector2(0f, 0.5f);
                    roomRect.sizeDelta = new Vector2(roomNameWidth, deviceItemHeight - 10f);
                    roomRect.anchoredPosition = Vector2.zero;
                    
                    LayoutElement roomLayout = roomObj.AddComponent<LayoutElement>();
                    roomLayout.preferredWidth = roomNameWidth;
                    roomLayout.flexibleWidth = 0f;
                    
                    TextMeshProUGUI roomText = roomObj.AddComponent<TextMeshProUGUI>();
                    roomText.text = device.roomId;
                    roomText.fontSize = 12f;
                    roomText.color = new Color(0.8f, 0.8f, 0.8f);
                    roomText.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;
                    roomText.enableWordWrapping = false;
                    roomText.overflowMode = TextOverflowModes.Ellipsis;
                    roomText.raycastTarget = false;
                }

                // 状态文本
                GameObject statusObj = new GameObject("Status");
                statusObj.transform.SetParent(itemObj.transform, false);
                RectTransform statusRect = statusObj.AddComponent<RectTransform>();
                statusRect.anchorMin = new Vector2(0f, 0.5f);
                statusRect.anchorMax = new Vector2(0f, 0.5f);
                statusRect.pivot = new Vector2(0.5f, 0.5f);
                statusRect.sizeDelta = new Vector2(statusWidth, deviceItemHeight - 10f);
                statusRect.anchoredPosition = Vector2.zero;
                
                LayoutElement statusLayout = statusObj.AddComponent<LayoutElement>();
                statusLayout.preferredWidth = statusWidth;
                statusLayout.flexibleWidth = 0f;
                
                TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
                statusText.text = "OFF";
                statusText.fontSize = 12f;
                statusText.color = Color.gray;
                statusText.alignment = TextAlignmentOptions.Center | TextAlignmentOptions.Midline;
                statusText.enableWordWrapping = false;
                statusText.raycastTarget = false;

                // 控制按钮
                GameObject buttonObj = new GameObject("ToggleButton");
                buttonObj.transform.SetParent(itemObj.transform, false);
                RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0f, 0.5f);
                buttonRect.anchorMax = new Vector2(0f, 0.5f);
                buttonRect.pivot = new Vector2(0.5f, 0.5f);
                buttonRect.sizeDelta = new Vector2(buttonWidth, deviceItemHeight - 10f);
                buttonRect.anchoredPosition = Vector2.zero;
                
                LayoutElement buttonLayout = buttonObj.AddComponent<LayoutElement>();
                buttonLayout.preferredWidth = buttonWidth;
                buttonLayout.flexibleWidth = 0f;
                
                Image buttonImage = buttonObj.AddComponent<Image>();
                buttonImage.color = new Color(0.2f, 0.6f, 1f);
                
                Button button = buttonObj.AddComponent<Button>();
                button.onClick.AddListener(() => ToggleDeviceById(device.deviceId));
                
                GameObject buttonTextObj = new GameObject("Text");
                buttonTextObj.transform.SetParent(buttonObj.transform, false);
                RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
                buttonTextRect.anchorMin = Vector2.zero;
                buttonTextRect.anchorMax = Vector2.one;
                buttonTextRect.sizeDelta = Vector2.zero;
                
                TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
                buttonText.text = "Turn On";
                buttonText.fontSize = 12f;
                buttonText.color = Color.white;
                buttonText.alignment = TextAlignmentOptions.Center | TextAlignmentOptions.Midline;
                buttonText.raycastTarget = false;
            }

            _deviceItems[device.deviceId] = itemObj;
        }

        /// <summary>
        /// 更新设备项显示
        /// </summary>
        private void UpdateDeviceItem(DeviceDefinition device)
        {
            if (!_deviceItems.TryGetValue(device.deviceId, out var itemObj)) return;

            var controller = device.GetComponent<BaseDeviceController>();
            bool isOn = controller != null && controller.IsOn;

            if (deviceItemPrefab == null)
            {
                var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
                var buttons = itemObj.GetComponentsInChildren<Button>();
                
                // 更新状态文本
                TextMeshProUGUI statusText = null;
                Button toggleButton = null;
                
                foreach (var text in texts)
                {
                    if (text.name == "Status")
                    {
                        statusText = text;
                    }
                }
                
                foreach (var button in buttons)
                {
                    if (button.name == "ToggleButton")
                    {
                        toggleButton = button;
                    }
                }

                if (statusText != null)
                {
                    if (isOn)
                    {
                        statusText.text = "<color=#00FF00>ON</color>";
                    }
                    else
                    {
                        statusText.text = "<color=#888888>OFF</color>";
                    }
                }

                if (toggleButton != null)
                {
                    var buttonText = toggleButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = isOn ? "Turn Off" : "Turn On";
                    }
                    
                    var buttonImage = toggleButton.GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        buttonImage.color = isOn ? new Color(1f, 0.4f, 0.4f) : new Color(0.2f, 0.6f, 1f);
                    }
                }
            }
            else
            {
                // 如果使用预制体，需要根据预制体的结构来更新
                var deviceItem = itemObj.GetComponent<EnvironmentDeviceItem>();
                if (deviceItem != null)
                {
                    deviceItem.UpdateData(device, isOn);
                }
            }
        }

        /// <summary>
        /// 通过设备ID切换设备状态
        /// </summary>
        private void ToggleDeviceById(string deviceId)
        {
            if (envController == null || DeviceManager.Instance == null) return;

            if (!DeviceManager.Instance.TryGetDevice(deviceId, out var device)) return;

            var controller = device.GetComponent<BaseDeviceController>();
            if (controller == null) return;

            bool isOn = controller.IsOn;
            envController.ManualControlDeviceById(deviceId, !isOn);
        }
    }

    /// <summary>
    /// 环境设备项组件（用于预制体）
    /// </summary>
    public class EnvironmentDeviceItem : MonoBehaviour
    {
        public TextMeshProUGUI deviceNameText;
        public TextMeshProUGUI roomNameText;
        public TextMeshProUGUI statusText;
        public Button toggleButton;

        public void UpdateData(DeviceDefinition device, bool isOn)
        {
            if (deviceNameText != null)
            {
                deviceNameText.text = device.deviceId;
            }

            if (roomNameText != null && !string.IsNullOrEmpty(device.roomId))
            {
                roomNameText.text = device.roomId;
            }

            if (statusText != null)
            {
                statusText.text = isOn ? "<color=#00FF00>ON</color>" : "<color=#888888>OFF</color>";
            }

            if (toggleButton != null)
            {
                var buttonText = toggleButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = isOn ? "Turn Off" : "Turn On";
                }
            }
        }
    }
}

