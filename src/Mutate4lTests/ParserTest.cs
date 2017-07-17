using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Cli;
using System;

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
            Lexer lexer = new Lexer("interleave A1 C4 ranges A1 counts => A2");
            var command = Parser.ParseTokensToCommand(lexer.GetTokens());
            Assert.AreEqual(command.Id, TokenType.Interleave);
            Assert.AreEqual(command.Options[TokenType.Ranges].Count, 1);
            Assert.AreEqual(command.Options[TokenType.Counts].Count, 0);
            Assert.AreEqual(command.SourceClips.Count, 2);
            Assert.AreEqual(command.TargetClips.Count, 1);
        }
    }
}
