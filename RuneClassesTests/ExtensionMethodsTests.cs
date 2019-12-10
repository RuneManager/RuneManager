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
    public class ExtensionMethodsTests
    {
        [TestMethod()]
        public void EqualToTest()
        {
            double q = -1;
            double w = 0;
            w -= 1;

            Assert.IsTrue(q == w);
            Assert.IsTrue(q.EqualTo(w));
            Assert.IsTrue(q > w - 0.00000001 && q < w + 0.00000001);
        }
    }
}