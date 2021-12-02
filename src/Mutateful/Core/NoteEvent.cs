namespace Mutateful.Core;

public class NoteEvent : IComparable<NoteEvent>, IEquatable<NoteEvent>
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
    public float Probability { get; set; } = 1;
    public float VelocityDeviation { get; set; }
    public float ReleaseVelocity { get; set; } = 64;

    public float Velocity
    {
        get => Math.Clamp(VelocityField, 0f, 127f);
        set
        {
            if (value == 0)
            {
                Console.WriteLine($"I was set to {value}");
            }
            VelocityField = value;
        }
    }

    public decimal End => Start + Duration;

    public bool IsSelected { get; set; }

    public List<NoteEvent> Children { get; set; }

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

    public NoteEvent(int pitch, decimal start, decimal duration, float velocity, float probability,
        float velocityDeviation, float releaseVelocity)
    {
        Pitch = pitch;
        Start = start;
        Duration = duration;
        Velocity = velocity;
        Probability = probability;
        VelocityDeviation = velocityDeviation;
        ReleaseVelocity = releaseVelocity;
    }

    public NoteEvent(int pitch, decimal start, decimal duration, float velocity) : this(pitch, start, duration, velocity, 1, 0, 64) {}

    public NoteEvent(NoteEvent note)
    {
        Pitch = note.Pitch;
        Start = note.Start;
        Duration = note.Duration;
        Velocity = note.Velocity;
        if (note.Children == null) return;
        var children = new List<NoteEvent>(note.Children.Count);
        foreach (var childNote in note.Children)
        {
            children.Add(new NoteEvent(childNote));
        }
        Children = children;
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

    public bool Equals(NoteEvent other)
    {
        // We don't consider velocity or duration as only one note can occupy a specific start time and pitch
        return Start == other.Start && Pitch == other.Pitch;
    }
}