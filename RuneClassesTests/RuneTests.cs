using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneOptim.swar;

namespace RuneOptim.Tests {
    [TestClass()]
    public class RuneTests
    {
        [TestMethod()]
        public void SetValue()
        {
            Rune r = new Rune();
            r.Level = 6;
            r.Grade = 5;
            r.Slot = 2;
            r.Set = RuneSet.Swift;
            r.SetValue(-1, Attr.Resistance, 3);
            r.SetValue(0, Attr.AttackPercent, 22);
            r.SetValue(1, Attr.CritRate, 9);
            r.SetValue(2, Attr.Speed, 5);

            Assert.AreEqual(22, r.AttackPercent[0]);
            Assert.AreEqual(r.AttackPercent[0], r.GetValue(Attr.AttackPercent));
            Assert.AreEqual(r.AttackPercent[12 + 16], r.GetValue(Attr.AttackPercent, 12, true));
            Assert.AreEqual(r.AttackPercent[15], r.GetValue(Attr.AttackPercent, 15));

            Assert.AreEqual(0, r.HealthPercent[0]);
            Assert.AreEqual(r.GetValue(Attr.HealthPercent), r.HealthPercent[0]);
            Assert.AreEqual(1, r.HealthPercent[12 + 16]);
            Assert.AreEqual(r.GetValue(Attr.HealthPercent, 12, true), r.HealthPercent[12 + 16]);
            Assert.AreEqual(0, r.HealthPercent[15]);
            Assert.AreEqual(r.GetValue(Attr.HealthPercent, 15), r.HealthPercent[15]);
        }

        [TestMethod()]
        public void FlatCountTest()
        {
            Assert.AreEqual(0, TestData.Rune1().FlatCount());
            Assert.AreEqual(0, TestData.Rune2().FlatCount());
            Assert.AreEqual(0, TestData.Rune3().FlatCount());
            Assert.AreEqual(1, TestData.Rune4().FlatCount());
            Assert.AreEqual(0, TestData.Rune5().FlatCount());
            Assert.AreEqual(2, TestData.Rune6().FlatCount());
        }

        [TestMethod()]
        public void SetRequiredTest()
        {
            Assert.AreEqual(2, Rune.SetRequired(TestData.Rune1().Set));
            Assert.AreEqual(4, Rune.SetRequired(TestData.Rune2().Set));
        }

        [TestMethod()]
        public void GetValueTest()
        {
            var rune = TestData.Rune1();
            Assert.AreEqual(78, rune.GetValue(Attr.AttackFlat));
            Assert.AreEqual(99, rune.GetValue(Attr.AttackFlat, 12, true));
            Assert.AreEqual(135, rune.GetValue(Attr.AttackFlat, 15));

            Assert.AreEqual(0, rune.GetValue(Attr.HealthPercent));
            Assert.AreEqual(1, rune.GetValue(Attr.HealthPercent, 12, true));
            Assert.AreEqual(0, rune.GetValue(Attr.HealthPercent, 15));
        }

        [TestMethod()]
        public void FixShitTest()
        {
            var rune = TestData.Rune1();
            rune.PrebuildAttributes();
            Assert.AreEqual(78, rune.AttackFlat[0]);
            Assert.AreEqual(99, rune.AttackFlat[12 + 16]);
            Assert.AreEqual(135, rune.AttackFlat[15]);

            Assert.AreEqual(0, rune.HealthPercent[0]);
            Assert.AreEqual(1, rune.HealthPercent[12 + 16]);
            Assert.AreEqual(0, rune.HealthPercent[15]);
        }

        [TestMethod()]
        public void HasStatTest()
        {
            var rune = TestData.Rune1();
            Assert.IsTrue(rune.HasStat(Attr.Speed));
            Assert.IsTrue(rune.HasStat(Attr.AttackFlat));
            Assert.IsFalse(rune.HasStat(Attr.CritDamage));
            Assert.IsFalse(rune.HasStat(Attr.HealthPercent));
        }

        [TestMethod()]
        public void OrTest()
        {
            Stats weightFlat = new Stats()
            {
                Speed = 1
            };
            Stats weightPerc = new Stats()
            {
                Attack = 1,
                Health = 1,
                Defense = 1,
                CritDamage = 1,
                CritRate = 1,
                Accuracy = 1,
                Resistance = 1,
            };
            Stats weightTest = new Stats()
            {
                Attack = 15,
                Health = 15,
                Defense = 15,
                Speed = 10,
                CritRate = 20,
                CritDamage = 15,
            };

            Assert.IsTrue(TestData.Rune1().Or(weightFlat, weightPerc, weightTest));
            Assert.IsTrue(TestData.Rune2().Or(weightFlat, weightPerc, weightTest));
            Assert.IsFalse(TestData.Rune3().Or(weightFlat, weightPerc, weightTest));
            Assert.IsTrue(TestData.Rune4().Or(weightFlat, weightPerc, weightTest));
            Assert.IsTrue(TestData.Rune5().Or(weightFlat, weightPerc, weightTest));
            Assert.IsTrue(TestData.Rune6().Or(weightFlat, weightPerc, weightTest));
        }

        [TestMethod()]
        public void AndTest()
        {
            Stats weightFlat = new Stats()
            {
                Speed = 1
            };
            Stats weightPerc = new Stats()
            {
                Attack = 1,
            };
            Stats weightTest = new Stats()
            {
                Attack = 3,
                Speed = 3,
            };

            Assert.IsTrue(TestData.Rune1().And(weightFlat, weightPerc, weightTest));
            Assert.IsTrue(TestData.Rune2().And(weightFlat, weightPerc, weightTest));
            Assert.IsFalse(TestData.Rune3().And(weightFlat, weightPerc, weightTest));
            Assert.IsFalse(TestData.Rune4().And(weightFlat, weightPerc, weightTest));
            Assert.IsTrue(TestData.Rune5().And(weightFlat, weightPerc, weightTest));
            Assert.IsFalse(TestData.Rune6().And(weightFlat, weightPerc, weightTest));
        }

        [TestMethod()]
        public void TestTest()
        {
            Stats weightFlat = new Stats()
            {
                Speed = 1
            };
            Stats weightPerc = new Stats()
            {
                Attack = 1,
                Health = 1,
            };
            Assert.AreEqual(22, TestData.Rune1().Test(weightFlat, weightPerc));
            Assert.AreEqual(27, TestData.Rune2().Test(weightFlat, weightPerc));
            Assert.AreEqual(5, TestData.Rune3().Test(weightFlat, weightPerc));
            Assert.AreEqual(23, TestData.Rune4().Test(weightFlat, weightPerc));
            Assert.AreEqual(14, TestData.Rune5().Test(weightFlat, weightPerc));
            Assert.AreEqual(5, TestData.Rune6().Test(weightFlat, weightPerc));
        }
    }
}