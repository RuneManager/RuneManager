using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RuneOptim.Tests
{
    [TestClass()]
    public class RuneStatTests
    {
        public Rune MakeTestRune()
        {
            return new Rune()
            {
                Level = 0,
                Grade = 6,
                Slot = 2,
                Set = RuneSet.Energy,
                MainType = Attr.HealthPercent,
                MainValue = 11,
                InnateType = Attr.AttackFlat,
                InnateValue = 17,
                Sub1Type = Attr.DefensePercent,
                Sub1Value = 7,
            };
        }

        [TestMethod()]
        public void ValueGetTestMain()
        {
            Rune test = MakeTestRune();
            
            Assert.AreEqual(11, test.HealthPercent.Value);
        }

        [TestMethod()]
        public void ValueGetTestMainNot()
        {
            Rune test = MakeTestRune();

            Assert.AreEqual(0, test.CritDamage.Value);
        }

        [TestMethod()]
        public void ValueGetTestMain15()
        {
            Rune test = MakeTestRune();

            Assert.AreEqual(63, test.HealthPercent[15,false]);
        }

        [TestMethod()]
        public void ValueGetTestMain15Not()
        {
            Rune test = MakeTestRune();

            Assert.AreEqual(0, test.CritDamage[15, false]);
        }

        [TestMethod()]
        public void ValueGetTestInnate()
        {
            Rune test = MakeTestRune();

            Assert.AreEqual(17, test.AttackFlat.Value);
        }

        [TestMethod()]
        public void ValueGetTestSub()
        {
            Rune test = MakeTestRune();

            Assert.AreEqual(7, test.DefensePercent.Value);
        }

    }
}