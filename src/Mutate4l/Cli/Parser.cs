using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mutate4l.Dto;

namespace Mutate4l.Cli
{
    public class Parser
    {
        public static Tuple<int, int> ResolveClipReference(string reference)
        {
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
        public static ChainedCommand ParseTokensToChainedCommand(IEnumerable<Token> tokens)
        {
            var sourceClips = tokens.TakeWhile(t => t.IsClipReference);
            var commandTokens = tokens.Skip(sourceClips.Count()).TakeWhile(t => t.IsCommand || t.IsOption || t.IsOptionValue).ToArray();
            var destClips = tokens.Skip(sourceClips.Count() + commands.Count()).TakeWhile(t => t.IsClipReference);

            var commands = new List<List<Token>>();
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
                        commands.Add(activeCommandTokenList);
                        activeCommandTokenList = new List<Token> { token };
                    }
                }
                else
                {
                    activeCommandTokenList.Add(token);
                }
            }

            var i = 0;
            while (i < commands.Length)
            {
                if (commands[i].IsCommand)
                {
                    var command = new Command() { Id = commands[i].Type };
                    var values = new List<Token>();
                    i++;
                    while (i < commands.Length && (commands[i].IsOption || commands[i).IsOptionValue) {
                        values.Add(commands[i++]);
                    }
                    command.Options.Add(type, values);
                }
                else
                {
                    // error: not a valid command
                }
            }
        }*/

        public static Command ParseTokensToCommand(IEnumerable<Token> tokens)
        {
            var command = new Command();
            
            List<Token> tokensAsList = tokens.ToList();
            command.Id = tokensAsList[0].Type;
            var i = 1;
            while (i < tokensAsList.Count)
            {
                if (tokensAsList[i].Type > TokenType._OptionsBegin && tokensAsList[i].Type < TokenType._OptionsEnd)
                {
                    var type = tokensAsList[i].Type;
                    var values = new List<Token>();
                    i++;
                    while (i < tokensAsList.Count && (tokensAsList[i].Type > TokenType._ValuesBegin && tokensAsList[i].Type < TokenType._ValuesEnd) 
                        || (tokensAsList[i].Type > TokenType._EnumValuesBegin && tokensAsList[i].Type < TokenType._EnumValuesEnd))
                    {
                        values.Add(tokensAsList[i++]);
                    }
                    command.Options.Add(type, values);
                }
                else if (tokensAsList[i].Type == TokenType.Destination)
                {
                    i++;
                    while (i < tokensAsList.Count && tokensAsList[i].Type == TokenType.ClipReference)
                    {
                        command.TargetClips.Add(ResolveClipReference(tokensAsList[i++].Value));
                    }
                }
                while (i < tokensAsList.Count && tokensAsList[i].Type == TokenType.ClipReference)
                {
                    command.SourceClips.Add(ResolveClipReference(tokensAsList[i++].Value));
                }
            }
            return command;
        }
    }
}
