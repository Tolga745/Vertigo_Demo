using VertigoWheel.Domain;

namespace VertigoWheel.UI
{
    /// <summary>One cell of the zone bar: which zone it is, its type, and whether it is the current zone.</summary>
    public readonly struct ZoneCellData
    {
        public readonly int Index;
        public readonly ZoneType Type;
        public readonly bool IsCurrent;

        public ZoneCellData(int index, ZoneType type, bool isCurrent)
        {
            Index = index;
            Type = type;
            IsCurrent = isCurrent;
        }
    }
}
