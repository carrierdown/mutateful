///<reference path="big-def.ts"/>
///<reference path="Clip.ts"/>

declare function post(msg: string);

/*
var a: IBig = BigFactory.create(1.5);
var b: IBig = BigFactory.create(0.7);

console.log(a.toFixed(4));
console.log(b.lt(a));
console.log(a.lt(b));
*/

function bang() {
    var clp = new Clip();
    var notes: Note[] = clp.getSelectedNotes();
    for (let note of notes) {
        post(note.toString());
    }
}