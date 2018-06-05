outlets = 1;
inlets = 1;

liveObservers = [];
liveObserversById = [];

function getClip(trackNo, clipNo) {
    var liveObject = new LiveAPI("live_set tracks " + trackNo + " clip_slots " + clipNo + " clip"), result = "";
    if (!liveObject) {
        post('Invalid liveObject, exiting...');
        return;
    }
    var result = getClipData(liveObject);
    outlet(0, ['/mu4l/clip/get', result]);
}

function getClipData(liveObject) {
    var loopStart = liveObject.get('loop_start');
    var clipLength = liveObject.get('length');
    var looping = liveObject.get('looping');
    var data = liveObject.call("get_notes", loopStart, 0, clipLength, 128);
    result += clipLength + " " + looping + " ";
    for (var i = 2, len = data.length - 1; i < len; i += 6) {
        if (data[i + 5 /* muted */] === 1) {
            continue;
        }
        result += data[i + 1 /* pitch */] + " " + data[i + 2 /* start */] + " " + data[i + 3 /* duration */] + " " + data[i + 4 /* velocity */] + " ";
    }
    return result.slice(0, result.length - 1);  // remove last space
}

// todo: robustify handling of clip references. Track should refer to midi tracks only, filtering out audio tracks. Clip numbers must be checked for overflow wrt number of scenes available.
function setClip(trackNo, clipNo, dataString) {
    post("setClip: " + dataString);
    var data = dataString.split(' ');
    if (data.length < 3)
        return;
    var pathCurrentTrack = "live_set tracks " + trackNo;
    var pathCurrentClipHolder = pathCurrentTrack + " clip_slots " + clipNo;
    var pathCurrentClip = pathCurrentClipHolder + " clip";
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
        liveObject.call('note', data[c], data[c + 1], data[c + 2], data[c + 3], 0);
    }
    liveObject.call('done');
    liveObject.set('looping', looping);
}
function setSelectedClip(dummyTrackNo, dummyClipNo, dataString) {
    post("setSelectedClip: " + dataString);
    var data = dataString.split(' ');
    if (data.length < 3)
        return;
    var pathCurrentClipHolder = "live_set view highlighted_clip_slot";
    var pathCurrentClip = pathCurrentClipHolder + " clip";
    var liveObject = new LiveAPI("live_set view selected_track");
    if (liveObject.get('has_audio_input') > 0 && liveObject.get('has_midi_input') < 1) {
        post('Not a midi track!');
        return;
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
        liveObject.call('note', data[c], data[c + 1], data[c + 2], data[c + 3], 0);
    }
    liveObject.call('done');
    liveObject.set('looping', looping);
}
function createSceneAndSetClip(trackNo, clipNo, data) {
    var liveObject = new LiveAPI("live_set");
    var numScenes = liveObject.get('scenes').length / 2; // output is of the form id 1, id 2, id 3 and so on, so we divide by 2 to get length
    var index = clipNo;
    if (clipNo >= numScenes) {
        index = -1; // add to end
        clipNo = numScenes;
    }
    liveObject.call('create_scene', index);
    setClip(trackNo, clipNo, data);
}
function enumerate() {
    var liveObject = new LiveAPI("live_set");
    var numScenes = liveObject.get('scenes').length / 2;
    var numTracks = liveObject.get("tracks").length / 2;
    //var trackIxs = [];
    for (var i = 0; i < numTracks; i++) {
        liveObject.goto("live_set tracks " + i);
        if (liveObject.get('has_audio_input') < 1 && liveObject.get('has_midi_input') > 0) {
            for (var s = 0; s < numScenes; s++) {
                liveObject.goto("live_set tracks " + i + " clip_slots " + s);
                if (liveObject.get('has_clip') > 0) {
                    liveObject.goto("live_set tracks " + i + " clip_slots " + s + " clip");
                    var existingName = liveObject.get("name");
                    liveObject.set("name", String.fromCharCode(65 + i) + (s + 1) + existingName);
                }
            }
        }
    }
}
function getSelectedClip() {
    var liveObject = new LiveAPI("live_set view selected_track");
    var result = "";
    if (!liveObject) {
        post('Invalid liveObject, exiting...');
        return;
    }
    if (liveObject.get('has_audio_input') < 1 && liveObject.get('has_midi_input') > 0) {
        liveObject.goto("live_set view highlighted_clip_slot");
        if (liveObject.get('has_clip')) {
            liveObject.goto("live_set view highlighted_clip_slot clip");
            var loopStart = liveObject.get('loop_start');
            var clipLength = liveObject.get('length');
            var looping = liveObject.get('looping');
            var data = liveObject.call("get_notes", loopStart, 0, clipLength, 128);
            result += clipLength + " " + looping + " ";
            for (var i = 2, len = data.length - 1; i < len; i += 6) {
                if (data[i + 5 /* muted */] === 1) {
                    continue;
                }
                result += data[i + 1 /* pitch */] + " " + data[i + 2 /* start */] + " " + data[i + 3 /* duration */] + " " + data[i + 4 /* velocity */] + " ";
            }
        }
        outlet(0, ['/mu4l/selectedclip/get', result.slice(0, result.length - 1 /* remove last space */)]);
        return;
    }
    outlet(0, ['/mu4l/selectedclip/get', ["!"]]);
}

function ObservableCallback(id) {
//    this.track = track;
//    this.clip = clip;
    this.id = id;
}

ObservableCallback.prototype.getCallback = function() {
    var self = this;
    return {
        callback: function(arg) {
            post("cb called with " + arg + " on id: " + self.id + "\r\n");
            if (self.api !== undefined) {
                var name = self.api.get("name") || [""];
                var thisClipData = getClipData(self.api);
                var data = expandFormula(name[0], thisClipData);
                data = "{" + self.id + "} " + data;
                outlet(0, ["/mu4l/formula/process", data]);
            }
        }
    };
}

ObservableCallback.prototype.setLiveApi = function(api) {
    this.api = api;
}

function attachClipObservers() {
    var liveObject = new LiveAPI("live_set"),
        numScenes = liveObject.get('scenes').length / 2,
        numTracks = liveObject.get("tracks").length / 2,
        formulaStartIndex,
        clipName = "",
        formulaStopIndex,
        nameCallback,
        notesCallback;

    for (var i = 0; i < liveObservers.length; i++) {
        liveObservers[i].nameObserver.property = "";
        liveObservers[i].contentObserver.property = "";
    }
    liveObservers = [];

    for (var i = 0; i < numTracks; i++) {
        liveObject.goto("live_set tracks " + i);
        if (liveObject.get('has_audio_input') < 1 && liveObject.get('has_midi_input') > 0) {
            for (var s = 0; s < numScenes; s++) {
                liveObject.goto("live_set tracks " + i + " clip_slots " + s);
                if (liveObject.get("has_clip") > 0) { // todo: ...and if name of clip corresponds to a mutate4l command
                    liveObject.goto("live_set tracks " + i + " clip_slots " + s + " clip");
                    clipName = liveObject.get("name");
                    if (!clipName.length) clipName = [""];
//                    post("name: " + clipName + "\r\n");
                    formulaStartIndex = clipName[0].indexOf("=");
//                    post(clipName.indexOf("a1"));
                    if (formulaStartIndex >= 0) {
//                        post("Attaching observers");
                        formulaStopIndex = clipName[0].indexOf(";");
                        post(clipName[0].substring(formulaStartIndex, (formulaStopIndex > formulaStartIndex ? formulaStopIndex : -1)));
                        nameCallback = new ObservableCallback(liveObject.id);
                        notesCallback = new ObservableCallback(liveObject.id);
                        liveObservers[liveObservers.length] = {
                            nameObserver: new LiveAPI(nameCallback.getCallback().callback, liveObject.unquotedpath),
                            contentObserver: new LiveAPI(notesCallback.getCallback().callback, liveObject.unquotedpath)
                        };
                        liveObservers[liveObservers.length - 1].nameObserver.property = "name";
                        liveObservers[liveObservers.length - 1].contentObserver.property = "notes";
                        nameCallback.setLiveApi(liveObservers[liveObservers.length - 1].nameObserver);
                        notesCallback.setLiveApi(liveObservers[liveObservers.length - 1].contentObserver);
                    }

                    //liveObserversById[liveObject.id + ""] = liveObservers[liveObservers.length - 1];
                }
            }
        }
    }
    /*
    var highlightedClipCallback = new ObservableCallback(-1);
    var highlightedLiveObject = new LiveAPI(highlightedClipCallback.getCallback().highlightedClipCallback, "live_set view highlighted_clip_slot");
    highlightedClipCallback.setLiveApi(highlightedLiveObject);
    highlightedLiveObject.property = "name";*/
}

function expandFormula(formula, ownClipData) {
    var clipRefTester = /^([a-z]+\d+)$|^(\*)$/,
        clipRefsFound = false,
        clipRefs = [],
        clipRefsResolved = [],
        expandedFormulaParts = [];

    if (formula.length < 5) return;
    if (formula[0] == "=") formula = formula.substring(1);

    formula = formula.toLowerCase();
    var parts = formula.split(" ");
    for (var i = 0; i < parts.length; i++) { 
        var result = clipRefTester.test(parts[i]); 
        if (!result && clipRefsFound) break;
        if (!result && i == 0) break;
        if (result) {
            clipRefsFound = true;
            clipRefs.push(parts[i]);
        }
    }

    for (i = 0; i < clipRefs.length; i++) {
        if (clipRefs[i] == "*") {
            expandedFormulaParts.push("[" + ownClipData + "]");
        } else {
            var target = resolveClipReference(clipRefs[i]);
            expandedFormulaParts.push("[" + getClip(target.x, target.y) + "]");
        }
    }

    if (clipRefs.indexOf("*") == -1) expandedFormulaParts.push("[" + ownClipData + "]");

    for (i = 0; i < parts.length; i++) {
        if (!clipRefTester.test(parts[i])) {
            expandedFormulaParts.push(parts[i]);
        }
    }
    return expandedFormulaParts.join(" ");
}

function resolveClipReference(reference) {
    var channel = "", clip = "", c = 0;

    channel = reference[c++].toLowerCase();
    while (c < reference.length && isNumeric(reference[c]))
        clip += reference[c++];

    return {
        x: channel.charCodeAt(0) - "a".charCodeAt(0),
        y: parseInt(clip) - 1
    };
}

function isNumeric(c) {
    return c.length == 1 && c >= '0' && c <= '9';
}

function isAlpha(c) {
    return c.length == 1 && (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
}
// temp
//function getClip(x, y) { return "1 0 10 10 10 10"; }