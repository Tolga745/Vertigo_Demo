using System.Collections.Generic;
using UnityEngine;
using VertigoWheel.Animation;
using VertigoWheel.Config;
using VertigoWheel.Core;
using VertigoWheel.Domain;
using VertigoWheel.UI;

namespace VertigoWheel.Gameplay
{
    /// <summary>
    /// MonoBehaviour coordinator. It composes the (Unity-free) <see cref="WheelGameSession"/> from the
    /// <see cref="GameConfig"/>, wires button clicks in code (no editor OnClick), drives the wheel
    /// animation, and keeps the views in sync with the session state. This is the only class that knows
    /// about both the domain and the views — everything else stays decoupled.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameConfig _config;

        [Header("Tuning")]
        [Tooltip("Seconds to admire a winning spin before the next zone loads.")]
        [SerializeField] private float _postSpinDelay = 0.7f;

        [Header("Views (auto-bound)")]
        [SerializeField] private WheelView _wheel;
        [SerializeField] private HudView _hud;
        [SerializeField] private BombPopupView _bombPopup;
        [SerializeField] private CashOutPopupView _cashOutPopup;
        [SerializeField] private ZoneBarView _zoneBar;
        [SerializeField] private RewardLogView _rewardLog;
        [SerializeField] private SpinHeaderView _spinHeader;

        [Header("Zone bar")]
        [Tooltip("How many upcoming zones the bar shows, starting from the current one.")]
        [SerializeField] private int _zoneBarWindow = 7;

        [Header("Currency icons (for reward lists)")]
        [SerializeField] private Sprite _goldIcon;
        [SerializeField] private Sprite _cashIcon;

        private WheelGameSession _session;
        private ISaveService _saveService;
        private RewardScaler _scaler;

        private void Awake()
        {
            BuildSession();
        }

        private void Start()
        {
            WireButtons();
            _session.StartNewRun();
            RefreshAll();
        }

        private void OnDestroy()
        {
            if (_session != null) _session.ZoneChanged -= OnZoneChanged;
            if (_session != null && _session.Wallet != null) _session.Wallet.Changed -= OnWalletChanged;
        }

        // ---- Composition root -------------------------------------------------

        private void BuildSession()
        {
            _saveService = new PlayerPrefsSaveService();
            var data = _saveService.Load(new WalletData(_config.StartingGold, _config.StartingCash, 0));
            var wallet = new PlayerWallet(data.Gold, data.Cash);

            var planner = new ZonePlanner(_config.SafeZoneInterval, _config.SuperZoneInterval);
            _scaler = new RewardScaler(_config.RewardGrowthPerZone, _config.SuperZoneRewardMultiplier);
            var resolver = new SpinResolver(new UnityRandomProvider());

            _session = new WheelGameSession(planner, _scaler, resolver, SelectWheel, wallet,
                _config.ReviveCashCost, _config.AllowCashOutAnytime);
            _session.ZoneChanged += OnZoneChanged;
            wallet.Changed += OnWalletChanged;
        }

        private WheelDefinition SelectWheel(ZoneInfo zone) => zone.Type switch
        {
            ZoneType.Safe => _config.SafeWheel,
            ZoneType.Super => _config.SuperWheel,
            _ => _config.NormalWheel
        };

        private void WireButtons()
        {
            // Clicks are wired here in code, never from the editor (per the brief).
            _hud.SpinButton.onClick.AddListener(OnSpinClicked);
            _hud.LeaveButton.onClick.AddListener(OnLeaveClicked);
            _bombPopup.ReviveButton.onClick.AddListener(OnReviveClicked);
            _bombPopup.GiveUpButton.onClick.AddListener(OnGiveUpClicked);
            _cashOutPopup.CollectButton.onClick.AddListener(OnCollectClicked);
        }

        // ---- Input handlers ---------------------------------------------------

        private void OnSpinClicked()
        {
            if (!_session.CanSpin) return;

            _hud.SetSpinInteractable(false);
            _hud.SetLeaveAvailable(false);
            UiTween.Punch(_hud.SpinButton.transform);

            var outcome = _session.BeginSpin();
            _wheel.SpinTo(outcome.SliceIndex, OnSpinLanded);
        }

        private void OnSpinLanded()
        {
            var result = _session.ResolveSpin();

            if (result.IsBomb)
            {
                _bombPopup.SetReviveState(_session.ReviveCashCost, _session.CanRevive);
                _bombPopup.Show();
                return;
            }

            // Non-bomb: reward already added to the pot. Log it, show it, pause, then advance.
            LogReward(result.Reward);
            UiTween.Punch(_wheel.transform);
            RefreshWallet();
            DG.Tweening.DOVirtual.DelayedCall(_postSpinDelay, () => _session.AdvanceZone());
        }

        private void OnLeaveClicked()
        {
            if (!_session.CanLeave) return;

            long banked = _session.Pot.Gold;
            var pot = _session.Pot;          // still intact after banking (cleared on the next run)
            _session.LeaveAndBank();
            PersistWallet();
            _cashOutPopup.SetAmount(banked);
            PopulateCashOutSummary(pot);
            _cashOutPopup.Show();
        }

        private void OnCollectClicked()
        {
            _cashOutPopup.Hide();
            if (_rewardLog != null) _rewardLog.Clear();
            _session.StartNewRun();
            RefreshAll();
        }

        private void OnReviveClicked()
        {
            if (!_session.Revive()) return;
            PersistWallet();
            _bombPopup.Hide();
            RefreshAll();
        }

        private void OnGiveUpClicked()
        {
            _session.GiveUp();
            _bombPopup.Hide();
            if (_rewardLog != null) _rewardLog.Clear();
            _session.StartNewRun();
            RefreshAll();
        }

        // ---- View sync --------------------------------------------------------

        private void OnZoneChanged(ZoneInfo zone)
        {
            _hud.SetZone(zone);
            _wheel.Configure(zone.Tier, BuildSliceDisplays(_session.CurrentWheel, zone));
            RefreshZoneBar();
            RefreshSpinHeader(zone);
            RefreshControls();
        }

        private void OnWalletChanged() => RefreshWallet();

        private void RefreshAll()
        {
            _hud.SetZone(_session.CurrentZone);
            _wheel.Configure(_session.CurrentZone.Tier, BuildSliceDisplays(_session.CurrentWheel, _session.CurrentZone));
            RefreshZoneBar();
            RefreshSpinHeader(_session.CurrentZone);
            RefreshWallet();
            RefreshControls();
        }

        private static readonly Color BronzeColor = new Color(0.85f, 0.55f, 0.28f);
        private static readonly Color SilverColor = new Color(0.86f, 0.88f, 0.92f);
        private static readonly Color GoldColor = new Color(1f, 0.76f, 0.24f);

        private void RefreshSpinHeader(ZoneInfo zone)
        {
            if (_spinHeader == null) return;
            switch (zone.Type)
            {
                case ZoneType.Safe:
                    _spinHeader.SetSpin("SILVER SPIN", SilverColor, "Risk-free spin — no bomb here.");
                    break;
                case ZoneType.Super:
                    _spinHeader.SetSpin("GOLDEN SPIN", GoldColor, "Jackpot rewards — and no bomb!");
                    break;
                default:
                    _spinHeader.SetSpin("BRONZE SPIN", BronzeColor, "Watch out — one slice is a bomb!");
                    break;
            }
        }

        private void RefreshZoneBar()
        {
            if (_zoneBar == null) return;

            int window = Mathf.Max(1, _zoneBarWindow);
            var cells = new List<ZoneCellData>(window);
            for (int i = 0; i < window; i++)
            {
                int index = _session.ZoneIndex + i;
                var info = _session.PeekZone(index);
                cells.Add(new ZoneCellData(index, info.Type, i == 0));
            }
            _zoneBar.SetZones(cells);
        }

        private void LogReward(GrantedReward reward)
        {
            if (_rewardLog == null || reward.Source == null) return;
            _rewardLog.AddEntry(reward.Source.Icon, FormatLogEntry(reward));
        }

        /// <summary>Fills the cash-out popup's list with everything collected this run: gold, cash and items.</summary>
        private void PopulateCashOutSummary(RewardPot pot)
        {
            var list = _cashOutPopup != null ? _cashOutPopup.Summary : null;
            if (list == null || pot == null) return;

            list.Clear();
            if (pot.Gold > 0) list.AddEntry(_goldIcon, $"+{RewardFormatter.Abbreviate(pot.Gold)} Gold");
            if (pot.Cash > 0) list.AddEntry(_cashIcon, $"+{RewardFormatter.Abbreviate(pot.Cash)} Cash");
            foreach (var item in pot.Items)
                list.AddEntry(item.Source != null ? item.Source.Icon : null, FormatLogEntry(item));
        }

        private static string FormatLogEntry(GrantedReward r) => r.Category switch
        {
            RewardCategory.Gold => $"+{RewardFormatter.Abbreviate(r.Amount)} Gold",
            RewardCategory.Cash => $"+{RewardFormatter.Abbreviate(r.Amount)} Cash",
            RewardCategory.Multiplier => $"x{r.Amount} Multiplier!",
            _ => r.Source != null ? r.Source.DisplayName : string.Empty
        };

        private void RefreshWallet()
        {
            _hud.SetWallet(_session.Wallet.Gold, _session.Wallet.Cash);
            _hud.SetPot(_session.Pot.Gold);
        }

        private void RefreshControls()
        {
            _hud.SetSpinInteractable(_session.CanSpin);
            _hud.SetLeaveAvailable(_session.CanLeave);
            _hud.SetSpinLabel(_session.CurrentZone.HasBomb ? "SPIN" : "FREE SPIN");
        }

        private IReadOnlyList<SliceDisplay> BuildSliceDisplays(WheelDefinition wheel, ZoneInfo zone)
        {
            var list = new List<SliceDisplay>(wheel.Count);
            for (int i = 0; i < wheel.Count; i++)
            {
                var slice = wheel.GetSlice(i);
                if (slice.IsBomb)
                {
                    list.Add(new SliceDisplay(slice.Reward != null ? slice.Reward.Icon : null, string.Empty, true));
                    continue;
                }

                var granted = _scaler.Resolve(slice, zone);
                list.Add(new SliceDisplay(slice.Reward != null ? slice.Reward.Icon : null, FormatSlice(granted), false));
            }
            return list;
        }

        private static string FormatSlice(GrantedReward r) => r.Category switch
        {
            RewardCategory.Multiplier => $"x{r.Amount}",
            RewardCategory.Cash => $"${RewardFormatter.Abbreviate(r.Amount)}",
            RewardCategory.Gold => RewardFormatter.Abbreviate(r.Amount),
            _ => string.Empty // weapons / chests / cosmetics are shown by icon only
        };

        private void PersistWallet()
        {
            _saveService.Save(new WalletData(_session.Wallet.Gold, _session.Wallet.Cash, _session.ZoneIndex));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_wheel == null) _wheel = GetComponentInChildren<WheelView>(true);
            if (_hud == null) _hud = GetComponentInChildren<HudView>(true);
            if (_bombPopup == null) _bombPopup = GetComponentInChildren<BombPopupView>(true);
            if (_cashOutPopup == null) _cashOutPopup = GetComponentInChildren<CashOutPopupView>(true);
            if (_zoneBar == null) _zoneBar = GetComponentInChildren<ZoneBarView>(true);
            if (_rewardLog == null) _rewardLog = GetComponentInChildren<RewardLogView>(true);
            if (_spinHeader == null) _spinHeader = GetComponentInChildren<SpinHeaderView>(true);
        }

        public void EditorAssignConfig(GameConfig config) => _config = config;

        public void EditorAssignIcons(Sprite goldIcon, Sprite cashIcon)
        {
            _goldIcon = goldIcon;
            _cashIcon = cashIcon;
        }
#endif
    }
}
