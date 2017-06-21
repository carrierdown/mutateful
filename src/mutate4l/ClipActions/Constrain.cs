using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.ClipActions
{
    public class Constrain
    {
        public static bool ConstrainPitch;
        public static bool ConstrainStart;

        /*public static Clip Apply(Clip a, Clip b)
        {
            // normaliser lengde på a og b (finn lengste og loop korteste til å få samme lengde) - kan være en funksjon i Utility: NormalizeClipLengths(List<Clip> clips)
        }*/

        /*        var notesToMutate:Note[] = clipToMutate.getNotes(),
            notesToSourceFrom:Note[] = clipToSourceFrom.getNotes(),
            resultClip:GhostClip = ClipActions.newGhostClip();

        for (let note of notesToMutate) {
            console.log(note.toString());
            let result = Note.clone(note);

            if (options.constrainNotePitch) {
                result.setPitch(ClipActions.findNearestNotePitchInSet(note, notesToSourceFrom));
            }
            if (options.constrainNoteStart) {
                result.setStart(ClipActions.findNearestNoteStartInSet(note, notesToSourceFrom));
            }
            resultClip.notes.push(result);
        }
        return resultClip;*/


    }
}
