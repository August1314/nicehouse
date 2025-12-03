using UnityEngine;
using UnityEditor;
using NiceHouse.Data;

namespace NiceHouse.Editor
{
    /// <summary>
    /// 清理重复的 Manager GameObject
    /// </summary>
    public class CleanupDuplicateManagers
    {
        [MenuItem("NiceHouse/Diagnostics/清理重复的 Manager")]
        public static void CleanupDuplicates()
        {
            int cleanedCount = 0;

            // 清理 RoomManager
            cleanedCount += CleanupManager<RoomManager>("RoomManager");

            // 清理 PersonStateController
            cleanedCount += CleanupManager<PersonStateController>("PersonStateController");

            // 清理 DeviceManager
            cleanedCount += CleanupManager<DeviceManager>("DeviceManager");

            // 清理 AlarmManager
            cleanedCount += CleanupManager<AlarmManager>("AlarmManager");

            // 清理 EnvironmentDataStore
            cleanedCount += CleanupManager<EnvironmentDataStore>("EnvironmentDataStore");

            // 清理 HealthDataStore
            cleanedCount += CleanupManager<HealthDataStore>("HealthDataStore");

            // 清理 EnergyManager
            cleanedCount += CleanupManager<EnergyManager>("EnergyManager");

            Debug.Log($"[Cleanup] 清理了 {cleanedCount} 个重复的 Manager GameObject");
            EditorUtility.DisplayDialog("完成", $"清理了 {cleanedCount} 个重复的 Manager GameObject\n\n请重新运行游戏测试。", "OK");
        }

        private static int CleanupManager<T>(string name) where T : MonoBehaviour
        {
            T[] managers = Object.FindObjectsOfType<T>(true); // true = 包括禁用的对象

            if (managers.Length == 0)
            {
                Debug.LogWarning($"[Cleanup] {name}: 未找到任何实例！");
                return 0;
            }

            if (managers.Length == 1)
            {
                Debug.Log($"[Cleanup] {name}: 只有 1 个实例，正常。");
                return 0; // 没有重复
            }

            Debug.LogWarning($"[Cleanup] {name}: 找到 {managers.Length} 个实例，保留第一个（{managers[0].gameObject.name}），删除其他...");

            // 确保第一个实例是启用的
            if (!managers[0].gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[Cleanup] {name}: 第一个实例被禁用，启用它...");
                managers[0].gameObject.SetActive(true);
            }
            if (!managers[0].enabled)
            {
                Debug.LogWarning($"[Cleanup] {name}: 第一个实例的脚本组件被禁用，启用它...");
                managers[0].enabled = true;
            }

            int deletedCount = 0;
            for (int i = 1; i < managers.Length; i++)
            {
                Debug.Log($"[Cleanup] 删除重复的 {name}: {managers[i].gameObject.name}");
                Object.DestroyImmediate(managers[i].gameObject);
                deletedCount++;
            }

            return deletedCount;
        }
    }
}

