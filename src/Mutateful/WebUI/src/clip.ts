export class Clip {
    length: number;
    notes: NoteEvent[] = [];
    
    constructor(length: number) {
        this.length = length;
    }
}

export class NoteEvent {
    pitch: number;
    start: number;
    duration: number;
    velocity: number;
    
    constructor(pitch: number, start: number, duration: number, velocity: number) {
        this.pitch = pitch;
        this.start = start;
        this.duration = duration;
        this.velocity = velocity;
    }
}