using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NiceHouse.LayeredVisualization.UI
{
    /// <summary>
    /// 控制图例面板的标题、范围和渐变显示。
    /// </summary>
    public class LegendPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI rangeText;
        public Image gradientImage;

        [Tooltip("渐变贴图宽度（像素）")]
        public int gradientResolution = 128;

        private Texture2D _gradientTexture;
        private Sprite _gradientSprite;

        /// <summary>
        /// 更新图例内容。
        /// </summary>
        public void UpdateLegend(string displayName, string unit, float minValue, float maxValue, Gradient gradient)
        {
            if (titleText != null)
            {
                titleText.text = string.IsNullOrEmpty(unit)
                    ? displayName
                    : $"{displayName} ({unit})";
            }

            if (rangeText != null)
            {
                rangeText.text = $"{minValue:F1} - {maxValue:F1} {unit}".Trim();
            }

            if (gradientImage != null && gradient != null)
            {
                EnsureGradientAssets();
                for (int x = 0; x < _gradientTexture.width; x++)
                {
                    float t = x / (float)(_gradientTexture.width - 1);
                    var color = gradient.Evaluate(t);
                    _gradientTexture.SetPixel(x, 0, color);
                }

                _gradientTexture.Apply();
                gradientImage.sprite = _gradientSprite;
            }
        }

        private void EnsureGradientAssets()
        {
            if (gradientResolution <= 0) gradientResolution = 128;

            if (_gradientTexture == null || _gradientTexture.width != gradientResolution)
            {
                if (_gradientTexture != null)
                {
                    Destroy(_gradientTexture);
                }

                _gradientTexture = new Texture2D(gradientResolution, 1, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp
                };

                if (_gradientSprite != null)
                {
                    Destroy(_gradientSprite);
                }

                _gradientSprite = Sprite.Create(
                    _gradientTexture,
                    new Rect(0, 0, _gradientTexture.width, _gradientTexture.height),
                    new Vector2(0.5f, 0.5f),
                    _gradientTexture.width);
            }
        }

        private void OnDestroy()
        {
            if (_gradientTexture != null)
            {
                Destroy(_gradientTexture);
            }

            if (_gradientSprite != null)
            {
                Destroy(_gradientSprite);
            }
        }
    }
}


