# `mutate4l`

### About
`mutate4l` is a swiss army knife for offline processing of MIDI clips in Ableton Live. It can be used for tasks like aligning the notes of one clip rhythmically with another, using one clip as a transpose track for another clip, recursively applying the contents of a clip on itself (fractalize!), and a ton of other uses.

## Operations

### Constrain

Constrain applies the rhythm and/or pitch information from one clip to another, sort of like a more flexible quantize.

Option | Type | Range | Default | Description
--- | --- | --- | --- | ---
pitch | Switch | | On | Specifies if pitch should be taken into account. If specified, only pitch is taken into account.
start | Switch | | On | Specifies if start position should be taken into account. If specified, only start position is taken into account.
strength | Number | 1-100 | 100 | Specifies the strength of the quantization from 1 to 100, where 100 is full quantization.

### Ratchet

Ratchet applies ratcheting to a clip using either itself or another clip as a control clip. Various options can be specified to determine how the control clip is used.

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

