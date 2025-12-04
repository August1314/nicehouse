using UnityEngine;
using UnityEditor;
using NiceHouse.Data;

namespace NiceHouse.Editor
{
    /// <summary>
    /// Manager 诊断工具
    /// 检查所有 Manager 是否正确初始化
    /// </summary>
    public class ManagerDiagnostics : EditorWindow
    {
        [MenuItem("NiceHouse/Diagnostics/检查 Manager 状态")]
        public static void CheckManagers()
        {
            Debug.Log("=== Manager 状态检查 ===");

            // 检查 RoomManager
            CheckManager<RoomManager>("RoomManager");

            // 检查 PersonStateController
            CheckManager<PersonStateController>("PersonStateController");

            // 检查 DeviceManager
            CheckManager<DeviceManager>("DeviceManager");

            // 检查 AlarmManager
            CheckManager<AlarmManager>("AlarmManager");

            // 检查 EnvironmentDataStore
            CheckManager<EnvironmentDataStore>("EnvironmentDataStore");

            // 检查 HealthDataStore
            CheckManager<HealthDataStore>("HealthDataStore");

            // 检查 EnergyManager
            CheckManager<EnergyManager>("EnergyManager");

            Debug.Log("=== 检查完成 ===");
        }

        private static void CheckManager<T>(string name) where T : MonoBehaviour
        {
            // 查找 GameObject
            T[] managers = FindObjectsOfType<T>(true); // true = 包括禁用的对象

            if (managers.Length == 0)
            {
                Debug.LogError($"[{name}] ❌ 未找到 GameObject！请在 Hierarchy 中创建 {name} GameObject 并挂载脚本。");
                return;
            }

            if (managers.Length > 1)
            {
                Debug.LogWarning($"[{name}] ⚠️ 找到 {managers.Length} 个实例，应该有且仅有一个！");
            }

            T manager = managers[0];

            // 检查 GameObject 是否启用
            if (!manager.gameObject.activeInHierarchy)
            {
                Debug.LogError($"[{name}] ❌ GameObject 被禁用！请启用 {manager.gameObject.name} GameObject。");
            }

            // 检查脚本组件是否启用
            if (!manager.enabled)
            {
                Debug.LogError($"[{name}] ❌ 脚本组件被禁用！请在 Inspector 中启用 {name} 脚本组件。");
            }

            // 检查 Instance
            if (manager is RoomManager)
            {
                if (RoomManager.Instance == null)
                    Debug.LogError($"[{name}] ❌ Instance 为 null！");
                else
                    Debug.Log($"[{name}] ✅ Instance 正常");
            }
            else if (manager is PersonStateController)
            {
                if (PersonStateController.Instance == null)
                    Debug.LogError($"[{name}] ❌ Instance 为 null！");
                else
                    Debug.Log($"[{name}] ✅ Instance 正常");
            }
            else if (manager is DeviceManager)
            {
                if (DeviceManager.Instance == null)
                    Debug.LogError($"[{name}] ❌ Instance 为 null！");
                else
                    Debug.Log($"[{name}] ✅ Instance 正常");
            }
            else if (manager is AlarmManager)
            {
                if (AlarmManager.Instance == null)
                    Debug.LogError($"[{name}] ❌ Instance 为 null！");
                else
                    Debug.Log($"[{name}] ✅ Instance 正常");
            }
            else if (manager is EnvironmentDataStore)
            {
                if (EnvironmentDataStore.Instance == null)
                    Debug.LogError($"[{name}] ❌ Instance 为 null！");
                else
                    Debug.Log($"[{name}] ✅ Instance 正常");
            }
            else if (manager is HealthDataStore)
            {
                if (HealthDataStore.Instance == null)
                    Debug.LogError($"[{name}] ❌ Instance 为 null！");
                else
                    Debug.Log($"[{name}] ✅ Instance 正常");
            }
            else if (manager is EnergyManager)
            {
                if (EnergyManager.Instance == null)
                    Debug.LogError($"[{name}] ❌ Instance 为 null！");
                else
                    Debug.Log($"[{name}] ✅ Instance 正常");
            }
        }
    }
}

