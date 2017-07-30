using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Core
{
    public enum OptionType
    {
        InverseToggle, // If none specified, all are active. Otherwise, only specified options are active.
        Value // Option that takes one or more values.
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class OptionInfo : Attribute
    {
        public int GroupId { get; }
        public OptionType Type { get; }

        public OptionInfo(int groupId, OptionType type)
        {
            GroupId = groupId;
            Type = type;
        }

        public OptionInfo(OptionType type) : this(0, type) { }
    }
}
