# `mutate4l`

### About
`mutate4l` is a swiss army knife for offline processing of MIDI clips in Ableton Live. It can be used for tasks like aligning the notes of one clip rhythmically with another, using one clip as a transpose track for another clip, recursively applying the contents of a clip on itself (fractalize!), and a ton of other uses. Below is a list of the functions I'd like to support for version 1.0:

&#10003; Constrain note start positions<br>
&#10003; Constrain note values<br>
&#10003; Interleave clips<br>
&#10003; Slice clips<br>
&#9675; Sustain notes<br>
&#9675; Transpose from clip<br>
&#9675; Retrigger from clip (arpeggiate)<br>
&#9675; Multitrigger from several clips<br>
&#9675; Rescale<br>
&#9675; Explode clip (e.g. unique notes > new clip, or unique note positions > new clip)<br>
Maybe:
&#9675; Create retrig/ratchet<br>
&#9675; Selection logic (even, every #, series from start, series from end, pitch values, pairs, invert flag)<br>

&#10003;=Implemented, &#9675;=Planned

### Status October 2017
The project has been fully ported to C# using .NET Core 2. The first version will be a cross-platform command line tool. A GUI version is planned for a later release.
