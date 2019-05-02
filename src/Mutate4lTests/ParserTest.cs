using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Cli;
using Mutate4l.Commands;
using System;
using System.Collections.Generic;
using Mutate4l.Core;
using System.Linq;

namespace Mutate4lTests
{
    [TestClass]
    public class ParserTest
    {
        private Clip Clip1 = new Clip(4, true) {
            Notes = new SortedList<NoteEvent>()
            {
                new NoteEvent(60, 0, .5m, 100), // C
                new NoteEvent(55, 1, .5m, 100), // G
                new NoteEvent(62, 2, .5m, 100)  // D
            }
        };
        private Clip Clip2 = new Clip(4, true) {
            Notes = new SortedList<NoteEvent>()
            {
                new NoteEvent(47, 0, .5m, 100), // B
                new NoteEvent(63, 3, .5m, 100), // D#
                new NoteEvent(81, 4, .5m, 100)  // A
            }
        };

        [TestMethod]
        public void TestResolveClipReference()
        {
            Tuple<int, int> result = Parser.ResolveClipReference("B4");
            Assert.AreEqual(result.Item1, 1);
            Assert.AreEqual(result.Item2, 3);
        }

        [TestMethod]
        public void TestParseInvalidInput()
        {
            var result = Parser.ParseFormulaToChainedCommand("[0] interleave % fisk -by [1] -mode event", new List<Clip> {Clip1, Clip2}, new ClipMetaData(100, 0));
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void TestParseMusicalDivision()
        {
            var result = Parser.ParseFormulaToChainedCommand("[0] interleave -mode time -ranges 1/16 15/16 16/1", new List<Clip> { Clip1 }, new ClipMetaData(100, 0));
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void TestCastNumberToMusicalDivision()
        {
            var command = Parser.ParseFormulaToChainedCommand("[0] interleave -mode time -ranges 1/8 2 1 4", new List<Clip> { Clip1 }, new ClipMetaData(100, 0));
            Assert.IsTrue(command.Success);
            var (success, _) = OptionParser.TryParseOptions(command.Result.Commands.First(), out InterleaveOptions options);
            Assert.IsTrue(success);
            Assert.AreEqual(0.5m, options.Ranges[0]);
            Assert.AreEqual(8, options.Ranges[1]);
            Assert.AreEqual(4, options.Ranges[2]);
            Assert.AreEqual(16, options.Ranges[3]);
        }        

        [TestMethod]
        public void TestConvertBarsBeatsSixteenths()
        {
            var command = Parser.ParseFormulaToChainedCommand("[0] interleave -mode time -ranges 0.0.2 0.2.0 0.1.0 1.0.0 1.0.1", new List<Clip> { Clip1 }, new ClipMetaData(100, 0));
            Assert.IsTrue(command.Success);
            var (success, _) = OptionParser.TryParseOptions(command.Result.Commands.First(), out InterleaveOptions options);
            Assert.IsTrue(success);
            Assert.AreEqual(0.5m, options.Ranges[0]);
            Assert.AreEqual(2, options.Ranges[1]);
            Assert.AreEqual(1, options.Ranges[2]);
            Assert.AreEqual(4, options.Ranges[3]);
            Assert.AreEqual(4.25m, options.Ranges[4]);
        }        
        
        [TestMethod]
        public void TestCastNumberWhenNoImplicitCastSet()
        {
            var command = Parser.ParseFormulaToChainedCommand("[0] resize 1/8", new List<Clip> { Clip1 }, new ClipMetaData(100, 0));
            Assert.IsTrue(command.Success);
            var (success, _) = OptionParser.TryParseOptions(command.Result.Commands.First(), out ResizeOptions _);
            Assert.IsFalse(success);
            command = Parser.ParseFormulaToChainedCommand("[0] resize 0.5", new List<Clip> { Clip1 }, new ClipMetaData(100, 0));
            (success, _) = OptionParser.TryParseOptions(command.Result.Commands.First(), out ResizeOptions _);
            Assert.IsTrue(success);
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
