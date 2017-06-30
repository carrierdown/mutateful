using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.ClipActions;
using Mutate4l.Core;
using Mutate4l.Cli;
using Mutate4l.Dto;
using System;
using System.Collections.Generic;

namespace Mutate4lTests.ClipActions
{
    [TestClass]
    public class ConstrainTest
    {
        [TestMethod]
        public void TestConstrainInitialize()
        {
            var commands = new string[] {
                "constrain A1 A2 start pitch => A3",
                "constrain A1 A2 start       => A3",
                "constrain A1 A2       pitch => A3",
                "constrain A1 A2             => A3"
            };
            var expectedConstrainStartStates = new bool[] { true, true, false, true };
            var expectedConstrainPitchStates = new bool[] { true, false, true, true };

            for (int i = 0; i < commands.Length; i++)
            {
                var command = Parser.ParseTokensToCommand(new Lexer(commands[i]).GetTokens());
                Assert.AreEqual(command.Id, TokenType.Constrain);
                var constrain = new Constrain(command.Options);
                Assert.AreEqual(constrain.ConstrainStart, expectedConstrainStartStates[i]);
                Assert.AreEqual(constrain.ConstrainPitch, expectedConstrainPitchStates[i]);
            }
        }
    }
}
