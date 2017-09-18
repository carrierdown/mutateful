# `mutate4l`

### Status June 2017
I'm in the process of porting all code for this project to C# using the .NET Core platform. The first version of this tool will be a cross-platform command line application, though a GUI-version might follow at a later date.

### About
`mutate4l` is a swiss army knife for offline processing of MIDI clips in Ableton Live. It can be used for tasks like aligning the notes of one clip rhythmically with another, using one clip as a transpose track for another clip, recursively applying the contents of a clip on itself (fractalize!), and a ton of other uses. Below is a list of the functions I'd like to support for version 1.0:

&#10003; Constrain note start positions<br>
&#10003; Constrain note values<br>
&#10003; Interleave clips<br>
&#9675; Fractalize clip<br>
&#9675; Transpose from clip<br>
&#9675; Retrigger from clip<br>
&#9675; Rescale<br>
&#9675; Create retrig/ratchet<br>
&#9675; Explode clip (e.g. unique notes > new clip, or unique note positions > new clip)<br>
&#9675; Selection logic (even, every #, series from start, series from end, pitch values, pairs, invert flag)<br>

&#10003;=Implemented, &#9675;=Planned


Ideer

Med den nye chain-tankegangen på plass kan man begynne å nærme seg en del av de samme konseptene man hadde i recurse. F.eks. kan selections håndteres vha select og end-kommandoer som modifiserer klippene ved å dra ut visse noter i select-tilfellet. End-tilfellet vil kreve litt strukturelle endringer, kanskje en historikk på hvert klipp, eller kanskje et selected parameter på hver note. Sistnevnte er nok klart enklest å implementere og vil funke greit.

noen andre ideer: 
iterate - leverer de valgte klippene i rekkefølge til etterfølgende prosesser, slik at man i praksis lager en lengre sekvens av flere klipp
add - legg inn ekstra klipp i en chain, f.eks. a1 a2 interleave add a3 a4 iterate => a5
filter - filtrer noter på eksempelvis lengde eller pitch, eller alt (nyttig ved selections)


## Documentation

`mutate4l` is not runnable yet, but I will be documenting each function here as a reference while I'm developing.

### `interleave`

Interleaves the contents of one clip with another (FR: Interleave contents of x clips).

#### Options
- mode
	- eventcount: Interleaved clip is created by counting events in each clip and intertwining them, such that resulting clip will contain first note of first clip + distance to next note, followed by first note of second clip + distance to next note, and so on.
	- timerange: Interleaved clip is created by first splitting both clips at given lengths, like 1/8 or 1/16 (splitting any notes that cross these boundaries). The resulting chunks are then placed into the interleaved clip in an interleaved fashion.

