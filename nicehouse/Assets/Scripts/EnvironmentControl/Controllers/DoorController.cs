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
            Quaternion endRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, targetAngle, transform.rotation.eulerAngles.z);

            float elapsedTime = 0f;
            float duration = 1f / rotationSpeed; // 旋转时间由 rotationSpeed 控制

            while (elapsedTime < duration)
            {
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.rotation = endRotation; // 确保最终角度精确
            _rotationCoroutine = null;
        }
    }
}
