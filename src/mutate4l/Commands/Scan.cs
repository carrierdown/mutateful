using Mutate4l.Dto;
using Mutate4l.Options;
using Mutate4l.Utility;

namespace Mutate4l.Commands
{
    public class Scan
    {
        public static ProcessResultArray<Clip> Apply(ScanOptions options, params Clip[] clips)
        {
            var processedClips = new Clip[clips.Length];

            for (var c = 0; c < clips.Length; c++)
            {
                var clip = clips[c];
                var processedClip = new Clip(options.Window * options.Count, clip.IsLooping);
                decimal delta = clip.Length / options.Count,
                    curPos = 0;

                for (int i = 0; i < options.Count; i++)
                {
                    processedClip.Notes.AddRange(ClipUtilities.GetSplitNotesInRangeAtPosition(curPos, curPos + options.Window, clip.Notes, options.Window * i));
                    curPos += delta;
                }
                processedClips[c] = processedClip;
            }

            return new ProcessResultArray<Clip>(processedClips);
        }
    }
}
