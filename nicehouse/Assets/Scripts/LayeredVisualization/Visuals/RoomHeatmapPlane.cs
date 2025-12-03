using NiceHouse.Data;
using UnityEngine;

namespace NiceHouse.LayeredVisualization
{
    /// <summary>
    /// 在每个房间底部生成一个简单的矩形平面，并根据数值上色。
    /// 这是分层可视化的最小可用版本，用于调试房间 Bounds 与颜色映射。
    /// </summary>
    public class RoomHeatmapPlane : MonoBehaviour
    {
        [Header("可视化设置")]
        public Material heatmapMaterial;
        public float heightOffset = 0.05f; // 稍微抬离地面避免 Z-fighting

        [Header("颜色范围（占位：使用 X 轴位置作为示例值）")]
        public Color minColor = Color.blue;
        public Color maxColor = Color.red;

        private void Start()
        {
            if (heatmapMaterial == null)
            {
                Debug.LogWarning("[RoomHeatmapPlane] 未指定 heatmapMaterial，不会生成平面。");
                return;
            }

            var rooms = FindObjectsOfType<RoomDefinition>();
            foreach (var room in rooms)
            {
                CreatePlaneForRoom(room);
            }
        }

        private void CreatePlaneForRoom(RoomDefinition room)
        {
            Bounds b = room.GetBounds();

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plane.name = $"{room.roomId}_HeatmapPlane";
            plane.transform.SetParent(room.transform, worldPositionStays: true);

            Vector3 center = b.center;
            center.y = b.min.y + heightOffset;
            plane.transform.position = center;

            // Quad 默认朝 +Z，旋转到朝上
            plane.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // 使用 bounds 的 XZ 作为尺寸
            plane.transform.localScale = new Vector3(b.size.x, b.size.z, 1f);

            var renderer = plane.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(heatmapMaterial);

            // 占位：用房间中心 X 值做一个 0-1 归一化映射，仅用于调试颜色
            float value = Mathf.InverseLerp(-15f, 10f, b.center.x);
            Color c = Color.Lerp(minColor, maxColor, value);
            renderer.sharedMaterial.color = c;

            // 不参与物理
            Destroy(plane.GetComponent<Collider>());
        }
    }
}


