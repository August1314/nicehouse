using UnityEngine;

public class FlashingLight : MonoBehaviour
{
    // 公开变量，可以在 Inspector 中调整
    public float minIntensity = 0.5f; // 最小亮度
    public float maxIntensity = 3.0f; // 最大亮度
    public float speed = 5.0f;        // 闪烁速度 (频率)

    private Light lightComponent;

    void Start()
    {
        // 获取附加到此游戏对象的 Light 组件
        lightComponent = GetComponent<Light>();

        // 检查 Light 组件是否存在
        if (lightComponent == null)
        {
            Debug.LogError("FlashingLight script requires a Light component on the same GameObject.");
            enabled = false; // 禁用脚本以防错误
            return;
        }

        // 确保灯光是红色的
        lightComponent.color = Color.red; 
    }

    public float flashRate = 0.2f; // 每 0.2 秒切换一次

    void Update()
    {
        // 使用取余运算符 (%) 实现周期性闪烁
        if (Time.time % flashRate < flashRate / 2f)
        {
            lightComponent.intensity = maxIntensity;
        }
        else
        {
            lightComponent.intensity = minIntensity;
        }
    }
}