namespace Mutateful.Core
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

        public static ClipReference FromString(string clipRef)
        {
            var track = 0;
            var clip = 0;
            
            var trackIdent = clipRef[0..1].ToLowerInvariant();
            var clipIdent = clipRef[1..];

            track = (byte) trackIdent[0] - 0x61;
            int.TryParse(clipIdent, out clip);
            
            return new ClipReference(track, clip - 1);
        }

        public override string ToString()
        {
            return $"{Track},{Clip}";
        }

        public static readonly ClipReference Empty = new ClipReference(0, 0);
    }
}