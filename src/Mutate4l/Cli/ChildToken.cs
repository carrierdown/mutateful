namespace Mutate4l.Cli
{
    public class ChildToken
    {
        public TokenType Type { get; }
        public string Value { get; }

        public ChildToken(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}