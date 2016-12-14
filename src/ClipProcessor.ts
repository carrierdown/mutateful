///<reference path="ClipActions.ts"/>
///<reference path="Clip.ts"/>

class ClipProcessor {

    public clipToMutate: Clip;
    public clipToSourceFrom: Clip;
    public clipActions: ClipActions;
    public action: Action;
    public options: IActionOptions;

    constructor() {
        this.clipActions = new ClipActions();
    }

    public setClipToMutate(clip = new Clip()): void {
        this.clipToMutate = clip;
    }

    public setClipToSourceFrom(clip = new Clip()): void {
        this.clipToSourceFrom = clip;
    }

    public setAction(action: Action): void {
        this.action = action;
    }

    public setOption(optionName: string, value: any) {

    }

    public processClip(options: IActionOptions = {}) {
        if (!this.clipToMutate || !this.clipToSourceFrom) return;

        var notesToMutate: Note[] = this.clipToMutate.getNotes();
        var notesToSourceFrom: Note[] = this.clipToSourceFrom.getNotes();

        if (notesToMutate.length === 0 || notesToSourceFrom.length === 0) return;

        // todo: selection logic goes here...
        // console.log("processClip");

        var mutatedNotes: Note[] = this.clipActions.apply(this.action, notesToMutate, notesToSourceFrom, options);
        this.clipToMutate.setNotes(mutatedNotes);
    }
}