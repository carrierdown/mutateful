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

        public static bool TryExtractMetaData(string formula, out ClipMetaData metaData)
        {
            int index = formula.IndexOf('{') + 1;
            string rawMetaData = formula.Substring(index, formula.IndexOf('}') - index);
            var metaDataPairs = rawMetaData.Split(',');
            string[] metaDataHeaders = new string[] { "id:", "trackIx:" };
            int succesfullyExtractedParams = 0;
            metaData.Id = "";
            metaData.TrackNumber = -1;

            foreach (var metaDataPair in metaDataPairs)
            {
                int ix, value;
                if ((ix = metaDataPair.IndexOf(metaDataHeaders[0])) >= 0)
                {
                    metaData.Id = metaDataPair.Substring(ix + metaDataHeaders[0].Length);
                    succesfullyExtractedParams++;
                }
                else if ((ix = metaDataPair.IndexOf(metaDataHeaders[1])) >= 0 && int.TryParse(metaDataPair.Substring(ix + metaDataHeaders[1].Length), out value))
                {
                    metaData.TrackNumber = value;
                    succesfullyExtractedParams++;
                }
            }
            return succesfullyExtractedParams == metaDataHeaders.Length;
        }

        public static ProcessResult<ChainedCommand> ParseFormulaToChainedCommand(string formula)
        {
            var valid = new char[] { '{', '[', ']', '}' }.All(c => formula.IndexOf(c) >= 0);
            if (!valid) return new ProcessResult<ChainedCommand>($"Invalid formula: {formula}");

            ClipMetaData metadata;
            if (!TryExtractMetaData(formula, out metadata))
                return new ProcessResult<ChainedCommand>($"Unable to extract metadata for formula: {formula}");

            string command = formula.Substring(formula.LastIndexOf('}') + 1);

            var lexer = new Lexer(command);
            Token[] commandTokens = lexer.GetTokens().ToArray();
            var commandTokensLists = new List<List<Token>>();
            var activeCommandTokenList = new List<Token>();

            var sourceClips = commandTokens.TakeWhile(x => x.Type == TokenType.InlineClip).Select(x => IOUtilities.StringToClip(x.Value.Substring(1, x.Value.Length - 2))).ToArray();
            var tokensToProcess = commandTokens.Skip(sourceClips.Count());

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
