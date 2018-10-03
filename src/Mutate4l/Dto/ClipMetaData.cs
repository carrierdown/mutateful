using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Dto
{
    public struct ClipMetaData
    {
        public string Id;
        public int TrackNumber;

        public ClipMetaData(string id, int trackNumber)
        {
            Id = id;
            TrackNumber = trackNumber;
        }
    }
}
