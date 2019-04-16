using Mutate4l.Core;
using System.Globalization;

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
    }
}
