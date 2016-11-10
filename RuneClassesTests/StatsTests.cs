using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneOptim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuneOptim.Tests
{
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
            stat1.SetZero();
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
            Assert.IsFalse(stats.NonZero());
        }

        [TestMethod()]
        public void FirstNonZeroTest()
        {
            var stat = new Stats();
            Assert.AreEqual(Attr.Null, stat.FirstNonZero());
        }
    }
}