using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 窗户开合控制：支持旋转式或滑动式窗扇。
    /// </summary>
    [AddComponentMenu("NiceHouse/Environment Control/Window Controller")]
    public class WindowController : BaseDeviceController
    {
        public enum WindowOpenMode
        {
            Rotate, // 旋转开合（合页式）
            Slide   // 平移开合（推拉式）
        }

        [Header("窗扇")]
        [Tooltip("需要开合的窗扇 Transform（可多个）")]
        public Transform[] windowSashes;

        [Header("模式")]
        [Tooltip("Rotate: 绕轴开合；Slide: 平移开合")]
        public WindowOpenMode openMode = WindowOpenMode.Rotate;

        [Header("旋转参数（Rotate）")]
        [Tooltip("完全打开时相对初始姿态的旋转角度（度）")]
        public float openAngle = 80f;

        [Tooltip("开合速度（度/秒）；内部按比例换算为 0-1 的插值速度")]
        public float openSpeed = 120f;

        [Tooltip("绕本地哪一轴旋转开合")]
        public Vector3 localAxis = Vector3.up;

        [Header("滑动参数（Slide）")]
        [Tooltip("完全打开时的平移距离（米）")]
        public float slideDistance = 1.0f;

        [Tooltip("平移方向（本地坐标）")]
        public Vector3 slideDirection = Vector3.right;

        private Quaternion[] _closedRotations;
        private Vector3[] _closedPositions;
        private float _targetOpen01 = 0f;
        private float _currentOpen01 = 0f;

        protected override void Awake()
        {
            base.Awake();
            CacheClosedRotations();
            CacheClosedPositions();
        }

        private void CacheClosedRotations()
        {
            if (windowSashes == null) return;

            _closedRotations = new Quaternion[windowSashes.Length];
            for (int i = 0; i < windowSashes.Length; i++)
            {
                _closedRotations[i] = windowSashes[i] != null
                    ? windowSashes[i].localRotation
                    : Quaternion.identity;
            }
        }

        private void CacheClosedPositions()
        {
            if (windowSashes == null) return;

            _closedPositions = new Vector3[windowSashes.Length];
            for (int i = 0; i < windowSashes.Length; i++)
            {
                _closedPositions[i] = windowSashes[i] != null
                    ? windowSashes[i].localPosition
                    : Vector3.zero;
            }
        }

        public override void TurnOn()
        {
            base.TurnOn();
            currentState = DeviceState.Running;
            _targetOpen01 = 1f;
        }

        public override void TurnOff()
        {
            base.TurnOff();
            _targetOpen01 = 0f;
        }

        private void Update()
        {
            if (windowSashes == null || windowSashes.Length == 0)
            {
                return;
            }

            switch (openMode)
            {
                case WindowOpenMode.Rotate:
                    ApplyRotate();
                    break;
                case WindowOpenMode.Slide:
                    ApplySlide();
                    break;
            }
        }

        private void OnValidate()
        {
            if (localAxis.sqrMagnitude < 1e-4f)
            {
                localAxis = Vector3.up;
            }

            if (slideDirection.sqrMagnitude < 1e-4f)
            {
                slideDirection = Vector3.right;
            }
        }

        private void ApplyRotate()
        {
            float angle = Mathf.Max(1e-3f, Mathf.Abs(openAngle));
            float speed01 = Mathf.Max(0f, openSpeed) / angle;
            _currentOpen01 = Mathf.MoveTowards(_currentOpen01, _targetOpen01, speed01 * Time.deltaTime);

            float signedAngle = openAngle * _currentOpen01;
            Quaternion delta = Quaternion.AngleAxis(signedAngle, localAxis.normalized);

            for (int i = 0; i < windowSashes.Length; i++)
            {
                var sash = windowSashes[i];
                if (sash == null) continue;

                Quaternion closed = (_closedRotations != null && i < _closedRotations.Length)
                    ? _closedRotations[i]
                    : sash.localRotation;

                sash.localRotation = closed * delta;
            }
        }

        private void ApplySlide()
        {
            float distance = Mathf.Max(1e-4f, Mathf.Abs(slideDistance));
            float speed01 = Mathf.Max(0f, openSpeed) / distance;
            _currentOpen01 = Mathf.MoveTowards(_currentOpen01, _targetOpen01, speed01 * Time.deltaTime);

            Vector3 offset = slideDirection.normalized * slideDistance * _currentOpen01;

            for (int i = 0; i < windowSashes.Length; i++)
            {
                var sash = windowSashes[i];
                if (sash == null) continue;

                Vector3 closed = (_closedPositions != null && i < _closedPositions.Length)
                    ? _closedPositions[i]
                    : sash.localPosition;

                sash.localPosition = closed + offset;
            }
        }
    }
}

