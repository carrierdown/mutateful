using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Cli;
using Mutate4l.Commands;
using Mutate4l.Dto;
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

//        [TestMethod]
        /*public void TestParseTokensToCommand()
        {
            Lexer lexer = new Lexer("interleave A1 C4 -ranges A1 -repeats => A2");
            var command = Parser.ParseTokensToCommand(lexer.GetTokens());
            Assert.AreEqual(command.Id, TokenType.Interleave);
            Assert.AreEqual(command.Options[TokenType.Ranges].Count, 1);
            Assert.AreEqual(command.Options[TokenType.Repeats].Count, 0);
        }*/

        /*[TestMethod]
        public void TestParseFormulaToCommand()
        {
            var command = Parser.ParseFormulaToChainedCommand("{id:456,trackIx:1} [1,0:4 1 70 0 4 100] constrain -by [0,0:4 1 70 0 0.5 100 70 0.5 0.5 100 70 1 0.5 100 70 1.5 0.5 100 70 2 0.5 100 70 2.5 0.5 100 70 3 0.5 100 70 3.5 0.5 100] -mode absolute");
            var parsedOptions = OptionParser.ParseOptions<TransposeOptions>(command.Result.Commands[0]);
            var i = 0;
        }*/
    }
}
