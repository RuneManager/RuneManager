using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RuneOptim.Tests
{
    [TestClass()]
    public class RuneTests
    {
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
            Assert.AreEqual(135, rune.GetValue(Attr.AttackFlat, 15));
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