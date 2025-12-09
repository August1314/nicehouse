using UnityEngine;

namespace NiceHouse.Data
{
    /// <summary>
    /// 数字人行为状态枚举。
    /// </summary>
    public enum PersonState
    {
        Idle,        // 空闲/站立
        Walking,     // 行走
        Sitting,     // 久坐
        Bathing,     // 久浴
        Sleeping,    // 睡觉
        Fallen,      // 跌倒
        OutOfBed     // 坠床
    }

    /// <summary>
    /// 数字人当前状态数据。
    /// </summary>
    [System.Serializable]
    public class PersonStatus
    {
        public PersonState state;
        public string currentRoomId;
        public string personId;
        public float stateDuration; // 当前状态持续时间（秒）
    }

    /// <summary>
    /// 数字人状态控制器，管理单个数字人的状态与所在房间。
    /// 兼容旧版单例（Instance 指向首个 Agent），但允许场景中存在多个实例，配合 PersonStateManager 使用。
    /// </summary>
    public class PersonStateController : MonoBehaviour
    {
        public static PersonStateController Instance { get; private set; }

        [Tooltip("数字人唯一标识（多数字人时需唯一）")]
        [SerializeField] private string personId = "PersonA";

        [Tooltip("默认房间（避免为空）")]
        [SerializeField] private string defaultRoomId = "LivingRoom01";

        [Tooltip("当前数字人状态")]
        public PersonStatus Status { get; private set; }

        [Tooltip("状态变化事件，参数：新状态、房间ID")]
        public System.Action<PersonState, string> OnStateChanged;

        /// <summary>
        /// 带实例引用的状态变化事件（多 Agent 监听用）。
        /// </summary>
        public System.Action<PersonStateController, PersonState, string> OnStateChangedWithAgent;

        public string PersonId => personId;

        private float _stateStartTime;

        private void Awake()
        {
            Initialize();
            TryRegisterToManager();
        }

        private void Start()
        {
            // 确保在 Start 时也初始化（防止 Awake 执行顺序问题）
            if (Instance == null)
            {
                Initialize();
            }
            TryRegisterToManager();
        }

        private void Initialize()
        {
            // 保持首个实例作为兼容单例
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Debug.LogWarning($"[PersonStateController] Multiple instances detected. Instance kept as {Instance.gameObject.name}, current: {gameObject.name}");
            }

            // 注意：DontDestroyOnLoad 只能用于根 GameObject，如果挂载在子对象上会失败

            // 初始化状态
            Status = new PersonStatus
            {
                state = PersonState.Idle,
                currentRoomId = string.IsNullOrEmpty(defaultRoomId) ? "LivingRoom01" : defaultRoomId,
                personId = personId,
                stateDuration = 0f
            };
            _stateStartTime = Time.time;

            Debug.Log("[PersonStateController] Initialized");
        }

        private void OnDestroy()
        {
            if (PersonStateManager.Instance != null)
            {
                PersonStateManager.Instance.UnregisterAgent(this);
            }
        }

        private void Update()
        {
            if (Status != null)
            {
                Status.stateDuration = Time.time - _stateStartTime;
            }
        }

        /// <summary>
        /// 改变数字人状态。
        /// </summary>
        /// <param name="newState">新状态</param>
        /// <param name="roomId">所在房间ID</param>
        public void ChangeState(PersonState newState, string roomId)
        {
            if (Status == null)
            {
                Status = new PersonStatus();
            }

            Status.state = newState;
            Status.currentRoomId = roomId;
            Status.personId = personId;
            _stateStartTime = Time.time;
            Status.stateDuration = 0f;

            OnStateChanged?.Invoke(newState, roomId);
            OnStateChangedWithAgent?.Invoke(this, newState, roomId);
        }

        /// <summary>
        /// 获取当前状态持续时间（秒）。
        /// </summary>
        public float GetStateDuration()
        {
            return Status != null ? Status.stateDuration : 0f;
        }

        private void TryRegisterToManager()
        {
            if (PersonStateManager.Instance != null)
            {
                PersonStateManager.Instance.RegisterAgent(this);
            }
        }
    }
}

