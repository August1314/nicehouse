using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 在3D面板上显示TopView相机的画面
/// 可以挂载在房间中的智能面板上，让玩家在第一人称视角中查看俯视图
/// </summary>
[RequireComponent(typeof(Renderer))]
public class TopViewDisplayPanel : MonoBehaviour
{
    [Header("相机设置")]
    [Tooltip("要显示的TopView相机")]
    public Camera topViewCamera;
    
    [Header("渲染纹理设置")]
    [Tooltip("渲染纹理分辨率宽度")]
    public int renderTextureWidth = 512;
    
    [Tooltip("渲染纹理分辨率高度")]
    public int renderTextureHeight = 512;
    
    [Tooltip("是否启用抗锯齿")]
    public bool enableMSAA = true;
    
    [Header("画面裁剪设置")]
    [Tooltip("是否启用画面裁剪（只显示指定区域）")]
    public bool enableViewportCrop = true;
    
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
    
    [Header("面板设置")]
    [Tooltip("面板材质（如果为空，会自动创建）")]
    public Material panelMaterial;
    
    [Tooltip("是否在启动时自动查找TopView相机")]
    public bool autoFindCamera = true;
    
    [Header("交互设置（可选）")]
    [Tooltip("是否支持玩家交互（靠近时显示/隐藏）")]
    public bool enableInteraction = false;
    
    [Tooltip("交互距离（米）")]
    public float interactionDistance = 3f;
    
    [Tooltip("玩家标签（用于检测玩家）")]
    public string playerTag = "Player";
    
    private RenderTexture _renderTexture;
    private Material _instanceMaterial;
    private Camera _playerCamera;
    private bool _isPlayerNearby = false;
    private Rect _originalViewportRect; // 保存原始Viewport Rect
    
    private void Awake()
    {
        // 自动查找TopView相机
        if (autoFindCamera && topViewCamera == null)
        {
            FindTopViewCamera();
        }
        
        // 查找玩家相机（用于距离检测）
        if (enableInteraction)
        {
            var fpsController = FindObjectOfType<FirstPersonController>();
            if (fpsController != null && fpsController.cameraPivot != null)
            {
                _playerCamera = fpsController.cameraPivot.GetComponent<Camera>();
            }
        }
    }
    
    private void Start()
    {
        SetupRenderTexture();
        SetupViewportCrop();
        SetupMaterial();
    }
    
    private void Update()
    {
        // 处理交互逻辑
        if (enableInteraction)
        {
            CheckPlayerDistance();
        }
    }
    
    private void OnDestroy()
    {
        // 恢复相机的原始Viewport Rect
        if (topViewCamera != null)
        {
            topViewCamera.rect = _originalViewportRect;
        }
        
        // 清理资源
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            if (Application.isPlaying)
            {
                Destroy(_renderTexture);
            }
            else
            {
                DestroyImmediate(_renderTexture);
            }
        }
        
        if (_instanceMaterial != null && Application.isPlaying)
        {
            Destroy(_instanceMaterial);
        }
    }
    
    /// <summary>
    /// 自动查找TopView相机
    /// </summary>
    [ContextMenu("自动查找TopView相机")]
    public void FindTopViewCamera()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
        {
            if (cam.name.Contains("TopView") || cam.name.Contains("topview"))
            {
                topViewCamera = cam;
                Debug.Log($"[TopViewDisplayPanel] 已自动找到TopView相机：{cam.name}");
                
                // 如果已经设置了渲染纹理，重新配置
                if (_renderTexture != null)
                {
                    SetupRenderTexture();
                }
                break;
            }
        }
        
        if (topViewCamera == null)
        {
            Debug.LogWarning("[TopViewDisplayPanel] 未找到TopView相机！请手动指定。");
        }
    }
    
    /// <summary>
    /// 设置渲染纹理
    /// </summary>
    private void SetupRenderTexture()
    {
        if (topViewCamera == null)
        {
            Debug.LogError("[TopViewDisplayPanel] TopView相机未指定！");
            return;
        }
        
        // 创建或更新Render Texture
        if (_renderTexture == null)
        {
            _renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 24);
            _renderTexture.name = "TopViewRenderTexture";
        }
        else if (_renderTexture.width != renderTextureWidth || _renderTexture.height != renderTextureHeight)
        {
            // 如果分辨率改变，重新创建
            _renderTexture.Release();
            Destroy(_renderTexture);
            _renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 24);
            _renderTexture.name = "TopViewRenderTexture";
        }
        
        // 设置抗锯齿
        if (enableMSAA)
        {
            _renderTexture.antiAliasing = 4;
        }
        else
        {
            _renderTexture.antiAliasing = 1;
        }
        
        // 将TopView相机设置为渲染到这个纹理
        topViewCamera.targetTexture = _renderTexture;
        
        Debug.Log($"[TopViewDisplayPanel] 已设置渲染纹理：{renderTextureWidth}x{renderTextureHeight}");
    }
    
    /// <summary>
    /// 设置相机的Viewport Rect来裁剪画面
    /// </summary>
    private void SetupViewportCrop()
    {
        if (topViewCamera == null)
        {
            return;
        }
        
        // 保存原始Viewport Rect
        _originalViewportRect = topViewCamera.rect;
        
        if (enableViewportCrop)
        {
            // 设置裁剪区域
            // Unity的Viewport Rect使用：x, y, width, height（都是0-1范围）
            // x, y是左下角坐标
            topViewCamera.rect = new Rect(cropX, cropY, cropWidth, cropHeight);
            Debug.Log($"[TopViewDisplayPanel] 已设置画面裁剪：X={cropX}, Y={cropY}, W={cropWidth}, H={cropHeight}");
        }
        else
        {
            // 不裁剪，使用完整画面
            topViewCamera.rect = new Rect(0, 0, 1, 1);
        }
    }
    
    /// <summary>
    /// 设置材质
    /// </summary>
    private void SetupMaterial()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("[TopViewDisplayPanel] 未找到Renderer组件！");
            return;
        }
        
        if (_renderTexture == null)
        {
            Debug.LogError("[TopViewDisplayPanel] 渲染纹理未创建！");
            return;
        }
        
        // 创建材质实例
        if (panelMaterial != null)
        {
            _instanceMaterial = new Material(panelMaterial);
        }
        else
        {
            // 使用默认的Unlit/Texture材质
            _instanceMaterial = new Material(Shader.Find("Unlit/Texture"));
        }
        
        // 将Render Texture赋值给材质
        // 尝试不同的纹理属性名称（兼容不同的Shader）
        if (_instanceMaterial.HasProperty("_MainTex"))
        {
            _instanceMaterial.SetTexture("_MainTex", _renderTexture);
        }
        else if (_instanceMaterial.HasProperty("_BaseMap"))
        {
            _instanceMaterial.SetTexture("_BaseMap", _renderTexture);
        }
        else
        {
            // 如果都没有，尝试直接设置mainTexture
            _instanceMaterial.mainTexture = _renderTexture;
        }
        
        // 应用到Renderer
        renderer.material = _instanceMaterial;
        
        Debug.Log($"[TopViewDisplayPanel] 材质已设置，使用Shader: {_instanceMaterial.shader.name}");
    }
    
    /// <summary>
    /// 检查玩家距离
    /// </summary>
    private void CheckPlayerDistance()
    {
        if (_playerCamera == null) return;
        
        float distance = Vector3.Distance(transform.position, _playerCamera.transform.position);
        bool wasNearby = _isPlayerNearby;
        _isPlayerNearby = distance <= interactionDistance;
        
        // 如果距离状态改变，可以触发事件
        if (wasNearby != _isPlayerNearby)
        {
            OnPlayerDistanceChanged(_isPlayerNearby);
        }
    }
    
    /// <summary>
    /// 玩家距离改变时的回调
    /// </summary>
    private void OnPlayerDistanceChanged(bool isNearby)
    {
        // 可以在这里添加显示/隐藏逻辑
        // 例如：显示提示UI、播放音效等
        if (isNearby)
        {
            Debug.Log("[TopViewDisplayPanel] 玩家靠近面板");
        }
        else
        {
            Debug.Log("[TopViewDisplayPanel] 玩家远离面板");
        }
    }
    
    /// <summary>
    /// 手动设置TopView相机
    /// </summary>
    public void SetTopViewCamera(Camera camera)
    {
        if (camera == null)
        {
            Debug.LogWarning("[TopViewDisplayPanel] 相机不能为空！");
            return;
        }
        
        // 如果之前有相机，恢复其targetTexture
        if (topViewCamera != null && topViewCamera.targetTexture == _renderTexture)
        {
            topViewCamera.targetTexture = null;
        }
        
        topViewCamera = camera;
        SetupRenderTexture();
    }
    
    /// <summary>
    /// 更新渲染纹理分辨率（运行时）
    /// </summary>
    public void UpdateResolution(int width, int height)
    {
        renderTextureWidth = width;
        renderTextureHeight = height;
        SetupRenderTexture();
        SetupViewportCrop();
        SetupMaterial();
    }
    
    /// <summary>
    /// 更新裁剪区域（运行时）
    /// </summary>
    public void UpdateCropArea(float x, float y, float width, float height)
    {
        cropX = Mathf.Clamp01(x);
        cropY = Mathf.Clamp01(y);
        cropWidth = Mathf.Clamp01(width);
        cropHeight = Mathf.Clamp01(height);
        SetupViewportCrop();
    }
    
    /// <summary>
    /// 自动计算裁剪区域以匹配房子的长宽比（需要RoomManager）
    /// </summary>
    [ContextMenu("自动计算裁剪区域")]
    public void AutoCalculateCropArea()
    {
        if (topViewCamera == null)
        {
            Debug.LogWarning("[TopViewDisplayPanel] TopView相机未指定，无法计算裁剪区域");
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
                    
                    // 获取相机的Orthographic Size（正交相机的半高）
                    float cameraSize = topViewCamera.orthographicSize;
                    float cameraWidth = cameraSize * 2f * (topViewCamera.aspect); // 相机宽度
                    float cameraHeight = cameraSize * 2f; // 相机高度
                    
                    // 计算房子在相机视野中的位置和大小
                    Vector3 houseCenter = totalBounds.center;
                    Vector3 cameraPos = topViewCamera.transform.position;
                    
                    // 计算房子中心相对于相机的位置
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
                    
                    SetupViewportCrop();
                    Debug.Log($"[TopViewDisplayPanel] 已自动计算裁剪区域：X={cropX:F3}, Y={cropY:F3}, W={cropWidth:F3}, H={cropHeight:F3}");
                    return;
                }
            }
        }
        
        Debug.LogWarning("[TopViewDisplayPanel] 无法自动计算裁剪区域，请手动设置");
    }
    
    /// <summary>
    /// 启用/禁用显示
    /// </summary>
    public void SetDisplayEnabled(bool enabled)
    {
        if (topViewCamera != null)
        {
            topViewCamera.enabled = enabled;
        }
        
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = enabled;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TopViewDisplayPanel))]
public class TopViewDisplayPanelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        TopViewDisplayPanel panel = (TopViewDisplayPanel)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("快捷操作", EditorStyles.boldLabel);
        
        if (GUILayout.Button("自动查找TopView相机"))
        {
            panel.FindTopViewCamera();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("画面裁剪", EditorStyles.boldLabel);
        if (GUILayout.Button("自动计算裁剪区域（基于房间布局）"))
        {
            panel.AutoCalculateCropArea();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "使用步骤：\n" +
            "1. 在场景中创建一个Quad或平板模型作为显示面板\n" +
            "2. 将TopViewDisplayPanel脚本添加到面板上\n" +
            "3. 点击'自动查找TopView相机'按钮（或手动指定）\n" +
            "4. 调整面板位置和大小\n" +
            "5. 运行游戏，玩家可以在第一人称视角中看到TopView相机的画面\n\n" +
            "提示：\n" +
            "- 可以调整renderTextureWidth和renderTextureHeight来控制画面质量\n" +
            "- 面板应该面向玩家（Quad默认朝Z轴正方向）\n" +
            "- 如果面板是Quad，可能需要旋转90度（绕X轴）",
            MessageType.Info);
    }
}
#endif

