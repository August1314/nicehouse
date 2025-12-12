using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// TopView相机辅助工具
/// 用于方便地调整相机的Viewport Rect，只显示需要的区域（例如只显示房子部分）
/// </summary>
[RequireComponent(typeof(Camera))]
public class TopViewCameraHelper : MonoBehaviour
{
    [Header("画面裁剪设置")]
    [Tooltip("是否启用画面裁剪")]
    public bool enableCrop = false;
    
    [Tooltip("裁剪区域 - X坐标（0-1，0=左边缘，1=右边缘）")]
    [Range(0f, 1f)]
    public float cropX = 0f;
    
    [Tooltip("裁剪区域 - Y坐标（0-1，0=下边缘，1=上边缘）")]
    [Range(0f, 1f)]
    public float cropY = 0f;
    
    [Tooltip("裁剪区域 - 宽度（0-1）")]
    [Range(0f, 1f)]
    public float cropWidth = 1f;
    
    [Tooltip("裁剪区域 - 高度（0-1）")]
    [Range(0f, 1f)]
    public float cropHeight = 1f;
    
    [Header("相机调整")]
    [Tooltip("正交相机的Size（半高）")]
    public float orthographicSize = 13f;
    
    [Tooltip("相机位置X")]
    public float positionX = -3.98f;
    
    [Tooltip("相机位置Y（高度）")]
    public float positionY = 35.42f;
    
    [Tooltip("相机位置Z")]
    public float positionZ = 3.6f;
    
    private Camera _camera;
    private Rect _originalViewportRect;
    private float _originalOrthographicSize;
    private Vector3 _originalPosition;
    
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            Debug.LogError("[TopViewCameraHelper] 未找到Camera组件！");
            return;
        }
        
        // 保存原始设置
        _originalViewportRect = _camera.rect;
        _originalOrthographicSize = _camera.orthographicSize;
        _originalPosition = transform.position;
    }
    
    private void Start()
    {
        ApplySettings();
    }
    
    private void OnValidate()
    {
        // 在编辑器中修改参数时立即应用
        if (Application.isPlaying && _camera != null)
        {
            ApplySettings();
        }
    }
    
    /// <summary>
    /// 应用所有设置
    /// </summary>
    private void ApplySettings()
    {
        if (_camera == null) return;
        
        // 应用位置
        transform.position = new Vector3(positionX, positionY, positionZ);
        
        // 应用正交Size
        _camera.orthographicSize = orthographicSize;
        
        // 应用Viewport Rect
        if (enableCrop)
        {
            _camera.rect = new Rect(cropX, cropY, cropWidth, cropHeight);
        }
        else
        {
            _camera.rect = new Rect(0, 0, 1, 1);
        }
    }
    
    /// <summary>
    /// 重置为原始设置
    /// </summary>
    [ContextMenu("重置为原始设置")]
    public void ResetToOriginal()
    {
        if (_camera == null) return;
        
        enableCrop = false;
        cropX = _originalViewportRect.x;
        cropY = _originalViewportRect.y;
        cropWidth = _originalViewportRect.width;
        cropHeight = _originalViewportRect.height;
        orthographicSize = _originalOrthographicSize;
        positionX = _originalPosition.x;
        positionY = _originalPosition.y;
        positionZ = _originalPosition.z;
        
        ApplySettings();
    }
    
    /// <summary>
    /// 自动计算裁剪区域以匹配房子（需要RoomManager）
    /// </summary>
    [ContextMenu("自动计算裁剪区域")]
    public void AutoCalculateCrop()
    {
        if (_camera == null)
        {
            Debug.LogWarning("[TopViewCameraHelper] 相机未找到");
            return;
        }
        
        // 尝试从RoomManager获取所有房间的包围盒
        if (NiceHouse.Data.RoomManager.Instance != null)
        {
            var allRooms = NiceHouse.Data.RoomManager.Instance.GetAllRooms();
            if (allRooms.Count > 0)
            {
                // 计算所有房间的总包围盒
                bool hasBounds = false;
                Bounds totalBounds = new Bounds();
                foreach (var room in allRooms.Values)
                {
                    Bounds roomBounds = room.GetBounds();
                    if (!hasBounds)
                    {
                        totalBounds = roomBounds;
                        hasBounds = true;
                    }
                    else
                    {
                        totalBounds.Encapsulate(roomBounds);
                    }
                }
                
                if (hasBounds)
                {
                    // 计算房子在世界空间中的尺寸
                    float houseWidth = totalBounds.size.x;
                    float houseHeight = totalBounds.size.z; // Z轴是深度
                    
                    // 获取相机的Orthographic Size
                    float cameraSize = _camera.orthographicSize;
                    float cameraWidth = cameraSize * 2f * _camera.aspect; // 相机宽度
                    float cameraHeight = cameraSize * 2f; // 相机高度
                    
                    // 计算房子中心相对于相机的位置
                    Vector3 houseCenter = totalBounds.center;
                    Vector3 cameraPos = transform.position;
                    Vector3 offset = houseCenter - cameraPos;
                    
                    // 转换为Viewport坐标（0-1范围）
                    float viewportX = (offset.x + cameraWidth * 0.5f) / cameraWidth;
                    float viewportY = (offset.z + cameraHeight * 0.5f) / cameraHeight; // 注意：俯视图Z轴对应Y
                    float viewportWidth = houseWidth / cameraWidth;
                    float viewportHeight = houseHeight / cameraHeight;
                    
                    // 限制在0-1范围内
                    cropX = Mathf.Clamp01(viewportX);
                    cropY = Mathf.Clamp01(viewportY);
                    cropWidth = Mathf.Clamp01(viewportWidth);
                    cropHeight = Mathf.Clamp01(viewportHeight);
                    
                    enableCrop = true;
                    ApplySettings();
                    
                    Debug.Log($"[TopViewCameraHelper] 已自动计算裁剪区域：X={cropX:F3}, Y={cropY:F3}, W={cropWidth:F3}, H={cropHeight:F3}");
                    return;
                }
            }
        }
        
        Debug.LogWarning("[TopViewCameraHelper] 无法自动计算裁剪区域，请确保场景中有RoomDefinition且RoomManager已初始化");
    }
    
    /// <summary>
    /// 从当前Transform同步位置到参数
    /// </summary>
    [ContextMenu("从Transform同步位置")]
    public void SyncPositionFromTransform()
    {
        positionX = transform.position.x;
        positionY = transform.position.y;
        positionZ = transform.position.z;
    }
    
    /// <summary>
    /// 从参数同步位置到Transform
    /// </summary>
    [ContextMenu("应用位置到Transform")]
    public void ApplyPositionToTransform()
    {
        transform.position = new Vector3(positionX, positionY, positionZ);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TopViewCameraHelper))]
public class TopViewCameraHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        TopViewCameraHelper helper = (TopViewCameraHelper)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("快捷操作", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("自动计算裁剪区域"))
        {
            helper.AutoCalculateCrop();
        }
        if (GUILayout.Button("重置为原始设置"))
        {
            helper.ResetToOriginal();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("从Transform同步位置"))
        {
            helper.SyncPositionFromTransform();
        }
        if (GUILayout.Button("应用位置到Transform"))
        {
            helper.ApplyPositionToTransform();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "使用说明：\n" +
            "1. 调整Orthographic Size来改变视野大小\n" +
            "2. 调整Position X/Y/Z来移动相机位置\n" +
            "3. 启用Enable Crop并调整Crop参数来裁剪画面\n" +
            "4. 点击'自动计算裁剪区域'可以基于房间布局自动设置\n" +
            "5. 在Game视图中实时查看调整效果\n\n" +
            "提示：\n" +
            "- Viewport Rect参数使用0-1范围（归一化坐标）\n" +
            "- 修改参数后会在运行时立即生效\n" +
            "- 可以在Scene视图中移动相机，然后点击'从Transform同步位置'",
            MessageType.Info);
    }
}
#endif

