using UnityEngine;

/// <summary>
/// 最基础的第一人称控制器：WASD 移动、鼠标视角、重力与可选跳跃。
/// 使用旧 Input Manager（Horizontal/Vertical/Mouse X/Mouse Y/Jump）。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("移动参数")]
    [Tooltip("行走速度（m/s）")]
    public float moveSpeed = 3.5f;

    [Tooltip("按住左 Shift 时的奔跑速度（m/s）")]
    public float sprintSpeed = 5.5f;

    [Tooltip("跳跃高度（米）。设为 0 可禁用跳跃。")]
    public float jumpHeight = 1.2f;

    [Tooltip("重力加速度（负值）")]
    public float gravity = -9.81f;

    [Tooltip("输入死区阈值，小于此值的输入将被忽略（防止微小输入导致移动）")]
    public float inputDeadZone = 0.1f;

    [Header("鼠标视角")]
    public Transform cameraPivot;

    [Tooltip("鼠标灵敏度")]
    public float mouseSensitivity = 2f;

    [Tooltip("垂直视角限制（角度）")]
    public float pitchLimit = 80f;

    [Header("光标控制")]
    [Tooltip("进入游戏时是否自动锁定并隐藏鼠标（使用 UI 面板时建议关闭）")]
    public bool lockCursorOnStart = true;

    [Header("相机设置")]
    [Tooltip("相机近裁剪面距离，防止穿模（推荐 0.05）")]
    public float cameraNearClipPlane = 0.05f;

    [Tooltip("启用相机碰撞检测，防止相机进入模型内部")]
    public bool enableCameraCollision = true;

    [Tooltip("相机碰撞检测半径")]
    public float cameraCollisionRadius = 0.2f;

    [Tooltip("相机默认距离（相机相对于角色的默认 Z 位置）")]
    public float defaultCameraDistance = 0f;

    private CharacterController _controller;
    private Camera _camera;
    private float _verticalVelocity;
    private float _cameraPitch;
    private Vector3 _defaultCameraLocalPos;
    /// <summary>
    /// 用户通过 Esc 切换的“长期锁定”状态。
    /// </summary>
    private bool _userLockedCursor;

    /// <summary>
    /// 是否正在按住右键进行临时视角控制。
    /// </summary>
    private bool _isRightMouseLookActive;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        
        // 查找相机并设置近裁剪面，防止穿模
        if (cameraPivot == null)
        {
            _camera = GetComponentInChildren<Camera>();
            if (_camera != null)
            {
                cameraPivot = _camera.transform;
                // 设置较小的近裁剪面，防止相机进入模型内部
                _camera.nearClipPlane = cameraNearClipPlane;
            }
        }
        else
        {
            _camera = cameraPivot.GetComponent<Camera>();
            if (_camera != null)
            {
                _camera.nearClipPlane = cameraNearClipPlane;
            }
        }

        // 保存相机的默认本地位置
        if (cameraPivot != null)
        {
            _defaultCameraLocalPos = cameraPivot.localPosition;
            if (defaultCameraDistance == 0f)
            {
                defaultCameraDistance = _defaultCameraLocalPos.z;
            }
        }
    }

    private void OnEnable()
    {
        _userLockedCursor = lockCursorOnStart;
        _isRightMouseLookActive = false;
        ApplyCursorLockState();
    }

    private void OnDisable()
    {
        _userLockedCursor = false;
        _isRightMouseLookActive = false;
        ApplyCursorLockState();
    }

    private void Update()
    {
        HandleCursorToggle();

        // 只要当前实际是锁定状态，就允许视角旋转。
        if (IsCursorLocked())
        {
            HandleMouseLook();
        }

        HandleMovement();
    }

    private void LateUpdate()
    {
        // 在 LateUpdate 中处理相机碰撞，确保在移动和旋转之后执行
        if (enableCameraCollision && cameraPivot != null)
        {
            HandleCameraCollision();
        }
    }

    private void HandleMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        // 应用输入死区，防止微小输入导致移动
        if (Mathf.Abs(inputX) < inputDeadZone) inputX = 0f;
        if (Mathf.Abs(inputZ) < inputDeadZone) inputZ = 0f;

        // 只有在有明确输入时才计算移动方向
        Vector3 moveDir = Vector3.zero;
        if (Mathf.Abs(inputX) > 0f || Mathf.Abs(inputZ) > 0f)
        {
            moveDir = (transform.right * inputX + transform.forward * inputZ).normalized;
        }

        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        Vector3 horizontalVelocity = moveDir * targetSpeed;

        if (_controller.isGrounded)
        {
            if (_verticalVelocity < 0f)
            {
                _verticalVelocity = -2f; // 让角色紧贴地面
            }

            if (jumpHeight > 0f && Input.GetButtonDown("Jump"))
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        _verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = horizontalVelocity + Vector3.up * _verticalVelocity;
        _controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (cameraPivot == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 只有当鼠标真正移动时才旋转（避免微小抖动）
        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
        {
            // 旋转角色（水平视角）
            transform.Rotate(Vector3.up * mouseX, Space.World);

            // 旋转相机（垂直视角）
            _cameraPitch = Mathf.Clamp(_cameraPitch - mouseY, -pitchLimit, pitchLimit);
            cameraPivot.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
        }
    }

    private void HandleCursorToggle()
    {
        // Esc：切换“长期锁定”状态。适合纯 FPS 游玩模式。
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _userLockedCursor = !_userLockedCursor;
        }

        // 按住右键：临时锁定光标并启用视角旋转；松开右键恢复原状态。
        // 这样在 UI 模式下鼠标仍可用，只在你“按住右键拖拽”时临时进入观察模式。
        if (Input.GetMouseButtonDown(1))
        {
            _isRightMouseLookActive = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            _isRightMouseLookActive = false;
        }

        ApplyCursorLockState();
    }

    /// <summary>
    /// 计算综合的锁定状态（Esc 锁定 或 右键临时锁定），并应用到系统光标上。
    /// </summary>
    private void ApplyCursorLockState()
    {
        bool shouldLock = _userLockedCursor || _isRightMouseLookActive;
        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldLock;
    }

    private bool IsCursorLocked()
    {
        return Cursor.lockState == CursorLockMode.Locked;
    }

    /// <summary>
    /// 处理相机碰撞检测，防止相机进入模型内部
    /// </summary>
    private void HandleCameraCollision()
    {
        if (cameraPivot == null) return;

        // 计算相机的目标世界位置（默认位置）
        Vector3 targetLocalPos = _defaultCameraLocalPos;
        Vector3 targetWorldPos = transform.TransformPoint(targetLocalPos);

        // 从角色头部位置向相机目标位置发射射线
        Vector3 characterHeadPos = transform.position + Vector3.up * (_controller.height * 0.5f + _controller.center.y);
        Vector3 toCamera = targetWorldPos - characterHeadPos;
        float distance = toCamera.magnitude;
        
        if (distance < 0.01f)
        {
            // 相机就在头部位置，不需要检测
            return;
        }

        Vector3 direction = toCamera.normalized;

        // 使用 SphereCast 检测碰撞（考虑相机半径）
        RaycastHit hit;
        float maxDistance = distance + cameraCollisionRadius;
        
        if (Physics.SphereCast(
            characterHeadPos,
            cameraCollisionRadius,
            direction,
            out hit,
            maxDistance,
            ~0, // 所有层
            QueryTriggerInteraction.Ignore))
        {
            // 检测到碰撞，将相机拉回
            // hit.distance 是从起点到碰撞点的距离
            float safeDistance = hit.distance - cameraCollisionRadius - 0.05f; // 保持 5cm 安全距离
            safeDistance = Mathf.Max(0.1f, safeDistance); // 最小保持 10cm 距离

            // 计算新的相机世界位置
            Vector3 newWorldPos = characterHeadPos + direction * safeDistance;
            
            // 转换回本地坐标
            Vector3 newLocalPos = transform.InverseTransformPoint(newWorldPos);

            // 保持 X 和 Y 不变，只调整 Z（前后距离）
            // 如果相机在角色前方（Z > 0），则拉回；如果在后方（Z < 0），也保持相对位置
            cameraPivot.localPosition = new Vector3(
                targetLocalPos.x,
                targetLocalPos.y,
                newLocalPos.z
            );
        }
        else
        {
            // 没有碰撞，恢复默认位置
            cameraPivot.localPosition = targetLocalPos;
        }
    }
}


