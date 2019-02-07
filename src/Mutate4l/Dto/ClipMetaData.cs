using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Dto
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
