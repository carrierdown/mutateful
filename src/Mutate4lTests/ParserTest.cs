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

        [TestMethod]
        public void TestParseTokensToCommand()
        {
            Lexer lexer = new Lexer("interleave A1 C4 range A1 count => A2");
            var command = Parser.ParseTokensToCommand(lexer.GetTokens());

        }
    }
}
