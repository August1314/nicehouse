using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.SmartMonitoring
{
    /// <summary>
    /// 数字人状态模拟器
    /// 用于演示和测试，可以通过代码或UI按钮切换数字人状态
    /// </summary>
    public class PersonStateSimulator : MonoBehaviour
    {
        [Header("当前设置")]
        [Tooltip("当前选择的房间ID")]
        public string currentRoomId = "LivingRoom01";

        [Header("自动切换设置（可选）")]
        [Tooltip("是否启用自动状态切换")]
        public bool enableAutoSwitch = false;

        [Tooltip("状态切换间隔（秒）")]
        public float switchInterval = 10f;

        [Tooltip("状态切换序列")]
        public PersonState[] stateSequence = new PersonState[]
        {
            PersonState.Idle,
            PersonState.Walking,
            PersonState.Sitting,
            PersonState.Sleeping
        };

        private float _timer;
        private int _currentStateIndex = 0;
        private string[] _roomIds = { "LivingRoom01", "BedRoom01", "Kitchen01", "BathRoom01" };
        private int _currentRoomIndex = 0;

        private void Update()
        {
            if (!enableAutoSwitch) return;

            _timer += Time.deltaTime;
            if (_timer >= switchInterval)
            {
                _timer = 0f;
                AutoSwitchState();
            }
        }

        /// <summary>
        /// 自动切换状态
        /// </summary>
        private void AutoSwitchState()
        {
            if (stateSequence.Length == 0) return;

            // 切换到下一个状态
            _currentStateIndex = (_currentStateIndex + 1) % stateSequence.Length;
            PersonState newState = stateSequence[_currentStateIndex];

            // 切换房间（可选）
            if (_roomIds.Length > 0)
            {
                _currentRoomIndex = (_currentRoomIndex + 1) % _roomIds.Length;
                currentRoomId = _roomIds[_currentRoomIndex];
            }

            ChangeState(newState, currentRoomId);
        }

        /// <summary>
        /// 改变数字人状态（公共方法，供UI按钮调用）
        /// </summary>
        public void ChangeState(PersonState newState, string roomId = null)
        {
            if (PersonStateController.Instance == null)
            {
                Debug.LogWarning("[PersonStateSimulator] PersonStateController.Instance is null!");
                return;
            }

            string targetRoomId = roomId ?? currentRoomId;
            PersonStateController.Instance.ChangeState(newState, targetRoomId);

            Debug.Log($"[PersonStateSimulator] Changed state to {newState} in {targetRoomId}");
        }

        /// <summary>
        /// 切换到空闲状态
        /// </summary>
        public void ChangeToIdle()
        {
            ChangeState(PersonState.Idle);
        }

        /// <summary>
        /// 切换到行走状态
        /// </summary>
        public void ChangeToWalking()
        {
            ChangeState(PersonState.Walking);
        }

        /// <summary>
        /// 切换到久坐状态
        /// </summary>
        public void ChangeToSitting()
        {
            ChangeState(PersonState.Sitting);
        }

        /// <summary>
        /// 切换到久浴状态
        /// </summary>
        public void ChangeToBathing()
        {
            ChangeState(PersonState.Bathing);
        }

        /// <summary>
        /// 切换到睡觉状态
        /// </summary>
        public void ChangeToSleeping()
        {
            ChangeState(PersonState.Sleeping);
        }

        /// <summary>
        /// 切换到跌倒状态（用于测试告警）
        /// </summary>
        public void ChangeToFallen()
        {
            ChangeState(PersonState.Fallen);
        }

        /// <summary>
        /// 切换到坠床状态（用于测试告警）
        /// </summary>
        public void ChangeToOutOfBed()
        {
            ChangeState(PersonState.OutOfBed);
        }

        /// <summary>
        /// 设置当前房间
        /// </summary>
        public void SetCurrentRoom(string roomId)
        {
            currentRoomId = roomId;
        }

        /// <summary>
        /// 重置状态（切换到空闲）
        /// </summary>
        public void ResetState()
        {
            ChangeState(PersonState.Idle, currentRoomId);
            _currentStateIndex = 0;
            _timer = 0f;
        }
    }
}

