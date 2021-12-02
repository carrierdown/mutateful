﻿namespace Mutateful.Commands;

public static class Concat
{
    // # desc: Concatenates two or more clips together.
    public static ProcessResult<Clip[]> Apply(params Clip[] clips)
    {
        Clip resultClip = new Clip(clips.Select(c => c.Length).Sum(), true);
        decimal pos = 0;
        foreach (var clip in clips)
        {
            resultClip.Notes.AddRange(ClipUtilities.GetNotesInRangeAtPosition(0, clip.Length, clip.Notes, pos));
            pos += clip.Length;
        }
        return new ProcessResult<Clip[]>(new[] { resultClip });
    }
}