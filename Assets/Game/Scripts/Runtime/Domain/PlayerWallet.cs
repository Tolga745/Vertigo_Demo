using System;

namespace VertigoWheel.Domain
{
    /// <summary>
    /// The player's persistent currency, kept separate from the per-run <see cref="RewardPot"/>.
    /// Gold/cash banked by walking away land here; cash is spent to revive after a bomb.
    /// </summary>
    public sealed class PlayerWallet
    {
        public long Gold { get; private set; }
        public long Cash { get; private set; }

        public event Action Changed;

        public PlayerWallet(long gold, long cash)
        {
            Gold = gold;
            Cash = cash;
        }

        public void Deposit(RewardPot pot)
        {
            if (pot == null) return;
            Gold += pot.Gold;
            Cash += pot.Cash;
            Changed?.Invoke();
        }

        public void AddCash(long amount)
        {
            Cash += amount;
            Changed?.Invoke();
        }

        public bool CanAfford(long cashCost) => Cash >= cashCost;

        /// <summary>Spends cash if affordable. Returns false (and changes nothing) otherwise.</summary>
        public bool TrySpendCash(long cashCost)
        {
            if (!CanAfford(cashCost)) return false;
            Cash -= cashCost;
            Changed?.Invoke();
            return true;
        }
    }
}
