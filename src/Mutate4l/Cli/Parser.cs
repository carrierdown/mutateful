using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mutate4l.Core;

namespace Mutate4l.Cli
{
    public static class Parser
    {
        public static Tuple<int, int> ResolveClipReference(string reference)
        {
            if (reference == "*") return new Tuple<int, int>(-1, -1);
            string channel = "", clip = "";
            int c = 0;
            channel = reference[c++].ToString();
            while (c < reference.Length && Lexer.IsNumeric(reference[c]))
            {
                clip += reference[c++];
            }

            int x = 0, y = 0;
            x = Encoding.ASCII.GetBytes(channel.ToLower())[0] - Encoding.ASCII.GetBytes("a")[0];
            y = int.Parse(clip) - 1; // indexes into Live are 0-based
            return new Tuple<int, int>(x, y);
        }

        public static ProcessResult<ChainedCommand> ParseFormulaToChainedCommand(string formula, List<Clip> clips, ClipMetaData metadata)
        {
            var valid = new char[] { '[', ']' }.All(c => formula.IndexOf(c) >= 0);
            if (!valid) return new ProcessResult<ChainedCommand>($"Invalid formula: {formula}");

            var lexer = new Lexer(formula, clips);
            var result = lexer.GetTokens();
            if (!result.Success) return new ProcessResult<ChainedCommand>(result.ErrorMessage);
            Token[] commandTokens = result.Result;
            var commandTokensLists = new List<List<Token>>();
            var activeCommandTokenList = new List<Token>();
            var sourceClips = commandTokens.TakeWhile(x => x.Type == TokenType.InlineClip).Select(x => x.Clip).ToArray();
            var tokensToProcess = commandTokens.Skip(sourceClips.Count()).ToArray();

            if (tokensToProcess.Length == 0) 
            {
                // Empty command, assume concat
                tokensToProcess = new Token[] {new Token(TokenType.Concat, "concat", 0), };
            }

            foreach (var token in tokensToProcess)
            {
                if (token.IsCommand)
                {
                    if (activeCommandTokenList.Count == 0)
                    {
                        activeCommandTokenList.Add(token);
                    }
                    else
                    {
                        commandTokensLists.Add(activeCommandTokenList);
                        activeCommandTokenList = new List<Token> { token };
                    }
                }
                else
                {
                    activeCommandTokenList.Add(token);
                }
            }
            commandTokensLists.Add(activeCommandTokenList); // add last command token list
            var commands = commandTokensLists.Select(x => ParseTokensToCommand(x)).ToList();

            var chainedCommand = new ChainedCommand(commands, sourceClips, metadata);
            return new ProcessResult<ChainedCommand>(chainedCommand);
        }

        private static Command ParseTokensToCommand(IEnumerable<Token> tokens)
        {
            var command = new Command();
            
            List<Token> tokensAsList = tokens.ToList();
            command.Id = tokensAsList[0].Type;
            var i = 1;
            while (i < tokensAsList.Count)
            {
                if (tokensAsList[i].IsOption)
                {
                    var type = tokensAsList[i].Type;
                    var values = new List<Token>();
                    i++;
                    while (i < tokensAsList.Count && tokensAsList[i].IsOptionValue)
                    {
                        values.Add(tokensAsList[i++]);
                    }
                    command.Options.Add(type, values);
                }
                else 
                {
                    while (i < tokensAsList.Count && tokensAsList[i].IsOptionValue) {
                        // If we don't get an option header but just one or more values, assume these are values for the default option
                        command.DefaultOptionValues.Add(tokensAsList[i++]);
                    }
                }
            }
            return command;
        }
    }
}
