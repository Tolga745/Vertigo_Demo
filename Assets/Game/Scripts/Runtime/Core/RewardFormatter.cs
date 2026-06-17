using System.Globalization;

namespace VertigoWheel.Core
{
    /// <summary>Presentation-neutral number/reward formatting shared by the wheel, HUD and popups.</summary>
    public static class RewardFormatter
    {
        /// <summary>Compact money/score formatting: 950 -> "950", 1500 -> "1.5K", 2_000_000 -> "2M".</summary>
        public static string Abbreviate(long value)
        {
            if (value < 0) return "-" + Abbreviate(-value);
            if (value < 1000) return value.ToString(CultureInfo.InvariantCulture);
            if (value < 1_000_000) return Trim(value / 1000.0) + "K";
            if (value < 1_000_000_000) return Trim(value / 1_000_000.0) + "M";
            return Trim(value / 1_000_000_000.0) + "B";
        }

        private static string Trim(double v)
        {
            // One decimal place, but drop a trailing ".0" (1.0K -> 1K).
            string s = v.ToString("0.0", CultureInfo.InvariantCulture);
            return s.EndsWith(".0") ? s.Substring(0, s.Length - 2) : s;
        }
    }
}
