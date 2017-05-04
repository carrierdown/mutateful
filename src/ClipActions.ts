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

interface IAction {
    (notesToMutate: Note[], notesToSourceFrom: Note[], options: IActionOptions): Note[];
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
    interleaveEventCountA?: number;
    interleaveEventCountB?: number;
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

        this.actions[Action.Constrain] = (notesToMutate: Note[], notesToSourceFrom: Note[], options: IActionOptions) => {
            var results: Note[] = [];

            for (let note of notesToMutate) {
                let result = note;

                if (options.constrainNotePitch) {
                    result.setPitch(ClipActions.findNearestNotePitchInSet(note, notesToSourceFrom));
                }
                if (options.constrainNoteStart) {
                    result.setStart(ClipActions.findNearestNoteStartInSet(note, notesToSourceFrom));
                }
                results.push(result);
            }
            return results;
        };

        this.actions[Action.Interleave] = (a: Note[], b: Note[], options: IActionOptions) => {
            var results: Note[] = [];
            ClipActions.sortNotes(a);
            ClipActions.sortNotes(b);

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
                    results.push(ca);
                    position = position.plus(a[(i + 1) % a.length].getStart()).minus(ca.getStart());
                    // todo: if ((i + 1) % a.length === 0) {
                    cb.setStart(position);
                    results.push(cb);
                    position = position.plus(b[(i + 1) % b.length].getStart()).minus(cb.getStart());

                    i++;
                }
            }
            return results;
        };
    }

    public process(action: Action, notesToMutate: Note[], notesToSourceFrom: Note[], options: IActionOptions): Note[] {
        return this.actions[action](notesToMutate, notesToSourceFrom, options);
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