# `mutate4l`

**tl;dr:** `mutate4l` enables live coding in Ableton Live's session view. Set up transformations that trigger whenever a source clip is changed, including arpeggiation, shuffling, and ratcheting/retriggering.

> With live coding being all the rage this looks like a groovy new idea to bring that excitement and its new possibilities to Ableton. I’m watching closely.
>
> &mdash; Matt Black, co-founder of Ninja Tune & Coldcut.

<p align="center">
  <img src="https://github.com/carrierdown/mutate4l/blob/feature/new-readme/assets/mu4l-walkthrough.gif" alt="mutate4l in action">
</p>

## Intro
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

## Command reference [Incomplete]

A note on usage examples: All parameters are prefixed with -, and are always optional. The basic syntax of a command is `commandname -parameter value -anotherparameter value`. Values can be numbers, in which case the valid range is displayed as e.g. 1-100. 

### Constrain

Constrain applies the rhythm and/or pitch information from one clip to another, sort of like a more flexible quantize.

Usage: `constrain -mode pitch|rhythm|*both* -strength 1-100`

*strength* Specifies the strength of the quantization from 1 to 100, where 100 is full quantization. Default value is 100.
*mode* Specifies whether pitch, rhythm, or both aspects should be taken into account when aligning clip contents.

Option | Type | Range | Default | Description
--- | --- | --- | --- | ---
mode | Enum | pitch, rhythm, both | both | Specifies whether pitch and/or rhythmic data should be taken into account when constraining the target clip.
strength | Number | 1-100 | 100 | Specifies the strength of the quantization from 1 to 100, where 100 is full quantization. Only applies to the timing of notes, not their pitch.

### Ratchet

Ratchet applies ratcheting to a clip using either itself or another clip as a control clip. Various options can be specified to determine how the control clip is used.

Note: Will be rewritten slightly so that velocity is used instead of pitch to control ratcheting. This makes a lot more sense for polyphonic clips and should be easier to reason about in general (typical workflow would be to simply duplicate the clip you want to apply ratcheting to, then adjust the velocities accordingly).

Option | Type | Range | Default | Description
--- | --- | --- | --- | ---
min | Number | 1-20 | 1 | Minimum ratchet value
max | Number | 1-20 | 8 | Maximum ratchet value
strength | Number | 1-100 | 100 | Determines how much shaping should be applied, as a percentage (maybe rename to shapingamount)
velocitytostrength | Flag | | Off | Use velocity of notes in control clip to determine shaping amount
shape | Enum | linear, easein | linear | Optionally specify a curve to control how the ratchets should be shaped
autoscale | Flag | | Off | Automatically scale control sequence so that lowest note corresponds to minimum ratchet value and highest note corresponds to maximum ratchet value
controlmin | Number | 1-127 | 60 | Cutoff point for low values in control clip (will correspond to minimum ratchet value)
controlmax | Number | 1-127 | 68 | Cutoff point for high values in control clip (will correspond to maximum ratchet value)

### Interleave

Interleaves two or more clips together.

Option | Type | Range | Default | Description
--- | --- | --- | --- | ---
interleavemode | Enum | event, time | time | Specifies whether interleaving should be done on a note-by-note basis (event), or based on a specific time range, e.g. every 1/16 measure.
repeats | Array of numbers | | 1 | Optionally specify how many times an event or range should be repeated before moving on to the next. Specifying for instance 1 2 3 4 would result in 1 repeat of the first event, 2 repeats of the second, and so on, starting at 1 again once the cycle completes.
ranges | Array of numbers | | 1 | Only applicable for time mode. Specify the size of each range, specified in the same manner as the repeats option above.
mask | Flag | | Off | If specified, the current input "masks" the corresponding location of other inputs.

### Filter

Filters the contents of a clip based on the duration of note events.

Option | Type | Range | Default | Description
--- | --- | --- | --- | ---
duration | Decimal | | 1/64 | Specifies the cutoff point for filtering: Notes shorter than this will be filtered out.

### Slice

Slices the note events in a clip based on the lengths specified.

### Scan [Experimental]

"Scans" through a clip by moving a section of the specified size from the start to the end of the clip, keeping whatever falls within the window at each position and stitching this together to form a new clip.

Option | Type | Range | Default | Description
--- | --- | --- | --- | ---
window | Decimal | | 1 | Specifies the size of the section which will be used to scan through the contents of the clip.
count | Number | 1-500 | 8 | The number of times scanning will take place.

