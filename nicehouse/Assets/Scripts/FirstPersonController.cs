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

    [Header("鼠标视角")]
    public Transform cameraPivot;

    [Tooltip("鼠标灵敏度")]
    public float mouseSensitivity = 2f;

    [Tooltip("垂直视角限制（角度）")]
    public float pitchLimit = 80f;

    [Header("光标控制")]
    [Tooltip("进入游戏时是否自动锁定并隐藏鼠标（使用 UI 面板时建议关闭）")]
    public bool lockCursorOnStart = true;

    private CharacterController _controller;
    private float _verticalVelocity;
    private float _cameraPitch;
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
        if (cameraPivot == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraPivot = cam.transform;
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

    private void HandleMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.right * inputX + transform.forward * inputZ);
        if (moveDir.sqrMagnitude > 1f)
        {
            moveDir.Normalize();
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

        transform.Rotate(Vector3.up * mouseX);

        _cameraPitch = Mathf.Clamp(_cameraPitch - mouseY, -pitchLimit, pitchLimit);
        cameraPivot.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
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
}


