///<reference path="big-def.ts"/>
///<reference path="Clip.ts"/>

declare function post(msg: string);
declare var outlets: number;
declare var inlets: number;


/*
var a: IBig = BigFactory.create(0.03);
var b: IBig = BigFactory.create(0.7);

console.log(a.toFixed(4));
console.log(b.lt(a));
console.log(a.lt(b));
*/

outlets = 1;
inlets = 1;

var sourceClip: Clip,
    targetClip: Clip;

function bang() {
    var clp = new Clip();
    // clp.selectAllNotes();
    // var notes: Note[] = clp.getSelectedNotes();
    var notes: Note[] = clp.getNotes();
    for (let note of notes) {
        post(note.toString());
    }
}

function setSource(): void {
    sourceClip = new Clip();
}

function setTarget(): void {
    targetClip = new Clip();
}

function findNearestNoteStartInSet(needle: Note, haystack: Note[]): Note {
    var nearestIndex: number = 0,
        nearestDelta: IBig;
    for (let i = 0; i < haystack.length; i++) {
        if (nearestDelta === undefined) {
            nearestDelta = needle.getStart().minus(haystack[i].getStart()).abs();
        }
        let currentDelta: IBig = needle.getStart().minus(haystack[i].getStart()).abs();
        if (currentDelta.lt(nearestDelta)) {
            nearestDelta = currentDelta;
            nearestIndex = i;
        }
    }
    return haystack[nearestIndex];
}

function doConstrainStart(source: Clip, target: Clip, options: any = {}): Note[] {
    if (!sourceClip || !targetClip) return;

    var sourceNotes: Note[] = source.getNotes();
    var targetNotes: Note[] = target.getNotes();

    if (sourceNotes.length === 0 || targetNotes.length === 0) return;

    for (let note of targetNotes) {
        note.setStart(findNearestNoteStartInSet(note, sourceNotes).getStart());
    }

    return targetNotes;
}
