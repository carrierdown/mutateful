declare function post(msg: string);
declare function outlet(index: number, data: any);
declare function notifyclients();
declare var outlets: number;
declare var inlets: number;
declare var LiveAPI: any;

outlets = 1;
inlets = 1;

// live_set tracks N clip_slots M clip

const enum NoteDataMap {
    pitch = 1,
    start,
    duration,
    velocity,
    muted,
}

function getClip(trackNo: number, clipNo: number): string {
    var liveObject = new LiveAPI(`live_set tracks ${trackNo} clip_slots ${clipNo} clip`),
        result: string = "";

    if (!liveObject) {
        post('Invalid liveObject, exiting...');
        return;
    }

    liveObject.call("select_all_notes");
    var data: any[] = liveObject.call('get_selected_notes');
    for (let i = 2, len = data.length - 1; i < len; i += 6) {
        // and each note starts with "note" (which we ignore) and is 6 items in the list
        result += `${data[i + NoteDataMap.pitch]} ${data[i + NoteDataMap.start]} ${data[i + NoteDataMap.duration]} ${data[i + NoteDataMap.velocity]} ${data[i + NoteDataMap.muted]}`;
    }
    return result;
}

function setClip(trackNo: number, clipNo: number, data: string): void {
    var liveObject = new LiveAPI(`live_set tracks ${trackNo} clip_slots ${clipNo} clip`);

}

/*

 basePath = "live_set view selected_track";
 // clip_slot: has_clip, create_clip
 // clip: is_midi_clip, length  -  select_all_notes,

 liveObject = new LiveAPI(basePath);

 if (!liveObject) {
 post('Invalid liveObject, exiting...');
 return;
 }

 // the liveAPI seems to have some weird issues with comparing directly with 1 and 0, so we use < and > instead
 if (liveObject.get('has_audio_input') < 1 && liveObject.get('has_midi_input') > 0) {
 post('track type is valid');

 for (var i = 0; i < input.length; i++) {
 var notes = input[i].notes,
 loopLength = input[i].loopLength;

 liveObject.goto(basePath + ' clip_slots ' + i);

 if (liveObject.get('has_clip') < 1) {
 liveObject.call('create_clip', '4.0');
 } else {
 post('no clip to create');
 }

 liveObject.goto(basePath + ' clip_slots ' + i + ' clip');
 liveObject.call('select_all_notes');
 liveObject.call('replace_selected_notes');

 liveObject.call('notes', notes.length);
 for (var c = 0; c < notes.length; c++) {
 liveObject.call('note', convertPitch(notes[c].pitch),
 convertStart(notes[c].start), convertDuration(notes[c].duration),
 convertVelocity(notes[c].velocity), convertMuted(false));
 }
 liveObject.call('done');
 liveObject.set('looping', 1);
 liveObject.set('loop_end', loopLength);
 }
 } else {
 post('not a midi track!');
 }

 */