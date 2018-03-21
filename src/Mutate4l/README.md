### Ideer

- iterate - leverer de valgte klippene i rekkefølge til etterfølgende prosesser, slik at man i praksis lager en lengre sekvens av flere klipp
- add - legg inn ekstra klipp i en chain, f.eks. a1 a2 interleave add a3 a4 iterate => a5
- filter - filtrer noter på eksempelvis lengde eller pitch, eller alt (nyttig ved selections)

- arpeggiate - samle overlappende noter fra A, arpeggier gjennom dem med noter i samme range fra B. Repeter for hver blokk med overlappende noter.

&#10003; Constrain note start positions<br>
&#10003; Constrain note values<br>
&#10003; Interleave clips<br>
&#10003; Slice clips<br>
&#10003; Retrigger from clip (arpeggiate)<br>
&#9675; Transpose from clip<br>
&#9675; Multitrigger from several clips<br>
&#9675; Rescale<br>
&#9675; Sustain notes<br>
&#9675; Filter notes based on length, velocity, mute-state<br>
&#9675; Explode clip (e.g. unique notes > new clip, or unique note positions > new clip)<br>
Maybe:<br>
&#9675; Create retrig/ratchet<br>
&#9675; Selection logic (even, every #, series from start, series from end, pitch values, pairs, invert flag)<br>

&#10003;=Implemented, &#9675;=Planned

