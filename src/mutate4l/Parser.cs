using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mutate4l
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
                var token = tokensAsList[i++];
                if (token.Type > TokenType._OptionsBegin && token.Type < TokenType._OptionsEnd)
                {
                    var type = token.Type;
                    var values = new List<string>();

                    while (tokensAsList[i + 1].Type > TokenType._ValuesBegin && tokensAsList[i + 1].Type < TokenType._ValuesEnd)
                    {
                        values.Add(tokensAsList[++i].Value);
                    }
                }
            }

            /*                if (token.Type > TokenType._CommandsBegin && token.Type < TokenType._CommandsEnd)
                            {
                                command.Id = token.Type;
                            }
            */
            return command;
        }
    }
}
