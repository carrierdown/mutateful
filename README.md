# `mutate4l`

**tl;dr:** `mutate4l` enables live coding in Ableton Live's session view. Set up transformations that trigger whenever a source clip is changed, including arpeggiation, shuffling, and ratcheting/retriggering.

<p align="center">
  <img src="https://github.com/carrierdown/mutate4l/blob/feature/new-readme/assets/mu4l-walkthrough.gif" alt="mutate4l in action">
</p>

> With live coding being all the rage this looks like a groovy new idea to bring that excitement and its new possibilities to Ableton. I’m watching closely.
>
> &mdash; Matt Black, co-founder of Ninja Tune & Coldcut.

## Concept
`mutate4l` reimagines Ableton Live's session view as a spreadsheet-like live coding setup. You add formulas to dynamically shuffle, arpeggiate, constrain, scale, retrigger, and otherwise transform clips in myriad ways. Formulas operate on other clips and/or textual inputs to produce new clips. Unlike most experimental sequencers, `mutate4l` leverages Live's built-in sequencing functionality, allowing you to use any existing MIDI clip as a source of fresh inspiration.

## Usage examples

### Creating an arpeggio from a sustained note and a chord

`=C1 slice 1/2 transpose 0 7 slice 1/16 1/8 transpose -by B1`

This formula does the following: Start with the single sustained note contained in the clip at position `C1`. Slice it in half, keeping the first note unchanged and transposing the second note 7 semitones up (a perfect fifth). Now, slice the resulting two half-notes into a pattern alternating between 1/16 and 1/8th notes. Finally, transpose this pattern with the chord contained in the clip located at position `B1`.

### Interleaving two clips together

`=b1 c1 interleave -mode event`

This formula combines the notes contained in the clips located at positions B1 and C1, so that the notes from each clip are played one after the other in an alternating fashion.

![Alt text](./assets/Generated637012367962797269-clip.svg)

### Turning a beat and a chord into an arpeggio

`=c1 scale -by b2 -strict transpose -12 transpose 0 7 5 monophonize`

This formula takes the beat contained in clip C1, transforming notes as needed so as to fit with the notes (or scale if you will) contained in clip B2. It does this in a strict manner, meaning that the resulting notes will get the same absolute pitch instead of the closest pitch in the current octave. All notes are then transposed down an octave, and are then transposed so that the resulting notes alternate between 0, 7, and 5 semitones transposition. Finally the result is made monophonic so that only one note is sounding at any given moment. 

<!--
 Formulas are composed of one or more commands operating on one or more clips. Most commands have various options that can be set depending on your specific needs. They range from simple things like filtering out all notes with a length shorter than a given interval, to more esoteric things like arpeggiating one clip based on another, or creating glitchy beats by adding ratcheting/retriggering to specific notes.
-->

## See it in action

<table cellpadding="0" cellspacing="0" style="border:none;"><tr><td><a href="https://www.youtube.com/watch?v=YNI9ZxhSkWQ"><img alt="mutate4l demo video 1" src="https://img.youtube.com/vi/YNI9ZxhSkWQ/0.jpg" width="250"><p>Demo #1: concat, constrain, transpose</p></a></td><td><a href="https://www.youtube.com/watch?v=bGMBDap1-ko"><img alt="mutate4l demo video 2" src="https://img.youtube.com/vi/bGMBDap1-ko/0.jpg" width="250"><p>Demo #2: ratchet, shuffle, interleave</p></a></td></tr></table>

## A musical spreadsheet

The easiest way to understand what `mutate4l` does is by comparing it to a traditional spreadsheet. Let's say you have two numbers that you'd like to multiply. You put one number in cell `A1`, another in `A2`, and in `A3` you enter the following (very simple) formula: `=A1 * A2`. Cell `A3` will then contain the result of this operation, and will update automatically whenever `A1` or `A2` changes. 

Since the session view in Ableton Live presents clips in a spreadsheet-like grid, `mutate4l` works the same way, only with more musically interesting commands. For instance, you could shuffle the contents of clip `A1` using the contents of clip `A2`. The pitch values of the various notes in clip `A2` would then be used to shuffle the order of notes in `A1`. Similar to the example above, we would like the result to be inserted into clip `A3`, but instead of using a spreadsheet command we will use a `mutate4l` command, as follows: `=A1 shuffle -by A2`. In this example, `A1` is a *source clip* (i.e. the clip that will be transformed), and `A2` is the *control clip* (i.e. the clip that controls the transformation). The latter could be omitted, in which case clip `A1` would be shuffled using itself as the control clip. The formula for this would simply be `=A1 shuffle`.

Full documentation of all commands will follow at a later date. In the meantime, star this repo and/or follow me at [twitter.com/KnUpland](https://twitter.com/KnUpland) for updates.

<!--
## Available commands [Incomplete]

Basic syntax: [ClipReference #1](#parameter-types) ... [ClipReference #N](#parameter-types) commandname -parameter1 value -parameter2 value

Command | Parameters (default values in **bold**) | Description
--- | --- | ---
arpeggiate | -by <[ClipReference](#parameter-types)> -removeoffset -rescale <[Number](#parameter-types) in range 1-10> | Arpeggiates the given clip by another clip (could also be itself)
concat | | Concatenates the given clips into one clip.
constrain | -by <[ClipReference](#parameter-types)> -mode **pitch**&#124;rhythm&#124;both -strength <[Number](#parameter-types) in range 1-**100**> |
filter | -duration <[MusicalDivision](#parameter-types): **1/64**> -invert | Filters out notes shorter than the length specified (default 1/64). Works the other way round if -invert is specified. 
interleave | -chunkchords -mode event&#124;time -ranges <list of [MusicalDivision](#parameter-types)> -repeats <list of [Number](#parameter-types)> -skip -solo |
monophonize | | Removes any overlapping notes. Often useful on control clips, which often work best when there are no overlapping events.
ratchet | <ratchet values e.g. 1 2 3 4> -autoscale -by [ClipReference](#parameter-types) -mode velocity&#124;pitch -shape **linear**&#124;easeinout&#124;easein&#124;easeout -strength <[Decimal](#parameter-types) in range 0.0-**1.0**> -velocitytostrength |
relength | -factor <[Decimal](#parameter-types) in range 0.0-**1.0**> |
resize | -factor <[Decimal](#parameter-types) in range 0.0-**1.0**> |
shuffle | <shuffle indexes e.g. 1 2 3 4> -by [ClipReference](#parameter-types) |
slice | list of [MusicalDivision](#parameter-types) |
transpose | <transpose values, e.g. 0 -12 12> -by [ClipReference](#parameter-types) -mode absolute&#124;relative&#124;overwrite |

## Parameter types

Type | Description
--- | ---
ClipReference | Cells in the session view are referenced like they would be in a spreadsheet, i.e. tracks are assigned letters (A-Z) and clip rows are assigned numbers (1-N). Example: track 1 clip 1 becomes A1, track 2 clip 3 becomes B3.
MusicalDivision | These are commonly used in sequencer software to denote musical divisions like quarter notes, eight notes and so on. Examples: quarter note = 1/4, eight note = 1/8.
Number | Whole number (integer), either negative or positive
Decimal | Decimal number, from 0.0 and upwards
-->