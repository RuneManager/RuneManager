using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneOptim.swar;

namespace RuneOptim.Tests {
    [TestClass()]
    public class StatsTests
    {
        [TestMethod()]
        public void StatsTest()
        {
            var stat1 = TestData.statsFull();
            var stat2 = new Stats(stat1);
            Assert.AreEqual(stat1.Health, stat2.Health);
            Assert.AreEqual(stat1.Attack, stat2.Attack);
            Assert.AreEqual(stat1.Defense, stat2.Defense);
            Assert.AreEqual(stat1.Speed, stat2.Speed);

            Assert.AreEqual(stat1.CritDamage, stat2.CritDamage);
            Assert.AreEqual(stat1.CritRate, stat2.CritRate);
            Assert.AreEqual(stat1.Accuracy, stat2.Accuracy);
            Assert.AreEqual(stat1.Resistance, stat2.Resistance);

            Assert.AreEqual(stat1.EffectiveHP, stat2.EffectiveHP);
            Assert.AreEqual(stat1.EffectiveHPDefenseBreak, stat2.EffectiveHPDefenseBreak);
            Assert.AreEqual(stat1.MaxDamage, stat2.MaxDamage);
            Assert.AreEqual(stat1.AverageDamage, stat2.AverageDamage);
            Assert.AreEqual(stat1.DamagePerSpeed, stat2.DamagePerSpeed);
        }
        
        [TestMethod()]
        public void SetZeroTest()
        {
            var stat1 = TestData.statsFull();
            var stat2 = new Stats();
            stat1.SetTo(0);
            Assert.AreEqual(stat2, stat2);
        }

        [TestMethod()]
        public void SumTest()
        {
            var stat = TestData.statsBase();
            Assert.AreEqual(1365, stat.Sum());
        }

        [TestMethod()]
        public void GreaterEqualTest()
        {
            var stat1 = TestData.statsBase();
            var stat2 = TestData.statsFull();
            Assert.IsTrue(stat2.GreaterEqual(stat1));
        }

        [TestMethod()]
        public void DivisionTest()
        {
            var stat1 = TestData.statsBase();
            var stat2 = new Stats()
            {
                Attack = 200,
                CritDamage = 65,
                Defense = 300,
                Resistance = 10,
            };

            var stat3 = stat2 / stat1;

            Assert.AreEqual(0, stat3.Health);
            Assert.AreEqual(2, stat3.Attack);
            Assert.AreEqual(3, stat3.Defense);
            Assert.AreEqual(0, stat3.Speed);
            Assert.AreEqual(0, stat3.CritRate);
            Assert.AreEqual(1.3, stat3.CritDamage);
            Assert.AreEqual(0, stat3.Resistance);
            Assert.AreEqual(0, stat3.Accuracy);

            Assert.AreEqual(0, stat3.EffectiveHP);
            Assert.AreEqual(0, stat3.EffectiveHPDefenseBreak);
            Assert.AreEqual(0, stat3.DamagePerSpeed);
            Assert.AreEqual(0, stat3.AverageDamage);
            Assert.AreEqual(0, stat3.MaxDamage);
        }

        [TestMethod()]
        public void OfTest()
        {
            var stat1 = TestData.statsBase();
            var stat2 = TestData.statsFull();
            Assert.AreEqual(3.02, stat2.Of(stat1).Health);
            Assert.AreEqual(2.22, stat2.Of(stat1).Attack);
            Assert.AreEqual(2.34, stat2.Of(stat1).Defense);
        }

        [TestMethod()]
        public void BoostTest()
        {
            var stat1 = TestData.statsBase().Boost(new Stats()
            {
                Attack = 40,
                Health = 30,
                Defense = 20,
                Speed = 10,

                CritDamage = 40,
                CritRate = 30,
                Accuracy = 20,
                Resistance = 10,
            });
            Assert.AreEqual(140, (int)stat1.Attack);
            Assert.AreEqual(1300, (int)stat1.Health);
            Assert.AreEqual(120, (int)stat1.Defense);
            Assert.AreEqual(110, (int)stat1.Speed);

            Assert.AreEqual(90, (int)stat1.CritDamage);
            Assert.AreEqual(45, (int)stat1.CritRate);
            Assert.AreEqual(20, (int)stat1.Accuracy);
            Assert.AreEqual(10, (int)stat1.Resistance);
        }

        [TestMethod()]
        public void NonZeroTest()
        {
            var stats = new Stats();
            Assert.IsFalse(stats.IsNonZero);
        }

        [TestMethod()]
        public void FirstNonZeroTest()
        {
            var stat = new Stats();
            Assert.AreEqual(Attr.Null, stat.NonZeroStats.FirstOrDefault());
        }
    }
}