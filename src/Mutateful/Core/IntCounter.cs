namespace Mutateful.Core
{
    public class IntCounter
    {
        public int Value { get; private set; }

        private int Max { get; }

        public bool Overflow { get; private set; }

        public IntCounter(int max)
        {
            Max = max;
        }

        public void Inc()
        {
            Inc(1);
        }

        public void Inc(int amount)
        {
            Value += amount;
            if (Value >= Max)
            {
                Value = 0;
                Overflow = true;
            }
        }
    }
}
