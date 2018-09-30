// v 4
outlets = 3;
inlets = 3;

// global vars
var debuglogging = true;
var selectedClipObserver = {};
var clipNameObserver = {};
var clipContentsObserver = {};
var nameCallback = new ObservableCallback(-1);
var notesCallback = new ObservableCallback(-1);
var watchedClips = [];

function debuglog(msg) {
    if (!debuglogging) return;
    post(msg + "\r\n");
}

function ObservableCallback(id) {
    this.id = id;
    this.name = "<not set>";
}

ObservableCallback.prototype.getCallback = function() {
    var self = this;
    return {
        onNameChanged: function(arg) {
            var name = "";
            if (arg.indexOf("name") >= 0) {
                name = arg[arg.indexOf("name") + 1];
                // quirk: name is contained in "" if it contains spaces, otherwise not
                if (name.indexOf("\"") === 0) {
                    name = name.substr(1, name.length - 2);
                }
            }
            debuglog("name " + name + " self.name " + self.name);
            if (name.length > 0 && self.name !== name) {
                debuglog("Name changed! cb called with " + arg + " on id: " + self.id);
                self.name = name;
                debuglog("outletting to onSelectedClipRenamedOrChanged: " + self.id + "," + name);
                outlet(2, ["onSelectedClipRenamedOrChanged", parseInt(self.id, 10), name]);
            }
        },
        onNotesChanged: function(arg) {
            if (arg.indexOf("notes") >= 0) {
                debuglog("Notes changed!");
                outlet(2, ["onSelectedClipRenamedOrChanged", parseInt(self.id, 10)]);
            }
        }
    };
}

ObservableCallback.prototype.setLiveApi = function(api) {
    this.api = api;
}

function onInit() {
    debuglog("onInit called");
    debuglog("setting up selectedClipObserver");
    selectedClipObserver = new LiveAPI(onSelectedClipChanged, "live_set view");
    selectedClipObserver.property = "detail_clip";
    processAllClips();
}

function onSelectedClipRenamedOrChanged(arg1, arg2) {
    debuglog("onSelectedClipRenamedOrChanged " + arg1 + " " + arg2);
    var clipId = arg1;
    var name = arg2 || "";

    if (watchedClips[clipId] !== undefined && watchedClips[clipId].length !== 0) {
        var currentlyWatchedClips = watchedClips[clipId];
        var indexesToRemove = [];
        var updatedWatchedClips = [];

        for (var i = 0; i < currentlyWatchedClips.length; i++) {
            var id = currentlyWatchedClips[i];
            var formulaSlot = new LiveAPI("id " + id);
            var formula = getClipName(formulaSlot);
            var referredIds = formulaToReferredIds(formula);
            debuglog("attempting to find id " + clipId + " in referred ids: " + referredIds + " extra stuff " + referredIds.length);
            if (referredIds.indexOf(clipId) >= 0) {
                debuglog("found current clip in referring formula - all is well\r\n");

                var expandedFormula = expandFormulaClipRefsAnywhereEdition(formula, id);
                if (expandedFormula) {
                    expandedFormula = "{" + id + "} " + expandedFormula;
                    outlet(0, ["/mu4l/formula/process", expandedFormula]);
                } else {
                    debuglog("Unable to expand formula - check syntax: " + formula);
                }
            } else {
                debuglog("could not find current clip in referring formula");
                indexesToRemove.push(id);
            }
        }
        if (indexesToRemove.length > 0) {
            for (i = 0; i < currentlyWatchedClips.length; i++) {
                if (indexesToRemove.indexOf(i) >= 0) {
                    continue;
                } else {
                    updatedWatchedClips.push(currentlyWatchedClips[i]);
                }
            }
            watchedClips[clipId] = updatedWatchedClips;
        }
    }
    debuglog("name: " + name);
    if (containsFormula(name)) {
        debuglog("hei");
        var formulaSlot = new LiveAPI("id " + clipId);
        var formula = getClipName(formulaSlot);
        var expandedFormula = expandFormulaClipRefsAnywhereEdition(formula, clipId);
        if (expandedFormula) {
            expandedFormula = "{" + clipId + "} " + expandedFormula;
            outlet(0, ["/mu4l/formula/process", expandedFormula]);
        } else {
            debuglog("Unable to expand formula - check syntax: " + formula);
        }
    }
}

function updateObserversOnClipChange(rawId) {
    debuglog("Updating observers...");
    var id = "id " + rawId;
    clipNameObserver.property = "";
    clipNameObserver = null;
    clipNameObserver = new LiveAPI(nameCallback.getCallback().onNameChanged, id);
    nameCallback.id = clipNameObserver.id;
    nameCallback.name = getClipName(clipNameObserver);
    clipNameObserver.property = "name";
    nameCallback.setLiveApi(clipNameObserver);
    
    clipContentsObserver.property = "";
    clipContentsObserver = null;
    clipContentsObserver = new LiveAPI(notesCallback.getCallback().onNotesChanged, id);
    notesCallback.id = clipContentsObserver.id;
    notesCallback.name = getClipName(clipContentsObserver);
    clipContentsObserver.property = "notes";
    notesCallback.setLiveApi(clipContentsObserver);
}

function onSelectedClipChanged(args) {
    if (!args || args.length === 0) return;
    var id = args[args.length - 1];
    if (id === 0) return; // return on empty clip
    debuglog("Selected clip changed, id: " + id);
    // update watchers
    outlet(1, ["updateObserversOnClipChange", id]); // Max does not support creating LiveAPI objects in custom callbacks, so this is handled by piping data back into inlet 2 (see msg_int function above)
}

function getClipName(liveObject) {
    var clipName = liveObject.get("name");
    if (!clipName.length || clipName.length === 0) return "";
    else return clipName[0];
}

function clipRefToId(clipRef) {
    var target = resolveClipReference(clipRef);
    var liveObjectAtClip = getLiveObjectAtClip(target.x, target.y);
    if (!liveObjectAtClip) return;
    return liveObjectAtClip.id;
}

function getClip(trackNo, clipNo) {
    var liveObjectAtClip = getLiveObjectAtClip(trackNo, clipNo);
    if (!liveObjectAtClip) return;
    return getClipData(liveObjectAtClip);
}

function getLiveObjectAtClip(trackNo, clipNo) {
    debuglog("getting track " + trackNo + " clip " + clipNo);
    var liveObject = new LiveAPI("live_set tracks " + trackNo);

    if (!liveObject) {
        debuglog('Invalid liveObject, exiting...');
        return;
    }
    if (liveObject.get('has_audio_input') > 0 && liveObject.get('has_midi_input') < 1) {
        debuglog('Not a midi track!');
        return;
    }
    liveObject.goto("live_set tracks " + trackNo + " clip_slots " + clipNo);

    if (liveObject.get('has_clip') < 1) {
        debuglog("No clip present at track: " + trackNo + 1 + " clip: " + clipNo + 1);
        return;
    }
    liveObject.goto("live_set tracks " + trackNo + " clip_slots " + clipNo + " clip");

    return liveObject;
}

function getClipData(liveObject) {
    var loopStart = liveObject.get('loop_start');
    var clipLength = liveObject.get('length');
    var looping = liveObject.get('looping');
    var data = liveObject.call("get_notes", loopStart, 0, clipLength, 128);
    var result = clipLength + " " + looping + " ";
    for (var i = 2, len = data.length - 1; i < len; i += 6) {
        if (data[i + 5 /* muted */] === 1) {
            continue;
        }
        result += data[i + 1 /* pitch */] + " " + data[i + 2 /* start */] + " " + data[i + 3 /* duration */] + " " + data[i + 4 /* velocity */] + " ";
    }
//    debuglog(result);
    return result.slice(0, result.length - 1);  // remove last space
}

// todo: robustify handling of clip references. Track should refer to midi tracks only, filtering out audio tracks. Clip numbers must be checked for overflow wrt number of scenes available.
function setClip(trackNo, clipNo, dataString) {
    debuglog("setClip: " + dataString);
    var data = dataString.split(' ');
    if (data.length < 3)
        return;
    var pathCurrentTrack = "live_set tracks " + trackNo;
    var pathCurrentClipHolder = pathCurrentTrack + " clip_slots " + clipNo;
    var pathCurrentClip = pathCurrentClipHolder + " clip";
    var liveObject = new LiveAPI(pathCurrentTrack);
    if (liveObject.get('has_audio_input') > 0 && liveObject.get('has_midi_input') < 1) {
        debuglog('Not a midi track!');
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

function setClipById(id, dummy, dataString) {
    debuglog("hello from setClipById");
    var data = dataString.split(' ');
    if (data.length < 3)
        return;
    var liveObject = new LiveAPI("id " + id);
    var clipLength = data[0];
    var looping = data[1];
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
    debuglog("setSelectedClip: " + dataString);
    var data = dataString.split(' ');
    if (data.length < 3)
        return;
    var pathCurrentClipHolder = "live_set view highlighted_clip_slot";
    var pathCurrentClip = pathCurrentClipHolder + " clip";
    var liveObject = new LiveAPI("live_set view selected_track");
    if (liveObject.get('has_audio_input') > 0 && liveObject.get('has_midi_input') < 1) {
        debuglog('Not a midi track!');
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
                    var existingName = getClipName(liveObject);
debuglog(existingName.charCodeAt(0));
                    var newName = "";
                    var clipRefString = String.fromCharCode(65 + i) + (s + 1);
                    var startBracketIx = existingName.indexOf("[");
debuglog(startBracketIx);
                    var endBracketIx = existingName.indexOf("]", startBracketIx);
debuglog(endBracketIx);
                    if (startBracketIx >= 0 && endBracketIx >= 0) {
                        newName = existingName.substring(0, startBracketIx + 1) + clipRefString + existingName.substring(endBracketIx);
                    } else {
                        newName = "[" + clipRefString + "] " + existingName;
                    }
                    liveObject.set("name",  newName);
                }
            }
        }
    }
}

function getSelectedClip() {
    var liveObject = new LiveAPI("live_set view selected_track");
    var result = "";
    if (!liveObject) {
        debuglog('Invalid liveObject, exiting...');
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

function containsFormula(clipName) {
    return clipName.indexOf("=") >= 0;
}

function processAllClips() {
    var liveObject = new LiveAPI("live_set"),
        numScenes = liveObject.get('scenes').length / 2,
        numTracks = liveObject.get("tracks").length / 2,
        formulaStartIndex,
        clipName = "",
        formulaStopIndex;

    for (var i = 0; i < numTracks; i++) {
        liveObject.goto("live_set tracks " + i);
        if (liveObject.get('has_audio_input') < 1 && liveObject.get('has_midi_input') > 0) {
            for (var s = 0; s < numScenes; s++) {
                liveObject.goto("live_set tracks " + i + " clip_slots " + s);
                if (liveObject.get("has_clip") > 0) { // todo: ...and if name of clip corresponds to a mutate4l command
                    liveObject.goto("live_set tracks " + i + " clip_slots " + s + " clip");
                    clipName = getClipName(liveObject);
                    if (containsFormula(clipName)) {
                        var expandedFormula = expandFormulaClipRefsAnywhereEdition(clipName, liveObject.id);
                        if (expandedFormula) {
                            expandedFormula = "{" + liveObject.id + "} " + expandedFormula;
                            outlet(0, ["/mu4l/formula/process", expandedFormula]);
                        } else {
                            debuglog("Unable to expand formula for track " + (i + 1) + " clip " + (s + 1) + " - check syntax");
                        }
                    }
                }
            }
        }
    }
}

function formulaToReferredIds(formula) {
    var clipRefTester = /^([a-z]+\d+)$|^(\*)$/,
        clipRefsFound = false,
        clipRefs = [],
        referredIds = [];

    if (formula.length < 5) return;

    var formulaStartIndex = formula.indexOf("=");
    var formulaStopIndex = formula.indexOf(";");
    if (formulaStartIndex == -1) return; // no valid formula

    if (formulaStopIndex >= 0) {
        formula = formula.substring(formulaStartIndex + 1, formulaStopIndex).toLowerCase();
    } else {
        formula = formula.substring(formulaStartIndex + 1).toLowerCase();
    }
    
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
        var target = resolveClipReference(clipRefs[i]);
        var liveObjectAtClip = getLiveObjectAtClip(target.x, target.y);
        referredIds.push(parseInt(liveObjectAtClip.id, 10));
    }
    return referredIds;
}

function expandFormula(formula, ownId) {
    var clipRefTester = /^([a-z]+\d+)$|^(\*)$/,
        clipRefsFound = false,
        clipRefs = [],
        expandedFormulaParts = [];

    if (formula.length < 5) return;

    var formulaStartIndex = formula.indexOf("=");
    var formulaStopIndex = formula.indexOf(";");
    if (formulaStartIndex == -1) return; // no valid formula

    if (formulaStopIndex >= 0) {
        formula = formula.substring(formulaStartIndex + 1, formulaStopIndex).toLowerCase();
    } else {
        formula = formula.substring(formulaStartIndex + 1).toLowerCase();
    }
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

    debuglog("Found cliprefs...");
    for (i = 0; i < clipRefs.length; i++) {
        debuglog("clipref " + clipRefs[i]);
    }

    for (i = 0; i < clipRefs.length; i++) {
        var target = resolveClipReference(clipRefs[i]);
        var liveObjectAtClip = getLiveObjectAtClip(target.x, target.y);

        debuglog("Iterating cliprefs");
        if (watchedClips[liveObjectAtClip.id] === undefined) {
            debuglog("watchedclips at " + liveObjectAtClip.id + " set to " + ownId);
            watchedClips[liveObjectAtClip.id] = [ownId];
        } else if (watchedClips[liveObjectAtClip.id].indexOf(ownId) < 0) {
            watchedClips[liveObjectAtClip.id].push(ownId);
        }
        debuglog("updated watchedClips for id " + liveObjectAtClip.id + ": " + watchedClips[liveObjectAtClip.id]);

        var clipData = getClipData(liveObjectAtClip);
        if (!clipData) {
            return;
        }
        expandedFormulaParts.push("[" + clipData + "]");
    }

    for (i = 0; i < parts.length; i++) {
        if (!clipRefTester.test(parts[i])) {
            expandedFormulaParts.push(parts[i]);
        }
    }
    return expandedFormulaParts.join(" ");
}

function expandFormulaClipRefsAnywhereEdition(formula, ownId) {
    var clipRefTester = /^([a-z]+\d+)$|^(\*)$/,
        expandedFormulaParts = [];

    if (formula.length < 5) return;

    var formulaStartIndex = formula.indexOf("=");
    var formulaStopIndex = formula.indexOf(";");
    if (formulaStartIndex == -1) return; // no valid formula

    if (formulaStopIndex >= 0) {
        formula = formula.substring(formulaStartIndex + 1, formulaStopIndex).toLowerCase();
    } else {
        formula = formula.substring(formulaStartIndex + 1).toLowerCase();
    }
    var parts = formula.split(" ");

    for (var i = 0; i < parts.length; i++) {
        var part = parts[i];
        var result = clipRefTester.test(part); 
        if (result) {
            var target = resolveClipReference(part);
            var liveObjectAtClip = getLiveObjectAtClip(target.x, target.y);

            debuglog("Getting clipRef " + part);
            var clipData = getClipData(liveObjectAtClip);

            if (watchedClips[liveObjectAtClip.id] === undefined) {
                debuglog("watchedclips at " + liveObjectAtClip.id + " set to " + ownId);
                watchedClips[liveObjectAtClip.id] = [ownId];
            } else if (watchedClips[liveObjectAtClip.id].indexOf(ownId) < 0) {
                watchedClips[liveObjectAtClip.id].push(ownId);
            }
            debuglog("updated watchedClips for id " + liveObjectAtClip.id + ": " + watchedClips[liveObjectAtClip.id]);
            var transformedPart = "[" + clipData + "]";
        } else {
            transformedPart = part;
        }
        expandedFormulaParts.push(transformedPart);
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