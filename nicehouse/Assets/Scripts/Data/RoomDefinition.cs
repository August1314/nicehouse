using UnityEngine;

namespace NiceHouse.Data
{
    public enum RoomType
    {
        LivingRoom,
        Bedroom,
        Kitchen,
        Bathroom,
        Study,
        Corridor,
        Other
    }

    /// <summary>
    /// 挂在每个房间 GameObject 上，标记房间基础信息。
    /// </summary>
    public class RoomDefinition : MonoBehaviour
    {
        [Tooltip("房间唯一 ID，例如 LivingRoom01")]
        public string roomId;

        public RoomType roomType;

        [Tooltip("在 UI 中展示用的名称，例如 客厅")]
        public string displayName;

        [Tooltip("可选：手动指定房间体积（例如子物体 RoomVolume 上的 BoxCollider）")]
        public Collider roomVolume;

        /// <summary>
        /// 获取房间的空间包围盒，用于热力图、剖切等可视化。
        /// 优先使用显式指定的 roomVolume，其次自动从子物体查找 Collider / Renderer。
        /// </summary>
        public Bounds GetBounds()
        {
            if (roomVolume != null)
            {
                return roomVolume.bounds;
            }

            var col = GetComponentInChildren<Collider>();
            if (col != null)
            {
                return col.bounds;
            }

            var rend = GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                return rend.bounds;
            }

            return new Bounds(transform.position, Vector3.zero);
        }
    }
}


