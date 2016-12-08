enum Action {
    Constrain,
    Transpose
}

interface IActionMap {
    [index: number]: (src: Note[], dst: Note[], options?: IActionOptions) => Note[];
}

interface IActionOptions {
    constrainNoteStart?: boolean;
    constrainNotePitch?: boolean;
}

class ClipProcessor {

    public sourceClip: Clip;
    public destClip: Clip;
    public actions: IActionMap;

    constructor() {
        this.actions = [];
        this.actions[Action.Constrain] = ClipActions.applyConstrain;
    }

    public setSource(sourceClip = new Clip()): void {
        this.sourceClip = sourceClip;
    }

    public setTarget(destClip = new Clip()): void {
        this.destClip = destClip;
    }

    public processClip(action: Action, options: IActionOptions = {}): Note[] {
        if (!this.sourceClip || !this.destClip) return;

        var sourceNotes: Note[] = this.sourceClip.getNotes();
        var destNotes: Note[] = this.destClip.getNotes();
        var results: Note[] = [];

        if (sourceNotes.length === 0 || destNotes.length === 0) return;

        // todo: selection logic goes here...

        return this.actions[action](sourceNotes, destNotes, options);
    }
}