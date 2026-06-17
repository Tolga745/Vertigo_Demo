using UnityEditor;
using UnityEngine;

namespace VertigoWheel.EditorTools
{
    /// <summary>
    /// Editor utility to inspect and overwrite the persisted wallet (gold/cash) that the game
    /// loads on start. Keys mirror <c>PlayerPrefsSaveService</c> so changes take effect next launch.
    /// </summary>
    public sealed class WalletDebugMenu : EditorWindow
    {
        // Must match the constants in PlayerPrefsSaveService.
        private const string GoldKey = "vw_gold";
        private const string CashKey = "vw_cash";
        private const string BestZoneKey = "vw_best_zone";

        private int _gold;
        private int _cash;
        private int _bestZone;

        [MenuItem("Vertigo/Edit Saved Wallet (Gold + Cash)", priority = 20)]
        public static void Open()
        {
            var window = GetWindow<WalletDebugMenu>(true, "Saved Wallet", true);
            window.minSize = new Vector2(320, 170);
            window.ReadFromPrefs();
            window.Show();
        }

        private void ReadFromPrefs()
        {
            _gold = PlayerPrefs.GetInt(GoldKey, 0);
            _cash = PlayerPrefs.GetInt(CashKey, 0);
            _bestZone = PlayerPrefs.GetInt(BestZoneKey, 0);
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Overwrites the saved PlayerPrefs the game loads on start. " +
                "If the game is running, stop it first — it re-saves on exit.",
                MessageType.Info);

            EditorGUILayout.Space();
            _gold = EditorGUILayout.IntField("Gold", _gold);
            _cash = EditorGUILayout.IntField("Cash", _cash);
            _bestZone = EditorGUILayout.IntField("Best Zone", _bestZone);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply"))
                {
                    PlayerPrefs.SetInt(GoldKey, _gold);
                    PlayerPrefs.SetInt(CashKey, _cash);
                    PlayerPrefs.SetInt(BestZoneKey, _bestZone);
                    PlayerPrefs.Save();
                    Debug.Log($"[Vertigo] Saved wallet set: gold={_gold}, cash={_cash}, bestZone={_bestZone}.");
                }

                if (GUILayout.Button("Reload"))
                {
                    ReadFromPrefs();
                }
            }

            if (GUILayout.Button("Clear Save (Delete Keys)"))
            {
                PlayerPrefs.DeleteKey(GoldKey);
                PlayerPrefs.DeleteKey(CashKey);
                PlayerPrefs.DeleteKey(BestZoneKey);
                PlayerPrefs.Save();
                ReadFromPrefs();
                Debug.Log("[Vertigo] Saved wallet cleared. Next launch uses GameConfig defaults.");
            }
        }
    }
}
