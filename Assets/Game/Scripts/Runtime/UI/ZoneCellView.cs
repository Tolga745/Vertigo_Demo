using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Core;

namespace VertigoWheel.UI
{
    /// <summary>A single number cell in the zone bar: a highlight box (shown for the current zone) and
    /// the zone number, coloured by zone type. Child refs auto-bind in <see cref="OnValidate"/>.</summary>
    [DisallowMultipleComponent]
    public sealed class ZoneCellView : MonoBehaviour
    {
        [SerializeField] private Image _highlight;  // ui_image_zonecell_highlight_value
        [SerializeField] private TMP_Text _number;  // ui_text_zonecell_value

        public void SetContent(int number, Color numberColor, bool showHighlight, Color highlightColor, bool visible)
        {
            gameObject.SetActive(visible);
            if (!visible) return;

            if (_number != null)
            {
                _number.text = number.ToString();
                _number.color = numberColor;
            }
            if (_highlight != null)
            {
                _highlight.enabled = showHighlight;
                _highlight.color = highlightColor;
            }
        }

        private void OnValidate()
        {
            if (_highlight == null) _highlight = UiBinder.FindByName<Image>(transform, "ui_image_zonecell_highlight_value");
            if (_number == null) _number = UiBinder.FindByName<TMP_Text>(transform, "ui_text_zonecell_value");
        }
    }
}
