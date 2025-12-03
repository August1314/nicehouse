using UnityEngine;
using UnityEditor;
using NiceHouse.Data;

namespace NiceHouse.Editor
{
    /// <summary>
    /// 创建或恢复 DataRoot 和所有 Manager
    /// </summary>
    public class CreateDataRoot
    {
        [MenuItem("NiceHouse/Setup/创建或恢复 DataRoot")]
        public static void CreateOrRestoreDataRoot()
        {
            // 查找或创建 DataRoot
            GameObject dataRoot = GameObject.Find("DataRoot");
            if (dataRoot == null)
            {
                dataRoot = new GameObject("DataRoot");
                Debug.Log("[CreateDataRoot] 创建了 DataRoot GameObject");
            }
            else
            {
                Debug.Log("[CreateDataRoot] DataRoot 已存在");
            }

            // 确保 DataRoot 是启用的
            if (!dataRoot.activeInHierarchy)
            {
                dataRoot.SetActive(true);
                Debug.Log("[CreateDataRoot] 启用了 DataRoot");
            }

            int createdCount = 0;

            // 创建或检查 RoomManager
            createdCount += CreateOrCheckManager<RoomManager>(dataRoot, "RoomManager");

            // 创建或检查 PersonStateController
            createdCount += CreateOrCheckManager<PersonStateController>(dataRoot, "PersonStateController");

            // 创建或检查 DeviceManager
            createdCount += CreateOrCheckManager<DeviceManager>(dataRoot, "DeviceManager");

            // 创建或检查 AlarmManager
            createdCount += CreateOrCheckManager<AlarmManager>(dataRoot, "AlarmManager");

            // 创建或检查 EnvironmentDataStore
            createdCount += CreateOrCheckManager<EnvironmentDataStore>(dataRoot, "EnvironmentDataStore");

            // 创建或检查 HealthDataStore
            createdCount += CreateOrCheckManager<HealthDataStore>(dataRoot, "HealthDataStore");

            // 创建或检查 EnergyManager
            createdCount += CreateOrCheckManager<EnergyManager>(dataRoot, "EnergyManager");

            // 创建或检查 SafetyDataStore
            createdCount += CreateOrCheckManager<SafetyDataStore>(dataRoot, "SafetyDataStore");

            // 创建或检查 ActivityTracker
            var activityTracker = dataRoot.GetComponent<ActivityTracker>();
            if (activityTracker == null)
            {
                GameObject trackerObj = new GameObject("ActivityTracker");
                trackerObj.transform.SetParent(dataRoot.transform);
                trackerObj.AddComponent<ActivityTracker>();
                createdCount++;
                Debug.Log("[CreateDataRoot] 创建了 ActivityTracker");
            }

            Debug.Log($"[CreateDataRoot] 完成！创建了 {createdCount} 个 Manager");
            EditorUtility.DisplayDialog("完成", $"DataRoot 和所有 Manager 已创建/恢复！\n\n创建了 {createdCount} 个 Manager GameObject。", "OK");
        }

        private static int CreateOrCheckManager<T>(GameObject parent, string name) where T : MonoBehaviour
        {
            // 先在整个场景中查找
            T existing = Object.FindObjectOfType<T>(true);
            
            if (existing != null)
            {
                // 如果找到，确保它在 DataRoot 下
                if (existing.transform.parent != parent.transform)
                {
                    existing.transform.SetParent(parent.transform);
                    Debug.Log($"[CreateDataRoot] 将 {name} 移动到 DataRoot 下");
                }
                
                // 确保启用
                if (!existing.gameObject.activeInHierarchy)
                {
                    existing.gameObject.SetActive(true);
                }
                if (!existing.enabled)
                {
                    existing.enabled = true;
                }
                
                return 0; // 已存在，不需要创建
            }

            // 如果不存在，创建新的
            GameObject managerObj = new GameObject(name);
            managerObj.transform.SetParent(parent.transform);
            managerObj.AddComponent<T>();
            Debug.Log($"[CreateDataRoot] 创建了 {name}");
            return 1;
        }
    }
}

