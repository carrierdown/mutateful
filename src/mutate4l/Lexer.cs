using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;

namespace Mutate4l
{
    public enum TokenType
    {
        _CommandsBegin,
        Interleave,
        ConstrainStart,
        ConstrainPitch,
        Slice,
        Explode,
        _CommandsEnd,
        _OptionsBegin,
        Start,
        Pitch,
        Range,
        Count,
        _OptionsEnd,
        Colon,
        Destination,
        ClipReference
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Position { get; set; }

        public Token(TokenType type, string value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }
    }

    public class Lexer
    {
        private string Buffer;
        private int Position = 0;

        private Dictionary<char, TokenType> SingleOperators = new Dictionary<char, TokenType>
        {
            { ':', TokenType.Colon }
        };

        private Dictionary<string, TokenType> DoubleOperators = new Dictionary<string, TokenType>
        {
            { "=>", TokenType.Destination }
        };

        private Dictionary<string, TokenType> Commands = new Dictionary<string, TokenType>
        {
            { "interleave", TokenType.Interleave },
            { "constrainstart", TokenType.ConstrainStart },
            { "constrainpitch", TokenType.ConstrainPitch },
            { "explode", TokenType.Explode }
        };

        private Dictionary<string, TokenType> Options = new Dictionary<string, TokenType>
        {
            { "start", TokenType.Start },
            { "pitch", TokenType.Pitch },
            { "range", TokenType.Range },
            { "count", TokenType.Count }
        };

        public Lexer(string buffer)
        {
            Buffer = buffer;
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

            return (Buffer.Length > pos + 1) && IsAlpha(pos) && IsNumeric(pos + 1);
        }

        private bool IsAlpha(int pos)
        {
            char c = Buffer[pos];
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private bool IsNumeric(int pos)
        {
            char c = Buffer[pos];
            return c >= '0' && c <= '9';
        }

        private Token GetIdentifier(int pos, Dictionary<string, TokenType> validValues)
        {
            string identifier = "";
            int initialPos = pos;

            while (IsAlpha(pos))
            {
                identifier += Buffer[pos++].ToString();
            }
            if (identifier.Length > 0 && validValues.Any(v => v.Key == identifier))
            {
                return new Token(validValues.First(v => v.Key == identifier).Value, identifier, initialPos);
            }
            return null;
        }

        public IEnumerable<Token> GetTokens()
        {
            while (Position < Buffer.Length)
            {
                Token token = null;
                if (IsSingleOperator(Position))
                {
                    char value = Buffer[Position];
                    token = new Token(SingleOperators[value], value.ToString(), Position);
                }
                else if (IsDoubleOperator(Position))
                {
                    string value = $"{Buffer[Position]}{Buffer[Position + 1]}";
                    token = new Token(DoubleOperators[value], value, Position);
                }
                else if (IsClipReference(Position))
                {
                    string value = $"{Buffer[Position]}{Buffer[Position + 1]}";
                    int pos = 2;
                    while (Position + pos < Buffer.Length && IsNumeric(Position + pos))
                    {
                        value += Buffer[Position + pos++].ToString();
                    }

                    token = new Token(TokenType.ClipReference, value, Position);
                }
                else if (IsAlpha(Position))
                {
                    Token identifierToken = GetIdentifier(Position, Commands);
                    if (identifierToken != null)
                    {
                        token = identifierToken;
                    }

                    identifierToken = GetIdentifier(Position, Options);
                    if (identifierToken != null)
                    {
                        token = identifierToken;
                    }
                }
                if (token != null)
                {
                    Position += token.Value.Length;
                    yield return token;
                }
                Position = SkipNonTokens(Position);
            }
            yield break;
        }

        private int SkipNonTokens(int position)
        {
            while (position < Buffer.Length && (Buffer[position] == ' ' || Buffer[position] == '\t' || Buffer[position] == '\r' || Buffer[position] == '\n'))
            {
                position++;
            }
            return position;
        }
    }
}
