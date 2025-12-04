using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NiceHouse.ControlHub
{
    /// <summary>
    /// 控制屏幕中心的准星与提示文本。
    /// </summary>
    public class CrosshairUI : MonoBehaviour
    {
        [SerializeField] private Image crosshairImage;
        [SerializeField] private TMP_Text hintLabel;
        [SerializeField] private Color idleColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(0f, 0.8f, 1f);
        [SerializeField] private float hoverScale = 1.15f;

        private Vector3 _defaultScale;

        private void Awake()
        {
            if (crosshairImage != null)
            {
                _defaultScale = crosshairImage.rectTransform.localScale;
            }
        }

        /// <summary>
        /// 外部调用以更新准星外观与提示文案。
        /// </summary>
        public void SetHoverState(bool isHovering, string hint = null)
        {
            if (crosshairImage != null)
            {
                crosshairImage.color = isHovering ? hoverColor : idleColor;

                var targetScale = _defaultScale == Vector3.zero ? Vector3.one : _defaultScale;
                crosshairImage.rectTransform.localScale = targetScale * (isHovering ? hoverScale : 1f);
            }

            if (hintLabel != null)
            {
                hintLabel.text = isHovering && !string.IsNullOrEmpty(hint) ? hint : string.Empty;
                hintLabel.gameObject.SetActive(!string.IsNullOrEmpty(hintLabel.text));
            }
        }
    }
}


