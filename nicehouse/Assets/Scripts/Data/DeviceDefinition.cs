using UnityEngine;

namespace NiceHouse.Data
{
    public enum DeviceType
    {
        AirConditioner,
        FreshAirSystem,
        AirPurifier,
        Fan,
        Light,
        Window,
        SmokeSensor,
        Pm25Sensor,
        HelpButton,
        Other
    }

    /// <summary>
    /// 挂在每个可交互设备 / 传感器上。
    /// </summary>
    public class DeviceDefinition : MonoBehaviour
    {
        [Tooltip("设备唯一 ID，例如 AC_LivingRoom_01")]
        public string deviceId;

        public DeviceType type;

        [Tooltip("设备所在房间的 roomId，需与 RoomDefinition 保持一致")]
        public string roomId;
    }
}


