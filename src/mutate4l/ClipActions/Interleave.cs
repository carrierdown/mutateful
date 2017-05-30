using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using static Mutate4l.ClipActions.InterleaveMode;

namespace Mutate4l.ClipActions
{
    public enum InterleaveMode
    {
        EventCount,
        TimeRange
    }

    public class Interleave
    {

        public static InterleaveMode Mode { get; set; }
        public static List<int> EventCounts { get; set; }
        public static decimal EventRangeA { get; set; } // todo: support list of any number of ranges instead
        public static decimal EventRangeB { get; set; } // todo: support list of any number of ranges instead

        private static decimal AddNextNote(List<Note> noteSrc, decimal position, int ix, Clip a, Clip b, Clip resultClip)
        {
            decimal pos = position;
            noteSrc[ix % noteSrc.Count].Start = pos;
            resultClip.Notes.Add(noteSrc[ix % noteSrc.Count]);
            if ((ix + 1) % noteSrc.Count == 0 && ix > 0)
            {
                pos = pos + a.Length - noteSrc[ix % noteSrc.Count].Start;
                pos = pos + noteSrc[(ix + 1) % noteSrc.Count].Start;
            }
            else
            {
                pos = pos + noteSrc[(ix + 1) % noteSrc.Count].Start - noteSrc[ix % noteSrc.Count].Start;
            }
            return pos;
        }

        public static Clip Apply(Clip a, Clip b) // todo: Expand to interleave any list of two clips or more
        {

        a.Notes.Sort();
        b.Notes.Sort();
            Clip resultClip = new Clip(0, true);
            //resultClip.length = a.getLength().plus(b.getLength()); // todo: update length as we go

            decimal position = 0;
        switch (Mode) {
            case EventCount:
                int i = 0, nix = 0;
                while (i < b.Notes.Count + a.Notes.Count) {
                    
                    if (i === 0) {
                        position = position.plus(a[nix % a.length].getStart());
                    }
                    if (i % 2 === 0) {
                        position = addNextNote(a, position, nix);
                    }
                    if (i % 2 === 1) {
                        position = addNextNote(b, position, nix);
                    }
                    i++;
                    nix = Math.floor(i / 2);
                }
                break;
            case TimeRange:
                let srcPositionA = new Big(0),
                    srcPositionB = new Big(0);
a = ClipActions.splitNotesAtEvery(a, options.interleaveEventRangeA, b.getLength());
                b = ClipActions.splitNotesAtEvery(b, options.interleaveEventRangeB, a.getLength());

                while (position.lt(resultClip.length)) {
                    resultClip.notes = resultClip.notes.concat(ClipActions.getNotesFromRangeAtPosition(srcPositionA, srcPositionA.plus(options.interleaveEventRangeA), a, position));
                    position = position.plus(options.interleaveEventRangeA);
                    srcPositionA = srcPositionA.plus(options.interleaveEventRangeA);

                    resultClip.notes = resultClip.notes.concat(ClipActions.getNotesFromRangeAtPosition(srcPositionB, srcPositionB.plus(options.interleaveEventRangeB), b, position));
                    position = position.plus(options.interleaveEventRangeB);
                    srcPositionB = srcPositionB.plus(options.interleaveEventRangeB);
                }
                break;
        }
        return resultClip;
    }
    }
}
