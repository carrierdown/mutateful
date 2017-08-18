using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Cli;
using static Mutate4l.Cli.TokenType;

namespace Mutate4lTests
{
    [TestClass]
    public class LexerTest
    {
        [TestMethod]
        public void TestLexer()
        {
            Lexer lexer = new Lexer("interleave A1:C4 ranges 1/16 repeats 2 10  => A2");
            var expected = new TokenType[] { Interleave, ClipReference, Colon, ClipReference, Ranges, MusicalDivision, Repeats, Number, Number, Destination, ClipReference };
            var i = 0;

            foreach (var token in lexer.GetTokens())
            {
                Assert.AreEqual(token.Type, expected[i++]);
            }
        }
    }
}
