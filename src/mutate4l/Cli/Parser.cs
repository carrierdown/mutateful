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
