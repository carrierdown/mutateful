///<reference path="ClipActions.ts"/>
///<reference path="Clip.ts"/>

class ClipProcessor {

    public clipToMutate: Clip;
    public clipToSourceFrom: Clip;
    public clipActions: ClipActions;
    public action: Action;
    public options: IActionOptions;
    public defaultOptions: IActionOptions = {
        constrainNotePitch: true,
        constrainNoteStart: false
    };

    constructor() {
        this.clipActions = new ClipActions();
        this.options = this.getDefaultOptions();
    }

    public getDefaultOptions() {
        var options: IActionOptions = {};
        for (let option of Object.keys(this.defaultOptions)) {
            options[option] = this.defaultOptions[option];
        }
        return options;
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

    // Sets option. 1 = true, 0 = false
    public setOption(optionName: string, value: number) {
        if (this.options[optionName] !== undefined) {
            this.options[optionName] = (value === 1);
        }
    }

    public processClip() {
        if (!this.clipToMutate || !this.clipToSourceFrom) return;

        var notesToMutate: Note[] = this.clipToMutate.getNotes();
        var notesToSourceFrom: Note[] = this.clipToSourceFrom.getNotes();

        if (notesToMutate.length === 0 || notesToSourceFrom.length === 0) return;

        // todo: selection logic goes here...
        // console.log("processClip");

        var mutatedNotes: Note[] = this.clipActions.process(this.action, notesToMutate, notesToSourceFrom, this.options);
        this.clipToMutate.setNotes(mutatedNotes);
    }
}