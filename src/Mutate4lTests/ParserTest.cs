using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutate4lTests
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void TestResolveClipReference()
        {
            Tuple<int, int> result = Parser.ResolveClipReference("B4");
            Assert.AreEqual(result.Item1, 1);
            Assert.AreEqual(result.Item2, 3);
        }
    }
}
