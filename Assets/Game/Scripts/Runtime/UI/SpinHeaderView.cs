using TMPro;
using UnityEngine;
using VertigoWheel.Core;

namespace VertigoWheel.UI
{
    /// <summary>
    /// Banner above the wheel naming the current spin (BRONZE / SILVER / GOLDEN) and describing what is
    /// special about it. Child refs auto-bind in <see cref="OnValidate"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SpinHeaderView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title; // ui_text_spintype_value
        [SerializeField] private TMP_Text _info;  // ui_text_spininfo_value

        public void SetSpin(string title, Color titleColor, string info)
        {
            if (_title != null)
            {
                _title.text = title;
                _title.color = titleColor;
            }
            if (_info != null) _info.text = info;
        }

        private void OnValidate()
        {
            if (_title == null) _title = UiBinder.FindByName<TMP_Text>(transform, "ui_text_spintype_value");
            if (_info == null) _info = UiBinder.FindByName<TMP_Text>(transform, "ui_text_spininfo_value");
        }
    }
}
