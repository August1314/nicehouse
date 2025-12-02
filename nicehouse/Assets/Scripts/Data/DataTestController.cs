using UnityEngine;
using NiceHouse.Data;

/// <summary>
/// 数据底座功能测试脚本。
/// 挂在 DataRoot 上，用于验证所有数据模块是否正常工作。
/// </summary>
public class DataTestController : MonoBehaviour
{
    [Header("测试配置")]
    [Tooltip("测试的房间ID")]
    public string testRoomId = "LivingRoom01";

    [Tooltip("测试的设备ID")]
    public string testDeviceId = "Windows01";

    [Tooltip("是否自动运行测试")]
    public bool autoRun = false;

    [Tooltip("自动测试间隔（秒）")]
    public float autoTestInterval = 5f;

    private float _timer;
    private int _testStep = 0;

    private void Update()
    {
        if (autoRun)
        {
            _timer += Time.deltaTime;
            if (_timer >= autoTestInterval)
            {
                _timer = 0f;
                RunNextTest();
            }
        }
    }

    /// <summary>
    /// 运行下一个测试步骤。
    /// </summary>
    private void RunNextTest()
    {
        switch (_testStep % 6)
        {
            case 0:
                TestEnvironmentData();
                break;
            case 1:
                TestEnergyData();
                break;
            case 2:
                TestPersonState();
                break;
            case 3:
                TestHealthData();
                break;
            case 4:
                TestActivityTracking();
                break;
            case 5:
                TestAlarmSystem();
                break;
        }
        _testStep++;
    }

    /// <summary>
    /// 测试环境数据。
    /// </summary>
    [ContextMenu("测试环境数据")]
    public void TestEnvironmentData()
    {
        Debug.Log("=== 测试环境数据 ===");
        
        if (EnvironmentDataStore.Instance == null)
        {
            Debug.LogError("EnvironmentDataStore.Instance 为空！");
            return;
        }

        if (EnvironmentDataStore.Instance.TryGetRoomData(testRoomId, out var env))
        {
            Debug.Log($"[环境数据] 房间={testRoomId}, 温度={env.temperature:F1}°C, " +
                     $"湿度={env.humidity:F1}%, PM2.5={env.pm25:F1} μg/m³");
        }
        else
        {
            Debug.LogWarning($"未找到房间 {testRoomId} 的环境数据");
        }
    }

    /// <summary>
    /// 测试能耗数据。
    /// </summary>
    [ContextMenu("测试能耗数据")]
    public void TestEnergyData()
    {
        Debug.Log("=== 测试能耗数据 ===");

        if (EnergyManager.Instance == null)
        {
            Debug.LogError("EnergyManager.Instance 为空！");
            return;
        }

        // 模拟设备开启
        EnergyManager.Instance.StartConsume(testDeviceId);
        Debug.Log($"[能耗] 设备 {testDeviceId} 开始耗电");

        float consumption = EnergyManager.Instance.GetDeviceDailyConsumption(testDeviceId);
        Debug.Log($"[能耗] 设备 {testDeviceId} 累计用电: {consumption:F4} kWh");
    }

    /// <summary>
    /// 测试数字人状态。
    /// </summary>
    [ContextMenu("测试数字人状态")]
    public void TestPersonState()
    {
        Debug.Log("=== 测试数字人状态 ===");

        if (PersonStateController.Instance == null)
        {
            Debug.LogError("PersonStateController.Instance 为空！");
            return;
        }

        // 切换不同状态
        PersonState[] states = { PersonState.Walking, PersonState.Sitting, PersonState.Sleeping };
        PersonState newState = states[Random.Range(0, states.Length)];

        PersonStateController.Instance.ChangeState(newState, testRoomId);
        Debug.Log($"[数字人] 状态切换为: {newState}, 所在房间: {testRoomId}");

        var status = PersonStateController.Instance.Status;
        Debug.Log($"[数字人] 当前状态持续时间: {status.stateDuration:F1}秒");
    }

    /// <summary>
    /// 测试健康数据。
    /// </summary>
    [ContextMenu("测试健康数据")]
    public void TestHealthData()
    {
        Debug.Log("=== 测试健康数据 ===");

        if (HealthDataStore.Instance == null)
        {
            Debug.LogError("HealthDataStore.Instance 为空！");
            return;
        }

        var health = HealthDataStore.Instance.Current;
        Debug.Log($"[健康] 心率={health.heartRate} bpm, " +
                 $"呼吸率={health.respirationRate} 次/分钟, " +
                 $"体动强度={health.bodyMovement:F2}, " +
                 $"睡眠阶段={GetSleepStageName(health.sleepStage)}");
    }

    /// <summary>
    /// 测试活动追踪。
    /// </summary>
    [ContextMenu("测试活动追踪")]
    public void TestActivityTracking()
    {
        Debug.Log("=== 测试活动追踪 ===");

        if (ActivityTracker.Instance == null)
        {
            Debug.LogError("ActivityTracker.Instance 为空！");
            return;
        }

        // 模拟进入房间
        ActivityTracker.Instance.OnPersonEnterRoom(testRoomId);
        Debug.Log($"[活动追踪] 数字人进入房间: {testRoomId}");

        var activity = ActivityTracker.Instance.GetRoomActivity(testRoomId);
        Debug.Log($"[活动追踪] 房间 {testRoomId} - 访问次数={activity.visitCount}, " +
                 $"累计停留时间={activity.totalStayTime:F1}秒");
    }

    /// <summary>
    /// 测试告警系统。
    /// </summary>
    [ContextMenu("测试告警系统")]
    public void TestAlarmSystem()
    {
        Debug.Log("=== 测试告警系统 ===");

        if (AlarmManager.Instance == null)
        {
            Debug.LogError("AlarmManager.Instance 为空！");
            return;
        }

        // 触发不同类型的告警
        AlarmType[] alarmTypes = 
        { 
            AlarmType.Smoke, 
            AlarmType.LongSitting, 
            AlarmType.HealthAbnormal 
        };
        AlarmType alarmType = alarmTypes[Random.Range(0, alarmTypes.Length)];

        AlarmManager.Instance.AddAlarm(alarmType, testRoomId);
        Debug.Log($"[告警] 触发告警: {alarmType} in {testRoomId}");

        // 查看最近告警
        var recentAlarms = AlarmManager.Instance.GetRecentAlarms(3);
        Debug.Log($"[告警] 最近3条告警:");
        foreach (var alarm in recentAlarms)
        {
            Debug.Log($"  - {alarm.type} in {alarm.roomId} at {alarm.time:HH:mm:ss} " +
                     $"({(alarm.handled ? "已处理" : "未处理")})");
        }
    }

    /// <summary>
    /// 测试安全数据。
    /// </summary>
    [ContextMenu("测试安全数据")]
    public void TestSafetyData()
    {
        Debug.Log("=== 测试安全数据 ===");

        if (SafetyDataStore.Instance == null)
        {
            Debug.LogError("SafetyDataStore.Instance 为空！");
            return;
        }

        // 模拟烟雾浓度升高
        SafetyDataStore.Instance.SetSmokeLevel(testRoomId, 50f);
        Debug.Log($"[安全] 房间 {testRoomId} 烟雾浓度设置为: 50");

        if (SafetyDataStore.Instance.TryGetRoomSafety(testRoomId, out var safety))
        {
            Debug.Log($"[安全] 房间 {testRoomId} - 烟雾={safety.smokeLevel:F1}, " +
                     $"燃气={safety.gasLevel:F1}");
        }
    }

    /// <summary>
    /// 运行所有测试。
    /// </summary>
    [ContextMenu("运行所有测试")]
    public void RunAllTests()
    {
        Debug.Log("========== 开始运行所有数据底座测试 ==========");
        
        TestEnvironmentData();
        TestEnergyData();
        TestPersonState();
        TestHealthData();
        TestActivityTracking();
        TestAlarmSystem();
        TestSafetyData();

        Debug.Log("========== 所有测试完成 ==========");
    }

    private string GetSleepStageName(int stage)
    {
        return stage switch
        {
            0 => "清醒",
            1 => "浅睡",
            2 => "深睡",
            _ => "未知"
        };
    }
}

