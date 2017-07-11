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

function getClip(trackNo: number, clipNo: number): void {
    var liveObject = new LiveAPI(`live_set tracks ${trackNo} clip_slots ${clipNo} clip`),
        result: string = "";

    if (!liveObject) {
        post('Invalid liveObject, exiting...');
        return;
    }
    var loopStart = liveObject.get('loop_start');
    var clipLength = liveObject.get('length');
    var looping = liveObject.get('looping');
    var data: any[] = liveObject.call("get_notes", loopStart, 0, clipLength, 128);
    result += `${clipLength} ${looping} `;
    for (let i = 2, len = data.length - 1; i < len; i += 6) {
        if (data[i + NoteDataMap.muted] === 1) { // skip muted notes
            continue;
        }
        result += `${data[i + NoteDataMap.pitch]} ${data[i + NoteDataMap.start]} ${data[i + NoteDataMap.duration]} ${data[i + NoteDataMap.velocity]} `;
    }
    outlet(0, ['/mu4l/clip/get', result.slice(0, result.length - 1 /* remove last space */)]);
}

function setClip(trackNo: number, clipNo: number, data: string): void {
    if (data.length < 3) return;
    var liveObject = new LiveAPI(`live_set tracks ${trackNo} clip_slots ${clipNo} clip`);

}

function createSceneAndSetClip(trackNo: number, clipNo: number, data: string): void {
    var liveObject = new LiveAPI(`live_set`);
    var numScenes = liveObject.get('scenes').length / 2; // output is of the form id 1, id 2, id 3 and so on, so we divide by 2 to get length
    var index = clipNo;
    if (clipNo >= numScenes) {
        index = -1; // add to end
    }
    liveObject.call('create_scene', index);
    setClip(trackNo, index, data);
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