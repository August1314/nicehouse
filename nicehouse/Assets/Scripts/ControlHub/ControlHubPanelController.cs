using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 负责构建控制中枢入口列表，并在用户选择模块时切换到对应 UI。
    /// </summary>
    public class ControlHubPanelController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ControlHubStatusAggregator statusAggregator;
        [SerializeField] private ControlHubEntryWidget entryPrefab;
        [SerializeField] private RectTransform entryContainer;
        [SerializeField] private GameObject landingRoot;
        [SerializeField] private GameObject embeddedContentRoot;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text activeModuleTitle;
        [SerializeField] private ControlHubAnimator panelAnimator;

        [Header("Events")]
        public UnityEvent<string> onModuleOpened;
        public UnityEvent onModuleClosed;

        private readonly Dictionary<string, ControlHubEntryWidget> _entryLookup = new();
        private readonly List<ControlHubEntryWidget> _spawnedEntries = new();
        private IControlPanelModule _activeModule;

        private void Awake()
        {
            if (statusAggregator == null)
            {
                statusAggregator = GetComponent<ControlHubStatusAggregator>();
            }

            statusAggregator?.RebuildModuleLookup();
            BuildEntryGrid();
            CloseActiveModule();
        }

        private void OnEnable()
        {
            if (statusAggregator != null)
            {
                statusAggregator.SnapshotUpdated += HandleSnapshotUpdated;
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(CloseActiveModule);
            }
        }

        private void OnDisable()
        {
            if (statusAggregator != null)
            {
                statusAggregator.SnapshotUpdated -= HandleSnapshotUpdated;
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(CloseActiveModule);
            }
        }

        private void HandleSnapshotUpdated(IControlPanelModule module, ControlHubModuleSnapshot snapshot)
        {
            if (module == null || string.IsNullOrEmpty(module.ModuleId)) return;
            if (_entryLookup.TryGetValue(module.ModuleId, out var widget) && widget != null)
            {
                widget.UpdateSnapshot(snapshot);
            }
        }

        private void BuildEntryGrid()
        {
            if (entryPrefab == null || entryContainer == null || statusAggregator == null) return;

            foreach (var entry in _spawnedEntries)
            {
                if (entry != null)
                {
                    Destroy(entry.gameObject);
                }
            }
            _spawnedEntries.Clear();
            _entryLookup.Clear();

            foreach (var kvp in statusAggregator.ModuleLookup)
            {
                var widget = Instantiate(entryPrefab, entryContainer);
                widget.name = $"HubEntry_{kvp.Key}";
                widget.Bind(kvp.Value, HandleModuleSelected);
                _spawnedEntries.Add(widget);
                _entryLookup[kvp.Key] = widget;
            }
        }

        private void HandleModuleSelected(string moduleId)
        {
            if (string.IsNullOrEmpty(moduleId)) return;
            if (!statusAggregator.ModuleLookup.TryGetValue(moduleId, out var module)) return;

            Debug.Log($"[ControlHubPanelController] HandleModuleSelected: {moduleId}", this);

            if (_activeModule == module)
            {
                CloseActiveModule();
                return;
            }

            _activeModule?.HidePanel();
            _activeModule = module;
            _activeModule.ShowPanel();

            if (activeModuleTitle != null)
            {
                activeModuleTitle.text = module.DisplayName;
            }

            landingRoot?.SetActive(false);
            embeddedContentRoot?.SetActive(true);
            panelAnimator?.SetActiveVisual(true);
            onModuleOpened?.Invoke(module.ModuleId);
        }

        public void CloseActiveModule()
        {
            _activeModule?.HidePanel();
            _activeModule = null;
            if (activeModuleTitle != null)
            {
                activeModuleTitle.text = "控制中枢";
            }

            landingRoot?.SetActive(true);
            embeddedContentRoot?.SetActive(false);
            panelAnimator?.SetActiveVisual(false);
            onModuleClosed?.Invoke();
        }
    }
}


