using static Mutate4l.Cli.TokenType;

namespace Mutate4l.Cli
{
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

        public bool IsClipReference => Type == ClipReference;
        public bool IsOption => Type > _OptionsBegin && Type < _OptionsEnd;
        public bool IsCommand => Type > _CommandsBegin && Type < _CommandsEnd;
        public bool IsOptionValue => (Type > _EnumValuesBegin && Type < _EnumValuesEnd) || Type == Number || Type == MusicalDivision;
    }
}
