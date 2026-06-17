using UnityEngine;

namespace VertigoWheel.Domain
{
    /// <summary>PlayerPrefs-backed implementation of <see cref="ISaveService"/>.</summary>
    public sealed class PlayerPrefsSaveService : ISaveService
    {
        private const string GoldKey = "vw_gold";
        private const string CashKey = "vw_cash";
        private const string BestZoneKey = "vw_best_zone";

        public WalletData Load(WalletData defaults)
        {
            long gold = (long)PlayerPrefs.GetInt(GoldKey, (int)defaults.Gold);
            long cash = (long)PlayerPrefs.GetInt(CashKey, (int)defaults.Cash);
            int bestZone = PlayerPrefs.GetInt(BestZoneKey, defaults.BestZone);
            return new WalletData(gold, cash, bestZone);
        }

        public void Save(WalletData data)
        {
            PlayerPrefs.SetInt(GoldKey, (int)data.Gold);
            PlayerPrefs.SetInt(CashKey, (int)data.Cash);
            PlayerPrefs.SetInt(BestZoneKey, data.BestZone);
            PlayerPrefs.Save();
        }
    }
}
