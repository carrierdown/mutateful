using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Cli;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public void Testing()
        {
            var lexer = new Lexer("[0] tp 2 remap -to [0] shuffle 1 2|3|9x6 1 2 4x3 5|6|7 8", new List<Clip> {Clip.Empty, Clip.Empty});
            var result = lexer.GetTokens();
            Assert.IsTrue(result.Success);
            var sTokens = Parser.CreateSyntaxTree(result.Result);
            PrintSyntaxTree(sTokens.Result.Children);
        }
        
        public void PrintSyntaxTree(List<TreeToken> treeTokens, int indent = 0)
        {
            foreach (var treeToken in treeTokens)
            {
                Console.WriteLine($"{GetIndent(indent)}{treeToken.Value}");
                if (treeToken.HasChildren) PrintSyntaxTree(treeToken.Children, indent + 1);
            }
        }

        public string GetIndent(int indent)
        {
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < indent; i++)
            {
                sb.Append("  ");
            }
            return sb.ToString();
        }
    }
}
