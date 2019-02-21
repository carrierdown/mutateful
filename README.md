# `mutate4l`

### About
`mutate4l` is a swiss army knife for offline processing of MIDI clips in Ableton Live. It can be used for tasks like aligning the notes of one clip rhythmically with another, using one clip as a transpose track for another clip, recursively applying the contents of a clip on itself (fractalize!), and a ton of other uses.

## Available commands

Basic syntax: [ClipReference #1](#parameter-types) ... [ClipReference #N](#parameter-types) commandname -parameter1 value -parameter2 value

Command | Parameters (default values in **bold**) | Description | Example
--- | --- | --- | ---
arpeggiate | -by [ClipReference](#parameter-types) -removeoffset -rescale 1-10
constrain | -by a1 -mode **pitch**|rhythm|both -strength 1-**100**
filter | -duration **1/64**
interleave | -chunkchords -enablemask 1 0 -mode event|time -ranges 1/16 1/8 -repeats 1 2 -skip -solo
monophonize
ratchet | -autoscale -by a1 -controlmin 60 -controlmax 68 -min 1 -max 8 -mode velocity|pitch -shape **linear**|easeinout|easein -strength 0-100 -velocitytostrength
relength | -factor 2
shuffle | -by a1
slice | list of [MusicalDivision](#parameter-types)
transpose | -by a1 -mode absolute|relative|overwrite

## Parameter types

Type | Description
--- | ---
ClipReference | Cells in the session view are referenced like they would be in a spreadsheet, i.e. tracks are assigned letters (A-Z) and clip rows are assigned numbers (1-N). Example: track 1 clip 1 becomes A1, track 2 clip 3 becomes B3.
MusicalDivision | These are commonly used in sequencer software to denote musical divisions like quarter notes, eight notes and so on. Examples: quarter note = 1/4, eight note = 1/8.

## Command reference

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

### Scan

"Scans" through a clip by moving a section of the specified size from the start to the end of the clip, keeping whatever falls within the window at each position and stitching this together to form a new clip.

Option | Type | Range | Default | Description
--- | --- | --- | --- | ---
window | Decimal | | 1 | Specifies the size of the section which will be used to scan through the contents of the clip.
count | Number | 1-500 | 8 | The number of times scanning will take place.

