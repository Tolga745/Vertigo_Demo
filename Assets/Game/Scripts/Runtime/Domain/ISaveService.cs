namespace VertigoWheel.Domain
{
    /// <summary>Persisted profile values that survive between sessions.</summary>
    public readonly struct WalletData
    {
        public readonly long Gold;
        public readonly long Cash;
        public readonly int BestZone;

        public WalletData(long gold, long cash, int bestZone)
        {
            Gold = gold;
            Cash = cash;
            BestZone = bestZone;
        }
    }

    /// <summary>
    /// Abstraction over persistence so the game logic never touches PlayerPrefs directly
    /// (Dependency Inversion). Swap in a cloud/file backend without changing callers.
    /// </summary>
    public interface ISaveService
    {
        WalletData Load(WalletData defaults);
        void Save(WalletData data);
    }
}
