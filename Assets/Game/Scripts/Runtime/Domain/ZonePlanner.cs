using VertigoWheel.Config;

namespace VertigoWheel.Domain
{
    public interface IZonePlanner
    {
        ZoneInfo GetZone(int index);
    }

    /// <summary>
    /// Pure rule engine that classifies a zone from its index:
    /// every Nth zone is safe (silver), every Mth zone is super (golden, takes priority),
    /// everything else is a normal bronze zone with the bomb.
    /// </summary>
    public sealed class ZonePlanner : IZonePlanner
    {
        private readonly int _safeInterval;
        private readonly int _superInterval;

        public ZonePlanner(int safeInterval, int superInterval)
        {
            _safeInterval = safeInterval;
            _superInterval = superInterval;
        }

        public ZoneInfo GetZone(int index)
        {
            // Super takes priority over safe (e.g. zone 30 is divisible by both 5 and 30).
            if (_superInterval > 0 && index % _superInterval == 0)
                return new ZoneInfo(index, ZoneType.Super, WheelTier.Golden);

            if (_safeInterval > 0 && index % _safeInterval == 0)
                return new ZoneInfo(index, ZoneType.Safe, WheelTier.Silver);

            return new ZoneInfo(index, ZoneType.Normal, WheelTier.Bronze);
        }
    }
}
