using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneOptim.BuildProcessing;

namespace RuneOptim.Tests {
    [TestClass()]
    public class RuneFilterTests
    { 
        [TestMethod()]
        public void DominantTest()
        {
            var f1 = new RuneFilter(null, 3, 1);
            var f2 = new RuneFilter(2, null, 2);

            var ft = RuneFilter.Dominant(f1, f2);
            Assert.AreEqual(2, ft.Flat);
            Assert.AreEqual(3, ft.Percent);
            Assert.AreEqual(1, ft.Test);

            ft = RuneFilter.Dominant(f2, f1);
            Assert.AreEqual(2, ft.Flat);
            Assert.AreEqual(3, ft.Percent);
            Assert.AreEqual(2, ft.Test);
        }

        [TestMethod()]
        public void MinTest()
        {
            var f1 = new RuneFilter(null, 3, 1);
            var f2 = new RuneFilter(2, null, 0);
            var ft = RuneFilter.Min(f1, f2);
            Assert.AreEqual(2, ft.Flat);
            Assert.AreEqual(3, ft.Percent);
            Assert.AreEqual(0, ft.Test);
        }

        [TestMethod()]
        public void SqBrackTest()
        {
            var f = new RuneFilter(1, 2, 3);
            Assert.AreEqual(1, f["flat"]);
            Assert.AreEqual(2, f["perc"]);
            Assert.AreEqual(3, f["test"]);
            f["flat"] = 3;
            f["perc"] = 4;
            f["test"] = 5;
            Assert.AreEqual(3, f["flat"]);
            Assert.AreEqual(4, f["perc"]);
            Assert.AreEqual(5, f["test"]);
        }

        [TestMethod()]
        public void MinNZeroTest()
        {
            Assert.AreEqual(null, RuneFilter.MinNZero(null, null));
            Assert.AreEqual(1, RuneFilter.MinNZero(1, null));
            Assert.AreEqual(2, RuneFilter.MinNZero(null, 2));
            Assert.AreEqual(2, RuneFilter.MinNZero(3, 2));
            Assert.AreEqual(4, RuneFilter.MinNZero(4, 6));
        }

        [TestMethod()]
        public void NonZeroTest()
        {
            Assert.IsFalse(new RuneFilter().NonZero);
            Assert.IsTrue(new RuneFilter(1).NonZero);
            Assert.IsTrue(new RuneFilter(p: 1).NonZero);
            Assert.IsTrue(new RuneFilter(t: 1).NonZero);
        }
    }
}