///<reference path="big-def.ts"/>
///<reference path="Clip.ts"/>
///<reference path="ClipProcessor.ts"/>


declare function post(msg: string);
declare var outlets: number;
declare var inlets: number;

outlets = 1;
inlets = 1;

var clipProcessor = new ClipProcessor();

function bang() {
    var clp = new Clip();
    // clp.selectAllNotes();
    // var notes: Note[] = clp.getSelectedNotes();
    var notes: Note[] = clp.getNotes();
    for (let note of notes) {
        post(note.toString());
    }
}

function setClipToMutate(): void {
    clipProcessor.setClipToMutate();
}

function setClipToSourceFrom(): void {
    clipProcessor.setClipToSourceFrom();
}

function setAction(action: string): void {
    var ix = Action[action];
    if (ix !== void 0) {
        clipProcessor.setAction(ix);
    }
}

function setOptions(options: IActionOptions) {
    this.options = options;
}

function process() {
    clipProcessor.processClip();
}

/*
function applyConstrainNoteStart(source: Clip, dest: Clip) {
    clipProcessor.processClip(Action.Constrain, {constrainNoteStart: true, constrainNotePitch: false});
}

function applyConstrainNotePitch(source: Clip, dest: Clip) {
    clipProcessor.processClip(Action.Constrain, {constrainNoteStart: true, constrainNotePitch: false});
}
*/
