namespace Mutateful.Core;

public enum OptionType
{
    //AllOrSpecified, // If none specified, all are active. Otherwise, only specified options are active.
    Value, // Option that takes one or more values.
    Default // If values are given without an option header, use them for this parameter.
}

[AttributeUsage(AttributeTargets.Property)]
public class OptionInfo : Attribute
{
    public OptionType Type { get; } = OptionType.Value;
    public int? MinNumberValue { get; }
    public int? MaxNumberValue { get; }        
    public float? MinDecimalValue { get; }
    public float? MaxDecimalValue { get; }
    public bool NoImplicitCast { get; } // Decimal values are usually assumed to be interchangeable with MusicalDivision and Number (i.e. numbers and musical divisions are converted to decimal values). Specify this to avoid this implicit casting, useful for some decimal parameters such as scale factors and such

    public OptionInfo(OptionType type)
    {
        Type = type;
    }

    public OptionInfo(OptionType type, bool noImplicitCast) : this(type)
    {
        NoImplicitCast = noImplicitCast;
    }

    public OptionInfo(int min, int max)
    {
        MinNumberValue = min;
        MaxNumberValue = max;
    }

    public OptionInfo(int min)
    {
        MinNumberValue = min;
    }        
    
    public OptionInfo(OptionType type, int min, int max, bool noImplicitCast = false) : this(type, noImplicitCast)
    {
        MinNumberValue = min;
        MaxNumberValue = max;
    }

    public OptionInfo(OptionType type, int min, bool noImplicitCast = false) : this(type, noImplicitCast)
    {
        MinNumberValue = min;
    }

    public OptionInfo(float min, float max)
    {
        MinDecimalValue = min;
        MaxDecimalValue = max;
    }

    public OptionInfo(float min)
    {
        MinDecimalValue = min;
    }

    public OptionInfo(OptionType type, float min, float max, bool noImplicitCast = false) : this(type, noImplicitCast)
    {
        MinDecimalValue = min;
        MaxDecimalValue = max;
    }

    public OptionInfo(OptionType type, float min, bool noImplicitCast = false) : this(type, noImplicitCast)
    {
        MinDecimalValue = min;
    }
}