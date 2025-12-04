using System.Text;
using NiceHouse.Data;
using UnityEditor;
using UnityEngine;

namespace NiceHouse.EditorTools
{
    /// <summary>
    /// 一键导出所有房间的中心点与包围盒尺寸，用于文档和分层可视化调试。
    /// 菜单：Tools/Room Data/Export Room Bounds
    /// </summary>
    public static class RoomBoundsExporter
    {
        [MenuItem("Tools/Room Data/Export Room Bounds")]
        public static void Export()
        {
            var rooms = Object.FindObjectsOfType<RoomDefinition>();
            if (rooms == null || rooms.Length == 0)
            {
                Debug.LogWarning("[RoomBoundsExporter] 场景中没有找到任何 RoomDefinition。");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("roomId,displayName,roomType,centerX,centerY,centerZ,sizeX,sizeY,sizeZ");

            foreach (var room in rooms)
            {
                Bounds b = room.GetBounds();
                Vector3 c = b.center;
                Vector3 s = b.size;
                sb.AppendLine(
                    $"{room.roomId},{room.displayName},{room.roomType},{c.x:F2},{c.y:F2},{c.z:F2},{s.x:F2},{s.y:F2},{s.z:F2}");
            }

            Debug.Log(sb.ToString());
        }
    }
}


