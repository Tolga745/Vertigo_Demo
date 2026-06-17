using TMPro;
using UnityEngine.UI;
using VertigoWheel.Core;

namespace VertigoWheel.UI
{
    /// <summary>Shown when the player walks away on a safe/super zone, banking the run's rewards.</summary>
    public sealed class CashOutPopupView : PopupView
    {
        [UnityEngine.SerializeField] private TMP_Text _amountValue; // ui_text_cashout_amount_value
        [UnityEngine.SerializeField] private Button _collectButton;  // ui_button_collect
        [UnityEngine.SerializeField] private RewardLogView _summary; // scroll list of everything won this run

        public Button CollectButton => _collectButton;

        /// <summary>The scrollable "everything you won" list, populated by the controller on cash out.</summary>
        public RewardLogView Summary => _summary;

        public void SetAmount(long gold)
        {
            if (_amountValue != null) _amountValue.text = RewardFormatter.Abbreviate(gold);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (_amountValue == null) _amountValue = UiBinder.FindByName<TMP_Text>(transform, "ui_text_cashout_amount_value");
            if (_collectButton == null) _collectButton = UiBinder.FindByName<Button>(transform, "ui_button_collect");
            if (_summary == null) _summary = GetComponentInChildren<RewardLogView>(true);
        }
    }
}
