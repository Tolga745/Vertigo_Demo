using NUnit.Framework;
using VertigoWheel.Config;
using VertigoWheel.Core;
using VertigoWheel.Domain;

namespace VertigoWheel.Tests
{
    public class ZonePlannerTests
    {
        private readonly ZonePlanner _planner = new ZonePlanner(5, 30);

        [TestCase(1, ZoneType.Normal)]
        [TestCase(4, ZoneType.Normal)]
        [TestCase(5, ZoneType.Safe)]
        [TestCase(10, ZoneType.Safe)]
        [TestCase(25, ZoneType.Safe)]
        [TestCase(30, ZoneType.Super)]   // divisible by both 5 and 30 -> super wins
        [TestCase(60, ZoneType.Super)]
        public void Classifies_Zone(int index, ZoneType expected)
            => Assert.AreEqual(expected, _planner.GetZone(index).Type);

        [Test]
        public void Normal_Zone_Has_Bomb_And_Cannot_Leave()
        {
            var z = _planner.GetZone(3);
            Assert.IsTrue(z.HasBomb);
            Assert.IsFalse(z.CanLeave);
        }

        [Test]
        public void Safe_And_Super_Zones_Are_Riskfree()
        {
            Assert.IsFalse(_planner.GetZone(5).HasBomb);
            Assert.IsTrue(_planner.GetZone(5).CanLeave);
            Assert.IsTrue(_planner.GetZone(30).CanLeave);
        }
    }

    public class RewardScalerTests
    {
        [Test]
        public void Scales_Linearly_With_Zone()
        {
            var scaler = new RewardScaler(0.20f, 3f);
            var slice = new WheelSlice(TestContent.Reward(RewardCategory.Gold, 100));
            var zone1 = new ZoneInfo(1, ZoneType.Normal, WheelTier.Bronze);
            var zone6 = new ZoneInfo(6, ZoneType.Normal, WheelTier.Bronze);

            Assert.AreEqual(100, scaler.Resolve(slice, zone1).Amount);
            Assert.AreEqual(200, scaler.Resolve(slice, zone6).Amount); // 100 * (1 + 0.2*5)
        }

        [Test]
        public void Super_Zone_Applies_Extra_Multiplier()
        {
            var scaler = new RewardScaler(0f, 3f);
            var slice = new WheelSlice(TestContent.Reward(RewardCategory.Gold, 100));
            var super = new ZoneInfo(30, ZoneType.Super, WheelTier.Golden);
            Assert.AreEqual(300, scaler.Resolve(slice, super).Amount);
        }

        [Test]
        public void Fixed_Amount_Rewards_Ignore_Scaling()
        {
            var scaler = new RewardScaler(0.5f, 3f);
            var slice = new WheelSlice(TestContent.Reward(RewardCategory.Multiplier, 10, fixedAmount: true));
            var zone10 = new ZoneInfo(10, ZoneType.Safe, WheelTier.Silver);
            Assert.AreEqual(10, scaler.Resolve(slice, zone10).Amount);
        }

        [Test]
        public void Bomb_Resolves_To_Bomb_Category()
        {
            var scaler = new RewardScaler(0.2f, 3f);
            var slice = new WheelSlice(TestContent.Bomb());
            var zone = new ZoneInfo(2, ZoneType.Normal, WheelTier.Bronze);
            Assert.IsTrue(scaler.Resolve(slice, zone).IsBomb);
        }
    }

    public class RewardPotTests
    {
        [Test]
        public void Multiplier_Multiplies_Collected_Gold()
        {
            var pot = new RewardPot();
            pot.Apply(new GrantedReward(null, RewardCategory.Gold, 500));
            pot.Apply(new GrantedReward(null, RewardCategory.Multiplier, 3));
            Assert.AreEqual(1500, pot.Gold);
        }

        [Test]
        public void Clear_Empties_Pot()
        {
            var pot = new RewardPot();
            pot.Apply(new GrantedReward(null, RewardCategory.Gold, 500));
            pot.Clear();
            Assert.IsTrue(pot.IsEmpty);
        }
    }

    public class PlayerWalletTests
    {
        [Test]
        public void Cannot_Spend_More_Than_Balance()
        {
            var wallet = new PlayerWallet(0, 10);
            Assert.IsFalse(wallet.TrySpendCash(25));
            Assert.AreEqual(10, wallet.Cash);
        }

        [Test]
        public void Deposit_Banks_The_Pot()
        {
            var wallet = new PlayerWallet(0, 0);
            var pot = new RewardPot();
            pot.Apply(new GrantedReward(null, RewardCategory.Gold, 750));
            wallet.Deposit(pot);
            Assert.AreEqual(750, wallet.Gold);
        }
    }
}
