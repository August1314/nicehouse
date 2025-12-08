using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 周期性从各功能模块收集状态快照，并对外派发事件。
    /// </summary>
    public class ControlHubStatusAggregator : MonoBehaviour
    {
        [SerializeField] private List<MonoBehaviour> moduleProviders = new();
        [SerializeField] private float updateInterval = 1f;

        private readonly Dictionary<string, IControlPanelModule> _moduleLookup = new();
        private Coroutine _updateRoutine;

        public event Action<IControlPanelModule, ControlHubModuleSnapshot> SnapshotUpdated;

        public IReadOnlyDictionary<string, IControlPanelModule> ModuleLookup => _moduleLookup;

        private void Awake()
        {
            RebuildModuleLookup();
        }

        private void OnEnable()
        {
            StartLoop();
        }

        private void OnDisable()
        {
            if (_updateRoutine != null)
            {
                StopCoroutine(_updateRoutine);
                _updateRoutine = null;
            }
        }

        /// <summary>
        /// 重新扫描模块引用，供编辑器或动态添加模块时调用。
        /// </summary>
        public void RebuildModuleLookup()
        {
            _moduleLookup.Clear();

            foreach (var provider in moduleProviders)
            {
                if (provider == null) continue;

                if (provider is IControlPanelModule module)
                {
                    if (string.IsNullOrEmpty(module.ModuleId))
                    {
                        Debug.LogWarning($"[ControlHub] 模块 {provider.name} 的 ModuleId 为空，已忽略。", provider);
                        continue;
                    }

                    if (_moduleLookup.TryGetValue(module.ModuleId, out var existing))
                    {
                        Debug.LogWarning($"[ControlHub] 模块 ID 重复：{module.ModuleId}，保留第一个实例 {existing}.", provider);
                        continue;
                    }

                    _moduleLookup.Add(module.ModuleId, module);
                }
                else
                {
                    Debug.LogWarning($"[ControlHub] {provider.name} 未实现 IControlPanelModule。", provider);
                }
            }
        }

        private void StartLoop()
        {
            if (_updateRoutine != null)
            {
                StopCoroutine(_updateRoutine);
            }

            _updateRoutine = StartCoroutine(UpdateLoop());
        }

        private IEnumerator UpdateLoop()
        {
            var wait = new WaitForSeconds(Mathf.Max(0.1f, updateInterval));

            while (enabled)
            {
                DispatchSnapshots();
                yield return wait;
            }
        }

        private void DispatchSnapshots()
        {
            foreach (var module in _moduleLookup.Values)
            {
                try
                {
                    var snapshot = module.BuildSnapshot();
                    SnapshotUpdated?.Invoke(module, snapshot);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ControlHub] 获取模块 {module.ModuleId} 快照失败：{ex.Message}");
                }
            }
        }
    }
}


