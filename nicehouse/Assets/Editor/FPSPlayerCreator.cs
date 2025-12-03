using UnityEditor;
using UnityEngine;

/// <summary>
/// 一键在场景中创建第一人称玩家对象（含 CharacterController + Camera + FirstPersonController）。
/// 菜单：Tools/First Person/Create FPS Player
/// </summary>
public static class FPSPlayerCreator
{
    [MenuItem("Tools/First Person/Create FPS Player")]
    public static void CreatePlayer()
    {
        // 根对象
        var playerGO = new GameObject("FPSPlayer");
        Undo.RegisterCreatedObjectUndo(playerGO, "Create FPS Player");

        var playerTransform = playerGO.transform;
        playerTransform.position = Vector3.zero;

        // 角色控制器
        var controller = playerGO.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.4f;
        controller.center = new Vector3(0f, 0.9f, 0f);

        // 相机
        var cameraGO = new GameObject("FpsCamera");
        cameraGO.transform.SetParent(playerTransform, false);
        cameraGO.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        cameraGO.transform.localRotation = Quaternion.identity;

        var cam = cameraGO.AddComponent<Camera>();
        cameraGO.AddComponent<AudioListener>();

        // 控制脚本
        var controllerScript = playerGO.AddComponent<FirstPersonController>();
        controllerScript.cameraPivot = cameraGO.transform;

        // 选中新建对象，方便用户移动到入口位置
        Selection.activeGameObject = playerGO;

        Debug.Log("[FPSPlayerCreator] FPSPlayer 已创建，请将其移动到房子入口附近，并禁用场景中的多余相机（Main Camera）。");
    }
}


