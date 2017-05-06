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
            resultClip.length = clipToMutate.getLength().plus(clipToSourceFrom.getLength());

            var position: IBig = new Big(0);
            if (options.interleaveMode === InterleaveMode.EventCount) {
                let i = 0;

                while (i < b.length || i < a.length) {
                    let ca = a[i % a.length],
                        cb = b[i % b.length];

                    // if i = 0 for a, update pos
                    // add a at pos
                    // update pos with next a
                    // if i = 0 for next a, calculate distance from start of event to end of clip and add to pos
                    // add b at pos
                    // update pos with next b
                    // if i = 0 for next b, calculate distance from start of event to end of clip and add to pos

                    if (i % a.length === 0) {
                        position = position.plus(ca.getStart());
                    }
                    //console.log(i % a.length, a[(i + 1) % a.length].getStartAsString(), position.toFixed(4), ca.getStartAsString());
                    ca.setStart(position);
                    resultClip.notes.push(ca);
                    position = position.plus(a[(i + 1) % a.length].getStart()).minus(ca.getStart());
                    // todo: if ((i + 1) % a.length === 0) {
                    cb.setStart(position);
                    resultClip.notes.push(cb);
                    position = position.plus(b[(i + 1) % b.length].getStart()).minus(cb.getStart());

                    i++;
                }
            } else if (options.interleaveMode === InterleaveMode.TimeRange) {
                // split pass
                let i = 0;
                while (position.lte(clipToMutate.getLength())) {
                    console.log("position", position.toFixed(4), "interleave", options.interleaveEventRangeA.toFixed(4), "clipLength", clipToMutate.getLength().toFixed(4));
                    while (i < a.length) {
                        let note = a[i];
                        if (note.getStart().gt(position)) break;
                        if (note.getStart().lt(position) && note.getStart().plus(note.getDuration()).gt(position)) {
                            // note runs across range boundary - split it
                            let rightSplitDuration: IBig;
                            rightSplitDuration = note.getStart().plus(note.getDuration()).minus(position);
                            note.setDuration(position.minus(note.getStart()));
                            let newNote = new Note(note.getPitch(), position.toFixed(4), rightSplitDuration.toFixed(4), note.getVelocity(), note.getMuted());
                            a.splice(i + 1, 0, newNote);
                            i++;
                        }
                        i++;
                    }
                    position = position.plus(options.interleaveEventRangeA);
                }
                resultClip.notes = a;
            }
            return resultClip;
        };
    }

    public process(action: Action, clipToMutate: Clip, clipToSourceFrom: Clip, options: IActionOptions): GhostClip {
        return this.actions[action](clipToMutate, clipToSourceFrom, options);
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