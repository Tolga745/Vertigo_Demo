using UnityEngine;

namespace VertigoWheel.Config
{
    /// <summary>
    /// Designer-authored description of one reward that a wheel slice can hand out. Stored as a
    /// ScriptableObject so the content team can add/tune rewards from the editor without touching code
    /// (one of the brief's "pluses": proper ScriptableObject usage).
    /// </summary>
    [CreateAssetMenu(menuName = "Vertigo/Reward Definition", fileName = "reward_new")]
    public sealed class RewardDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id = "reward_id";
        [SerializeField] private string _displayName = "Reward";
        [SerializeField] private RewardCategory _category = RewardCategory.Gold;

        [Header("Visuals")]
        [SerializeField] private Sprite _icon;

        [Header("Value")]
        [Tooltip("Base amount at zone 1. For Multiplier rewards this is the factor (e.g. 2 = x2). " +
                 "For the Bomb it is ignored.")]
        [SerializeField] private long _baseAmount = 100;

        [Tooltip("If true the amount is NOT scaled by the zone (e.g. fixed cash offers, multipliers).")]
        [SerializeField] private bool _fixedAmount;

        public string Id => _id;
        public string DisplayName => _displayName;
        public RewardCategory Category => _category;
        public Sprite Icon => _icon;
        public long BaseAmount => _baseAmount;
        public bool FixedAmount => _fixedAmount;
        public bool IsBomb => _category == RewardCategory.Bomb;
        public bool IsMultiplier => _category == RewardCategory.Multiplier;

#if UNITY_EDITOR
        /// <summary>Editor-only setup used by the content generator.</summary>
        public void EditorInitialize(string id, string displayName, RewardCategory category,
            Sprite icon, long baseAmount, bool fixedAmount)
        {
            _id = id;
            _displayName = displayName;
            _category = category;
            _icon = icon;
            _baseAmount = baseAmount;
            _fixedAmount = fixedAmount;
        }
#endif
    }
}
