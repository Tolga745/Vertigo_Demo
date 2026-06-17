using System;
using VertigoWheel.Config;

namespace VertigoWheel.Domain
{
    public interface IRewardScaler
    {
        /// <summary>Turns a slice into a concrete reward, scaling the amount for the given zone.</summary>
        GrantedReward Resolve(WheelSlice slice, ZoneInfo zone);
    }

    /// <summary>
    /// Applies the "rewards get better every zone" rule. Linear growth per zone, with an extra
    /// multiplier on super zones. Fixed-amount rewards (cash offers, multipliers) bypass scaling.
    /// </summary>
    public sealed class RewardScaler : IRewardScaler
    {
        private readonly float _growthPerZone;
        private readonly float _superMultiplier;

        public RewardScaler(float growthPerZone, float superMultiplier)
        {
            _growthPerZone = growthPerZone;
            _superMultiplier = superMultiplier;
        }

        public GrantedReward Resolve(WheelSlice slice, ZoneInfo zone)
        {
            var reward = slice.Reward;
            if (reward == null)
                return new GrantedReward(null, RewardCategory.Gold, 0);

            if (reward.IsBomb)
                return new GrantedReward(reward, RewardCategory.Bomb, 0);

            long baseAmount = slice.ResolveBaseAmount();

            if (reward.FixedAmount)
                return new GrantedReward(reward, reward.Category, baseAmount);

            double factor = 1.0 + _growthPerZone * (zone.Index - 1);
            if (zone.Type == ZoneType.Super)
                factor *= _superMultiplier;

            long scaled = (long)Math.Round(baseAmount * factor, MidpointRounding.AwayFromZero);
            return new GrantedReward(reward, reward.Category, scaled);
        }
    }
}
