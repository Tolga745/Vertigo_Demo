using System.Collections.Generic;
using VertigoWheel.Config;

namespace VertigoWheel.Domain
{
    /// <summary>
    /// The rewards a player has accumulated during the current run ("until that point"). A bomb clears
    /// it; walking away banks it. Gold is the running score that multiplier slices act on; items
    /// (weapons, chests, cosmetics, consumables) are collected individually.
    /// </summary>
    public sealed class RewardPot
    {
        private readonly List<GrantedReward> _items = new List<GrantedReward>();

        public long Gold { get; private set; }
        public long Cash { get; private set; }
        public IReadOnlyList<GrantedReward> Items => _items;
        public bool IsEmpty => Gold == 0 && Cash == 0 && _items.Count == 0;

        /// <summary>Applies a resolved reward to the pot. Multipliers scale the gold collected so far.</summary>
        public void Apply(GrantedReward reward)
        {
            switch (reward.Category)
            {
                case RewardCategory.Gold:
                    Gold += reward.Amount;
                    break;
                case RewardCategory.Cash:
                    Cash += reward.Amount;
                    break;
                case RewardCategory.Multiplier:
                    if (reward.Amount > 0) Gold *= reward.Amount;
                    break;
                case RewardCategory.Bomb:
                    // Bomb is handled by the game controller (clears the pot); nothing to add.
                    break;
                default:
                    _items.Add(reward); // weapon / consumable / chest / cosmetic
                    break;
            }
        }

        public void Clear()
        {
            Gold = 0;
            Cash = 0;
            _items.Clear();
        }
    }
}
