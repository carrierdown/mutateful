using System.Linq;
using Mutateful.Core;

namespace Mutateful.Utility
{
    public static class ClipExtensions
    {
        public static bool HasNestedNotes(this Clip clip)
        {
            return clip.Notes.Any(n => n.Children.Count > 0);
        }

        public static void Flatten(this Clip clip)
        {
            var flattenedNotes = new SortedList<NoteEvent>();

            foreach (var note in clip.Notes)
            {
                flattenedNotes.Add(note);
                if (note.HasChildren)
                {
                    foreach (var child in note.Children)
                    {
                        child.Parent = null;
                    }
                    flattenedNotes.AddRange(note.Children);
                    note.Children = null;
                }
            }
            clip.Notes = flattenedNotes;
        }

        public static void GroupSimultaneousNotes(this Clip clip)
        {
            var groupedNotes = new SortedList<NoteEvent>();
            var i = 0;

            do
            {
                var note = clip.Notes[i];
                var simultaneousNotes = clip.Notes.Skip(i).Where(x => x.Start == note.Start && x.Pitch != note.Pitch);
                if (simultaneousNotes.Count() > 0)
                {
                    foreach (var simultaneousNote in simultaneousNotes)
                    {
                        simultaneousNote.Parent = note;
                    }
                    note.Children = simultaneousNotes.ToList();
                    i += note.Children.Count;
                }
                groupedNotes.Add(note);
            } while (++i < clip.Count);

            clip.Notes = groupedNotes;
        }
    }
}
