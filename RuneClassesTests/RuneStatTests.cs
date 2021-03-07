using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneOptim.swar;

namespace RuneOptim.Tests {
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
                Main = new RuneAttr() { (int)Attr.HealthPercent, 11 },
                Innate = new RuneAttr() { (int)Attr.AttackFlat, 17 },
                Subs = { new RuneAttr() { (int)Attr.DefensePercent, 7 } },
            };
        }

        [TestMethod()]
        public void ValueGetTestMain()
        {
            Rune test = MakeTestRune();
            
            Assert.AreEqual(11, test.HealthPercent[0]);
        }

        [TestMethod()]
        public void ValueGetTestMainNot()
        {
            Rune test = MakeTestRune();

            Assert.AreEqual(0, test.CritDamage[0]);
        }

        [TestMethod()]
        public void ValueGetTestMain15()
        {
            Rune test = MakeTestRune();

            Assert.AreEqual(63, test.HealthPercent[15]);
        }

        [TestMethod()]
        public void ValueGetTestMain15Not()
        {
            Rune test = MakeTestRune();

            Assert.AreEqual(0, test.CritDamage[15]);
        }

        [TestMethod()]
        public void ValueGetTestInnate()
        {
            Rune test = MakeTestRune();

            Assert.AreEqual(17, test.AttackFlat[0]);
        }

        [TestMethod()]
        public void ValueGetTestSub()
        {
            Rune test = MakeTestRune();

            Assert.AreEqual(7, test.DefensePercent[0]);
        }

    }
}