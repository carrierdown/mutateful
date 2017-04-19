///<reference path="big-def.ts"/>
///<reference path="Clip.ts"/>
///<reference path="ClipProcessor.ts"/>


declare function post(msg: string);
declare function outlet(index: number, data: any);
declare function notifyclients();
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

function getvalueof() {
    return JSON.stringify(clipProcessor.options);
}

function setvalueof(data) {
    if (data === 0) {
        clipProcessor.options = clipProcessor.getDefaultOptions();
    } else {
        clipProcessor.options = JSON.parse(data);
    }
    for (let option of Object.keys(clipProcessor.options)) {
        outlet(0, [option, clipProcessor.options[option]]);
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

function setOption(key:string, value:number) {
    clipProcessor.setOption(key, value);
    notifyclients();
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
