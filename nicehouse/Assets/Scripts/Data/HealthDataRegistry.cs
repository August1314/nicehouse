using System.Collections.Generic;
using UnityEngine;

namespace NiceHouse.Data
{
    /// <summary>
    /// 多数字人健康数据注册表，按 personId 存储生命体征数据。
    /// </summary>
    public class HealthDataRegistry : MonoBehaviour
    {
        public static HealthDataRegistry Instance { get; private set; }

        [Tooltip("场景切换时保持不销毁（建议放在根节点）")]
        public bool dontDestroyOnLoad = true;

        private readonly Dictionary<string, VitalSignsData> _dataByPerson = new Dictionary<string, VitalSignsData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (dontDestroyOnLoad && transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// 获取或创建指定人物的健康数据。
        /// </summary>
        public VitalSignsData GetOrCreate(string personId)
        {
            if (string.IsNullOrEmpty(personId)) personId = "Unknown";
            if (!_dataByPerson.TryGetValue(personId, out var data) || data == null)
            {
                data = new VitalSignsData
                {
                    heartRate = 72,
                    respirationRate = 16,
                    bodyMovement = 0.1f,
                    sleepStage = 0
                };
                _dataByPerson[personId] = data;
            }

            return data;
        }

        /// <summary>
        /// 设置指定人物的健康数据（引用存储）。
        /// </summary>
        public void Set(string personId, VitalSignsData data)
        {
            if (string.IsNullOrEmpty(personId) || data == null) return;
            _dataByPerson[personId] = data;
        }

        /// <summary>
        /// 获取只读映射。
        /// </summary>
        public IReadOnlyDictionary<string, VitalSignsData> GetAll() => _dataByPerson;
    }
}

