using NUnit.Framework;
using VertigoWheel.Config;
using VertigoWheel.Domain;

namespace VertigoWheel.Tests
{
    public class WheelGameSessionTests
    {
        /// <summary>Resolver that always lands on a chosen index, for deterministic flow tests.</summary>
        private sealed class FixedResolver : ISpinResolver
        {
            public int Index;
            public SpinOutcome Resolve(WheelDefinition wheel) => new SpinOutcome(Index, wheel.GetSlice(Index));
        }

        private static WheelGameSession NewSession(FixedResolver resolver, PlayerWallet wallet,
            WheelDefinition normal, WheelDefinition safe, WheelDefinition super, long reviveCost = 25)
        {
            var planner = new ZonePlanner(5, 30);
            var scaler = new RewardScaler(0.2f, 3f);
            return new WheelGameSession(planner, scaler, resolver, zone => zone.Type switch
            {
                ZoneType.Safe => safe,
                ZoneType.Super => super,
                _ => normal
            }, wallet, reviveCost);
        }

        [Test]
        public void Winning_Spin_Adds_Gold_And_Advances_Zone()
        {
            var resolver = new FixedResolver { Index = 1 }; // slice 1 is gold (index 0 is the bomb)
            var normal = TestContent.Wheel(WheelTier.Bronze, withBomb: true, goldBase: 100);
            var session = NewSession(resolver, new PlayerWallet(0, 0), normal,
                TestContent.Wheel(WheelTier.Silver, false), TestContent.Wheel(WheelTier.Golden, false));

            session.StartNewRun();
            Assert.AreEqual(1, session.ZoneIndex);

            session.BeginSpin();
            var result = session.ResolveSpin();
            Assert.IsFalse(result.IsBomb);
            Assert.AreEqual(100, session.Pot.Gold);

            session.AdvanceZone();
            Assert.AreEqual(2, session.ZoneIndex);
        }

        [Test]
        public void Bomb_Preserves_Pot_For_Revive_Then_Clears_On_Give_Up()
        {
            var resolver = new FixedResolver { Index = 1 };
            var normal = TestContent.Wheel(WheelTier.Bronze, withBomb: true, goldBase: 100);
            var session = NewSession(resolver, new PlayerWallet(0, 100), normal,
                TestContent.Wheel(WheelTier.Silver, false), TestContent.Wheel(WheelTier.Golden, false));

            session.StartNewRun();
            session.BeginSpin();
            session.ResolveSpin();          // bank some gold first
            session.AdvanceZone();

            resolver.Index = 0;             // now land on the bomb
            session.BeginSpin();
            var bomb = session.ResolveSpin();
            Assert.IsTrue(bomb.IsBomb);
            Assert.AreEqual(GamePhase.Exploded, session.Phase);
            Assert.AreEqual(100, session.Pot.Gold, "pot must survive until revive/give-up decision");

            Assert.IsTrue(session.CanRevive);
            session.GiveUp();
            Assert.IsTrue(session.Pot.IsEmpty);
            Assert.AreEqual(GamePhase.Ended, session.Phase);
        }

        [Test]
        public void Revive_Spends_Cash_Keeps_Pot_And_Continues()
        {
            var resolver = new FixedResolver { Index = 0 }; // bomb immediately
            var normal = TestContent.Wheel(WheelTier.Bronze, withBomb: true, goldBase: 100);
            var wallet = new PlayerWallet(0, 25);
            var session = NewSession(resolver, wallet, normal,
                TestContent.Wheel(WheelTier.Silver, false), TestContent.Wheel(WheelTier.Golden, false), reviveCost: 25);

            session.StartNewRun();
            // Give the pot something by spinning a non-bomb slice first.
            resolver.Index = 1;
            session.BeginSpin();
            session.ResolveSpin();
            session.AdvanceZone();

            resolver.Index = 0;
            session.BeginSpin();
            session.ResolveSpin();          // bomb
            Assert.IsTrue(session.Revive());
            Assert.AreEqual(0, wallet.Cash);
            Assert.AreEqual(100, session.Pot.Gold);
            Assert.AreEqual(GamePhase.Ready, session.Phase);
        }

        [Test]
        public void AllowLeaveAnytime_Lets_Player_Cash_Out_On_Normal_Zone()
        {
            var resolver = new FixedResolver { Index = 1 };
            var normal = TestContent.Wheel(WheelTier.Bronze, withBomb: true, goldBase: 100);
            var planner = new ZonePlanner(5, 30);
            var scaler = new RewardScaler(0.2f, 3f);
            var session = new WheelGameSession(planner, scaler, resolver,
                zone => normal, new PlayerWallet(0, 0), 25, allowLeaveAnytime: true);

            session.StartNewRun();             // zone 1 = normal
            Assert.AreEqual(ZoneType.Normal, session.CurrentZone.Type);
            Assert.IsTrue(session.CanLeave, "cash out should be allowed on a normal zone when anytime is on");
        }

        [Test]
        public void Leaving_On_Safe_Zone_Banks_Pot_To_Wallet()
        {
            var resolver = new FixedResolver { Index = 0 };
            var wallet = new PlayerWallet(0, 0);
            var safe = TestContent.Wheel(WheelTier.Silver, withBomb: false, goldBase: 50);
            var session = NewSession(resolver, wallet,
                TestContent.Wheel(WheelTier.Bronze, true), safe, TestContent.Wheel(WheelTier.Golden, false));

            // Drive to zone 5 (safe). Land on a non-bomb slice each time.
            session.StartNewRun();
            resolver.Index = 1;
            for (int i = 0; i < 4; i++)
            {
                session.BeginSpin();
                session.ResolveSpin();
                session.AdvanceZone();
            }

            Assert.AreEqual(5, session.ZoneIndex);
            Assert.IsTrue(session.CanLeave, "safe zone while idle should allow leaving");

            long expected = session.Pot.Gold;
            session.LeaveAndBank();
            Assert.AreEqual(expected, wallet.Gold);
            Assert.AreEqual(GamePhase.Ended, session.Phase);
        }
    }
}
