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

function setSource(): void {
    clipProcessor.setClipToMutate();
}

function setTarget(): void {
    clipProcessor.setClipToSourceFrom();
}

function applyConstrainNoteStart(source: Clip, dest: Clip) {
    clipProcessor.processClip(Action.Constrain, {constrainNoteStart: true, constrainNotePitch: false});
}

function applyConstrainNotePitch(source: Clip, dest: Clip) {
    clipProcessor.processClip(Action.Constrain, {constrainNoteStart: true, constrainNotePitch: false});
}
