using UnityEngine;

/// <summary>
/// 第三人称角色控制器
/// 功能：WASD 移动、角色朝向移动方向、动画参数控制
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 4.0f;
    public float runSpeed = 7.0f;
    public float rotateSpeed = 10.0f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.0f;

    [Header("引用")]
    [Tooltip("主相机，用于计算移动方向")]
    public Camera mainCamera;
    [Tooltip("动画控制器（可选）")]
    public Animator animator;

    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _isGrounded;

    // 动画参数 Hash，提高性能
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        AssignAnimationIDs();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
    }

    private void Update()
    {
        HandleGravity();
        HandleMovement();
    }

    private void HandleGravity()
    {
        _isGrounded = _controller.isGrounded;
        
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // 确保贴地
        }

        // 跳跃
        if (_isGrounded && Input.GetButtonDown("Jump"))
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator) animator.SetTrigger(_animIDJump);
        }

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
        
        if (animator) animator.SetBool(_animIDGrounded, _isGrounded);
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 计算移动方向（相对于相机）
        Vector3 direction = new Vector3(h, 0, v);
        float targetSpeed = 0;

        if (direction.magnitude >= 0.1f)
        {
            // 目标角度 = 相机Y轴角度 + 输入角度
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            
            // 平滑旋转
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotateSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // 移动方向
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
            // 跑步/走路
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;
            targetSpeed = speed;

            _controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        // 更新动画
        if (animator)
        {
            // 简单的 Speed 参数，实际项目中可能需要 Damp
            animator.SetFloat(_animIDSpeed, targetSpeed, 0.1f, Time.deltaTime);
        }
    }
}
