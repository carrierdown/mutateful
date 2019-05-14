# `mutate4l`

**tl;dr:** `mutate4l` enables live coding in Ableton Live's session view. Set up transformations that trigger whenever a source clip is changed, including arpeggiation, shuffling, and ratcheting/retriggering.

<p align="center">
  <img src="https://github.com/carrierdown/mutate4l/blob/feature/new-readme/assets/mu4l-walkthrough.gif" alt="mutate4l in action">
</p>

> With live coding being all the rage this looks like a groovy new idea to bring that excitement and its new possibilities to Ableton. I’m watching closely.
>
> &mdash; Matt Black, co-founder of Ninja Tune & Coldcut.

### See it in action

<table cellpadding="0" cellspacing="0" style="border:none;"><tr><td><a href="https://www.youtube.com/watch?v=YNI9ZxhSkWQ"><img alt="mutate4l demo video 1" src="https://img.youtube.com/vi/YNI9ZxhSkWQ/0.jpg" width="250"><p>Demo #1: concat, constrain, transpose</p></a></td><td><a href="https://www.youtube.com/watch?v=YNI9ZxhSkWQ"><img alt="mutate4l demo video 1" src="https://img.youtube.com/vi/YNI9ZxhSkWQ/0.jpg" width="250"><p>Demo #2: ratchet, shuffle, interleave</p></a></td></tr></table>

## Concept
`mutate4l` reimagines Ableton Live's session view as a spreadsheet, enabling you to transform and create new clips dynamically whenever a source clip is changed. It can be compared to a lightweight live coding setup right inside Ableton Live. You can add formulas to shuffle, arpeggiate, constrain, scale, retrigger, and otherwise transform clips in myriad ways. Unlike most experimental sequencers however, `mutate4l` leverages Live's own clip sequencer and thus allows you to use any existing midi clip as a source of new inspiration. 

Formulas are composed of one or more commands operating on one or more clips. Most commands have various options that can be set depending on your specific needs. They range from simple things like filtering out all notes with a length shorter than a given interval, to more esoteric things like arpeggiating one clip based on another, or even fractalizing a clip by arpeggiating it with itself.

The easiest way to understand what `mutate4l` does is by comparing it to a traditional spreadsheet. Let's say you have two numbers that you'd like to multiply. You put one number in cell `A1`, another in `A2`, and in `A3` you enter the following (very simple) formula: `=a1 * a2`. Cell `A3` will then contain the result of this operation, and will update automatically whenever `A1` or `A2` changes. 

`mutate4l` works the same way, only with more musically interesting commands. For instance, you could shuffle the contents of clip `A1` using the contents of another clip, e.g. `A2`. The pitch values of the various notes in clip `A2` would then be used to shuffle the order of notes in `A1`. Similar to the example above, we would like the result to be inserted into clip `A3`, but instead of using a spreadsheet command we will use the following `mutate4l` formula: `=A1 shuffle -by A2`. In this example, `A1` is a *source clip* (i.e. the clip that will be transformed), and `A2` is the *control clip* (i.e. the clip that controls the transformation). The latter could be omitted, in which case clip `A1` would be shuffled using itself as the control clip. The formula for this would simply be `=A1 shuffle`. In the more technical documentation below, clip locations like `A1`, `A2` and so on are referred to as a `ClipReference`.

More usage examples will follow. In the meantime, star this repo and/or follow me at [twitter.com/KnUpland](https://twitter.com/KnUpland) for updates.

## Available commands [Incomplete]

Basic syntax: [ClipReference #1](#parameter-types) ... [ClipReference #N](#parameter-types) commandname -parameter1 value -parameter2 value

Command | Parameters (default values in **bold**) | Description | Example
--- | --- | --- | ---
arpeggiate | -by <[ClipReference](#parameter-types)> -removeoffset -rescale <[Number](#parameter-types) in range 1-10> | |
constrain | -by <[ClipReference](#parameter-types)> -mode **pitch**&#124;rhythm&#124;both -strength <[Number](#parameter-types) in range 1-**100**> | |
filter | -duration <[MusicalDivision](#parameter-types): **1/64**> | |
interleave | -chunkchords -mode event&#124;time -ranges <list of [MusicalDivision](#parameter-types)> -repeats <list of [Number](#parameter-types)> -skip -solo | |
monophonize | | |
ratchet | -autoscale -by [ClipReference](#parameter-types) -mode velocity&#124;pitch -shape **linear**&#124;easeinout&#124;easein -strength <[Number](#parameter-types) in range 0-**100**> -velocitytostrength | |
relength | -factor <[Number](#parameter-types) in range 1-10> | |
shuffle | -by [ClipReference](#parameter-types) | |
slice | list of [MusicalDivision](#parameter-types) | |
transpose | -by [ClipReference](#parameter-types) -mode absolute&#124;relative&#124;overwrite | |

## Parameter types

Type | Description
--- | ---
ClipReference | Cells in the session view are referenced like they would be in a spreadsheet, i.e. tracks are assigned letters (A-Z) and clip rows are assigned numbers (1-N). Example: track 1 clip 1 becomes A1, track 2 clip 3 becomes B3.
MusicalDivision | These are commonly used in sequencer software to denote musical divisions like quarter notes, eight notes and so on. Examples: quarter note = 1/4, eight note = 1/8.
Number | Whole number (integer) from 0 and upwards

