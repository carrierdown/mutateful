///<reference path="big-def.ts"/>
///<reference path="Clip.ts"/>

declare function post(msg: string);
declare var outlets: number;
declare var inlets: number;

/*
var a: IBig = BigFactory.create(1.5);
var b: IBig = BigFactory.create(0.7);

console.log(a.toFixed(4));
console.log(b.lt(a));
console.log(a.lt(b));
*/

outlets = 1;
inlets = 1;

function bang() {
    var clp = new Clip();
    // clp.selectAllNotes();
    // var notes: Note[] = clp.getSelectedNotes();
    var notes: Note[] = clp.getNotes();
    for (let note of notes) {
        post(note.toString());
    }
}