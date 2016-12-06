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

function findNearestNoteStartInSet(needle: Note, haystack: Note[]): IBig {
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
    return haystack[nearestIndex].getStart();
}

function findNearestNotePitchInSet(needle: Note, haystack: Note[]): number {
    var nearestIndex: number = 0,
        nearestDelta: number;
    for (let i = 0; i < haystack.length; i++) {
        if (nearestDelta === undefined) {
            nearestDelta = Math.abs(needle.getPitch() - haystack[i].getPitch());
        }
        let currentDelta: number = Math.abs(needle.getPitch() - haystack[i].getPitch());
        if (currentDelta < nearestDelta) {
            nearestDelta = currentDelta;
            nearestIndex = i;
        }
    }
    return haystack[nearestIndex].getPitch();
}

function applyConstrainNoteStart(source: Clip, dest: Clip): Note[] {
    return applyConstrain(source, dest, {constrainNoteStart: true, constrainNotePitch: false});
}

function applyConstrainNotePitch(source: Clip, dest: Clip): Note[] {
    return applyConstrain(source, dest, {constrainNoteStart: false, constrainNotePitch: true});
}

function applyConstrain(source: Clip, dest: Clip, options: any = {constrainNoteStart: true, constrainNotePitch: true}): Note[] {
    if (!source || !dest) return;

    var sourceNotes: Note[] = source.getNotes();
    var destNotes: Note[] = dest.getNotes();
    var results: Note[] = [];

    if (sourceNotes.length === 0 || destNotes.length === 0) return;

    for (let note of sourceNotes) {
        let result = note;
        if (options.constrainNotePitch) {
            result.setPitch(findNearestNotePitchInSet(note, destNotes));
        }
        if (options.constrainNoteStart) {
            result.setStart(findNearestNoteStartInSet(note, destNotes));
        }
        results.push(result);
    }

    return results;
}
