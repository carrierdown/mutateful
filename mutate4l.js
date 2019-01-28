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

// constants
var SIZE_OF_ONE_NOTE_IN_BYTES = 1 /* pitch */ + 4 /* start */ + 4 /* duration */ + 1 /* velocity */;
var SIZE_OF_HEADER_IN_BYTES = 1 /* track number */ + 1 /* clip number */ + 4 /* clipLength */ + 1 /* looping */ + 2 /* numNotes */;

function debuglog(msg) {
    if (!debuglogging) return;
    post(msg + "\r\n");
}

function floatToByteArray(value) {
    var floatValue = new Float32Array(1);
    floatValue[0] = value;
    return new Uint8Array(floatValue.buffer);
}

function asciiStringToByteArray(input) {
    var bytes = new Uint8Array(input.length);
    for (var i = 0; i < input.length; i++)
    {
        charCode = input.charCodeAt(i);
        bytes[i] = charCode & 0xFF;
    }
    return bytes;
}

function replaceAt(string, index, replace) {
  return string.substring(0, index) + replace + string.substring(index + 1);
}

function testOscBlob() {
    var string = "/mu4l/test\0\0,s\0\0heidetteerentest";
    post(string.length);
    var countBytes = [4,0,0,0];
    var valueBytes = [154,153,153,62];
    //var result = new Uint8Array(countBytes.length + valueBytes.length);

    /*for (var i = 0; i < string.length; i++)
    {
        charCode = string.charCodeAt(i);
        result[i] = charCode & 0xFF;
    }*//*
    var pos = 0;
    for (i = 0; i < countBytes.length; i++) {
        result[pos++] = countBytes[i];
    }
    for (i = 0; i < valueBytes.length; i++) {
        result[pos++] = valueBytes[i];
    }
    post(result.length + "\r\n");
    for (i = 0; i < result.length; i++) {
        post(result[i] + " ");
    }
    var finalRes = new Uint32Array(result.buffer);
    var finalfinal = [];
    finalfinal[0] = finalRes[0];
    finalfinal[1] = finalRes[1];
    finalfinal[2] = finalRes[2];
    finalfinal[3] = finalRes[3];
*/
    var result = [];
    for (var y = 0; y < 255; y++) {
        result[y] = y;
    }

    outlet(0, ["/mu4l/test", result]);
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
//            debuglog("attempting to find id " + clipId + " in referred ids: " + referredIds + " extra stuff " + referredIds.length);
            if (referredIds.indexOf(clipId) >= 0) {
//                debuglog("found current clip in referring formula - all is well\r\n");
                var expandedFormula = expandFormula(formula, id);
                if (expandedFormula) {
                   //outlet(0, ["/mu4l/formula/process", expandedFormula]);
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
        var expandedFormula = expandFormula(formula, clipId);
        debuglog("outlet 0: " + formula);
        if (expandedFormula) {
            //outlet(0, ["/mu4l/formula/process", expandedFormula]);
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

function getTrackNumber(liveObject) {
    var path = liveObject.path;
    debuglog("getTrackNumber " + path);
    var pathParts = path.split(" ");
    var trackNoIx = pathParts.indexOf("tracks");
    if (trackNoIx >= 0) {
        var trackNoPart = parseInt(pathParts[trackNoIx + 1], 10);
        return trackNoPart;
    }
    return false;
}

function getClipData(liveObject) {
    if (!liveObject) return;
    debuglog("Hello from getclipdata. trackNo is " + getTrackNumber(liveObject));
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
                        var expandedFormula = expandFormulaAsBytes(clipName, liveObject.id);
                        if (expandedFormula) {
                            outlet(0, expandedFormula);
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
            if (!liveObjectAtClip) {
                debuglog("liveobjectatclip undefined: " + target.x + "," + target.y);
                return;
            }

            debuglog("Getting clipRef " + part);
            var clipData = getClipData(liveObjectAtClip);

            if (watchedClips[liveObjectAtClip.id] === undefined) {
                debuglog("watchedclips at " + liveObjectAtClip.id + " set to " + ownId);
                watchedClips[liveObjectAtClip.id] = [ownId];
            } else if (watchedClips[liveObjectAtClip.id].indexOf(ownId) < 0) {
                watchedClips[liveObjectAtClip.id].push(ownId);
            }
            debuglog("updated watchedClips for id " + liveObjectAtClip.id + ": " + watchedClips[liveObjectAtClip.id]);
            var transformedPart = "[" + target.x + "," + target.y + ":" + clipData + "]";
        } else {
            transformedPart = part;
        }
        expandedFormulaParts.push(transformedPart);
    }
    return "{id:" + ownId + ",trackIx:" + getTrackNumber(new LiveAPI("id " + ownId)) + "} " + expandedFormulaParts.join(" ");
}

function expandFormulaAsBytes(formula, ownId) {
    var clipRefTester = /^([a-z]+\d+)$|^(\*)$/,
        expandedFormulaParts = [], // array of Uint8Array
        i;

    var formulaAsBytes = new Uint8Array(3);
    int16ToBufferAtPos(ownId, formulaAsBytes, 0);
    formulaAsBytes[2] = getTrackNumber(new LiveAPI("id " + ownId));
    var byteBuffer = [];
    byteBuffer[0] = formulaAsBytes[0];
    byteBuffer[1] = formulaAsBytes[1];
    byteBuffer[2] = formulaAsBytes[2];

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
    var clipDataBuffer = [];
    var transformedPart;
    var numberOfClips = 0;

    for (i = 0; i < parts.length; i++) {
        var part = parts[i];
        var result = clipRefTester.test(part); 
        if (result) {
            var target = resolveClipReference(part);
            var liveObjectAtClip = getLiveObjectAtClip(target.x, target.y);
            if (!liveObjectAtClip) {
                debuglog("liveobjectatclip undefined: " + target.x + "," + target.y);
                return;
            }
            if (watchedClips[liveObjectAtClip.id] === undefined) {
                debuglog("watchedclips at " + liveObjectAtClip.id + " set to " + ownId);
                watchedClips[liveObjectAtClip.id] = [ownId];
            } else if (watchedClips[liveObjectAtClip.id].indexOf(ownId) < 0) {
                watchedClips[liveObjectAtClip.id].push(ownId);
            }
            debuglog("updated watchedClips for id " + liveObjectAtClip.id + ": " + watchedClips[liveObjectAtClip.id]);
            debuglog("Getting clipRef " + part);
            var temp = getClipDataAsBytes(liveObjectAtClip, target.x, target.y);
            transformedPart = new Uint8Array(4 /* 4 length bytes */ + temp.length);
            int32ToBufferAtPos(temp.length, transformedPart, 0);
            transformedPart.set(temp, 4);
            for (var z = 0; z < transformedPart.length; z++) {
                clipDataBuffer[clipDataBuffer.length] = transformedPart[z];
            }
            numberOfClips++;
            transformedPart = getStringAsUint8Array("[" + numberOfClips + "]");
        } else {
            transformedPart = getStringAsUint8Array(part);
        }
        for (var y = 0; y < transformedPart.length; y++) {
            byteBuffer[byteBuffer.length] = transformedPart[y];
        }
//        expandedFormulaParts.push(transformedPart);
    }
    //var totalLength = 2 /* id bytes */ + 1 /* track number where formula resides */;
/*    for (i = 0; i < expandedFormulaParts.length; i++) {
        totalLength += expandedFormulaParts[i].length;
    }
    for (i = 0; i < expandedFormulaParts.length; i++) {
        for (var y = 0; y < expandedFormulaParts[i]; y++) {
            byteBuffer[byteBuffer.length] = expandedFormulaParts[i][x];
        }*/
//        formulaAsBytes.set(expandedFormulaParts[i], startPos);
//        startPos += expandedFormulaParts[i].length;
    //}
    return clipDataBuffer.concat(byteBuffer);
}

function getClipDataAsBytes(liveObject, trackNo, clipNo) {
    if (!liveObject) return;
    debuglog("Hello from getClipDataAsBytes. trackNo is " + getTrackNumber(liveObject));

    var loopStart = liveObject.get('loop_start'),
        clipLength = liveObject.get('length'),
        looping = liveObject.get('looping'),
        data = liveObject.call("get_notes", loopStart, 0, clipLength, 128),
        notes = [];

    for (var i = 2, len = data.length - 1; i < len; i += 6) {
        if (data[i + 5 /* muted */] === 1) {
            continue;
        }
        notes.push({pitch: data[i + 1], start: data [i + 2], duration: data[i + 3], velocity: data[i + 4]});
    }
    var resultBuffer = new Uint8Array(SIZE_OF_HEADER_IN_BYTES + (notes.length * SIZE_OF_ONE_NOTE_IN_BYTES)),
        currentNoteOffset = SIZE_OF_HEADER_IN_BYTES;

    resultBuffer[0] = trackNo;
    resultBuffer[1] = clipNo;
    floatToBufferAtPos(clipLength, resultBuffer, 2);
    resultBuffer[6] = looping ? 1 : 0;
    int16ToBufferAtPos(notes.length, resultBuffer, 7);

    for (i = 0; i < notes.length; i++) {
        var note = notes[i];
        resultBuffer[currentNoteOffset] = note.pitch;
        floatToBufferAtPos(note.start, resultBuffer, currentNoteOffset + 1);
        floatToBufferAtPos(note.duration, resultBuffer, currentNoteOffset + 5);
        resultBuffer[currentNoteOffset + 9] = note.velocity;
        currentNoteOffset += SIZE_OF_ONE_NOTE_IN_BYTES;
    }
    return resultBuffer;
}

function getSelectedClipAsBytes() {
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
            var buffer = getClipDataAsBytes(liveObject);
            for (var i = 0; i < buffer.length; i++) {
                post(buffer[i] + ",");
            }
        }
    }
}

function getStringAsUint8Array(value) {
    if (value.length === 0) return;
    var result = new Uint8ClampedArray(value.length);
    for (var i = 0; i < value.length; i++) {
        result[i] = value.charCodeAt(i);
    }
    return value;
}

function floatToBufferAtPos(value, byteBuffer, pos) {
    var buffer = floatToByteArray(value);
    for (var i = 0; i < buffer.length; i++) {
        byteBuffer[pos + i] = buffer[i];
    }
}

function int16ToBufferAtPos(value, byteBuffer, pos) {
    var temp = new Uint16Array(1);
    temp[0] = value;
    var buffer = new Uint8Array(temp.buffer);
    for (var i = 0; i < buffer.length; i++) {
        byteBuffer[pos + i] = buffer[i];
    }
}

function int32ToBufferAtPos(value, byteBuffer, pos) {
    var temp = new Uint32Array(1);
    temp[0] = value;
    var buffer = new Uint8Array(temp.buffer);
    for (var i = 0; i < buffer.length; i++) {
        byteBuffer[pos + i] = buffer[i];
    }
}

function floatToByteArray(value) {
    var floatValue = new Float32Array(1);
    floatValue[0] = value;
    return new Uint8Array(floatValue.buffer);
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

/*

data format:

2 bytes (id)
1 byte (track number where processed clip will be placed - i.e. where formula is specified)

x bytes - clip data chunk where
    5 bytes (clip data id chunk - must be unique - something like 255,254,253,252,251 is unlikely to occur by chance in the data)
    4 bytes (number of bytes in this chunk - excluding clip data id and the count itself (9 bytes total))
    1 byte  (track #)
    1 byte  (clip #)
    4 bytes (clip length - float)
    1 byte  (loop state - on/off)
    2 bytes (number of notes)
    x bytes - note data as chunks of 10 bytes where
        1 byte  (pitch)
        4 bytes (start - float)
        4 bytes (duration - float)
        1 byte  (velocity)

- or -

bytes to be converted into text and parsed

*/