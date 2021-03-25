using System;
using System.Collections.Generic;
using System.Text;
using Mutate4l.Compiler;
using Mutate4l.Core;
using NUnit.Framework;

namespace Mutate4lTests
{
    [TestFixture]
    public class LexerTest
    {
        [Test]
        public void TestLexer()
        {
            Lexer lexer = new Lexer("A1 A2 interleave -ranges 1/16", new List<Clip>());
            var expected = new TokenType[] { TokenType.ClipReference, TokenType.ClipReference, TokenType.Interleave, TokenType.Ranges, TokenType.MusicalDivision };
            var i = 0;

            var tokensContainer = lexer.GetTokens();
            Assert.IsTrue(tokensContainer.Success);
            foreach (var token in tokensContainer.Result)
            {
                Assert.AreEqual(token.Type, expected[i++]);
            }
        }

        [Test]
        public void TestIsDecimal()
        {
            var lex = new Lexer("1.0 123 34.08 35. 34.08.", new List<Clip>());
            Assert.IsTrue(lex.IsDecimalValue(0));
            Assert.IsFalse(lex.IsDecimalValue(4));
            Assert.IsTrue(lex.IsDecimalValue(8));
            Assert.IsFalse(lex.IsDecimalValue(14));
            Assert.IsFalse(lex.IsDecimalValue(18));
        }

        [Test]
        public void TestIsBarsBeatsSixteenths()
        {
            var lex = new Lexer("1.0.0 1.2.3.4 1..22 12.1. 4.04.7", new List<Clip>());
            Assert.IsTrue(lex.IsBarsBeatsSixteenths(0));
            Assert.IsFalse(lex.IsBarsBeatsSixteenths(6));
            Assert.IsFalse(lex.IsBarsBeatsSixteenths(14));
            Assert.IsFalse(lex.IsBarsBeatsSixteenths(20));
            Assert.IsTrue(lex.IsBarsBeatsSixteenths(26));
        }
    }
}
