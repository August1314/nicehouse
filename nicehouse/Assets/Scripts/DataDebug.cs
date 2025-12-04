using UnityEngine;
using NiceHouse.Data;

/// <summary>
/// 简单调试脚本：在 Console 中打印房间环境数据和设备能耗。
/// 可挂在 DataRoot 上使用。
/// </summary>
public class DataDebug : MonoBehaviour
{
    [Header("要观察的房间 / 设备 ID")]
    public string roomId = "LivingRoom01";
    public string deviceId = "Windows01";

    private float _logInterval = 2f;
    private float _timer;

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < _logInterval) return;
        _timer = 0f;

        if (EnvironmentDataStore.Instance != null &&
            EnvironmentDataStore.Instance.TryGetRoomData(roomId, out var env))
        {
            Debug.Log(
                $"[Env] Room={roomId}  Temp={env.temperature:F1}°C  Humidity={env.humidity:F1}%  PM2.5={env.pm25:F1}");
        }

        if (EnergyManager.Instance != null)
        {
            float kwh = EnergyManager.Instance.GetDeviceDailyConsumption(deviceId);
            Debug.Log($"[Energy] Device={deviceId}  Daily={kwh:F4} kWh");
        }
    }
}


