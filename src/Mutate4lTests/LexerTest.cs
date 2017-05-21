using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l;
using static Mutate4l.TokenType;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutate4lTests
{
    [TestClass]
    public class LexerTest
    {
        [TestMethod]
        public void TestLexer()
        {
            Lexer lexer = new Lexer("interleave A1:C4 range count  => A2");
            var expected = new TokenType[] { Interleave, ClipReference, Colon, ClipReference, Range, Count, Destination, ClipReference };
            var i = 0;

            foreach (var token in lexer.GetTokens())
            {
                Assert.AreEqual(token.Type, expected[i++]);
            }
        }
    }
}
