using UnityEngine;

/// <summary>
/// 第三人称相机控制器
/// 功能：跟随目标、鼠标右键旋转视角、滚轮缩放
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("目标设置")]
    [Tooltip("相机跟随的目标（通常是玩家角色的头部或中心点）")]
    public Transform target;

    [Header("距离与高度")]
    public float distance = 5.0f;
    public float height = 2.0f;
    
    [Header("控制灵敏度")]
    public float rotationSpeed = 3.0f;
    public float zoomSpeed = 2.0f;

    [Header("限制")]
    public float minDistance = 2.0f;
    public float maxDistance = 10.0f;
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 60f;

    [Header("防穿墙（阻挡处理）")]
    [Tooltip("启用相机碰撞检测，防止镜头穿墙或被遮挡")]
    public bool enableCollision = true;
    [Tooltip("用于碰撞检测的球体半径")]
    public float collisionRadius = 0.2f;
    [Tooltip("碰撞时与命中点保持的偏移距离")]
    public float collisionOffset = 0.2f;
    [Tooltip("检测层，默认检测全部")]
    public LayerMask collisionLayers = ~0;

    private float _currentX = 0.0f;
    private float _currentY = 0.0f;

    private void Start()
    {
        // 初始化角度
        Vector3 angles = transform.eulerAngles;
        _currentX = angles.y;
        _currentY = angles.x;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 鼠标右键按下时允许旋转视角，或者直接允许旋转（取决于需求，这里默认一直跟随，右键调整角度）
        // 如果希望像MMORPG那样按住右键才转，可以加上 Input.GetMouseButton(1)
        // 这里为了方便，默认鼠标移动直接控制视角，类似于 FPS 但带距离
        // 或者：标准 TPS 也是鼠标直接动。
        
        // 为了不与 UI 交互冲突，通常锁定光标
        // float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        // float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // _currentX += mouseX;
        // _currentY -= mouseY;
        // _currentY = Mathf.Clamp(_currentY, minVerticalAngle, maxVerticalAngle);

        // 滚轮缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // 计算位置
        // 如果要实现类似“按住右键旋转”，这里需要逻辑判断。
        // 既然是“数字人”，我们采用最通用的：鼠标移动控制视角旋转
        
        HandleCameraRotation();

        Quaternion rotation = Quaternion.Euler(_currentY, _currentX, 0);
        Vector3 direction = new Vector3(0, 0, -distance);
        Vector3 pivot = target.position + new Vector3(0, height, 0);
        Vector3 desiredPosition = pivot + rotation * direction;

        float adjustedDistance = ResolveCollision(pivot, desiredPosition);
        Vector3 position = pivot + rotation * new Vector3(0, 0, -adjustedDistance);

        transform.rotation = rotation;
        transform.position = position;
    }

    private void HandleCameraRotation()
    {
        // 如果鼠标被锁定，或者按下了右键
        if (Cursor.lockState == CursorLockMode.Locked || Input.GetMouseButton(1))
        {
            _currentX += Input.GetAxis("Mouse X") * rotationSpeed;
            _currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
            _currentY = Mathf.Clamp(_currentY, minVerticalAngle, maxVerticalAngle);
        }
    }

    // 防穿墙：检测 pivot 到理想相机位置之间是否有障碍物，若有则将距离缩短到命中点前
    private float ResolveCollision(Vector3 pivot, Vector3 desiredPosition)
    {
        if (!enableCollision) return distance;

        Vector3 dir = desiredPosition - pivot;
        float targetDist = dir.magnitude;
        if (targetDist <= 0.001f) return targetDist;

        dir.Normalize();

        if (Physics.SphereCast(pivot, collisionRadius, dir, out RaycastHit hit, targetDist, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            float safeDist = Mathf.Max(hit.distance - collisionOffset, minDistance);
            return safeDist;
        }

        return targetDist;
    }
}
