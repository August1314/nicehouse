using UnityEngine;

namespace NiceHouse.UI
{
    /// <summary>
    /// 能耗测试器 - 用于测试和验证能耗显示功能
    /// </summary>
    public class EnergyDisplayTester : MonoBehaviour
    {
        [Header("测试设置")]
        public bool enableTestMode = false;
        public float testConsumptionValue = 5.67f;

        [Header("状态显示")]
        [TextArea(5, 10)]
        public string energyStatus;

        private void Update()
        {
            if (!enableTestMode) return;

            UpdateEnergyStatus();
        }

        private void UpdateEnergyStatus()
        {
            System.Text.StringBuilder status = new System.Text.StringBuilder();
            status.AppendLine("=== 能耗显示状态检查 ===");

            // 检查EnergyManager
            if (NiceHouse.Data.EnergyManager.Instance != null)
            {
                status.AppendLine("✅ EnergyManager: 已找到");
                float realConsumption = NiceHouse.Data.EnergyManager.Instance.GetTotalDailyConsumption();
                status.AppendLine($"   实际能耗: {realConsumption:F2} kWh");

                if (enableTestMode)
                {
                    status.AppendLine($"   测试能耗: {testConsumptionValue:F2} kWh");
                }
            }
            else
            {
                status.AppendLine("❌ EnergyManager: 未找到实例");
            }

            // 检查EnvironmentDataStore
            if (NiceHouse.Data.EnvironmentDataStore.Instance != null)
            {
                status.AppendLine("✅ EnvironmentDataStore: 已找到");
            }
            else
            {
                status.AppendLine("❌ EnvironmentDataStore: 未找到实例");
            }

            // 检查设备数量
            var devices = FindObjectsOfType<NiceHouse.Data.DeviceDefinition>();
            status.AppendLine($"📊 场景中的设备数量: {devices.Length}");

            foreach (var device in devices)
            {
                if (device != null)
                {
                    status.AppendLine($"   - {device.deviceId}: {device.type}");
                }
            }

            status.AppendLine("\n=== 建议检查项 ===");
            status.AppendLine("1. 确保场景中有EnergyManager组件");
            status.AppendLine("2. 确保有设备在运行（空调、灯光等）");
            status.AppendLine("3. 检查EnvironmentValueSetter脚本中的energyInput引用");
            status.AppendLine("4. 确认EnergyInput字段已创建并设置为只读");

            energyStatus = status.ToString();
        }

        [ContextMenu("测试能耗显示")]
        public void TestEnergyDisplay()
        {
            enableTestMode = true;
            UpdateEnergyStatus();
            Debug.Log(energyStatus);
        }

        [ContextMenu("重置测试")]
        public void ResetTest()
        {
            enableTestMode = false;
            energyStatus = "测试已重置。点击'测试能耗显示'开始新测试。";
        }
    }
}