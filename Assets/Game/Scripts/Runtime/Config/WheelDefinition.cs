using System;
using System.Collections.Generic;
using UnityEngine;

namespace VertigoWheel.Config
{
    /// <summary>One physical chamber on the wheel: a reward plus an optional per-slice amount override.</summary>
    [Serializable]
    public sealed class WheelSlice
    {
        [SerializeField] private RewardDefinition _reward;

        [Tooltip("0 = use the reward's own base amount. Otherwise overrides it for this slice only.")]
        [SerializeField] private long _amountOverride;

        public RewardDefinition Reward => _reward;
        public long AmountOverride => _amountOverride;
        public bool IsBomb => _reward != null && _reward.IsBomb;

        public WheelSlice() { }

        public WheelSlice(RewardDefinition reward, long amountOverride = 0)
        {
            _reward = reward;
            _amountOverride = amountOverride;
        }

        /// <summary>The amount to use before zone scaling (override if set, otherwise the reward base).</summary>
        public long ResolveBaseAmount()
            => _amountOverride != 0 ? _amountOverride : (_reward != null ? _reward.BaseAmount : 0);
    }

    /// <summary>
    /// A complete wheel layout. The "content of slices of each wheel should be changeable from the
    /// editor" requirement is satisfied here: a designer edits this asset's slice list.
    /// </summary>
    [CreateAssetMenu(menuName = "Vertigo/Wheel Definition", fileName = "wheel_new")]
    public sealed class WheelDefinition : ScriptableObject
    {
        public const int SliceCount = 8;

        [SerializeField] private WheelTier _tier = WheelTier.Bronze;

        [Tooltip("Exactly 8 slices, ordered clockwise starting from the top (under the indicator).")]
        [SerializeField] private List<WheelSlice> _slices = new List<WheelSlice>();

        public WheelTier Tier => _tier;
        public IReadOnlyList<WheelSlice> Slices => _slices;
        public int Count => _slices.Count;

        public WheelSlice GetSlice(int index) => _slices[index];

        /// <summary>True when at least one slice is a bomb (normal/bronze wheels only).</summary>
        public bool ContainsBomb
        {
            get
            {
                for (int i = 0; i < _slices.Count; i++)
                    if (_slices[i] != null && _slices[i].IsBomb) return true;
                return false;
            }
        }

#if UNITY_EDITOR
        public void EditorInitialize(WheelTier tier, List<WheelSlice> slices)
        {
            _tier = tier;
            _slices = slices;
        }
#endif
    }
}
