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

// todo: robustify handling of clip references. Track should refer to midi tracks only, filtering out audio tracks. Clip numbers must be checked for overflow wrt number of scenes available.

function setClip(trackNo: number, clipNo: number, dataString: string): void {
    post("setClip");
    var data = dataString.split(' ');
    if (data.length < 3) return;

    var pathCurrentTrack = `live_set tracks ${trackNo}`;
    var pathCurrentClipHolder = `${pathCurrentTrack} clip_slots ${clipNo}`;
    var pathCurrentClip = `${pathCurrentClipHolder} clip`;

    var liveObject = new LiveAPI(pathCurrentTrack);
    if (liveObject.get('has_audio_input') > 0 && liveObject.get('has_midi_input') < 1) {
        post('Not a midi track!');
    }
    var clipLength = data[0];
    var looping = data[1];

    liveObject.goto(pathCurrentClipHolder);
    if (liveObject.get('has_clip') < 1) {
        liveObject.call('create_clip', clipLength);
    }
    liveObject.goto(pathCurrentClip);
    liveObject.set('loop_start', '0');
    liveObject.set('loop_end', clipLength);

    liveObject.call('select_all_notes');
    liveObject.call('replace_selected_notes');
    liveObject.call('notes', (data.length - 2) / 4);
    for (var c = 2; c < data.length; c += 4) {
        liveObject.call('note', data[c], data[c+1], data[c+2], data[c+3], 0);
    }
    liveObject.call('done');
    liveObject.set('looping', looping);
}

function createSceneAndSetClip(trackNo: number, clipNo: number, data: string): void {
    var liveObject = new LiveAPI(`live_set`);
    var numScenes = liveObject.get('scenes').length / 2; // output is of the form id 1, id 2, id 3 and so on, so we divide by 2 to get length
    var index = clipNo;
    if (clipNo >= numScenes) {
        index = -1; // add to end
        clipNo = numScenes;
    }
    liveObject.call('create_scene', index);
    setClip(trackNo, clipNo, data);
}