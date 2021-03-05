using System;
using System.Collections.Generic;
using Mutate4l.Core;
using System.Globalization;
using Mutate4l.IO;

namespace Mutate4l.Utility
{
    public static class IOUtilities
    {
        public static Clip StringToClip(string data)
        {
            if (data.IndexOf('[') >= 0 && data.IndexOf(']') >= 0)
            {
                data = data.Substring(data.IndexOf('[') + 1).Substring(0, data.IndexOf(']') - 1);
            }
            var metadataParts = new string[0];
            if (data.IndexOf(':') > 0)
            {
                var metadata = data.Substring(0, data.IndexOf(':'));
                metadataParts = metadata.Split(',');
            }
            var actualData = metadataParts.Length > 0 ? data.Substring(data.IndexOf(':') + 1) : data;
            var noteData = actualData.Split(' ');
            if (noteData.Length < 2)
            {
                return null;
            }
            decimal clipLength = decimal.Parse(noteData[0]);
            bool isLooping = noteData[1] == "1";
            var notes = new SortedList<NoteEvent>();
            for (var i = 2; i < noteData.Length; i += 4)
            {
                notes.Add(new NoteEvent(byte.Parse(noteData[i]), decimal.Parse(noteData[i + 1], NumberStyles.Any), decimal.Parse(noteData[i + 2], NumberStyles.Any), byte.Parse(noteData[i + 3])));
            }
            if (metadataParts.Length > 0)
            {
                return new Clip(clipLength, isLooping) { Notes = notes, ClipReference = new ClipReference(int.Parse(metadataParts[0]), int.Parse(metadataParts[1])) };
            }
            return new Clip(clipLength, isLooping) { Notes = notes };
        }

        public static string ClipToString(Clip clip)
        {
            string data = $"{clip.Length} {clip.IsLooping}";
            for (var i = 0; i < clip.Notes.Count; i++)
            {
                var note = clip.Notes[i];
                data = string.Join(' ', data, note.Pitch, note.Start.ToString("F5"), note.Duration.ToString("F5"), note.Velocity);
            }
            return data;
        }

        /*
            GetClipAsBytes: Convert Clip to array of bytes

            Format:

            2 bytes (id)
            4 bytes (clip length - float)
            1 byte (loop state - 1/0 for on/off)
            2 bytes (number of notes)
                1 byte  (pitch)
                4 bytes (start - float)
                4 bytes (duration - float)
                1 byte  (velocity)

            Above block repeated N times
        */
        public static List<byte> GetClipAsBytes(ushort id, Clip clip)
        {
            var result = new List<byte>(2 + 4 + 1 + 2 + (10 * clip.Notes.Count));
            result.AddRange(BitConverter.GetBytes(id));
            result.AddRange(BitConverter.GetBytes((Single)clip.Length));
            result.Add((byte)(clip.IsLooping ? 1 : 0));
            result.AddRange(BitConverter.GetBytes((ushort)clip.Notes.Count));

            foreach (var note in clip.Notes)
            {
                if (note.Velocity == 0) continue;
                result.Add((byte)note.Pitch);
                result.AddRange(BitConverter.GetBytes((Single)note.Start));
                result.AddRange(BitConverter.GetBytes((Single)note.Duration));
                result.Add((byte)note.Velocity);
            }
            return result;
        }
        
        /*
            GetClipAsBytes: Convert Clip to array of bytes

            Format:

            1 byte  (track #)
            1 byte  (clip #)
            4 bytes (clip length - float)
            1 byte  (loop state - on/off)
            2 bytes (number of notes N)
            x bytes - note data as chunks of 10 bytes where
                1 byte  (pitch)
                4 bytes (start - float)
                4 bytes (duration - float)
                1 byte  (velocity)

                Above block repeated N times
            
            For MPE support: Add another chunk of MPE data for each note. Laying out the data this way
            will make it possible to support Live 10 as well, since the Live 10 connector can skip/disregard this data.
            
        */
        
        private static List<byte> SetClipDataHeader = new() {Decoder.TypedDataFirstByte, Decoder.TypedDataSecondByte, Decoder.TypedDataThirdByte, Decoder.SetClipDataOnServerSignifier};
        
        public static List<byte> GetClipAsBytesV2(Clip clip)
        {
            // todo: If supporting multiple clients, we need to also send formula (if specified) when sending clip data
            var result = new List<byte>(4 + 1 + 1 + 4 + 1 + 2 + (10 * clip.Notes.Count));
            result.AddRange(SetClipDataHeader);
            result.Add((byte)clip.ClipReference.Track);
            result.Add((byte)clip.ClipReference.Clip);
            result.AddRange(BitConverter.GetBytes((Single)clip.Length));
            result.Add(clip.IsLooping ? 1 : 0);
            result.AddRange(BitConverter.GetBytes((ushort)clip.Notes.Count));

            foreach (var note in clip.Notes)
            {
                if (note.Velocity == 0) continue;
                result.Add((byte)note.Pitch);
                result.AddRange(BitConverter.GetBytes((Single)note.Start));
                result.AddRange(BitConverter.GetBytes((Single)note.Duration));
                result.Add((byte)note.Velocity);
            }
            return result;
        }
    }
}
