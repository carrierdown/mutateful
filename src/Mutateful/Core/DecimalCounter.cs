namespace Mutateful.Core;

public class DecimalCounter
{
    public decimal Value { get; private set; }

    private decimal Max { get; }

    public bool Overflow { get; private set; }

    public DecimalCounter(decimal max)
    {
        Max = max;
    }

    public void Inc(decimal amount)
    {
        Value += amount;
        if (Value >= Max)
        {
            Value = 0;
            Overflow = true;
        }
    }
}