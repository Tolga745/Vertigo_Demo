using VertigoWheel.Config;

namespace VertigoWheel.Domain
{
    /// <summary>
    /// A concrete reward instance after zone scaling has been applied — i.e. exactly what the player
    /// receives from a resolved spin. Decoupled from the authoring data (<see cref="RewardDefinition"/>)
    /// so the pot and UI deal in final values.
    /// </summary>
    public readonly struct GrantedReward
    {
        public readonly RewardDefinition Source;
        public readonly RewardCategory Category;

        /// <summary>Final amount, or for a multiplier the factor (e.g. 2 = x2).</summary>
        public readonly long Amount;

        public GrantedReward(RewardDefinition source, RewardCategory category, long amount)
        {
            Source = source;
            Category = category;
            Amount = amount;
        }

        public bool IsBomb => Category == RewardCategory.Bomb;
        public bool IsMultiplier => Category == RewardCategory.Multiplier;
    }
}
