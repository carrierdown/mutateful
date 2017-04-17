///<reference path="Note.ts"/>

enum Action {
    Constrain,
    Transpose,
    Monophonize,
    Fractalize,
    Mix,
    Interleave
}

interface IAction {
    (notesToMutate: Note[], notesToSourceFrom: Note[], options: IActionOptions): Note[];
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
}