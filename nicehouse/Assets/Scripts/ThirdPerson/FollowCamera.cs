using UnityEngine;

/// <summary>
/// 固定跟随相机 - 始终在角色右后方
/// 角色转身时相机自动跟随旋转
/// 带碰撞检测，遇到障碍物会自动拉近
/// </summary>
public class FollowCamera : MonoBehaviour
{
    [Header("跟随目标")]
    [Tooltip("要跟随的角色")]
    public Transform target;

    [Header("相机位置设置")]
    [Tooltip("相机在角色后方的距离")]
    public float distance = 4.0f;
    
    [Tooltip("相机高度（相对于角色）")]
    public float height = 2.0f;
    
    [Tooltip("相机水平偏移（正值=右边，负值=左边）")]
    public float horizontalOffset = 0.5f;
    
    [Tooltip("相机看向的高度偏移")]
    public float lookAtHeight = 1.2f;

    [Header("平滑设置")]
    [Tooltip("位置跟随平滑度（越小越平滑）")]
    public float positionSmoothTime = 0.1f;
    
    [Tooltip("旋转跟随平滑度（越小越平滑）")]
    public float rotationSmoothTime = 0.05f;

    [Header("滚轮缩放")]
    public float zoomSpeed = 2.0f;
    public float minDistance = 2.0f;
    public float maxDistance = 8.0f;

    [Header("鼠标旋转")]
    [Tooltip("是否允许鼠标控制相机绕目标旋转")]
    public bool enableMouseRotation = true;
    [Tooltip("是否需要按住右键才旋转")]
    public bool requireRightMouse = true;
    public float mouseSensitivity = 120f;
    [Tooltip("俯仰角限制（度）")]
    public float minPitch = -30f;
    public float maxPitch = 60f;

    [Header("碰撞检测")]
    [Tooltip("是否启用碰撞检测")]
    public bool enableCollision = true;
    
    [Tooltip("碰撞检测的层（默认检测所有层）")]
    public LayerMask collisionLayers = -1;
    
    [Tooltip("相机距离障碍物的最小距离")]
    public float collisionOffset = 0.3f;
    
    [Tooltip("最小相机距离（被障碍物推近时）")]
    public float minCollisionDistance = 1.0f;

    private Vector3 _velocity = Vector3.zero;
    private float _currentDistance;
    private float _yawOffset;
    private float _pitch = 10f;

    private void Start()
    {
        _currentDistance = distance;
        if (target != null)
        {
            _yawOffset = 0f;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleMouseInput();

        // 滚轮缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        float yaw = target.eulerAngles.y + _yawOffset;

        // 计算目标位置的起点（角色头部位置）
        Vector3 targetHead = target.position + Vector3.up * lookAtHeight;
        
        // 计算理想的相机位置（无遮挡情况下）
        Vector3 idealPosition = target.position
            + Quaternion.Euler(_pitch, yaw, 0f) * Vector3.back * distance
            + Quaternion.Euler(0f, yaw, 0f) * Vector3.right * horizontalOffset
            + Vector3.up * height;

        // 碰撞检测
        float desiredDistance = distance;
        
        if (enableCollision)
        {
            // 从角色头部向相机位置发射射线
            Vector3 directionToCamera = (idealPosition - targetHead).normalized;
            float rayDistance = Vector3.Distance(targetHead, idealPosition);
            
            RaycastHit hit;
            if (Physics.Raycast(targetHead, directionToCamera, out hit, rayDistance, collisionLayers))
            {
                // 有障碍物，计算新的距离
                float hitDistance = hit.distance - collisionOffset;
                desiredDistance = Mathf.Max(hitDistance, minCollisionDistance);
            }
        }

        // 平滑过渡到目标距离
        _currentDistance = Mathf.Lerp(_currentDistance, desiredDistance, Time.deltaTime * 10f);

        // 使用当前距离计算实际相机位置
        Vector3 targetPosition = target.position
            + Quaternion.Euler(_pitch, yaw, 0f) * Vector3.back * _currentDistance
            + Quaternion.Euler(0f, yaw, 0f) * Vector3.right * horizontalOffset
            + Vector3.up * height;

        // 平滑移动到目标位置
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref _velocity, 
            positionSmoothTime
        );

        // 计算看向的位置（角色中心偏上）
        Vector3 lookAtPosition = target.position + Vector3.up * lookAtHeight;

        // 平滑旋转朝向目标
        Quaternion targetRotation = Quaternion.LookRotation(lookAtPosition - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            1f / rotationSmoothTime * Time.deltaTime
        );
    }

    private void HandleMouseInput()
    {
        if (!enableMouseRotation) return;
        if (requireRightMouse && !Input.GetMouseButton(1)) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        _yawOffset += mouseX * mouseSensitivity * Time.deltaTime;
        _pitch -= mouseY * mouseSensitivity * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }

    // 在 Scene 视图中显示调试信息
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // 绘制理想相机位置
        Vector3 idealPos = target.position
            - target.forward * distance
            + target.right * horizontalOffset
            + Vector3.up * height;
            
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(idealPos, 0.3f);
        
        // 绘制射线检测路径
        Gizmos.color = Color.yellow;
        Vector3 targetHead = target.position + Vector3.up * lookAtHeight;
        Gizmos.DrawLine(targetHead, idealPos);
    }
}

