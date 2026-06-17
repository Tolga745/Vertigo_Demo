using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Core;

namespace VertigoWheel.UI
{
    /// <summary>"Oh no, a bomb exploded!" popup with Revive (spend cash) and Give Up actions.</summary>
    public sealed class BombPopupView : PopupView
    {
        [SerializeField] private TMP_Text _reviveCostValue; // ui_text_revive_cost_value
        [SerializeField] private Button _reviveButton;      // ui_button_revive
        [SerializeField] private Button _giveUpButton;      // ui_button_giveup

        public Button ReviveButton => _reviveButton;
        public Button GiveUpButton => _giveUpButton;

        public void SetReviveState(long cost, bool canAfford)
        {
            if (_reviveCostValue != null) _reviveCostValue.text = RewardFormatter.Abbreviate(cost);
            if (_reviveButton != null) _reviveButton.interactable = canAfford;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (_reviveCostValue == null) _reviveCostValue = UiBinder.FindByName<TMP_Text>(transform, "ui_text_revive_cost_value");
            if (_reviveButton == null) _reviveButton = UiBinder.FindByName<Button>(transform, "ui_button_revive");
            if (_giveUpButton == null) _giveUpButton = UiBinder.FindByName<Button>(transform, "ui_button_giveup");
        }
    }
}
