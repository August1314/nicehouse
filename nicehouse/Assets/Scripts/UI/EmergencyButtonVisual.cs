using UnityEngine;

namespace NiceHouse.UI
{
    /// <summary>
    /// 紧急按钮的悬停/按下颜色反馈（基于 Renderer 颜色或发光）。
    /// 需挂在带 Renderer 的按钮物体（或子物体）上，配合 EmergencyButton 使用。
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class EmergencyButtonVisual : MonoBehaviour
    {
        [Header("颜色设置")]
        public Color normalColor = Color.white;
        public Color hoverColor = new Color(1f, 0.9f, 0.6f);
        public Color pressedColor = new Color(1f, 0.6f, 0.4f);

        [Header("发光强度")]
        [Tooltip("是否写入 _EmissionColor（需材质支持）")]
        public bool useEmission = true;

        [Tooltip("Emission 基准强度")]
        public float emissionIntensity = 1.2f;

        [Header("按下保持")]
        [Tooltip("按下后保持按下颜色的时间（秒），0 表示不保持")]
        public float pressedHoldSeconds = 0f;

        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;
        private float _pressedTimer = 0f;
        private bool _isPressedHolding = false;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();
            ApplyColor(normalColor);
        }

        private void Update()
        {
            if (_isPressedHolding)
            {
                _pressedTimer -= Time.deltaTime;
                if (_pressedTimer <= 0f)
                {
                    _isPressedHolding = false;
                    ApplyColor(hoverColor);
                }
            }
        }

        private void OnMouseEnter()
        {
            if (_isPressedHolding) return;
            ApplyColor(hoverColor);
        }

        private void OnMouseExit()
        {
            if (_isPressedHolding) return;
            ApplyColor(normalColor);
        }

        private void OnMouseDown()
        {
            ApplyColor(pressedColor);
            if (pressedHoldSeconds > 0f)
            {
                _isPressedHolding = true;
                _pressedTimer = pressedHoldSeconds;
            }
        }

        private void OnMouseUp()
        {
            if (_isPressedHolding)
            {
                // 按下时已进入保持状态，等待 Update 倒计时结束
                return;
            }
            ApplyColor(hoverColor);
        }

        private void ApplyColor(Color c)
        {
            if (_renderer == null) return;

            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor("_Color", c);

            if (useEmission)
            {
                // Unity 标准材质需要先启用 emission 关键字
                _mpb.SetColor("_EmissionColor", c * emissionIntensity);
                _renderer.material.EnableKeyword("_EMISSION");
            }

            _renderer.SetPropertyBlock(_mpb);
        }
    }
}

