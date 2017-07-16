using Mutate4l.Cli;

namespace Mutate4l.Dto
{
    public enum OptionGroupType
    {
        InverseToggle, // If none specified, all are active. Otherwise, only specified options are active.
        Value // Option that takes one or more values.
    }

    public class OptionsDefinition
    {
        public OptionGroup[] OptionGroups;
    }

    public class OptionGroup
    {
        public OptionGroupType Type;
        public Option[] Options;
    }

    public class Option
    {
        public TokenType Name { get; }
        public dynamic[] Values { get; }

        public Option(TokenType name, dynamic[] values)
        {
            Name = name;
            Values = values;
        }

        public Option(TokenType name) : this(name, new dynamic[0]) { }
    }
}
