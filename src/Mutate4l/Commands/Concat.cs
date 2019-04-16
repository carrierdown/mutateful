using Mutate4l.Utility;
using System.Linq;
using Mutate4l.Core;

namespace Mutate4l.Commands
{
    public static class Concat
    {
        public static ProcessResultArray<Clip> Apply(params Clip[] clips)
        {
            Clip resultClip = new Clip(clips.Select(c => c.Length).Sum(), true);
            decimal pos = 0;
            foreach (var clip in clips)
            {
                resultClip.Notes.AddRange(ClipUtilities.GetNotesInRangeAtPosition(0, clip.Length, clip.Notes, pos));
                pos += clip.Length;
            }
            return new ProcessResultArray<Clip>(new[] { resultClip });
        }
    }
}
