using UnityEngine;
using UnityEditor;

public class ThirdPersonCreator
{
    [MenuItem("Tools/Third Person/Create TP Player")]
    public static void CreateTPPlayer()
    {
        // 1. 确定生成位置
        Vector3 spawnPos = Vector3.zero;
        if (SceneView.lastActiveSceneView != null)
        {
            Camera sceneCam = SceneView.lastActiveSceneView.camera;
            if (sceneCam != null)
            {
                // 生成在 Scene 相机前方 3 米处
                spawnPos = sceneCam.transform.position + sceneCam.transform.forward * 3f;
            }
        }
        else
        {
            spawnPos = new Vector3(0, 2, 0); // 默认抬高一点，防止卡在地里
        }

        // 2. 创建玩家根对象
        GameObject playerGO = new GameObject("ThirdPersonPlayer");
        Undo.RegisterCreatedObjectUndo(playerGO, "Create TP Player");
        playerGO.transform.position = spawnPos;

        // 3. 添加 CharacterController

        CharacterController cc = playerGO.AddComponent<CharacterController>();
        cc.center = new Vector3(0, 1, 0);
        cc.height = 2.0f;
        cc.radius = 0.5f;

        // 3. 创建可视化模型 (胶囊体) 作为占位符
        GameObject model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        model.name = "Model_Placeholder";
        model.transform.SetParent(playerGO.transform);
        model.transform.localPosition = new Vector3(0, 1, 0);
        // 移除碰撞体，因为父物体已经有 CharacterController
        Object.DestroyImmediate(model.GetComponent<Collider>());

        // 4. 添加控制脚本
        ThirdPersonController controller = playerGO.AddComponent<ThirdPersonController>();

        // 5. 创建相机
        GameObject camGO = new GameObject("TP_Camera");
        Camera cam = camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();
        camGO.tag = "MainCamera"; // 标记为主相机
        
        // 添加相机脚本
        ThirdPersonCamera tpCam = camGO.AddComponent<ThirdPersonCamera>();
        tpCam.target = playerGO.transform;
        
        // 设置引用
        controller.mainCamera = cam;

        // 选中
        Selection.activeGameObject = playerGO;
        
        Debug.Log("第三人称角色已创建！请按 Play 运行测试。WASD移动，鼠标右键旋转视角。");
    }
}
