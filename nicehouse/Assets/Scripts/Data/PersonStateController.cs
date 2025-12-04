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
        public float stateDuration; // 当前状态持续时间（秒）
    }

    /// <summary>
    /// 数字人状态控制器，管理数字人的行为状态和所在房间。
    /// 挂在代表数字人的 GameObject 上。
    /// </summary>
    public class PersonStateController : MonoBehaviour
    {
        public static PersonStateController Instance { get; private set; }

        [Tooltip("当前数字人状态")]
        public PersonStatus Status { get; private set; }

        [Tooltip("状态变化事件，参数：新状态、房间ID")]
        public System.Action<PersonState, string> OnStateChanged;

        private float _stateStartTime;

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            // 确保在 Start 时也初始化（防止 Awake 执行顺序问题）
            if (Instance == null)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            // 如果 Instance 已存在且不是当前对象，销毁当前对象
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[PersonStateController] Duplicate instance detected on {gameObject.name}, destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            // 如果 Instance 为 null，设置为当前对象
            if (Instance == null)
            {
                Instance = this;
            }

            // 注意：DontDestroyOnLoad 只能用于根 GameObject，如果挂载在子对象上会失败

            // 初始化状态
            Status = new PersonStatus
            {
                state = PersonState.Idle,
                currentRoomId = "LivingRoom01", // 默认房间，避免为空
                stateDuration = 0f
            };
            _stateStartTime = Time.time;

            Debug.Log("[PersonStateController] Initialized");
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
            _stateStartTime = Time.time;
            Status.stateDuration = 0f;

            OnStateChanged?.Invoke(newState, roomId);
        }

        /// <summary>
        /// 获取当前状态持续时间（秒）。
        /// </summary>
        public float GetStateDuration()
        {
            return Status != null ? Status.stateDuration : 0f;
        }
    }
}

