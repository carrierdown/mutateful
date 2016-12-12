///<reference path="ClipActions.ts"/>
///<reference path="Clip.ts"/>

class ClipProcessor {

    public sourceClip: Clip;
    public destClip: Clip;
    public clipActions: ClipActions;

    constructor() {
        this.clipActions = new ClipActions();
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

        if (sourceNotes.length === 0 || destNotes.length === 0) return;

        // todo: selection logic goes here...
        // console.log("processClip");

        return this.clipActions.apply(action, sourceNotes, destNotes, options);
    }
}