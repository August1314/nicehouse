using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NiceHouse.Data;
using NiceHouse.EnvironmentControl;

namespace NiceHouse.UI
{
    /// <summary>
    /// 能源消耗面板
    /// 显示所有设备的能源消耗情况，包括当前功率和累计用电量
    /// </summary>
    public class EnergyConsumptionPanel : MonoBehaviour
    {
        [Header("总体统计")]
        [Tooltip("总用电量文本")]
        public TextMeshProUGUI totalConsumptionText;
        
        [Tooltip("总当前功率文本")]
        public TextMeshProUGUI totalPowerText;

        [Header("设备列表")]
        [Tooltip("设备列表容器（ScrollView 的 Content）")]
        public RectTransform deviceListContainer;
        
        [Tooltip("设备项预制体（可选，如果为空则动态创建）")]
        public GameObject deviceItemPrefab;

        [Header("字体设置")]
        [Tooltip("设备名称字体大小")]
        public float deviceNameFontSize = 14f;
        
        [Tooltip("功率和消耗文本字体大小")]
        public float dataFontSize = 12f;

        [Header("布局设置")]
        [Tooltip("设备名称文本宽度")]
        public float deviceNameWidth = 200f;
        
        [Tooltip("功率文本宽度")]
        public float powerWidth = 80f;
        
        [Tooltip("消耗文本宽度")]
        public float consumptionWidth = 100f;
        
        [Tooltip("设备项高度")]
        public float deviceItemHeight = 50f;

        [Header("更新设置")]
        [Tooltip("更新间隔（秒）")]
        public float updateInterval = 1f;

        private float _timer;
        private readonly Dictionary<string, GameObject> _deviceItems = new Dictionary<string, GameObject>();

        private void Start()
        {
            if (deviceListContainer == null)
            {
                Debug.LogWarning("[EnergyConsumptionPanel] Device list container is not assigned!");
            }
            
            // 清除所有现有的设备项，强制重新创建
            ClearAllDeviceItems();
        }
        
        /// <summary>
        /// 清除所有设备项（用于重新创建）
        /// </summary>
        private void ClearAllDeviceItems()
        {
            foreach (var item in _deviceItems.Values)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            _deviceItems.Clear();
            
            // 也清除容器中的子对象（防止残留）
            if (deviceListContainer != null)
            {
                for (int i = deviceListContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(deviceListContainer.GetChild(i).gameObject);
                }
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;

            UpdateEnergyData();
        }

        private void UpdateEnergyData()
        {
            if (EnergyManager.Instance == null)
            {
                if (totalConsumptionText != null)
                {
                    totalConsumptionText.text = "Energy Manager not found";
                    totalConsumptionText.color = Color.white;
                }
                if (totalPowerText != null)
                {
                    totalPowerText.text = "--";
                    totalPowerText.color = Color.white;
                }
                return;
            }

            // 更新总体统计
            float totalConsumption = EnergyManager.Instance.GetTotalDailyConsumption();
            float totalPower = EnergyManager.Instance.GetTotalCurrentPower();

            if (totalConsumptionText != null)
            {
                totalConsumptionText.text = $"Total Consumption: <b>{totalConsumption:F3}</b> kWh";
                totalConsumptionText.color = Color.white;
            }

            if (totalPowerText != null)
            {
                totalPowerText.text = $"Current Power: <b>{totalPower:F1}</b> W";
                totalPowerText.color = Color.white;
            }

            // 更新设备列表
            UpdateDeviceList();
        }

        private void UpdateDeviceList()
        {
            if (deviceListContainer == null || DeviceManager.Instance == null || EnergyManager.Instance == null)
            {
                return;
            }

            var allDevices = DeviceManager.Instance.GetAllDevices();
            var energyData = EnergyManager.Instance.GetAllDevicesEnergyData();

            // 移除不存在的设备项
            var deviceIdsToRemove = _deviceItems.Keys.Where(id => !allDevices.ContainsKey(id)).ToList();
            foreach (var deviceId in deviceIdsToRemove)
            {
                if (_deviceItems.TryGetValue(deviceId, out var item))
                {
                    Destroy(item);
                    _deviceItems.Remove(deviceId);
                }
            }

            // 检查现有设备项是否有 LayoutElement，如果没有则重新创建（使用新布局）
            var itemsToRecreate = new List<string>();
            foreach (var kvp in _deviceItems)
            {
                var itemObj = kvp.Value;
                if (itemObj != null)
                {
                    // 检查设备名称是否有 LayoutElement
                    var nameObj = itemObj.transform.Find("DeviceName");
                    if (nameObj == null || nameObj.GetComponent<LayoutElement>() == null)
                    {
                        itemsToRecreate.Add(kvp.Key);
                    }
                }
            }

            // 重新创建需要更新的设备项
            foreach (var deviceId in itemsToRecreate)
            {
                if (_deviceItems.TryGetValue(deviceId, out var oldItem))
                {
                    Destroy(oldItem);
                    _deviceItems.Remove(deviceId);
                }
            }

            // 更新或创建设备项
            foreach (var device in allDevices.Values)
            {
                if (!_deviceItems.ContainsKey(device.deviceId))
                {
                    CreateDeviceItem(device.deviceId, device);
                }

                UpdateDeviceItem(device.deviceId, device, energyData);
            }
        }
        
        /// <summary>
        /// 强制重新创建所有设备项（公共方法，可在 Inspector 中调用）
        /// </summary>
        [ContextMenu("Force Recreate All Device Items")]
        public void ForceRecreateAllDeviceItems()
        {
            ClearAllDeviceItems();
            UpdateDeviceList();
        }

        private void CreateDeviceItem(string deviceId, DeviceDefinition device = null)
        {
            GameObject itemObj;
            
            if (deviceItemPrefab != null)
            {
                itemObj = Instantiate(deviceItemPrefab, deviceListContainer);
            }
            else
            {
                // 动态创建简单的设备项
                itemObj = new GameObject($"DeviceItem_{deviceId}");
                itemObj.transform.SetParent(deviceListContainer, false);
                
                RectTransform rectTransform = itemObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(0, deviceItemHeight);
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
                
                // 添加水平布局
                HorizontalLayoutGroup layoutGroup = itemObj.AddComponent<HorizontalLayoutGroup>();
                layoutGroup.spacing = 15f;  // 增加间距
                layoutGroup.padding = new RectOffset(10, 10, 5, 5);
                layoutGroup.childControlWidth = false;  // 改为 false，让子元素自己控制宽度
                layoutGroup.childControlHeight = false;  // 改为 false，让子元素自己控制高度
                layoutGroup.childForceExpandWidth = false;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childAlignment = TextAnchor.MiddleLeft;  // 垂直居中对齐

                // 设备名称文本
                GameObject nameObj = new GameObject("DeviceName");
                nameObj.transform.SetParent(itemObj.transform, false);
                RectTransform nameRect = nameObj.AddComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0f, 0.5f);
                nameRect.anchorMax = new Vector2(0f, 0.5f);
                nameRect.pivot = new Vector2(0f, 0.5f);
                nameRect.sizeDelta = new Vector2(deviceNameWidth, deviceItemHeight - 10f);  // 使用实际高度
                nameRect.anchoredPosition = Vector2.zero;
                
                // 添加 LayoutElement 确保宽度固定
                LayoutElement nameLayout = nameObj.AddComponent<LayoutElement>();
                nameLayout.preferredWidth = deviceNameWidth;
                nameLayout.flexibleWidth = 0f;
                
                TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
                // 直接使用 deviceId
                nameText.text = deviceId;
                nameText.fontSize = deviceNameFontSize;
                nameText.color = Color.white;
                nameText.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;
                nameText.enableWordWrapping = false;  // 禁用换行
                nameText.overflowMode = TextOverflowModes.Overflow;  // 改为 Overflow，允许文本超出边界显示
                nameText.raycastTarget = false;  // 禁用射线检测以提高性能
                nameText.autoSizeTextContainer = false;  // 禁用自动调整大小
                nameText.fontStyle = FontStyles.Normal;  // 确保使用正常字体样式
                
                // 调试日志
                Debug.Log($"[EnergyConsumptionPanel] Created device item: {deviceId}, Width: {deviceNameWidth}");

                // 功率文本
                GameObject powerObj = new GameObject("Power");
                powerObj.transform.SetParent(itemObj.transform, false);
                RectTransform powerRect = powerObj.AddComponent<RectTransform>();
                powerRect.anchorMin = new Vector2(0f, 0.5f);
                powerRect.anchorMax = new Vector2(0f, 0.5f);
                powerRect.pivot = new Vector2(1f, 0.5f);
                powerRect.sizeDelta = new Vector2(powerWidth, deviceItemHeight - 10f);  // 使用实际高度
                powerRect.anchoredPosition = Vector2.zero;
                
                // 添加 LayoutElement 确保宽度固定
                LayoutElement powerLayout = powerObj.AddComponent<LayoutElement>();
                powerLayout.preferredWidth = powerWidth;
                powerLayout.flexibleWidth = 0f;
                
                TextMeshProUGUI powerText = powerObj.AddComponent<TextMeshProUGUI>();
                powerText.text = "-- W";
                powerText.fontSize = dataFontSize;
                powerText.color = Color.white;
                powerText.alignment = TextAlignmentOptions.Right | TextAlignmentOptions.Midline;
                powerText.enableWordWrapping = false;
                powerText.raycastTarget = false;
                powerText.autoSizeTextContainer = false;
                powerText.fontStyle = FontStyles.Normal;

                // 消耗文本
                GameObject consumptionObj = new GameObject("Consumption");
                consumptionObj.transform.SetParent(itemObj.transform, false);
                RectTransform consumptionRect = consumptionObj.AddComponent<RectTransform>();
                consumptionRect.anchorMin = new Vector2(0f, 0.5f);
                consumptionRect.anchorMax = new Vector2(0f, 0.5f);
                consumptionRect.pivot = new Vector2(1f, 0.5f);
                consumptionRect.sizeDelta = new Vector2(consumptionWidth, deviceItemHeight - 10f);  // 使用实际高度
                consumptionRect.anchoredPosition = Vector2.zero;
                
                // 添加 LayoutElement 确保宽度固定
                LayoutElement consumptionLayout = consumptionObj.AddComponent<LayoutElement>();
                consumptionLayout.preferredWidth = consumptionWidth;
                consumptionLayout.flexibleWidth = 0f;
                
                TextMeshProUGUI consumptionText = consumptionObj.AddComponent<TextMeshProUGUI>();
                consumptionText.text = "-- kWh";
                consumptionText.fontSize = dataFontSize;
                consumptionText.color = Color.white;
                consumptionText.alignment = TextAlignmentOptions.Right | TextAlignmentOptions.Midline;
                consumptionText.enableWordWrapping = false;
                consumptionText.raycastTarget = false;
                consumptionText.autoSizeTextContainer = false;
                consumptionText.fontStyle = FontStyles.Normal;
            }

            _deviceItems[deviceId] = itemObj;
        }

        private void UpdateDeviceItem(string deviceId, DeviceDefinition device, IReadOnlyDictionary<string, EnergyData> energyData)
        {
            if (!_deviceItems.TryGetValue(deviceId, out var itemObj))
            {
                return;
            }

            var energy = energyData.TryGetValue(deviceId, out var data) ? data : null;
            float power = energy?.currentPower ?? 0f;
            float consumption = energy?.dailyConsumption ?? 0f;

            // 获取设备状态
            bool isOn = false;
            var controller = device.GetComponent<BaseDeviceController>();
            if (controller != null)
            {
                isOn = controller.IsOn;
            }

            // 更新文本（如果使用动态创建的项）
            if (deviceItemPrefab == null)
            {
                var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 3)
                {
                    // 设备名称
                    texts[0].text = GetDeviceDisplayName(device);
                    texts[0].color = Color.white;
                    
                    // 功率
                    texts[1].text = power > 0.1f ? $"{power:F1} W" : "0 W";
                    texts[1].color = Color.white;
                    
                    // 消耗
                    texts[2].text = consumption > 0.0001f ? $"{consumption:F4} kWh" : "0 kWh";
                    texts[2].color = Color.white;
                }
            }
            else
            {
                // 如果使用预制体，需要根据预制体的结构来更新
                // 这里假设预制体有特定的组件或命名约定
                var deviceItem = itemObj.GetComponent<EnergyDeviceItem>();
                if (deviceItem != null)
                {
                    deviceItem.UpdateData(device, power, consumption, isOn);
                }
            }
        }

        private string GetDeviceDisplayName(DeviceDefinition device)
        {
            // 直接返回 deviceId
            return device.deviceId;
        }
    }

    /// <summary>
    /// 能源设备项组件（用于预制体）
    /// </summary>
    public class EnergyDeviceItem : MonoBehaviour
    {
        public TextMeshProUGUI deviceNameText;
        public TextMeshProUGUI powerText;
        public TextMeshProUGUI consumptionText;
        public Image statusIndicator;

        public void UpdateData(DeviceDefinition device, float power, float consumption, bool isOn)
        {
            if (deviceNameText != null)
            {
                // 直接使用 deviceId
                deviceNameText.text = device.deviceId;
                deviceNameText.color = Color.white;
            }

            if (powerText != null)
            {
                powerText.text = power > 0.1f ? $"{power:F1} W" : "0 W";
                powerText.color = Color.white;
            }

            if (consumptionText != null)
            {
                consumptionText.text = consumption > 0.0001f ? $"{consumption:F4} kWh" : "0 kWh";
                consumptionText.color = Color.white;
            }

            if (statusIndicator != null)
            {
                statusIndicator.color = isOn ? new Color(0.2f, 1f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
            }
        }
    }
}

