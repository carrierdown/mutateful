using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Cli;
using System.Collections.Generic;
using Mutate4l.Core;

namespace Mutate4lTests
{
    [TestClass]
    public class LexerTest
    {
        /*[TestMethod]
        public void TestLexer()
        {
            Lexer lexer = new Lexer("interleave A1:C4 -ranges 1/16 -repeats 2 10  => A2");
            var expected = new TokenType[] { Interleave, ClipReference, Colon, ClipReference, Ranges, MusicalDivision, Repeats, Number, Number, Destination, ClipReference };
            var i = 0;

            foreach (var token in lexer.GetTokens())
            {
                Assert.AreEqual(token.Type, expected[i++]);
            }
        }*/

        [TestMethod]
        public void TestIsDecimal()
        {
            var lex = new Lexer("1.0 123 34.08 35. 34.08.", new List<Clip>());
            Assert.IsTrue(lex.IsDecimalValue(0));
            Assert.IsFalse(lex.IsDecimalValue(4));
            Assert.IsTrue(lex.IsDecimalValue(8));
            Assert.IsFalse(lex.IsDecimalValue(14));
            Assert.IsFalse(lex.IsDecimalValue(18));
        }
    }
}
