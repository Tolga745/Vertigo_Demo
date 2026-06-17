using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Core;

namespace VertigoWheel.UI
{
    /// <summary>
    /// One chamber of the wheel: an icon and an amount label. Both child references are resolved
    /// automatically in <see cref="OnValidate"/> by name, so nothing is wired by hand in the editor.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SliceView : MonoBehaviour
    {
        [SerializeField] private Image _icon;     // child: ui_image_slice_icon_value
        [SerializeField] private TMP_Text _amount; // child: ui_text_slice_amount_value

        public void SetContent(Sprite icon, string amountText, bool isBomb)
        {
            if (_icon != null)
            {
                _icon.sprite = icon;
                _icon.enabled = icon != null;
                _icon.preserveAspect = true;
            }

            if (_amount != null)
            {
                _amount.text = isBomb ? string.Empty : amountText;
                _amount.enabled = !isBomb && !string.IsNullOrEmpty(amountText);
            }
        }

        private void OnValidate()
        {
            if (_icon == null) _icon = UiBinder.FindByName<Image>(transform, "ui_image_slice_icon_value");
            if (_amount == null) _amount = UiBinder.FindByName<TMP_Text>(transform, "ui_text_slice_amount_value");
        }
    }
}
