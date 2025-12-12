using UnityEngine;
using NiceHouse.Data;

namespace NiceHouse.EnvironmentControl
{
    /// <summary>
    /// 窗帘开合控制：支持直线滑动或向轴卷起。
    /// 支持多片帘布（同向运动）。
    /// </summary>
    [AddComponentMenu("NiceHouse/Environment Control/Curtain Controller")]
    public class CurtainController : BaseDeviceController
    {
        public enum CurtainMode
        {
            Slide,  // 线性滑动
            Roll    // 围绕轴卷起（旋转）
        }

        [Header("帘布")]
        [Tooltip("需要开合的窗帘片 Transform（可多个）")]
        public Transform[] curtainPanels;

        [Header("模式")]
        [Tooltip("Slide: 平移；Roll: 围绕轴卷起")]
        public CurtainMode mode = CurtainMode.Slide;

        [Header("滑动参数（Slide）")]
        [Tooltip("完全打开时，沿本地方向平移的距离（米）")]
        public float openDistance = 1f;

        [Tooltip("开合速度（米/秒）；内部按比例换算为 0-1 的插值速度")]
        public float openSpeed = 1f;

        [Tooltip("本地移动方向，默认 X 正方向")]
        public Vector3 localSlideDirection = Vector3.right;

        [Header("卷起参数（Roll）")]
        [Tooltip("完全打开时，相对初始姿态的旋转角度（度）")]
        public float rollAngle = 100f;

        [Tooltip("卷轴本地方向（旋转轴），默认 Y")]
        public Vector3 rollAxis = Vector3.up;

        private Vector3[] _closedPositions;
        private Quaternion[] _closedRotations;
        private float _targetOpen01 = 0f;
        private float _currentOpen01 = 0f;

        protected override void Awake()
        {
            base.Awake();
            CacheClosedPositions();
            CacheClosedRotations();
        }

        private void CacheClosedPositions()
        {
            if (curtainPanels == null) return;

            _closedPositions = new Vector3[curtainPanels.Length];
            for (int i = 0; i < curtainPanels.Length; i++)
            {
                _closedPositions[i] = curtainPanels[i] != null
                    ? curtainPanels[i].localPosition
                    : Vector3.zero;
            }
        }

        private void CacheClosedRotations()
        {
            if (curtainPanels == null) return;

            _closedRotations = new Quaternion[curtainPanels.Length];
            for (int i = 0; i < curtainPanels.Length; i++)
            {
                _closedRotations[i] = curtainPanels[i] != null
                    ? curtainPanels[i].localRotation
                    : Quaternion.identity;
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
            if (curtainPanels == null || curtainPanels.Length == 0)
            {
                return;
            }

            switch (mode)
            {
                case CurtainMode.Slide:
                    ApplySlide();
                    break;
                case CurtainMode.Roll:
                    ApplyRoll();
                    break;
            }
        }

        private void OnValidate()
        {
            if (localSlideDirection.sqrMagnitude < 1e-4f)
            {
                localSlideDirection = Vector3.right;
            }

            if (rollAxis.sqrMagnitude < 1e-4f)
            {
                rollAxis = Vector3.up;
            }
        }

        private void ApplySlide()
        {
            float distance = Mathf.Max(1e-4f, Mathf.Abs(openDistance));
            float speed01 = Mathf.Max(0f, openSpeed) / distance;
            _currentOpen01 = Mathf.MoveTowards(_currentOpen01, _targetOpen01, speed01 * Time.deltaTime);

            Vector3 offset = localSlideDirection.normalized * openDistance * _currentOpen01;

            for (int i = 0; i < curtainPanels.Length; i++)
            {
                var panel = curtainPanels[i];
                if (panel == null) continue;

                Vector3 basePos = (_closedPositions != null && i < _closedPositions.Length)
                    ? _closedPositions[i]
                    : panel.localPosition;

                panel.localPosition = basePos + offset;
            }
        }

        private void ApplyRoll()
        {
            float angle = Mathf.Max(1e-3f, Mathf.Abs(rollAngle));
            float speed01 = Mathf.Max(0f, openSpeed) / angle; // 使用 openSpeed 作为角速度
            _currentOpen01 = Mathf.MoveTowards(_currentOpen01, _targetOpen01, speed01 * Time.deltaTime);

            float signedAngle = rollAngle * _currentOpen01;
            Quaternion delta = Quaternion.AngleAxis(signedAngle, rollAxis.normalized);

            for (int i = 0; i < curtainPanels.Length; i++)
            {
                var panel = curtainPanels[i];
                if (panel == null) continue;

                Quaternion baseRot = (_closedRotations != null && i < _closedRotations.Length)
                    ? _closedRotations[i]
                    : panel.localRotation;

                panel.localRotation = baseRot * delta;
            }
        }
    }
}

