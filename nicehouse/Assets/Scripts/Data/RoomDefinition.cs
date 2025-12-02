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
    }
}


