using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Cli;
using Mutate4l.ClipActions;
using Mutate4l.Core;
using Mutate4l.Dto;
using System;
using System.Collections.Generic;

namespace Mutate4lTests
{
    class OptionsClassOne
    {
        [OptionInfo(groupId: 1, type: OptionType.AllOrSpecified)]
        public bool GroupOneToggleOne { get; set; }

        [OptionInfo(groupId: 1, type: OptionType.AllOrSpecified)]
        public bool GroupOneToggleTwo { get; set; }

        [OptionInfo(groupId: 2, type: OptionType.AllOrSpecified)]
        public bool GroupTwoToggleOne { get; set; }

        [OptionInfo(groupId: 2, type: OptionType.AllOrSpecified)]
        public bool GroupTwoToggleTwo { get; set; }

        public decimal DecimalValue { get; set; }

        public int IntValue { get; set; }
    }

    class OptionsClassTwo
    {
        public decimal[] DecimalValue { get; set; }

        public int[] IntValue { get; set; }
    }

    [TestClass]
    public class OptionsTest
    {
        [TestMethod]
        public void TestToggleGroups()
        {
            var options = new Dictionary<TokenType, List<Token>>();
            options[TokenType.GroupOneToggleOne] = new List<Token>();
            var parsedOptions = OptionParser.ParseOptions<OptionsClassOne>(options);
            Assert.IsTrue(parsedOptions.GroupOneToggleOne);
            Assert.IsFalse(parsedOptions.GroupOneToggleTwo);
            Assert.IsTrue(parsedOptions.GroupTwoToggleOne);
            Assert.IsTrue(parsedOptions.GroupTwoToggleTwo);
        }

        [TestMethod]
        public void TestValues()
        {
            var options = new Dictionary<TokenType, List<Token>>();
            options[TokenType.DecimalValue] = new List<Token>() { new Token(TokenType.MusicalDivision, "1/8", 0) };
            options[TokenType.IntValue] = new List<Token>() { new Token(TokenType.Number, "14", 0) };
            var parsedOptions = OptionParser.ParseOptions<OptionsClassOne>(options);
            Assert.AreEqual(14, parsedOptions.IntValue);
            Assert.AreEqual(.5m, parsedOptions.DecimalValue);
        }

        [TestMethod]
        public void TestListValues()
        {
            var options = new Dictionary<TokenType, List<Token>>();
            options[TokenType.DecimalValue] = new List<Token>() { new Token(TokenType.MusicalDivision, "1/8", 0), new Token(TokenType.MusicalDivision, "1/16", 0) };
            options[TokenType.IntValue] = new List<Token>() { new Token(TokenType.Number, "14", 0), new Token(TokenType.Number, "2", 0), new Token(TokenType.Number, "900", 0) };
            var parsedOptions = OptionParser.ParseOptions<OptionsClassTwo>(options);
            Assert.AreEqual(2, parsedOptions.DecimalValue.Length);
            Assert.AreEqual(3, parsedOptions.IntValue.Length);
        }

        [TestMethod]
        public void TestInverseToggleGroup()
        {
            Lexer lexer = new Lexer("constrain A1 C4 start pitch => A2");
            var command = Parser.ParseTokensToCommand(lexer.GetTokens());
            var parsedOptions = OptionParser.ParseOptions<ConstrainOptions>(command.Options);
            Assert.IsTrue(parsedOptions.Pitch);
            Assert.IsTrue(parsedOptions.Start);
            lexer = new Lexer("constrain A1 C4 start => A2");
            command = Parser.ParseTokensToCommand(lexer.GetTokens());
            parsedOptions = OptionParser.ParseOptions<ConstrainOptions>(command.Options);
            Assert.IsFalse(parsedOptions.Pitch);
            Assert.IsTrue(parsedOptions.Start);

            lexer = new Lexer("constrain A1 C4 => A2");
            command = Parser.ParseTokensToCommand(lexer.GetTokens());
            parsedOptions = OptionParser.ParseOptions<ConstrainOptions>(command.Options);
            Assert.IsTrue(parsedOptions.Pitch);
            Assert.IsTrue(parsedOptions.Start);
        }
        /*
        [TestMethod]
        public void TestValueGroup()
        {
            // todo: complete
            var interleaveOptions = new InterleaveOptions();
            var optionSet = new OptionsDefinition()
            {
                OptionGroups = new OptionGroup[]
                {
                    new OptionGroup()
                    {
                        Type = OptionGroupType.Value,
                        Options = new TokenType[]
                        {
                            TokenType.Mode
                        }
                    }
                }
            };
        }*/
    }
}
