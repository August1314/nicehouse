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

        [Header("Layout Settings")]
        [Tooltip("Entry buttons 之间的纵向间距")]
        [SerializeField] private float entrySpacing = 10f;

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
            
            // 初始化时隐藏所有面板
            HideAllPanels();
            
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

            // 设置容器的布局间距
            var verticalLayoutGroup = entryContainer.GetComponent<VerticalLayoutGroup>();
            if (verticalLayoutGroup != null)
            {
                verticalLayoutGroup.spacing = entrySpacing;
            }
            else
            {
                var gridLayoutGroup = entryContainer.GetComponent<GridLayoutGroup>();
                if (gridLayoutGroup != null)
                {
                    gridLayoutGroup.spacing = new Vector2(gridLayoutGroup.spacing.x, entrySpacing);
                }
            }

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
            if (string.IsNullOrEmpty(moduleId))
            {
                Debug.LogWarning("[ControlHubPanelController] HandleModuleSelected called with null or empty moduleId!", this);
                return;
            }
            
            if (!statusAggregator.ModuleLookup.TryGetValue(moduleId, out var module))
            {
                Debug.LogWarning($"[ControlHubPanelController] Module '{moduleId}' not found in ModuleLookup. Available modules: {string.Join(", ", statusAggregator.ModuleLookup.Keys)}", this);
                return;
            }

            if (_activeModule == module)
            {
                CloseActiveModule();
                return;
            }

            // 先隐藏当前激活的面板
            _activeModule?.HidePanel();
            
            // 先激活 EmbeddedRoot，再显示面板，这样面板才能正确显示
            landingRoot?.SetActive(false);
            embeddedContentRoot?.SetActive(true);
            
            // 确保 BackButton 也是激活的
            if (backButton != null)
            {
                backButton.gameObject.SetActive(true);
            }
            
            // 隐藏所有面板（除了即将显示的这个），确保只有一个显示
            HideAllPanelsExcept(module);
            
            // 现在 EmbeddedRoot 已经激活，可以安全地显示面板了
            _activeModule = module;
            _activeModule.ShowPanel();
            
            // 强制刷新 Canvas，确保 UI 更新
            Canvas.ForceUpdateCanvases();

            if (activeModuleTitle != null)
            {
                activeModuleTitle.text = module.DisplayName;
            }
            
            panelAnimator?.SetActiveVisual(true);
            onModuleOpened?.Invoke(module.ModuleId);
        }

        public void CloseActiveModule()
        {
            // 先隐藏当前激活的模块
            if (_activeModule != null)
            {
                _activeModule.HidePanel();
            }
            
            _activeModule = null;
            if (activeModuleTitle != null)
            {
                activeModuleTitle.text = "Control Hub";
            }

            // 隐藏所有面板（确保没有残留）
            HideAllPanels();
            
            landingRoot?.SetActive(true);
            embeddedContentRoot?.SetActive(false);
            panelAnimator?.SetActiveVisual(false);
            onModuleClosed?.Invoke();
        }
        
        /// <summary>
        /// 隐藏所有注册的面板，确保切换时不会有残留显示。
        /// </summary>
        private void HideAllPanels()
        {
            if (statusAggregator == null) return;
            
            foreach (var module in statusAggregator.ModuleLookup.Values)
            {
                if (module != null)
                {
                    module.HidePanel();
                }
            }
        }
        
        /// <summary>
        /// 隐藏所有面板，除了指定的模块。
        /// </summary>
        private void HideAllPanelsExcept(IControlPanelModule exceptModule)
        {
            if (statusAggregator == null) return;
            
            foreach (var kvp in statusAggregator.ModuleLookup)
            {
                var module = kvp.Value;
                if (module != null && module != exceptModule)
                {
                    module.HidePanel();
                }
            }
        }
    }
}


