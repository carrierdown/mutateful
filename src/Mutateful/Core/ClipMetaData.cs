namespace Mutateful.Core
{
    public struct ClipMetaData
    {
        public ushort Id;
        public byte TrackNumber;

        public ClipMetaData(ushort id, byte trackNumber)
        {
            Id = id;
            TrackNumber = trackNumber;
        }
    }
}
