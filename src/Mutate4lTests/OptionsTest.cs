using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Cli;
using Mutate4l.ClipActions;
using Mutate4l.Dto;

namespace Mutate4lTests
{
    [TestClass]
    public class OptionsTest
    {
        [TestMethod]
        public void MyTestMethod()
        {
            var constrainOptions = new ConstrainOptions();
            var optionSet = new OptionsDefinition()
            {
                OptionGroups = new OptionGroup[] {
                    new OptionGroup() {
                        Type = OptionGroupType.InverseToggle,
                        Options = new Option[] {
                            new Option(TokenType.Pitch),
                            new Option(TokenType.Start)
                        }
                    }
                }
            };

            Lexer lexer = new Lexer("constrain A1 C4 start pitch => A2");
            var command = Parser.ParseTokensToCommand(lexer.GetTokens());
            var parsedOptions = OptionParser.ParseOptions<ConstrainOptions>(command.Options, optionSet);
            Assert.IsTrue(parsedOptions.Pitch);
            Assert.IsTrue(parsedOptions.Start);

            lexer = new Lexer("constrain A1 C4 start => A2");
            command = Parser.ParseTokensToCommand(lexer.GetTokens());
            parsedOptions = OptionParser.ParseOptions<ConstrainOptions>(command.Options, optionSet);
            Assert.IsFalse(parsedOptions.Pitch);
            Assert.IsTrue(parsedOptions.Start);

            lexer = new Lexer("constrain A1 C4 => A2");
            command = Parser.ParseTokensToCommand(lexer.GetTokens());
            parsedOptions = OptionParser.ParseOptions<ConstrainOptions>(command.Options, optionSet);
            Assert.IsTrue(parsedOptions.Pitch);
            Assert.IsTrue(parsedOptions.Start);
        }
    }
}
