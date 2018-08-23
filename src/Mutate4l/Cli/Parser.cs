using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mutate4l.Dto;
using Mutate4l.IO;
using Mutate4l.Utility;

namespace Mutate4l.Cli
{
    public class Parser
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

        /*
        public static ProcessResult<ChainedCommand> ParseTokensToChainedCommand(IEnumerable<Token> tokens)
        {
            Token[] tokenList = tokens.ToArray();
            var sourceClips = tokenList.TakeWhile(t => t.IsClipReference);
            var commandTokens = tokenList.Skip(sourceClips.Count()).TakeWhile(t => t.IsCommand || t.IsOption || t.IsOptionValue).ToArray();
            var destClips = tokenList.Skip(sourceClips.Count() + commandTokens.Count()).SkipWhile(t => t.Type == TokenType.Destination).TakeWhile(t => t.IsClipReference);

            if (sourceClips.Count() == 0)
                return new ProcessResult<ChainedCommand>("No source clips specified.");

            if (destClips.Count() == 0 && sourceClips.Count() == 1)
                destClips = sourceClips;
            else if (destClips.Count() == 0)
                return new ProcessResult<ChainedCommand>("No destination clip specified. When operating on a single clip, the destination clip may be omitted. Otherwise, it must be specified.");

            var commandTokensLists = new List<List<Token>>();
            var activeCommandTokenList = new List<Token>();
            
            foreach (var token in commandTokens)
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
            var commands = new List<Command>();
            foreach (var commandTokensList in commandTokensLists)
            {
                commands.Add(ParseTokensToCommand(commandTokensList));
            }

            var chainedCommand = new ChainedCommand
            {
                SourceClips = sourceClips.Select(c => ResolveClipReference(c.Value)).ToList(),
                TargetClips = destClips.Select(c => ResolveClipReference(c.Value)).ToList(),
                Commands = commands
            };
            return new ProcessResult<ChainedCommand>(chainedCommand);
        }*/

        public static ProcessResult<ChainedCommand> ParseFormulaToChainedCommand(string formula)
        {
            var valid = new char[] { '{', '[', ']', '}' }.All(c => formula.IndexOf(c) >= 0);
            if (!valid) return new ProcessResult<ChainedCommand>($"Invalid formula: {formula}");

            int index = formula.IndexOf('{') + 1;
            string targetId = formula.Substring(index, formula.IndexOf('}') - index);
            var sourceClips = formula
                .Substring(formula.IndexOf('['), formula.LastIndexOf(']') - formula.IndexOf('['))
                .Split(']')
                .Select(x => IOUtilities.StringToClip(x.Trim().TrimStart('[')))
                .ToArray();
            string command = formula.Substring(formula.LastIndexOf(']') + 1);

            var lexer = new Lexer(command);
            Token[] commandTokens = lexer.GetTokens().ToArray();
            var commandTokensLists = new List<List<Token>>();
            var activeCommandTokenList = new List<Token>();

            foreach (var token in commandTokens)
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

            var chainedCommand = new ChainedCommand
            {
                SourceClips = sourceClips,
                TargetId = targetId,
                Commands = commands
            };
            return new ProcessResult<ChainedCommand>(chainedCommand);
        }

        public static Command ParseTokensToCommand(IEnumerable<Token> tokens)
        {
            var command = new Command();
            
            List<Token> tokensAsList = tokens.ToList();
            command.Id = tokensAsList[0].Type;
            var i = 0;
            while (++i < tokensAsList.Count)
            {
                if (tokensAsList[i].Type > TokenType._OptionsBegin && tokensAsList[i].Type < TokenType._OptionsEnd)
                {
                    var type = tokensAsList[i].Type;
                    var values = new List<Token>();
                    i++;
                    while (i < tokensAsList.Count && ((tokensAsList[i].Type > TokenType._ValuesBegin && tokensAsList[i].Type < TokenType._ValuesEnd) 
                        || (tokensAsList[i].Type > TokenType._EnumValuesBegin && tokensAsList[i].Type < TokenType._EnumValuesEnd)))
                    {
                        values.Add(tokensAsList[i++]);
                    }
                    command.Options.Add(type, values);
                }
                else 
                {
                    while (i < tokensAsList.Count && ((tokensAsList[i].Type > TokenType._ValuesBegin && tokensAsList[i].Type < TokenType._ValuesEnd)
                        || (tokensAsList[i].Type > TokenType._EnumValuesBegin && tokensAsList[i].Type < TokenType._EnumValuesEnd))) {
                        // If we don't get an option header but just one or more values, assume these are values for the default option
                        command.DefaultOptionValues.Add(tokensAsList[i++]);
                    }
                }
            }
            return command;
        }
    }
}
