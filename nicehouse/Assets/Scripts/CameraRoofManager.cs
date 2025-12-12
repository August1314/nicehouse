using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 管理屋顶在不同相机下的显示/隐藏
/// 通过Layer系统实现：FPS相机显示屋顶，TopView相机隐藏屋顶
/// </summary>
public class CameraRoofManager : MonoBehaviour
{
    [Header("相机设置")]
    [Tooltip("第一人称相机（FPS相机），需要显示屋顶")]
    public Camera fpsCamera;
    
    [Tooltip("俯视图相机（TopView相机），需要隐藏屋顶")]
    public Camera topViewCamera;
    
    [Header("屋顶设置")]
    [Tooltip("屋顶所在的Layer名称（需要在Unity的Layer设置中创建）")]
    public string roofLayerName = "Roof";
    
    [Tooltip("是否在Start时自动配置相机和屋顶")]
    public bool autoConfigureOnStart = true;
    
    private int _roofLayer;
    private LayerMask _fpsCameraMask;
    private LayerMask _topViewCameraMask;
    
    private void Start()
    {
        if (autoConfigureOnStart)
        {
            ConfigureCameras();
        }
    }
    
    /// <summary>
    /// 配置相机的Culling Mask，使FPS相机显示屋顶，TopView相机隐藏屋顶
    /// </summary>
    [ContextMenu("配置相机")]
    public void ConfigureCameras()
    {
        // 获取屋顶Layer的索引
        _roofLayer = LayerMask.NameToLayer(roofLayerName);
        
        if (_roofLayer == -1)
        {
            Debug.LogError($"[CameraRoofManager] Layer '{roofLayerName}' 不存在！请在Unity的Edit > Project Settings > Tags and Layers中创建此Layer。");
            return;
        }
        
        // 配置FPS相机：渲染所有Layer（包括屋顶）
        if (fpsCamera != null)
        {
            _fpsCameraMask = fpsCamera.cullingMask;
            // 确保FPS相机渲染屋顶Layer
            _fpsCameraMask |= (1 << _roofLayer);
            fpsCamera.cullingMask = _fpsCameraMask;
            Debug.Log($"[CameraRoofManager] FPS相机已配置：渲染所有Layer（包括屋顶Layer '{roofLayerName}'）");
        }
        else
        {
            Debug.LogWarning("[CameraRoofManager] FPS相机未指定！");
        }
        
        // 配置TopView相机：渲染所有Layer，但排除屋顶Layer
        if (topViewCamera != null)
        {
            // 先设置为渲染所有Layer（包括默认Layer、家具、墙壁等）
            _topViewCameraMask = ~0; // 所有Layer
            // 然后排除屋顶Layer
            _topViewCameraMask &= ~(1 << _roofLayer);
            topViewCamera.cullingMask = _topViewCameraMask;
            Debug.Log($"[CameraRoofManager] TopView相机已配置：渲染所有Layer（除了屋顶Layer '{roofLayerName}'），包括家具、墙壁、热力图等");
        }
        else
        {
            Debug.LogWarning("[CameraRoofManager] TopView相机未指定！");
        }
    }
    
    /// <summary>
    /// 将指定的GameObject及其子对象移动到屋顶Layer
    /// </summary>
    [ContextMenu("将选中对象移动到屋顶Layer")]
    public void MoveSelectedToRoofLayer()
    {
#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject == null)
        {
            Debug.LogWarning("[CameraRoofManager] 请先在Hierarchy中选择要移动到屋顶Layer的对象！");
            return;
        }
        
        MoveToRoofLayer(UnityEditor.Selection.activeGameObject);
#else
        Debug.LogWarning("[CameraRoofManager] 此功能仅在编辑器模式下可用！请使用MoveToRoofLayer(GameObject)方法。");
#endif
    }
    
    /// <summary>
    /// 将GameObject及其所有子对象移动到屋顶Layer
    /// </summary>
    public void MoveToRoofLayer(GameObject obj)
    {
        _roofLayer = LayerMask.NameToLayer(roofLayerName);
        
        if (_roofLayer == -1)
        {
            Debug.LogError($"[CameraRoofManager] Layer '{roofLayerName}' 不存在！请在Unity的Edit > Project Settings > Tags and Layers中创建此Layer。");
            return;
        }
        
        SetLayerRecursive(obj, _roofLayer);
        Debug.Log($"[CameraRoofManager] 已将 '{obj.name}' 及其子对象移动到Layer '{roofLayerName}'");
    }
    
    /// <summary>
    /// 递归设置GameObject及其所有子对象的Layer
    /// </summary>
    private void SetLayerRecursive(GameObject obj, int layer)
    {
        if (obj == null) return;
        
        obj.layer = layer;
        
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
    
    /// <summary>
    /// 在编辑器中自动查找TopView相机
    /// </summary>
    [ContextMenu("自动查找TopView相机")]
    public void AutoFindTopViewCamera()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
        {
            if (cam.name.Contains("TopView") || cam.name.Contains("topview"))
            {
                topViewCamera = cam;
                Debug.Log($"[CameraRoofManager] 已自动找到TopView相机：{cam.name}");
                break;
            }
        }
    }
    
    /// <summary>
    /// 在编辑器中自动查找FPS相机
    /// </summary>
    [ContextMenu("自动查找FPS相机")]
    public void AutoFindFPSCamera()
    {
        FirstPersonController fpsController = FindObjectOfType<FirstPersonController>();
        if (fpsController != null && fpsController.cameraPivot != null)
        {
            fpsCamera = fpsController.cameraPivot.GetComponent<Camera>();
            if (fpsCamera != null)
            {
                Debug.Log($"[CameraRoofManager] 已自动找到FPS相机：{fpsCamera.name}");
            }
        }
        
        // 如果没找到，尝试查找名称包含"FPS"或"Fps"的相机
        if (fpsCamera == null)
        {
            Camera[] cameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in cameras)
            {
                if (cam.name.Contains("FPS") || cam.name.Contains("Fps") || cam.name.Contains("Main"))
                {
                    fpsCamera = cam;
                    Debug.Log($"[CameraRoofManager] 已自动找到FPS相机：{cam.name}");
                    break;
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraRoofManager))]
public class CameraRoofManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        CameraRoofManager manager = (CameraRoofManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("快捷操作", EditorStyles.boldLabel);
        
        if (GUILayout.Button("自动查找所有相机"))
        {
            manager.AutoFindTopViewCamera();
            manager.AutoFindFPSCamera();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("将选中对象移动到屋顶Layer:", GUILayout.Width(200));
        GUI.enabled = Selection.activeGameObject != null;
        if (GUILayout.Button("移动选中对象", GUILayout.Height(30)))
        {
            if (Selection.activeGameObject != null)
            {
                manager.MoveToRoofLayer(Selection.activeGameObject);
                EditorUtility.SetDirty(Selection.activeGameObject);
            }
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        
        if (Selection.activeGameObject == null)
        {
            EditorGUILayout.HelpBox("请在Hierarchy中选择要移动到屋顶Layer的对象", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox($"已选择: {Selection.activeGameObject.name}", MessageType.None);
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("配置相机Culling Mask"))
        {
            manager.ConfigureCameras();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "使用步骤：\n" +
            "1. 在Unity的Edit > Project Settings > Tags and Layers中创建'Roof' Layer\n" +
            "2. 点击'自动查找所有相机'按钮\n" +
            "3. 在Hierarchy中选择屋顶GameObject，然后点击上方的'移动选中对象'按钮\n" +
            "4. 点击'配置相机Culling Mask'按钮\n" +
            "完成！现在FPS相机会显示屋顶，TopView相机不会显示屋顶。",
            MessageType.Info);
    }
}
#endif

