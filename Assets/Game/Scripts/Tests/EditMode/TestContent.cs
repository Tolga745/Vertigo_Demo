using System.Collections.Generic;
using UnityEngine;
using VertigoWheel.Config;

namespace VertigoWheel.Tests
{
    /// <summary>Helpers to build lightweight reward/wheel assets in memory for tests.</summary>
    internal static class TestContent
    {
        public static RewardDefinition Reward(RewardCategory category, long baseAmount, bool fixedAmount = false)
        {
            var r = ScriptableObject.CreateInstance<RewardDefinition>();
            r.EditorInitialize($"r_{category}_{baseAmount}", category.ToString(), category, null, baseAmount, fixedAmount);
            return r;
        }

        public static RewardDefinition Bomb()
        {
            var r = ScriptableObject.CreateInstance<RewardDefinition>();
            r.EditorInitialize("r_bomb", "Bomb", RewardCategory.Bomb, null, 0, true);
            return r;
        }

        /// <summary>Builds an 8-slice wheel; index 0 is the bomb when <paramref name="withBomb"/> is true.</summary>
        public static WheelDefinition Wheel(WheelTier tier, bool withBomb, long goldBase = 100)
        {
            var slices = new List<WheelSlice>();
            for (int i = 0; i < WheelDefinition.SliceCount; i++)
            {
                if (i == 0 && withBomb)
                    slices.Add(new WheelSlice(Bomb()));
                else
                    slices.Add(new WheelSlice(Reward(RewardCategory.Gold, goldBase)));
            }

            var w = ScriptableObject.CreateInstance<WheelDefinition>();
            w.EditorInitialize(tier, slices);
            return w;
        }
    }
}
