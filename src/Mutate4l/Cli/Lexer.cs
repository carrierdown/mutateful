using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using static Mutate4l.Cli.TokenType;

namespace Mutate4l.Cli
{
    public class Lexer
    {
        private readonly string Buffer;
        private readonly List<Clip> Clips;

        private Dictionary<char, TokenType> SingleOperators = new Dictionary<char, TokenType>
        {
            { ':', Colon }
        };

        private Dictionary<string, TokenType> DoubleOperators = new Dictionary<string, TokenType>
        {
            { "=>", Destination },
            { "+>", AddToDestination }
        };

        private Dictionary<string, TokenType> Commands = new Dictionary<string, TokenType>
        {
            { "interleave", Interleave },
            { "intrlv", Interleave },
            { "il", Interleave },
            { "interleavevent", InterleaveEvent },
            { "intrlvnt", InterleaveEvent },
            { "ilvt", InterleaveEvent },

            { "constrain", Constrain },
            { "shuffle", Shuffle },
            { "slice", Slice },
            { "arpeggiate", Arpeggiate },
            { "monophonize", Monophonize },
            { "ratchet", Ratchet },
            { "relength", Relength },
            { "scan", Scan },
            { "take", Take },
            { "filter", Filter },
            { "transpose", Transpose }
        };

        private Dictionary<string, TokenType> Options = new Dictionary<string, TokenType>
        {
            { "-start", Start },
            { "-pitch", Pitch },
            { "-ranges", Ranges },
            { "-repeats", Repeats },
            { "-mode", Mode },
            { "-skip", Skip }, // rename to skip?
            { "-strength", Strength },
            { "-lengths", Lengths },
            { "-rescale", Rescale },
            { "-removeoffset", RemoveOffset },
            { "-min", Min },
            { "-max", Max },
            { "-shape", Shape },
            { "-autoscale", AutoScale },
            { "-controlmin", ControlMin },
            { "-controlmax", ControlMax },
            { "-velocitytostrength", VelocityToStrength },
            { "-window", Window },
            { "-count", Count },
            { "-duration", Duration },
            { "-enablemask", EnableMask },
            { "-chunkchords", ChunkChords },
            { "-by", By },
            { "-factor", Factor },
            { "-with", With },
            { "-solo", Solo }
        };

        private Dictionary<string, TokenType> EnumValues = new Dictionary<string, TokenType>
        {
            { "time", Time },
            { "event", Event },
            { "linear", Linear },
            { "easeinout", EaseInOut },
            { "easein", EaseIn },
            { "pitch", Pitches },
            { "rhythm", Rhythm },
            { "both", Both },
            { "absolute", Absolute },
            { "relative", Relative }
        };

        public Lexer(string buffer, List<Clip> clips)
        {
            Buffer = buffer;
            Clips = clips;
        }

        private bool IsSingleOperator(int pos)
        {
            return SingleOperators.Any(o => o.Key == Buffer[pos]);
        }

        private bool IsDoubleOperator(int pos)
        {
            if (Buffer.Length > pos + 1)
            {
                string nextTwoChars = $"{Buffer[pos]}{Buffer[pos + 1]}";
                return DoubleOperators.Any(o => o.Key == nextTwoChars);
            }
            return false;
        }

        private bool IsClipReference(int pos)
        {
            return (Buffer.Length > pos + 1 && IsAlpha(pos) && IsNumeric(pos + 1)) || Buffer[pos] == '*';
        }

        private bool IsInlineClip(int pos)
        {
            if (Buffer[pos] == '[')
            {
                int i = pos + 1;
                while(i < Buffer.Length && (IsNumeric(Buffer[i]) || Buffer[i] == ' ' || Buffer[i] == '.' || Buffer[i] == 'e' || Buffer[i] == '-' || Buffer[i] == ',' || Buffer[i] == ':'))
                {
                    i++;
                }
                if (Buffer[i] == ']')
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsMusicalDivision(int pos)
        {
            return Buffer.Length > pos + 2 && IsNumeric(pos) && Buffer[pos + 1] == '/' && IsNumeric(pos + 2);
        }

        private bool IsAlpha(int pos)
        {
            return IsAlpha(Buffer[pos]);
        }

        private bool IsOption(int pos)
        {
            return Buffer[pos] == '-';
        }

        public static bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private bool IsNumeric(int pos)
        {
            return IsNumeric(Buffer[pos]);
        }

        public static bool IsNumeric(char c)
        {
            return c >= '0' && c <= '9';
        }
        /*
        private Token GetIdentifier(int pos)
        {
            var isOptionHeader = IsOption(pos);

            int length = 1;
            while (pos + length < Buffer.Length && (IsAlpha(pos + length)) {
                length++;
            }
            return new Token(isOptionHeader ? TokenType.OptionHeader : TokenType)


        }*/

        private Token GetIdentifier(int pos, params Dictionary<string, TokenType>[] validValues)
        {
            string identifier = "";
            int initialPos = pos;
            int length = 0;

            while (pos < Buffer.Length && (IsAlpha(pos) || (initialPos == pos && IsOption(pos))))
            {
                length++;
                pos++;
            }
            identifier = Buffer.Substring(initialPos, length);

            if (identifier.Length > 0 && validValues.Any(va => va.Any(v => v.Key == identifier)))
            {
                return new Token(validValues.Where(va => va.Any(v => v.Key.Equals(identifier, StringComparison.InvariantCultureIgnoreCase))).First()[identifier.ToLower()], identifier, initialPos);
            }
            throw new Exception($"Unknown token encountered at position {initialPos}: {identifier}");
        }

        public IEnumerable<Token> GetTokens()
        {
            int position = 0;
            while (position < Buffer.Length) // todo: add safety net here to avoid endless loop on unrecognized input
            {
                Token token = null;
                if (IsSingleOperator(position))
                {
                    char value = Buffer[position];
                    token = new Token(SingleOperators[value], value.ToString(), position);
                }
                else if (IsDoubleOperator(position))
                {
                    string value = $"{Buffer[position]}{Buffer[position + 1]}";
                    token = new Token(DoubleOperators[value], value, position);
                }
                else if (IsClipReference(position))
                {
                    if (Buffer[position] == '*')
                        token = new Token(TokenType.ClipReference, "*", position);
                    else
                        token = new Token(TokenType.ClipReference, GetRemainingNumericToken(position, 2), position);
                }
                else if (IsInlineClip(position))
                {
                    var clipRef = Buffer.Substring(position, (Buffer.IndexOf(']', position) - position + 1));
                    int clipIx = int.Parse(clipRef.Substring(1, clipRef.Length - 2));
                    token = new Token(InlineClip, clipRef, Clips[clipIx], position);
                }
                else if (IsMusicalDivision(position))
                {
                    token = new Token(MusicalDivision, GetRemainingNumericToken(position, 3), position);
                }
                else if (IsNumeric(position))
                {
                    token = new Token(Number, GetRemainingNumericToken(position, 1), position);
                }
                else if (IsAlpha(position))
                {
                    Token identifierToken = GetIdentifier(position, Commands, EnumValues);
                    token = identifierToken;
                }
                else if (IsOption(position))
                {
                    Token identifierToken = GetIdentifier(position, Options);
                    token = identifierToken;
                }
                if (token != null)
                {
                    position += token.Value.Length;
                    yield return token;
                } else
                {

                }
                position = SkipNonTokens(position);
            }
            yield break;
        }

        // Fetches the remainder of a token consisting of numeric digits, with an optional offset which can be used in the case of values like 1/32 where you know the first 3 digits to be valid
        private string GetRemainingNumericToken(int position, int offset)
        {
            while (position + offset < Buffer.Length && IsNumeric(position + offset))
            {
                offset++;
            }
            return Buffer.Substring(position, offset);
        }

        private int SkipNonTokens(int position)
        {
            while (position < Buffer.Length && (Buffer[position] == ' ' || Buffer[position] == '\t' || Buffer[position] == '\r' || Buffer[position] == '\n'))
            {
                position++;
            }
            return position;
        }

        public bool IsValidCommand()
        {
            // to be expanded...
            return Commands.Keys.Any(c => Buffer.Contains(c));
        }
    }
}
