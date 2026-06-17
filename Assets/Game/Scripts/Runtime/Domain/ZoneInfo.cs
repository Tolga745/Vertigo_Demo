using VertigoWheel.Config;

namespace VertigoWheel.Domain
{
    public enum ZoneType
    {
        Normal, // bronze, contains a bomb
        Safe,   // silver, risk-free
        Super   // golden, risk-free + special rewards
    }

    /// <summary>Immutable description of a single zone, derived purely from its 1-based index.</summary>
    public readonly struct ZoneInfo
    {
        public readonly int Index;
        public readonly ZoneType Type;
        public readonly WheelTier Tier;

        public ZoneInfo(int index, ZoneType type, WheelTier tier)
        {
            Index = index;
            Type = type;
            Tier = tier;
        }

        /// <summary>Only normal zones carry the bomb.</summary>
        public bool HasBomb => Type == ZoneType.Normal;

        /// <summary>The player may walk away only on risk-free (safe/super) zones.</summary>
        public bool CanLeave => Type != ZoneType.Normal;
    }
}
