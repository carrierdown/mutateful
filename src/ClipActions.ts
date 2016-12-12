///<reference path="Note.ts"/>

enum Action {
    Constrain,
    Transpose,
    Monophonize
}

interface IAction {
    (sourceNotes: Note[], destNotes: Note[], options: IActionOptions): Note[];
}

interface IActionMap {
    [index: number]: IAction;
}

interface IActionOptions {
    constrainNoteStart?: boolean;
    constrainNotePitch?: boolean;
}

class ClipActions {

    public actions: IActionMap;

    constructor() {
        this.actions = [];

        this.actions[Action.Constrain] = (sourceNotes: Note[], destNotes: Note[], options: IActionOptions = {
            constrainNoteStart: true,
            constrainNotePitch: true
        }) => {
            var results: Note[] = [];

            for (let note of sourceNotes) {
                let result = note;

                if (options.constrainNotePitch) {
                    result.setPitch(ClipActions.findNearestNotePitchInSet(note, destNotes));
                }
                if (options.constrainNoteStart) {
                    result.setStart(ClipActions.findNearestNoteStartInSet(note, destNotes));
                }
                results.push(result);
            }
            return results;
        };
    }

    public apply(action: Action, sourceNotes: Note[], destNotes: Note[], options: IActionOptions): Note[] {
        return this.actions[action](sourceNotes, destNotes, options);
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
}