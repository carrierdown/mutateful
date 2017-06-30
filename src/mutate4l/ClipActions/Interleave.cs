using Mutate4l.Cli;
using Mutate4l.Core;
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

    /// <summary>
    /// Interleaves the contents of one clip with another (FR: Interleave contents of x clips).
    /// 
    /// <list type="table">
    ///     <listheader>
    ///         <term>Options</term>
    ///         <description>description</description>
    ///     </listheader>
    ///     <item>
    ///         <term>mode</term>
    ///         <description>
    ///         <para>eventcount: Interleaved clip is created by counting events in each clip and intertwining them, such that resulting clip will contain first note of first clip + distance to next note, followed by first note of second clip + distance to next note, and so on.</para>
    ///         <para>timerange: Interleaved clip is created by first splitting both clips at given lengths, like 1/8 or 1/16 (splitting any notes that cross these boundaries). The resulting chunks are then placed into the interleaved clip in an interleaved fashion.</para>
    ///         </description>
    ///     </item>
    /// </list>
    /// </summary>
    public class Interleave : IClipAction
    {
        public InterleaveMode Mode { get; set; } = TimeRange;
        public List<int> EventCounts { get; set; }
        public decimal EventRangeA { get; set; } = 1; // todo: support list of any number of ranges instead
        public decimal EventRangeB { get; set; } = 1; // todo: support list of any number of ranges instead

        private static decimal AddNextNote(SortedList<Note> noteSrc, decimal position, int ix, Clip a, Clip b, Clip resultClip)
        {
            decimal pos = position;
            var noteToAdd = new Note(noteSrc[ix % noteSrc.Count])
            {
                Start = pos
            };
            resultClip.Notes.Add(noteToAdd);
            var note = noteSrc[ix % noteSrc.Count];
            var nextNote = noteSrc[(ix + 1) % noteSrc.Count];
            if ((ix + 1) % noteSrc.Count == 0 && ix > 0)
            {
                pos = pos + a.Length - note.Start;
                pos = pos + nextNote.Start;
            }
            else
            {
                pos = pos + nextNote.Start - note.Start;
            }
            return pos;
        }

        public Clip Apply(Clip a, Clip b) // todo: Expand to interleave any list of two clips or more
        {
            Clip resultClip = new Clip(a.Length + b.Length, true);
            decimal position = 0;

            switch (Mode)
            {
                case EventCount:
                    int i = 0, nix = 0;
                    while (i < b.Notes.Count + a.Notes.Count)
                    {

                        if (i == 0)
                        {
                            position = position + a.Notes[nix % a.Notes.Count].Start;
                        }
                        if (i % 2 == 0)
                        {
                            position = AddNextNote(a.Notes, position, nix, a, b, resultClip);
                        }
                        if (i % 2 == 1)
                        {
                            position = AddNextNote(b.Notes, position, nix, a, b, resultClip);
                        }
                        i++;
                        nix = i / 2;
                    }
                    break;
                case TimeRange:
                    decimal srcPositionA = 0,
                        srcPositionB = 0;
                    a.Notes = Utility.SplitNotesAtEvery(a.Notes, EventRangeA, b.Length);
                    b.Notes = Utility.SplitNotesAtEvery(b.Notes, EventRangeB, a.Length);

                    while (position < resultClip.Length)
                    {
                        resultClip.Notes.AddRange(Utility.GetNotesInRangeAtPosition(srcPositionA, srcPositionA + EventRangeA, a.Notes, position));
                        position += EventRangeA;
                        srcPositionA += EventRangeA;

                        resultClip.Notes.AddRange(Utility.GetNotesInRangeAtPosition(srcPositionB, srcPositionB + EventRangeB, b.Notes, position));
                        position += EventRangeB;
                        srcPositionB += EventRangeB;
                    }
                    break;
            }
            return resultClip;
        }

        public void Initialize(Dictionary<TokenType, List<string>> options)
        {

            throw new NotImplementedException();
        }
    }
}
