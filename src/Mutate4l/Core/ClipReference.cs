namespace Mutate4l.Core
{
    public struct ClipReference
    {
        public int Track { get; set; }
        public int Clip { get; set; }

        public ClipReference(int track, int clip)
        {
            Track = track;
            Clip = clip;
        }
    }
}