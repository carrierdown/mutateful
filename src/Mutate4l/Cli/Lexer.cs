using System;
using System.Collections.Generic;
using System.Linq;
using Mutate4l.Core;
using static Mutate4l.Cli.TokenType;

namespace Mutate4l.Cli
{
    public class Lexer
    {
        private readonly string Buffer;
        private readonly List<Clip> Clips;
        private int Position = 0;
        private readonly Token NonToken = new Token(NoToken, "", -1);

        private Dictionary<char, TokenType> SingleOperators = new Dictionary<char, TokenType>
        {
            { ':', RangeOperator },
            { '\'', AlternationOperator },
            { '_', EmptyOperator },
            { '*', FillOperator }
        };

        private Dictionary<string, TokenType> Commands = new Dictionary<string, TokenType>
        {
            { "arpeggiate", Arpeggiate }, { "arp", Arpeggiate },
            { "concat", Concat }, { "cat", Concat },
            { "scale", Scale }, { "constrain", Scale /* Alias for previous name for scale */ },
            { "crop", Crop },
            { "filter", Filter }, { "flt", Filter },
            { "il", Interleave }, { "interleave", Interleave },
            { "ilev", InterleaveEvent }, { "interleaveevent", InterleaveEvent },
            { "legato", Legato }, { "leg", Legato },
            { "mask", Mask},
            { "monophonize", Monophonize }, { "mono", Monophonize },
            { "padding", Padding }, { "pad", Padding },  { "p", Padding },
            { "quantize", Quantize }, { "qnt", Quantize },
            { "ratchet", Ratchet }, { "rat", Ratchet },
            { "relength", Relength }, { "relen", Relength },
            { "remap", Remap }, 
            { "resize", Resize },
            { "scan", Scan },
            { "setlength", SetLength }, { "length", SetLength }, { "len", SetLength },
            { "setrhythm", SetRhythm }, { "rhythm", SetRhythm }, { "rm", SetRhythm },
            { "shuffle", Shuffle }, { "shf", Shuffle },
            { "skip", Skip },
            { "slice", Slice },
            { "take", Take },
            { "transpose", Transpose }, { "tran", Transpose }
        };

        private Dictionary<string, TokenType> Options = new Dictionary<string, TokenType>
        {
            { "-amount", Amount }, { "amt", Amount },
            { "-autoscale", AutoScale }, { "-auto", AutoScale },
            { "-by", By },
            { "-chunkchords", ChunkChords }, { "-chunk", ChunkChords },
            { "-controlmax", ControlMax },
            { "-controlmin", ControlMin },
            { "-count", Count }, { "-cnt", Count },
            { "-divisions", Divisions }, { "-div", Divisions },
            { "-duration", Duration }, { "-dur", Duration },
            { "-enablemask", EnableMask },
            { "-factor", Factor },
            { "-invert", Invert },
            { "-inv", Invert },
            { "-length", Length }, { "-len", Length },
            { "-lengths", Lengths }, { "-lens", Lengths },
            { "-magnetic", Magnetic }, { "-mag", Magnetic },
            { "-max", Max },
            { "-min", Min },
            { "-mode", Mode },
            { "-padamount", PadAmount },
            { "-pitch", Pitch },
            { "-post", Post },
            { "-ranges", Ranges },
            { "-ratchetvalues", RatchetValues },
            { "-removeoffset", RemoveOffset }, { "-remoff", RemoveOffset },
            { "-repeats", Repeats }, { "-rep", Repeats },
            { "-rescale", Rescale },
            { "-shape", Shape },
            { "-shufflevalues", ShuffleValues },
            { "-skip", Skip },
            { "-skipcounts", SkipCounts},
            { "-solo", Solo },
            { "-start", Start },
            { "-strength", Strength },
            { "-strict", Strict },
            { "-takecounts", TakeCounts	},
            { "-transposevalues", TransposeValues },
            { "-threshold", Threshold }, { "-thres", Threshold },
            { "-to", To},
            { "-velocitytostrength", VelocityToStrength }, { "-veltostr", VelocityToStrength },
            { "-window", Window },
            { "-with", With }
        };

        private Dictionary<string, TokenType> EnumValues = new Dictionary<string, TokenType>
        {
            { "absolute", Absolute },
            { "both", Both },
            { "easein", EaseIn },
            { "easeout", EaseOut },
            { "easeinout", EaseInOut },
            { "event", Event },
            { "linear", Linear },
            { "overwrite", Overwrite },
            { "pitch", Pitches },
            { "velocity", Velocity },
            { "relative", Relative },
            { "rhythm", Rhythm },
            { "time", Time }
        };

        public Lexer(string buffer, List<Clip> clips)
        {
            Buffer = buffer;
            Clips = clips;
        }

        private bool IsSingleOperator(int pos)
        {
            return SingleOperators.Any(o => o.Key == Buffer[pos]) || IsRepeatOperator(pos);
        }

        private bool IsRepeatOperator(int pos)
        {
            return pos + 1 < Buffer.Length && !IsAlpha(pos + 1) && (Buffer[pos] == 'x' || Buffer[pos] == 'X');
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
                while(i < Buffer.Length && (IsNumeric(Buffer[i]) || IsAlpha(Buffer[i]) || Buffer[i] == '<' || Buffer[i] == '>'))
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

        public bool IsDecimalValue(int pos)
        {
            var i = pos;
            var decimalPointFoundAt = -1;
            while (i < Buffer.Length && (IsNumeric(Buffer[i]) || Buffer[i] == '.'))
            {
                if (Buffer[i] == '.')
                {
                    if (decimalPointFoundAt < 0) decimalPointFoundAt = i - pos;
                    else return false;
                }
                i++;
            }
            if (decimalPointFoundAt < 0) return false;
            return i > pos + decimalPointFoundAt + 1;
        }

        public bool IsMusicalDivision(int pos)
        {
            var i = pos;
            var slashFoundAt = -1;
            while (i < Buffer.Length && (IsNumeric(Buffer[i]) || (Buffer[i] == '/' && i > pos)))
            {
                if (Buffer[i] == '/')
                {
                    if (slashFoundAt < 0) slashFoundAt = i - pos;
                    else return false;
                }
                i++;
            }
            if (slashFoundAt < 0) return false;
            return i > pos + slashFoundAt + 1;
        }
        
        public bool IsBarsBeatsSixteenths(int pos)
        {
            var foundPeriods = 0;
            var periodIxs = new int[2];
            var i = pos;
            while (i < Buffer.Length && (IsNumeric(Buffer[i]) || Buffer[i] == '.'))
            {
                if (Buffer[i] == '.')
                {
                    if (foundPeriods < periodIxs.Length)
                    {
                        periodIxs[foundPeriods++] = i - pos;
                    }
                    else
                    {
                        return false;
                    }
                }
                i++;
            }
            return foundPeriods == 2 && periodIxs[1] - periodIxs[0] > 1 && periodIxs[0] > 0 && periodIxs[1] < i - pos - 1;
        }

        private bool IsAlpha(int pos)
        {
            return IsAlpha(Buffer[pos]);
        }

        private bool IsOption(int pos)
        {
            return (Buffer.Length > pos + 1 && Buffer[pos] == '-' && IsAlpha(pos + 1));
        }

        public static bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private bool IsNumeric(int pos)
        {
            return IsNumeric(Buffer[pos]) || (Buffer.Length > pos + 1 && Buffer[pos] == '-' && IsNumeric(pos + 1));
        }

        public static bool IsNumeric(char c)
        {
            return c >= '0' && c <= '9';
        }
        
        private (bool Success, String ErrorMessage, Token Token) GetIdentifier(int pos, params Dictionary<string, TokenType>[] validValues)
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
                return (true, "", new Token(validValues.First(va => va.Any(v => v.Key.Equals(identifier, StringComparison.InvariantCultureIgnoreCase)))[identifier.ToLower()], identifier, initialPos));
            }
            return (false, $"Unknown token encountered at position {initialPos}: {identifier}", NonToken);
        }

        public ProcessResultArray<Token> GetTokens()
        {
            bool success, empty;
            string msg = "";
            var tokens = new List<Token>();

            while (true)
            {
                (success, empty, msg) = TryGetToken(out Token token);
                if (!success || empty)
                {
                    break;
                }
                tokens.Add(token);
            }

            if (!success && !empty)
            {
                return new ProcessResultArray<Token>(msg);
            }
            return new ProcessResultArray<Token>(tokens.ToArray());
        }

        public (bool Success, bool Empty, string ErrorMessage) TryGetToken(out Token token)
        {
            token = NonToken;
            while (Position < Buffer.Length) // todo: add safety net here to avoid endless loop on unrecognized input
            {
                if (IsSingleOperator(Position))
                {
                    char value = Buffer[Position];
                    if (SingleOperators.ContainsKey(value))
                    {
                        token = new Token(SingleOperators[value], value.ToString(), Position);
                    }
                    else
                    {
                        token = new Token(TokenType.RepeatOperator, "x", Position);
                    }
                }
                else if (IsClipReference(Position))
                {
                    if (Buffer[Position] == '*')
                        token = new Token(TokenType.ClipReference, "*", Position);
                    else
                        token = new Token(TokenType.ClipReference, GetRemainingNumericToken(Position, 2), Position);
                }
                else if (IsInlineClip(Position))
                {
                    // Inline clips are now sent in the format [<A2>0], i.e. original clip ref used for denoting the clip,
                    // followed by the internal ix. This makes autogenerated documentation possible, and possibly other use cases as well.
                    var inlineClip = Buffer.Substring(Position, (Buffer.IndexOf(']', Position) - Position + 1));

                    // Check whether clipRefs are passed in with new format, e.g. [<A2>0], or old format, e.g. [0].
                    int clipIx;
                    if (inlineClip.Contains('<'))
                    {
                        string rawClipRef = "";
                        (rawClipRef, clipIx) = ParseInlineClip(inlineClip);
                        Clips[clipIx].RawClipReference = rawClipRef;
                    }
                    else
                    {
                        clipIx = int.Parse(inlineClip.Substring(1, inlineClip.Length - 2));
                    }
                    token = new Token(InlineClip, inlineClip, Clips[clipIx], Position);
                }
                else if (IsMusicalDivision(Position))
                {
                    token = new Token(MusicalDivision, GetMusicalDivisionToken(Position), Position);
                }
                else if (IsDecimalValue(Position))
                {
                    token = new Token(TokenType.Decimal, GetDecimalToken(Position), Position);
                }
                else if (IsBarsBeatsSixteenths(Position))
                {
                    token = new Token(BarsBeatsSixteenths, GetDecimalToken(Position), Position);
                }
                else if (IsNumeric(Position))
                {
                    token = new Token(Number, GetRemainingNumericToken(Position, 1), Position);
                }
                else if (IsAlpha(Position))
                {
                    var (success, msg, identifierToken) = GetIdentifier(Position, Commands, EnumValues);
                    if (!success) return (false, false, msg);
                    token = identifierToken;
                }
                else if (IsOption(Position))
                {
                    var (success, msg, identifierToken) = GetIdentifier(Position, Options);
                    if (!success) return (false, false, msg);
                    token = identifierToken;
                }
                if (token.Type != NoToken)
                {
                    Position += token.Value.Length;
                    return (true, false, "");
                }
                int prevPos = Position;
                SkipNonTokens();
                if (prevPos == Position)
                {
                    return (false, false, $"Unrecognized input at position {Position}:\\r\\n{GetErroneousTokenExcerpt()}");
                }
            }
            return (false, true, "No more tokens available");
        }

        private static (string rawClipRef, int clipIx) ParseInlineClip(string clipRef)
        {
            var rawClipRefIx = clipRef.IndexOf('<') + 1;
            var rawClipRefLength = clipRef.IndexOf('>') - rawClipRefIx;
            var rawClipRef = clipRef.Substring(rawClipRefIx, rawClipRefLength);
            var clipIxOffset = 1 /* [ */ + 1 /* < */ + rawClipRefLength /* e.g. A2 */ + 1 /* > */;
            var clipIx = int.Parse(clipRef.Substring(clipIxOffset, clipRef.Length - clipIxOffset - 1));
            return (rawClipRef, clipIx);
        }

        private string GetErroneousTokenExcerpt()
        {
            int backTrack = Math.Max(Position - 3, 0);
            int fwdTrack = Math.Min(Buffer.Length - Position - 1, 7);
            char[] errorIndicator = new char[backTrack + fwdTrack];
            for (var i = 0; i < errorIndicator.Length; i++)
            {
                if (i == backTrack)
                    errorIndicator[i] = '^';
                else
                    errorIndicator[i] = ' ';
            }
            return Buffer.Substring(Position - backTrack, fwdTrack + backTrack) + @"\r\n" + new string(errorIndicator);
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
        
        private string GetDecimalToken(int position)
        {
            int offset = 0;
            while (position + offset < Buffer.Length && (IsNumeric(Buffer[position + offset]) || Buffer[position + offset] == '.')) 
            {
                offset++;
            }
            return Buffer.Substring(position, offset);
        }

        private string GetMusicalDivisionToken(int position)
        {
            int offset = 0;
            while (position + offset < Buffer.Length && (IsNumeric(Buffer[position + offset]) || Buffer[position + offset] == '/')) 
            {
                offset++;
            }
            return Buffer.Substring(position, offset);
        }

        private void SkipNonTokens()
        {
            while (Position < Buffer.Length && (Buffer[Position] == ' ' || Buffer[Position] == '\t' || Buffer[Position] == '\r' || Buffer[Position] == '\n'))
            {
                Position++;
            }
        }

        public bool IsValidCommand()
        {
            // to be expanded...
            return Commands.Keys.Any(c => Buffer.Contains(c));
        }
    }
}
