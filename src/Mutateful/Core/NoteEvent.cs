namespace Mutateful.Core;

public record NoteEvent(float VelocityDeviation = 0, float ReleaseVelocity = 64, float Probability = 1) : IComparable<NoteEvent>
{
    private int PitchField;
    private float VelocityField;
    private decimal StartField;

    public int Pitch
    {
        get => PitchField & 0x7F;
        set {
            if (HasChildren)
            {
                var delta = value - PitchField;
                Children.ForEach(c => c.Pitch += delta);
            }
            PitchField = value;
        }
    }

    public decimal Duration { get; set; }
    
    public float Velocity
    {
        get => Math.Clamp(VelocityField, 0f, 127f);
        set => VelocityField = value;
    }

    public decimal End => Start + Duration;

    public List<NoteEvent> Children = EmptyList;
    
    private static readonly List<NoteEvent> EmptyList = new(); 

    public bool HasChildren => Children?.Count > 0;

    public decimal Start
    {
        get => StartField;
        set
        {
            StartField = value;
            if (HasChildren)
            {
                Children.ForEach(c => c.Start = StartField);
            }
        }
    }

    public NoteEvent Parent { get; set; }

    public NoteEvent(int pitch, decimal start, decimal duration, float velocity, float probability = 1,
        float velocityDeviation = 0, float releaseVelocity = 64) : this(probability, velocityDeviation, releaseVelocity)
    {
        Pitch = pitch;
        Start = start;
        Velocity = velocity;
        Duration = duration;
    }

    public int CompareTo(NoteEvent b)
    {
        if (Start < b.Start)
        {
            return -1;
        }
        if (Start > b.Start)
        {
            return 1;
        }
        if (Pitch < b.Pitch)
        {
            return -1;
        }
        if (Pitch > b.Pitch)
        {
            return 1;
        }
        return 0;
    }
}