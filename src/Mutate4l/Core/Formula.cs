using System.Collections.Generic;

namespace Mutate4l.Core
{
    public class Formula
    {
        public List<Command> Commands { get; }

        public static readonly Formula Empty = new Formula();

        public Formula()
        {
            Commands = new List<Command>();
        }

        public Formula(List<Command> commands)
        {
            Commands = commands;
        }
    }
}