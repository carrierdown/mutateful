using Mutate4l.Core;
using System.Collections.Generic;
using Mutate4l.Compiler;
using NUnit.Framework;

namespace Mutate4lTests
{
    class OptionsClassOne
    {
        public decimal DecimalValue { get; set; }

        public bool SimpleBoolFlag { get; set; }

        [OptionInfo(min: 1, max: 100)]
        public int IntValue { get; set; }
    }

    enum TestEnum
    {
        EnumValue1,
        EnumValue2
    }

    class OptionsClassTwo
    {
        public decimal[] DecimalValue { get; set; }

        public int[] IntValue { get; set; }

        public TestEnum EnumValue { get; set; }
    }

    class OptionsClassThree
    {
        [OptionInfo(min: 0f, max: 1f)]
        public decimal[] DecimalValues { get; set; }

        [OptionInfo(min: 1, max: 100)]
        public int[] IntValues { get; set; }
    }

    [TestFixture]
    public class OptionsTest
    {
        [Test]
        public void TestValues()
        {
            var options = new Dictionary<TokenType, List<Token>>();
            options[TokenType.DecimalValue] = new List<Token>() { new Token(TokenType.MusicalDivision, "1/8", 0) };
            options[TokenType.IntValue] = new List<Token>() { new Token(TokenType.Number, "14", 0) };
            options[TokenType.SimpleBoolFlag] = new List<Token>();
            var (success, msg) = OptionParser.TryParseOptions(new Command { Options = options }, out OptionsClassOne parsedOptions);
            Assert.AreEqual(14, parsedOptions.IntValue);
            Assert.AreEqual(.5m, parsedOptions.DecimalValue);
            Assert.IsTrue(parsedOptions.SimpleBoolFlag);
        }

        [Test]
        public void TestMinMaxValue()
        {
            var options = new Dictionary<TokenType, List<Token>>();
            options[TokenType.IntValue] = new List<Token>() { new Token(TokenType.Number, "1000", 0) };
            var (success, msg) = OptionParser.TryParseOptions(new Command { Options = options }, out OptionsClassOne parsedOptions);
            Assert.AreEqual(100, parsedOptions.IntValue);
            options[TokenType.IntValue] = new List<Token>() { new Token(TokenType.Number, "0", 0) };
            parsedOptions = new OptionsClassOne();
            (success, msg) = OptionParser.TryParseOptions(new Command { Options = options }, out parsedOptions);
            Assert.AreEqual(1, parsedOptions.IntValue);
        }

        [Test]
        public void TestMinMaxValues()
        {
            var options = new Dictionary<TokenType, List<Token>>
            {
                [TokenType.DecimalValues] = new List<Token>
                {
                    new Token(TokenType.Decimal, "-1.10", 0),
                    new Token(TokenType.Decimal, ".5", 0),
                    new Token(TokenType.Decimal, "1.5", 0)
                },
                [TokenType.IntValues] = new List<Token>
                {
                    new Token(TokenType.Number, "-10", 0),
                    new Token(TokenType.Number, "50", 0),
                    new Token(TokenType.Number, "101", 0)
                }
            };

            var (success, _) = OptionParser.TryParseOptions(new Command { Options = options }, out OptionsClassThree parsedOptions);
            Assert.IsTrue(success);
            Assert.AreEqual(0m, parsedOptions.DecimalValues[0]);
            Assert.AreEqual(.5m, parsedOptions.DecimalValues[1]);
            Assert.AreEqual(1m, parsedOptions.DecimalValues[2]);
            Assert.AreEqual(1, parsedOptions.IntValues[0]);
            Assert.AreEqual(50, parsedOptions.IntValues[1]);
            Assert.AreEqual(100, parsedOptions.IntValues[2]);
        }

        [Test]
        public void TestListValues()
        {
            var options = new Dictionary<TokenType, List<Token>>();
            options[TokenType.DecimalValue] = new List<Token>() { new Token(TokenType.MusicalDivision, "1/8", 0), new Token(TokenType.MusicalDivision, "1/16", 0) };
            options[TokenType.IntValue] = new List<Token>() { new Token(TokenType.Number, "14", 0), new Token(TokenType.Number, "2", 0), new Token(TokenType.Number, "900", 0) };
            var (success, msg) = OptionParser.TryParseOptions(new Command { Options = options }, out OptionsClassTwo parsedOptions);
            Assert.AreEqual(2, parsedOptions.DecimalValue.Length);
            Assert.AreEqual(3, parsedOptions.IntValue.Length);
        }

        [Test]
        public void TestEnumValues()
        {
            var options = new Dictionary<TokenType, List<Token>>();
            options[TokenType.EnumValue] = new List<Token>() { new Token(TokenType.EnumValue, "enumvalue2", 0) };
            var (success, msg) = OptionParser.TryParseOptions(new Command { Options = options }, out OptionsClassTwo parsedOptions);
            Assert.AreEqual(TestEnum.EnumValue2, parsedOptions.EnumValue);
        }
    }
}
