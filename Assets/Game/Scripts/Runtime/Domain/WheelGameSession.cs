using System;
using VertigoWheel.Config;

namespace VertigoWheel.Domain
{
    public enum GamePhase
    {
        Ready,    // wheel idle, player may spin (and may leave if zone is safe/super)
        Spinning, // outcome decided, waiting for the wheel animation to finish
        Exploded, // bomb hit; pot preserved so the player can still revive
        Ended     // run finished (cashed out or gave up)
    }

    /// <summary>Outcome of resolving a spin once the animation lands.</summary>
    public readonly struct SpinResult
    {
        public readonly int SliceIndex;
        public readonly bool IsBomb;
        public readonly GrantedReward Reward;

        public SpinResult(int sliceIndex, bool isBomb, GrantedReward reward)
        {
            SliceIndex = sliceIndex;
            IsBomb = isBomb;
            Reward = reward;
        }
    }

    /// <summary>
    /// Pure (Unity-free) state machine for a single play-through. It owns the rules of the loop —
    /// spin, scale, bomb, advance, leave, revive — and exposes a small command surface that the
    /// MonoBehaviour controller drives. Keeping this engine-agnostic makes the core fully unit testable.
    /// </summary>
    public sealed class WheelGameSession
    {
        private readonly IZonePlanner _planner;
        private readonly IRewardScaler _scaler;
        private readonly ISpinResolver _resolver;
        private readonly Func<ZoneInfo, WheelDefinition> _wheelSelector;
        private readonly PlayerWallet _wallet;
        private readonly long _reviveCashCost;
        private readonly bool _allowLeaveAnytime;

        private SpinOutcome _pending;

        public GamePhase Phase { get; private set; } = GamePhase.Ended;
        public int ZoneIndex { get; private set; } = 1;
        public ZoneInfo CurrentZone { get; private set; }
        public RewardPot Pot { get; } = new RewardPot();
        public PlayerWallet Wallet => _wallet;
        public long ReviveCashCost => _reviveCashCost;

        public WheelDefinition CurrentWheel => _wheelSelector(CurrentZone);
        public bool CanSpin => Phase == GamePhase.Ready;
        public bool CanLeave => Phase == GamePhase.Ready && (_allowLeaveAnytime || CurrentZone.CanLeave);
        public bool CanRevive => Phase == GamePhase.Exploded && _wallet.CanAfford(_reviveCashCost);

        public event Action<ZoneInfo> ZoneChanged;

        public WheelGameSession(
            IZonePlanner planner,
            IRewardScaler scaler,
            ISpinResolver resolver,
            Func<ZoneInfo, WheelDefinition> wheelSelector,
            PlayerWallet wallet,
            long reviveCashCost,
            bool allowLeaveAnytime = false)
        {
            _planner = planner;
            _scaler = scaler;
            _resolver = resolver;
            _wheelSelector = wheelSelector;
            _wallet = wallet;
            _reviveCashCost = reviveCashCost;
            _allowLeaveAnytime = allowLeaveAnytime;
        }

        /// <summary>Classifies any zone by index without changing state (used to preview upcoming zones).</summary>
        public ZoneInfo PeekZone(int index) => _planner.GetZone(index);

        public void StartNewRun()
        {
            Pot.Clear();
            ZoneIndex = 1;
            EnterZone(ZoneIndex);
        }

        /// <summary>Decides where the wheel will stop. Call once per spin, then animate to the index.</summary>
        public SpinOutcome BeginSpin()
        {
            if (!CanSpin)
                throw new InvalidOperationException($"Cannot spin while phase is {Phase}.");

            _pending = _resolver.Resolve(CurrentWheel);
            Phase = GamePhase.Spinning;
            return _pending;
        }

        /// <summary>Applies the pending spin once the animation lands. Bomb preserves the pot for revive.</summary>
        public SpinResult ResolveSpin()
        {
            if (Phase != GamePhase.Spinning)
                throw new InvalidOperationException($"No spin in progress (phase {Phase}).");

            if (_pending.IsBomb)
            {
                Phase = GamePhase.Exploded; // pot kept intact until the player gives up or revives
                return new SpinResult(_pending.SliceIndex, true, new GrantedReward(_pending.Slice.Reward, RewardCategory.Bomb, 0));
            }

            var reward = _scaler.Resolve(_pending.Slice, CurrentZone);
            Pot.Apply(reward);
            Phase = GamePhase.Ready; // reward banked into pot; controller decides when to advance
            return new SpinResult(_pending.SliceIndex, false, reward);
        }

        /// <summary>Moves to the next zone after a successful (non-bomb) spin.</summary>
        public void AdvanceZone()
        {
            if (Phase != GamePhase.Ready)
                throw new InvalidOperationException($"Can only advance from Ready (phase {Phase}).");
            EnterZone(ZoneIndex + 1);
        }

        /// <summary>Walk away with everything collected. Allowed only on a risk-free zone when idle.</summary>
        public RewardPot LeaveAndBank()
        {
            if (!CanLeave)
                throw new InvalidOperationException("Leaving is not allowed right now (must be idle, and on a safe/super zone unless 'allow anytime' is enabled).");

            _wallet.Deposit(Pot);
            Phase = GamePhase.Ended;
            return Pot;
        }

        /// <summary>Spend cash to survive the bomb and keep the pot, continuing into the next zone.</summary>
        public bool Revive()
        {
            if (Phase != GamePhase.Exploded) return false;
            if (!_wallet.TrySpendCash(_reviveCashCost)) return false;

            Phase = GamePhase.Ready;     // pot is still intact
            EnterZone(ZoneIndex + 1);
            return true;
        }

        /// <summary>Accept the loss: clear the pot and end the run.</summary>
        public void GiveUp()
        {
            Pot.Clear();
            Phase = GamePhase.Ended;
        }

        private void EnterZone(int index)
        {
            ZoneIndex = index;
            CurrentZone = _planner.GetZone(index);
            Phase = GamePhase.Ready;
            ZoneChanged?.Invoke(CurrentZone);
        }
    }
}
