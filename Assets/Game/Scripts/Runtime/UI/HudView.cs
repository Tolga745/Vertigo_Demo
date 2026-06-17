using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Core;
using VertigoWheel.Domain;

namespace VertigoWheel.UI
{
    /// <summary>
    /// Top/bottom HUD: zone label, wallet readouts and the spin / cash-out buttons. Exposes the buttons
    /// so the coordinator can wire onClick in code (the brief forbids editor OnClick wiring). All child
    /// references are auto-bound in <see cref="OnValidate"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _zoneValue;  // ui_text_zone_value
        [SerializeField] private TMP_Text _goldValue;  // ui_text_gold_value
        [SerializeField] private TMP_Text _cashValue;  // ui_text_cash_value
        [SerializeField] private TMP_Text _potValue;   // ui_text_pot_value (collected this run)

        [SerializeField] private Button _spinButton;   // ui_button_spin
        [SerializeField] private TMP_Text _spinLabel;  // ui_text_spin_value
        [SerializeField] private Button _leaveButton;  // ui_button_leave

        public Button SpinButton => _spinButton;
        public Button LeaveButton => _leaveButton;

        public void SetZone(ZoneInfo zone)
        {
            if (_zoneValue == null) return;
            _zoneValue.text = zone.Type switch
            {
                ZoneType.Safe => $"SAFE ZONE {zone.Index}",
                ZoneType.Super => $"SUPER ZONE {zone.Index}",
                _ => $"ZONE {zone.Index}"
            };
        }

        public void SetWallet(long gold, long cash)
        {
            if (_goldValue != null) _goldValue.text = RewardFormatter.Abbreviate(gold);
            if (_cashValue != null) _cashValue.text = RewardFormatter.Abbreviate(cash);
        }

        public void SetPot(long gold)
        {
            if (_potValue != null) _potValue.text = RewardFormatter.Abbreviate(gold);
        }

        public void SetSpinInteractable(bool value)
        {
            if (_spinButton != null) _spinButton.interactable = value;
        }

        public void SetLeaveAvailable(bool value)
        {
            if (_leaveButton != null) _leaveButton.gameObject.SetActive(value);
        }

        public void SetSpinLabel(string text)
        {
            if (_spinLabel != null) _spinLabel.text = text;
        }

        private void OnValidate()
        {
            if (_zoneValue == null) _zoneValue = UiBinder.FindByName<TMP_Text>(transform, "ui_text_zone_value");
            if (_goldValue == null) _goldValue = UiBinder.FindByName<TMP_Text>(transform, "ui_text_gold_value");
            if (_cashValue == null) _cashValue = UiBinder.FindByName<TMP_Text>(transform, "ui_text_cash_value");
            if (_potValue == null) _potValue = UiBinder.FindByName<TMP_Text>(transform, "ui_text_pot_value");
            if (_spinButton == null) _spinButton = UiBinder.FindByName<Button>(transform, "ui_button_spin");
            if (_spinLabel == null) _spinLabel = UiBinder.FindByName<TMP_Text>(transform, "ui_text_spin_value");
            if (_leaveButton == null) _leaveButton = UiBinder.FindByName<Button>(transform, "ui_button_leave");
        }
    }
}
