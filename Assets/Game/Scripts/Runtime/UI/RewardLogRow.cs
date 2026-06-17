using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Core;

namespace VertigoWheel.UI
{
    /// <summary>A single line in the rewards log: an icon and a label. Used as a clonable template.</summary>
    [DisallowMultipleComponent]
    public sealed class RewardLogRow : MonoBehaviour
    {
        [SerializeField] private Image _icon;   // ui_image_logrow_icon_value
        [SerializeField] private TMP_Text _label; // ui_text_logrow_value

        public void SetContent(Sprite icon, string text)
        {
            if (_icon != null)
            {
                _icon.sprite = icon;
                _icon.enabled = icon != null;
                _icon.preserveAspect = true;
            }
            if (_label != null) _label.text = text;
        }

        private void OnValidate()
        {
            if (_icon == null) _icon = UiBinder.FindByName<Image>(transform, "ui_image_logrow_icon_value");
            if (_label == null) _label = UiBinder.FindByName<TMP_Text>(transform, "ui_text_logrow_value");
        }
    }
}
