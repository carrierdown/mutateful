///<reference path="Note.ts"/>

enum Action {
    Constrain,
    Transpose,
    Monophonize,
    Fractalize,
    Mix,
    Interleave
}

enum InterleaveMode {
    EventCount, // e.g. interleave 2 events from A for every 1 event from B
    TimeRange   // e.g. interleave 1/4 from A for every 1/8 from B
}

interface GhostClip {
    notes: Note[];
    length: IBig;
}

interface IAction {
    (notesToMutate: Clip, notesToSourceFrom: Clip, options: IActionOptions): GhostClip;
}

interface IActionMap {
    [index: number]: IAction;
}

interface INoteDurationMap {
    [durationName: string]: IBig;
}

interface IActionOptions {
    constrainNoteStart?: boolean;
    constrainNotePitch?: boolean;
    interleaveMode?: InterleaveMode;
    interleaveCountA?: number;
    interleaveCountB?: number;
    interleaveEventRangeA?: IBig;
    interleaveEventRangeB?: IBig;
}

class ClipActions {

    public actions: IActionMap;
    public noteDurations: INoteDurationMap;

    constructor() {

        this.noteDurations = {};
        var barLength = new Big(4);
        this.noteDurations["1"] = barLength;
        this.noteDurations["1/2"] = barLength.div(new Big(2));
        this.noteDurations["1/4"] = barLength.div(new Big(4));
        this.noteDurations["1/8"] = barLength.div(new Big(8));
        this.noteDurations["1/16"] = barLength.div(new Big(16));
        this.noteDurations["1/32"] = barLength.div(new Big(32));

        this.actions = [];

        this.actions[Action.Constrain] = (clipToMutate: Clip, clipToSourceFrom: Clip, options: IActionOptions) => {
            var notesToMutate: Note[] = clipToMutate.getNotes(),
                notesToSourceFrom: Note[] = clipToSourceFrom.getNotes(),
                resultClip: GhostClip = ClipActions.newGhostClip();

            for (let note of notesToMutate) {
                let result = note;

                if (options.constrainNotePitch) {
                    result.setPitch(ClipActions.findNearestNotePitchInSet(note, notesToSourceFrom));
                }
                if (options.constrainNoteStart) {
                    result.setStart(ClipActions.findNearestNoteStartInSet(note, notesToSourceFrom));
                }
                resultClip.notes.push(result);
            }
            return resultClip;
        };

        this.actions[Action.Interleave] = (clipToMutate: Clip, clipToSourceFrom: Clip, options: IActionOptions) => {
            var a: Note[] = clipToMutate.getNotes(),
                b: Note[] = clipToSourceFrom.getNotes(),
                resultClip: GhostClip = ClipActions.newGhostClip();

            ClipActions.sortNotes(a);
            ClipActions.sortNotes(b);
            resultClip.length = clipToMutate.getLength().plus(clipToSourceFrom.getLength()); // works as long as multiple repeats aren't allowed

            var position: IBig = new Big(0);
            switch (options.interleaveMode) {
                case InterleaveMode.EventCount:
                    let i = 0, nix = 0;
                    while (i < b.length + a.length) {

                        // if i = 0 for a, update pos
                        // add a at pos
                        // update pos with next a
                        // if i = 0 for next a, calculate distance from start of event to end of clip and add to pos
                        // add b at pos
                        // update pos with next b
                        // if i = 0 for next b, calculate distance from start of event to end of clip and add to pos

                        var addNextNote = (noteSrc: Note[], position: IBig, ix: number) => {
                            let pos = position;
                            console.log(`Set start ${pos.toFixed(4)}`);
                            resultClip.notes.push(Note.clone(noteSrc[ix % noteSrc.length]).setStart(pos));
                            if ((ix + 1) % noteSrc.length === 0 && ix > 0) {
                                pos = pos.plus(clipToMutate.getLength().minus(noteSrc[ix % noteSrc.length].getStart()));
                                console.log(`Update pos at boundary ${pos.toFixed(4)}`);
                                pos = pos.plus(noteSrc[(ix + 1) % noteSrc.length].getStart());
                            } else {
                                pos = pos.plus(noteSrc[(ix + 1) % noteSrc.length].getStart()).minus(noteSrc[ix % noteSrc.length].getStart());
                            }
                            console.log(`Update pos ${pos.toFixed(4)}`);
                            return pos;
                        };

                        if (i === 0) {
                            position = position.plus(a[nix % a.length].getStart());
                        }
                        if (i % 2 === 0) {
                            position = addNextNote(a, position, nix);
                        }
                        if (i % 2 === 1) {
                            position = addNextNote(b, position, nix);
                        }
                        i++;
                        nix = Math.floor(i / 2);
                    }
                    break;
                case InterleaveMode.TimeRange:
                    let srcPositionA = new Big(0),
                        srcPositionB = new Big(0);
                    a = ClipActions.splitNotesAtEvery(a, options.interleaveEventRangeA, clipToSourceFrom.getLength());
                    b = ClipActions.splitNotesAtEvery(b, options.interleaveEventRangeB, clipToMutate.getLength());

                    while (position.lt(resultClip.length)) {
                        resultClip.notes = resultClip.notes.concat(ClipActions.getNotesFromRangeAtPosition(srcPositionA, srcPositionA.plus(options.interleaveEventRangeA), a, position));
                        position = position.plus(options.interleaveEventRangeA);
                        srcPositionA = srcPositionA.plus(options.interleaveEventRangeA);

                        resultClip.notes = resultClip.notes.concat(ClipActions.getNotesFromRangeAtPosition(srcPositionB, srcPositionB.plus(options.interleaveEventRangeB), b, position));
                        position = position.plus(options.interleaveEventRangeB);
                        srcPositionB = srcPositionB.plus(options.interleaveEventRangeB);
                    }
                    break;
            }
            return resultClip;
        };
    }

    public process(action: Action, clipToMutate: Clip, clipToSourceFrom: Clip, options: IActionOptions): GhostClip {
        return this.actions[action](clipToMutate, clipToSourceFrom, options);
    }

    private static splitNotesAtEvery(notes: Note[], position: IBig, length: IBig): Note[] {
        let i = 0,
            currentPosition: IBig = new Big(0);
        while (currentPosition.lte(length)) {
            // console.log("position", currentPosition.toFixed(4), "interleave", position.toFixed(4), "clipLength", length.toFixed(4));
            while (i < notes.length) {
                let note = notes[i];
                // console.log("start", note.getStartAsString(), "end", note.getStart().plus(note.getDuration()).toFixed(4));
                if (note.getStart().gt(currentPosition)) break;
                if (note.getStart().lt(currentPosition) && note.getStart().plus(note.getDuration()).gt(currentPosition)) {
                    // note runs across range boundary - split it
                    let rightSplitDuration: IBig;
                    rightSplitDuration = note.getStart().plus(note.getDuration()).minus(currentPosition);
                    note.setDuration(currentPosition.minus(note.getStart()));
                    let newNote = new Note(note.getPitch(), currentPosition.toFixed(4), rightSplitDuration.toFixed(4), note.getVelocity(), note.getMuted());
                    notes.splice(i + 1, 0, newNote);
                    // console.log("new note: ", note.getPitch(), "pos", currentPosition.toFixed(4), "dur", rightSplitDuration.toFixed(4));
                    i++;
                }
                i++;
            }
            currentPosition = currentPosition.plus(position);
        }
        return notes;
    }

    private static getNotesFromRangeAtPosition(start: IBig, end: IBig, notes: Note[], position: IBig): Note[] {
        var results: Note[] = [];
        let notesFromRange: Note[] = ClipActions.getNotesInRange(start, end, notes);
        // console.log(`notes from range ${srcPositionA.toFixed(4)} - ${srcPositionA.plus(options.interleaveEventRangeA).toFixed(4)}`);
        // console.dir(notesFromRange);
        for (let note of notesFromRange) {
            note.setStart(note.getStart().plus(position).minus(start));
        }
        results = results.concat(notesFromRange);
        return results;
    }

    private static getNotesInRange(start: IBig, end: IBig, notes: Note[]): Note[] {
        var results: Note[] = [];

        for (let note of notes) {
            if (note.getStart().gt(end)) {
                break;
            }
            if (note.getStart().gte(start) && note.getStart().lt(end)) {
                results.push(new Note(note.getPitch(), note.getStart(), note.getDuration(), note.getVelocity(), note.getMuted()));
            }
        }
        return results;
    }

    public static findNearestNoteStartInSet(needle: Note, haystack: Note[]): IBig {
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

    private static newGhostClip(): GhostClip {
        return {
            notes: [],
            length: new Big(4)
        } as GhostClip;
    }

    public static findNearestNotePitchInSet(needle: Note, haystack: Note[]): number {
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

    // sorts notes according to position
    public static sortNotes(notes: Note[]): void {
        notes = notes.sort((a: Note, b: Note) => {
            if (a.getStart().lt(b.getStart())) {
                return -1;
            }
            if (a.getStart().gt(b.getStart())) {
                return 1;
            }
            return 0;
        });
/*
        notes = notes.sort((a: Note, b: Note) => {
            if (a.getPitch() < b.getPitch()) {
                return -1;
            }
            if (a.getPitch() > b.getPitch()) {
                return 1;
            }
            return 0;
        });
*/
    }
}