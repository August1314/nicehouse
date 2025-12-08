using UnityEngine;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 控制面板外壳的简单灯效与发光动画。
    /// </summary>
    public class ControlHubAnimator : MonoBehaviour
    {
        [SerializeField] private Renderer[] emissiveRenderers;
        [SerializeField] private string emissionColorProperty = "_EmissionColor";
        [SerializeField] private Color idleColor = new Color(0.0f, 0.2f, 0.35f);
        [SerializeField] private Color activeColor = new Color(0.0f, 0.7f, 1.0f);
        [SerializeField] private float pulseSpeed = 3f;

        private bool _isActive;

        private void Update()
        {
            var baseColor = _isActive ? activeColor : idleColor;
            var pulse = _isActive ? 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed) : 0f;
            var finalColor = Color.Lerp(baseColor * 0.5f, baseColor, pulse);

            foreach (var rend in emissiveRenderers)
            {
                if (rend == null) continue;
                if (rend.material.HasProperty(emissionColorProperty))
                {
                    rend.material.SetColor(emissionColorProperty, finalColor);
                }
            }
        }

        public void SetActiveVisual(bool isActive)
        {
            _isActive = isActive;
        }
    }
}


