using UnityEngine;
using System.Collections;

namespace NiceHouse.Interaction
{
    public class DoorController : MonoBehaviour
    {
        [Header("门状态设置")]
        [Tooltip("门打开时的目标角度")]
        public float openAngle = 90f;

        [Tooltip("门关闭时的目标角度")]
        public float closeAngle = 0f;

        [Tooltip("门旋转动画的速度")]
        public float rotationSpeed = 2f;

        public bool IsDoorOpen { get; private set; } = false;

        private Coroutine _rotationCoroutine;

        private void Start()
        {
            // 根据门的初始角度初始化状态
            InitializeDoorState();
        }

        private void InitializeDoorState()
        {
            float currentAngle = transform.rotation.eulerAngles.y;
            
            // 计算当前角度与打开角度、关闭角度的距离
            float distToOpen = Mathf.Abs(Mathf.DeltaAngle(currentAngle, openAngle));
            float distToClose = Mathf.Abs(Mathf.DeltaAngle(currentAngle, closeAngle));
            
            // 根据哪个更近来判断初始状态
            IsDoorOpen = distToOpen < distToClose;
        }

        public void Toggle()
        {
            IsDoorOpen = !IsDoorOpen;
            var targetAngle = IsDoorOpen ? openAngle : closeAngle;

            if (_rotationCoroutine != null)
            {
                StopCoroutine(_rotationCoroutine);
            }
            _rotationCoroutine = StartCoroutine(RotateDoor(targetAngle));
        }

        private IEnumerator RotateDoor(float targetAngle)
        {
            Quaternion startRotation = transform.rotation;
            float startAngle = startRotation.eulerAngles.y;
            
            // 处理角度跨越 0/360 度边界的问题，选择最短路径
            float angleDiff = Mathf.DeltaAngle(startAngle, targetAngle);
            float actualTargetAngle = startAngle + angleDiff;
            
            Quaternion endRotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x, 
                actualTargetAngle, 
                transform.rotation.eulerAngles.z
            );

            float elapsedTime = 0f;
            // 防止 rotationSpeed 为 0 或负数导致问题
            float safeSpeed = Mathf.Max(rotationSpeed, 0.0001f);
            float duration = 1f / safeSpeed; // 旋转时间由 rotationSpeed 控制

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.rotation = endRotation; // 确保最终角度精确
            _rotationCoroutine = null;
        }
    }
}
