using Mutate4l.Core;
using Mutate4l.Dto;
using System.Linq;

namespace Mutate4l.Utility
{
    public static class ClipExtensions
    {
        public static bool HasNestedNotes(this Clip clip)
        {
            return clip.Notes.Any(n => n.Children.Count > 0);
        }

        public static Clip Flatten(this Clip clip)
        {
            // todo: impl
        }

        public static Clip ChunkChords(this Clip clip)
        {
            var chunkedClip = new Clip(clip.Length, clip.IsLooping);
            var i = 0;

            do
            {
                var note = clip.Notes[i];
                var chordNotes = clip.Notes.Skip(i).Where(x => x.Start == note.Start && x.Pitch != note.Pitch).Select(x => new NoteEvent(x.Pitch, x.Start, x.Duration, x.Velocity));
                if (chordNotes.Count() > 1)
                {
                    var children = chordNotes.Skip(1).ToList();
                    foreach (var child in children)
                    {
                        child.Parent = note;
                    }
                    note.Children = children;
                    i += children.Count;
                }
                chunkedClip.Add(note);
            } while (++i < clip.Count);

            return chunkedClip;
        }
    }
}
