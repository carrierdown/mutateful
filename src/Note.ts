///<reference path="big.js"/>
///<reference path="big-def.ts"/>

class Note {
    private pitch: number;
    public start: IBig;
    public duration: IBig;
    public velocity: number;
    private muted: boolean;

    public static NOTE_NAMES: string[] = ['C-2', 'C#-2', 'D-2', 'D#-2', 'E-2', 'F-2', 'F#-2', 'G-2', 'G#-2', 'A-2', 'A#-2', 'B-2', 'C-1', 'C#-1', 'D-1', 'D#-1', 'E-1', 'F-1', 'F#-1', 'G-1', 'G#-1', 'A-1', 'A#-1', 'B-1', 'C0', 'C#0', 'D0', 'D#0', 'E0', 'F0', 'F#0', 'G0', 'G#0', 'A0', 'A#0', 'B0', 'C1', 'C#1', 'D1', 'D#1', 'E1', 'F1', 'F#1', 'G1', 'G#1', 'A1', 'A#1', 'B1', 'C2', 'C#2', 'D2', 'D#2', 'E2', 'F2', 'F#2', 'G2', 'G#2', 'A2', 'A#2', 'B2', 'C3', 'C#3', 'D3', 'D#3', 'E3', 'F3', 'F#3', 'G3', 'G#3', 'A3', 'A#3', 'B3', 'C4', 'C#4', 'D4', 'D#4', 'E4', 'F4', 'F#4', 'G4', 'G#4', 'A4', 'A#4', 'B4', 'C5', 'C#5', 'D5', 'D#5', 'E5', 'F5', 'F#5', 'G5', 'G#5', 'A5', 'A#5', 'B5', 'C6', 'C#6', 'D6', 'D#6', 'E6', 'F6', 'F#6', 'G6', 'G#6', 'A6', 'A#6', 'B6', 'C7', 'C#7', 'D7', 'D#7', 'E7', 'F7', 'F#7', 'G7', 'G#7', 'A7', 'A#7', 'B7', 'C8', 'C#8', 'D8', 'D#8', 'E8', 'F8', 'F#8', 'G8'];
    public static MIN_DURATION: any = new Big(1/128);

    constructor(pitch: number, start: string | number, duration: string | number, velocity: number, muted: boolean) {
        this.pitch = pitch;
        this.start = new Big(start);
        this.duration = new Big(duration);
        this.velocity = velocity;
        this.muted = muted;
    }

    public toString(): string {
        return `
        pitch: ${this.getPitch()}
        start: ${this.getStart()}
        duration: ${this.getDuration()}
        velocity: ${this.getVelocity()}
        muted: ${this.getMuted()}`;
    }

    public getPitch(): number {
        if(this.pitch < 0) return 0;
        if(this.pitch > 127) return 127;
        return this.pitch;
    }

    public getStart(): string {
        if (this.start.lt(0)) return "0.0";
        return this.start.toFixed(4);
    }

    public getDuration(): string {
        if (this.duration.lt(Note.MIN_DURATION)) return Note.MIN_DURATION.toFixed(4);
        return this.duration.toFixed(4);
    }

    public getVelocity(): number {
        if (this.velocity < 0) return 0;
        if (this.velocity > 127) return 127;
        return this.velocity;
    }

    public getMuted(): number {
        return this.muted ? 1 : 0;
    }
}

/*
function Clip() {
    var path = "live_set view highlighted_clip_slot clip";
    this.liveObject = new LiveAPI(path);
}

Clip.prototype.getLength = function() {
    return this.liveObject.get('length');
};

Clip.prototype._parseNoteData = function(data) {
    var notes = [];
    // data starts with "notes"/count and ends with "done" (which we ignore)
    for(var i=2,len=data.length-1; i<len; i+=6) {
        // and each note starts with "note" (which we ignore) and is 6 items in the list
        var note = new Note(data[i+1], data[i+2], data[i+3], data[i+4], data[i+5]);
        notes.push(note);
    }
    return notes;
};

Clip.prototype.getSelectedNotes = function() {
    var data = this.liveObject.call('get_selected_notes');
    return this._parseNoteData(data);
};


Clip.prototype.getNotes = function(startTime, timeRange, startPitch, pitchRange) {
    startTime = startTime || 0;
    timeRange = timeRange || this.getLength();
    startPitch = startPitch || 0;
    pitchRange = pitchRange || 128;

    var data = this.liveObject.call("get_notes", startTime, startPitch, timeRange, pitchRange);
    return this._parseNoteData(data);
};

Clip.prototype._sendNotes = function(notes) {
    var liveObject = this.liveObject;
    liveObject.call("notes", notes.length);
    notes.forEach(function(note) {
        liveObject.call("note", note.getPitch(),
            note.getStart(), note.getDuration(),
            note.getVelocity(), note.getMuted());
    });
    liveObject.call('done');
};

Clip.prototype.replaceSelectedNotes = function(notes) {
    this.liveObject.call("replace_selected_notes");
    this._sendNotes(notes);
};

Clip.prototype.setNotes = function(notes) {
    this.liveObject.call("set_notes");
    this._sendNotes(notes);
};

Clip.prototype.selectAllNotes = function() {
    this.liveObject.call("select_all_notes");
};

Clip.prototype.replaceAllNotes = function(notes) {
    this.selectAllNotes();
    this.replaceSelectedNotes(notes);
};*/
