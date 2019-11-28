using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Cli;
using System.Collections.Generic;
using System.Linq;
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

        [TestMethod]
        public void TestIsBarsBeatsSixteenths()
        {
            var lex = new Lexer("1.0.0 1.2.3.4 1..22 12.1. 4.04.7", new List<Clip>());
            Assert.IsTrue(lex.IsBarsBeatsSixteenths(0));
            Assert.IsFalse(lex.IsBarsBeatsSixteenths(6));
            Assert.IsFalse(lex.IsBarsBeatsSixteenths(14));
            Assert.IsFalse(lex.IsBarsBeatsSixteenths(20));
            Assert.IsTrue(lex.IsBarsBeatsSixteenths(26));
        }

        [TestMethod]
        public void TestOperatorResolving()
        {
            var lexer = new Lexer("shuffle 1 2'3 4 5'6'7 8", new List<Clip>());
            var result = lexer.GetTokens();
            Assert.IsTrue(result.Success);
            var resolvedTokens = Parser.ResolveOperators(result.Result);
            Assert.IsTrue(resolvedTokens.Success);
            Assert.IsTrue(resolvedTokens.Result.Length > 0);
            var fullyResolvedTokens = Parser.ApplyOperators(resolvedTokens.Result);
            Assert.IsTrue(fullyResolvedTokens.Select(x => x.Value).SequenceEqual(new [] {"1", "2", "4", "5", "8", "1", "3", "4", "6", "8", "1", "2", "4", "7", "8"}));
        }
    }
}
