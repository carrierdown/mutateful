using Mutate4l.Core;
using Mutate4l.Dto;
using static Mutate4l.Cli.TokenType;

namespace Mutate4l.Cli
{
    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Position { get; }
        public Clip Clip { get; }

        public Token(TokenType type, string value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }

        public Token(TokenType type, string value, Clip clip, int position) : this(type, value, position)
        {
            Clip = clip;
        }

        public bool IsClipReference => Type == TokenType.ClipReference;
        public bool IsOption => Type > _OptionsBegin && Type < _OptionsEnd;
        public bool IsCommand => Type > _CommandsBegin && Type < _CommandsEnd;
        public bool IsOptionValue => (Type > _EnumValuesBegin && Type < _EnumValuesEnd) || (Type > _ValuesBegin && Type < _ValuesEnd);
    }
}
