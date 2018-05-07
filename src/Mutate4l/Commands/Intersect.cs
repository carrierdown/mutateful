using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mutate4l.Commands
{
    public class Intersect
    {
        public static ProcessResultArray<Clip> Apply(params Clip[] clips)
        {
            var processedClips = new Clip[clips.Length];

            // Dictionary <int (pitch), NoteEvent[]>
            foreach (var clip in clips)
            {
                var inverseClip = new Clip(clip.Length, clip.IsLooping);

                var notesByPitch = new Dictionary<int, List<NoteEvent>>();
                clip.Notes.GroupBy(x => x.Pitch).ToList().ForEach(x =>
                {
//                    inverseClip.Add
                });
                foreach (var note in clip.Notes)
                {

                }
            }

            return new ProcessResultArray<Clip>(processedClips);
        }
    }
}
