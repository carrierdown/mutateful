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
        public TokenType[] Options;
    }
}
