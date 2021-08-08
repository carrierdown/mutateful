using System;
using System.Collections.Generic;
using Mutateful.Compiler;
using Mutateful.Core;
using Mutateful.Utility;

namespace Mutateful.Commands
{
    public class EchoOptions
    {
        [OptionInfo(type: OptionType.Default, 1/32f)]
        public decimal[] Lengths { get; set; } = { 4m/16 };

        public int[] Echoes { get; set; } = {3};
    }

    // # desc: Adds echoes to all notes in a clip, cycling through multiple delay times if more than one delay time is specified.
    public static class Echo
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out EchoOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(EchoOptions options, params Clip[] clips)
        {
            var processedClips = new Clip[clips.Length];

            // naive version
            // proper algo needs to also delete notes that are covered by an echoed note
            var i = 0;
            foreach (var clip in clips)
            {
                processedClips[i++] = AddEchoes(clip, options.Lengths, options.Echoes);
            }
            return new ProcessResultArray<Clip>(processedClips);
        }

        public static Clip AddEchoes(Clip clip, decimal[] lengths, int[] echoes)
        {
            var lengthIx = 0;
            var echoIx = 0;
            var newNotes = new List<NoteEvent>();
            foreach (var noteEvent in clip.Notes)
            {
                var delayTime = lengths[lengthIx++ % lengths.Length];
                var echoCount = Math.Max(echoes[echoIx++ % echoes.Length], 2);
                var velocityFalloff = (int) Math.Round((noteEvent.Velocity - 10) / (echoCount - 1));
                if (noteEvent.Duration > delayTime)
                {
                    noteEvent.Duration = delayTime;
                }
                for (var i = 0; i < echoCount; i++)
                {
                    var echoedNote = new NoteEvent(noteEvent);
                    echoedNote.Start += delayTime * i;
                    echoedNote.Velocity -= velocityFalloff * i;
                    newNotes.Add(echoedNote);
                }
            }
            foreach (var newNote in newNotes)
            {
                ClipUtilities.AddNoteCutting(clip, newNote);
            }
            // todo: handle wrapping echoes outside the length of the clip
            return clip;
        }
    }
}
