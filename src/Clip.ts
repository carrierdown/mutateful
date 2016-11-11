///<reference path="big.js"/>
///<reference path="big-def.ts"/>
///<reference path="Note.ts"/>


declare var LiveAPI: any;

const enum NoteDataMap {
    pitch = 1,
    start,
    duration,
    velocity,
    muted,
}

class Clip {
    private liveObject: any;

    constructor(path: string = "live_set view highlighted_clip_slot clip") {
        this.liveObject = new LiveAPI(path);
    }

    public getLength(): IBig {
        return BigFactory.create(this.liveObject.get('length'));
    }

    private static parseNoteData(data: any[]): Note[] {
        var notes: Note[] = [];
        // data starts with "notes"/count and ends with "done" (which we ignore)
        for (let i = 2, len = data.length - 1; i < len; i += 6) {
            // and each note starts with "note" (which we ignore) and is 6 items in the list
            let note = new Note(data[i + NoteDataMap.pitch], data[i + NoteDataMap.start], data[i + NoteDataMap.duration], data[i + NoteDataMap.velocity], data[i + NoteDataMap.muted]);
            notes.push(note);
        }
        return notes;
    }

    public getSelectedNotes(): Note[] {
        var data = this.liveObject.call('get_selected_notes');
        return Clip.parseNoteData(data);
    }

    public getNotes(startTime: number = 0, timeRange: IBig = this.getLength(), startPitch: number = 0, pitchRange: number = 128): Note[] {
        var data = this.liveObject.call("get_notes", startTime, startPitch, timeRange.toFixed(4), pitchRange);
        return Clip.parseNoteData(data);
    }

    private sendNotes(notes): void {
        var liveObject = this.liveObject;
        liveObject.call("notes", notes.length);
        notes.forEach(function (note) {
            liveObject.call("note", note.getPitch(),
                note.getStart(), note.getDuration(),
                note.getVelocity(), note.getMuted());
        });
        liveObject.call('done');
    }

    public replaceSelectedNotes(notes): void {
        this.liveObject.call("replace_selected_notes");
        this.sendNotes(notes);
    }

    public setNotes(notes): void {
        this.liveObject.call("set_notes");
        this.sendNotes(notes);
    }

    public selectAllNotes(): void {
        this.liveObject.call("select_all_notes");
    }

    public replaceAllNotes(notes): void {
        this.selectAllNotes();
        this.replaceSelectedNotes(notes);
    }
}



