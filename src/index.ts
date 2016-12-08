///<reference path="big-def.ts"/>
///<reference path="Clip.ts"/>

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
    clipProcessor.setSource();
}

function setTarget(): void {
    clipProcessor.setTarget();
}

function applyConstrainNoteStart(source: Clip, dest: Clip): Note[] {
    return clipProcessor.processClip(Action.Constrain, {constrainNoteStart: true, constrainNotePitch: false});
}

function applyConstrainNotePitch(source: Clip, dest: Clip): Note[] {
    return clipProcessor.processClip(Action.Constrain, {constrainNoteStart: true, constrainNotePitch: false});
}
