using System.Collections.Generic;
using UnityEngine;

namespace NiceHouse.Data
{
    /// <summary>
    /// 管理场景中的所有数字人状态 Agent，提供统一查询与事件分发。
    /// </summary>
    public class PersonStateManager : MonoBehaviour
    {
        public static PersonStateManager Instance { get; private set; }

        [Tooltip("注册时自动检查 personId 唯一性")]
        public bool enforceUniquePersonId = true;

        private readonly List<PersonStateController> _agents = new List<PersonStateController>();
        private readonly Dictionary<string, PersonStateController> _agentsById = new Dictionary<string, PersonStateController>();

        /// <summary>
        /// 任意 Agent 的状态变化事件。
        /// 参数：Agent、新状态、房间ID
        /// </summary>
        public System.Action<PersonStateController, PersonState, string> OnAnyStateChanged;

        /// <summary>
        /// 默认 Agent（首个注册的 Agent 或兼容单例）。
        /// </summary>
        public PersonStateController DefaultAgent
        {
            get
            {
                if (_agents.Count > 0) return _agents[0];
                return PersonStateController.Instance;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void RegisterAgent(PersonStateController agent)
        {
            if (agent == null) return;
            if (_agents.Contains(agent)) return;

            _agents.Add(agent);

            // 唯一性检查
            var id = string.IsNullOrEmpty(agent.PersonId) ? null : agent.PersonId;
            if (!string.IsNullOrEmpty(id))
            {
                if (_agentsById.ContainsKey(id))
                {
                    if (enforceUniquePersonId)
                    {
                        Debug.LogWarning($"[PersonStateManager] Duplicate personId '{id}' detected. Later agent will be kept in list but not mapped by id.");
                    }
                }
                else
                {
                    _agentsById[id] = agent;
                }
            }

            agent.OnStateChangedWithAgent += HandleAgentStateChanged;
        }

        public void UnregisterAgent(PersonStateController agent)
        {
            if (agent == null) return;
            if (_agents.Remove(agent))
            {
                if (!string.IsNullOrEmpty(agent.PersonId) && _agentsById.TryGetValue(agent.PersonId, out var existing) && existing == agent)
                {
                    _agentsById.Remove(agent.PersonId);
                }
            }

            agent.OnStateChangedWithAgent -= HandleAgentStateChanged;
        }

        public IReadOnlyList<PersonStateController> GetAllAgents() => _agents;

        public bool TryGetAgent(string personId, out PersonStateController agent)
        {
            if (string.IsNullOrEmpty(personId))
            {
                agent = null;
                return false;
            }

            return _agentsById.TryGetValue(personId, out agent);
        }

        private void HandleAgentStateChanged(PersonStateController agent, PersonState newState, string roomId)
        {
            OnAnyStateChanged?.Invoke(agent, newState, roomId);
        }
    }
}

