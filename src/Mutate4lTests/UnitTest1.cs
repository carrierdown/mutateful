using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l;
using System;

namespace Mutate4lTests
{
    [TestClass]
    public class LexerTest
    {
        [TestMethod]
        public void TestLexer()
        {
            Lexer lexer = new Lexer(": => :");
            foreach (var token in lexer.GetToken())
            {
                Console.WriteLine(token.Value);
            }
        }
    }
}
