namespace VertigoWheel.Config
{
    /// <summary>Visual / risk tier of a wheel. Drives which art set the view shows.</summary>
    public enum WheelTier
    {
        Bronze, // normal zones (contains the bomb)
        Silver, // safe zones  (every 5th zone, no bomb)
        Golden  // super zones (every 30th zone, special rewards, no bomb)
    }

    /// <summary>
    /// What a single slice gives. <see cref="Bomb"/> is the only non-reward kind and wipes the run pot.
    /// Everything else is a reward whose magnitude is scaled by the current zone.
    /// </summary>
    public enum RewardCategory
    {
        Gold,
        Cash,
        Multiplier, // multiplies the gold already collected this run (e.g. x2, x10)
        Weapon,
        Consumable,
        Chest,
        Cosmetic,
        Bomb
    }
}
