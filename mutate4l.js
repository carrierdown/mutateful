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
var messageQueue = [];
var stringMessageHeader = [127,126,125,124];

// constants
var SIZE_OF_ONE_NOTE_IN_BYTES = 1 /* pitch */ + 4 /* start */ + 4 /* duration */ + 1 /* velocity */;
var SIZE_OF_HEADER_IN_BYTES = 1 /* track number */ + 1 /* clip number */ + 4 /* clipLength */ + 1 /* looping */ + 2 /* numNotes */;

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
            //debuglog("name " + name + " self.name " + self.name);
            if (name.length > 0 && self.name !== name) {
                //debuglog("Name changed! cb called with ", arg, " on id: ", self.id);
                self.name = name;
                //debuglog("outletting to onSelectedClipRenamedOrChanged: ", self.id, name);
                outlet(2, ["onSelectedClipRenamedOrChanged", parseInt(self.id, 10), name]);
            }
        },
        onNotesChanged: function(arg) {
            if (arg.indexOf("notes") >= 0) {
                ////debuglog("Notes changed!");
                outlet(2, ["onSelectedClipRenamedOrChanged", parseInt(self.id, 10)]);
            }
        }
    };
}

ObservableCallback.prototype.setLiveApi = function(api) {
    this.api = api;
}

function onInit() {
    selectedClipObserver = new LiveAPI(onSelectedClipChanged, "live_set view");
    selectedClipObserver.property = "detail_clip";
    processAllClips();
}

function onSelectedClipRenamedOrChanged(arg1, arg2) {
    var clipId = arg1;
    var name = arg2 || "";
    var clipSlot;

    /*if (clipId > 0) {
        var lo = new LiveAPI("id " + clipId);
        enumerateClip(getTrackNumber(lo), getClipNumber(lo), lo);
    }*/

    if (name.indexOf("[") === -1 && name.indexOf("]") === -1) {
        clipSlot = new LiveAPI("id " + clipId);
        enumerateClip(getTrackNumber(clipSlot), getClipNumber(clipSlot) + 1, clipSlot);
    }

    if (watchedClips[clipId] !== undefined && watchedClips[clipId].length !== 0) {
        var currentlyWatchedClips = watchedClips[clipId];
        var indexesToRemove = [];
        var updatedWatchedClips = [];

        for (var i = 0; i < currentlyWatchedClips.length; i++) {
            var id = currentlyWatchedClips[i];
            var formulaSlot = new LiveAPI("id " + id);
            var formula = getClipName(formulaSlot);
            var referredIds = formulaToReferredIds(formula);
            //debuglog("attempting to find id ", clipId, " in referred ids: ", referredIds);
            if (referredIds.indexOf(clipId) >= 0) {
                debuglogExt("found current clip in referring formula - all is well");
                var expandedFormula = expandFormulaAsBytes(formula, id);
                if (expandedFormula) {
                    debuglog("send expandedFormula", expandedFormula);
                    //outlet(0, expandedFormula);
                    messageQueue[messageQueue.length] = expandedFormula;
                } else {
                    debuglogExt("Unable to expand formula for track " + (i + 1) + " clip " + (s + 1) + " - check syntax", expandedFormula);
                }
            } else {
                debuglogExt("could not find current clip in referring formula");
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
    if (containsFormula(name)) {
        if (clipSlot === undefined) { 
            clipSlot = new LiveAPI("id " + clipId); 
        }
        var formula = getClipName(clipSlot);
        var expandedFormula = expandFormulaAsBytes(formula, clipId);
        if (expandedFormula) {
            //outlet(0, expandedFormula);
            messageQueue[messageQueue.length] = expandedFormula;
        } else {
            debuglogExt("Unable to expand formula - check syntax: " + formula);
        }
    }
}

function updateObserversOnClipChange(rawId) {
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
    var id = args[args.length - 1];
    if (id === 0) return; // return on empty clip
    // update watchers
    outlet(1, ["updateObserversOnClipChange", id]); // Max does not support creating LiveAPI objects in custom callbacks, so this is handled by piping data back into itself (see updateObserversOnClipChange function above)
}

function getClipName(liveObject) {
    var clipName = liveObject.get("name");
    if (!clipName.length || clipName.length === 0) return "";
    return clipName[0] + ""; // if name is numeric, the live api turns it into a number instead of a string, so we need to coerce it.
}

function getClipLength(liveObject) {
    var clipLength = liveObject.get("length");
    if (clipLength.length > 0) return clipLength[0];
    return clipLength;
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

function isMidiTrack(liveObject) {
    if (liveObject.get('has_audio_input')[0] === 0 && liveObject.get('has_midi_input')[0] === 1) {
        return true;
    }
    return false;
}

function hasClip(liveObject) {
    if (liveObject.get('has_clip')[0] === 1) {
        return true;
    }
    return false;
}

function getLiveObjectAtClip(trackNo, clipNo) {
    //debuglog("getting track " + trackNo + " clip " + clipNo);
    var liveObject = new LiveAPI("live_set tracks " + trackNo);

    if (!liveObject) {
        debuglogExt('Invalid liveObject, exiting...');
        return;
    }
    if (!isMidiTrack(liveObject)) {
        debuglogExt('Not a midi track!');
        return;
    }
    liveObject.goto("live_set tracks " + trackNo + " clip_slots " + clipNo);

    if (!hasClip(liveObject)) {
        debuglogExt("No clip present at track: " + trackNo + 1 + " clip: " + clipNo + 1);
        return;
    }
    liveObject.goto("live_set tracks " + trackNo + " clip_slots " + clipNo + " clip");

    return liveObject;
}

function getTrackNumber(liveObject) {
    var path = liveObject.path;
    var pathParts = path.split(" ");
    var trackNoIx = pathParts.indexOf("tracks");
    if (trackNoIx >= 0) {
        var trackNoPart = parseInt(pathParts[trackNoIx + 1], 10);
        return trackNoPart;
    }
    return false;
}

function getClipNumber(liveObject) {
    var path = liveObject.path;
    var pathParts = path.split(" ");
    var clipNoIx = pathParts.indexOf("clip_slots");
    if (clipNoIx >= 0) {
        var clipNoPart = parseInt(pathParts[clipNoIx + 1], 10);
        return clipNoPart;
    }
    return false;
}

function getClipData(liveObject) {
    if (!liveObject) return;
    //debuglog("Hello from getclipdata. trackNo is " + getTrackNumber(liveObject));
    var loopStart = liveObject.get('loop_start');
    var clipLength = getClipLength(liveObject);
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
    //debuglog("setClip: " + dataString);
    var data = dataString.split(' ');
    if (data.length < 3)
        return;
    var pathCurrentTrack = "live_set tracks " + trackNo;
    var pathCurrentClipHolder = pathCurrentTrack + " clip_slots " + clipNo;
    var pathCurrentClip = pathCurrentClipHolder + " clip";
    var liveObject = new LiveAPI(pathCurrentTrack);
    if (!isMidiTrack(liveObject)) {
        debuglogExt('Not a midi track!');
    }
    var clipLength = data[0];
    var looping = data[1];
    liveObject.goto(pathCurrentClipHolder);
    if (!hasClip(liveObject)) {
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

function getUint16FromByteArray(bytes, start) {
    var temp = new Uint8Array(2);
    temp[0] = bytes[start];
    temp[1] = bytes[start + 1];
    var value = new Uint16Array(temp.buffer);
    return value[0];
}

function getFloat32FromByteArray(bytes, start) {
    var temp = new Uint8Array(4);
    for (var i = 0; i < 4; i++) {
        temp[i] = bytes[i + start];
    }
    var value = new Float32Array(temp.buffer);
    return value[0];
}

function getNormalizedFloatValue(val) {
    var temp = val + ""; // convert to string
    if (temp.indexOf(".") == -1) {
        return temp + ".0";
    } else {
        return temp;
    }
}

function setClipFromBytes(/* ... arguments */) {
    debuglog("setClipFromBytes called");
    if (arguments.length < 9) {
        post("Error - expected bigger payload");
    }
    var id = getUint16FromByteArray(arguments, 0);
    var clipLength = getFloat32FromByteArray(arguments, 2);
    var isLooping = arguments[6];
    var numNotes = getUint16FromByteArray(arguments, 7);
    var startOffset = 9;

    var liveObject = new LiveAPI("id " + id);
    liveObject.set('loop_start', '0');
    liveObject.set('loop_end', clipLength);
    liveObject.set('end_marker', clipLength);
    liveObject.set('looping', isLooping);
    liveObject.call('select_all_notes');
    liveObject.call('replace_selected_notes');
    liveObject.call('notes', numNotes);
    for (var c = 0; c < numNotes; c++) {
        var pitch = arguments[startOffset];
        var start = getNormalizedFloatValue(getFloat32FromByteArray(arguments, startOffset + 1));
        var duration = getNormalizedFloatValue(getFloat32FromByteArray(arguments, startOffset + 5));
        var velocity = arguments[startOffset + 9];
        liveObject.call('note', pitch, start, duration, velocity, 0 /* not muted */);
        startOffset += 10;
    }
    liveObject.call('done');
}

function setClipById(id, dummy, dataString) {
    //debuglog("hello from setClipById");
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
    //debuglog("setSelectedClip: " + dataString);
    var data = dataString.split(' ');
    if (data.length < 3)
        return;
    var pathCurrentClipHolder = "live_set view highlighted_clip_slot";
    var pathCurrentClip = pathCurrentClipHolder + " clip";
    var liveObject = new LiveAPI("live_set view selected_track");
    if (!isMidiTrack(liveObject)) {
        debuglog('Not a midi track!');
        return;
    }
    var clipLength = data[0];
    var looping = data[1];
    liveObject.goto(pathCurrentClipHolder);
    if (!hasClip(liveObject)) {
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
                if (hasClip(liveObject)) {
                    liveObject.goto("live_set tracks " + i + " clip_slots " + s + " clip");
                    enumerateClip(i, s + 1, liveObject);
                }
            }
        }
    }
}

function enumerateClip(trackNo, clipNo, liveObject) {
    var existingName = getClipName(liveObject);
    //debuglogExt("existingName", existingName);
    var newName = "";
    var clipRefString = String.fromCharCode(65 + trackNo) + clipNo;
    var startBracketIx = existingName.indexOf("[");
    var endBracketIx = existingName.indexOf("]", startBracketIx);
    if (startBracketIx >= 0 && endBracketIx >= 0) {
        newName = existingName.substring(0, startBracketIx + 1) + clipRefString + existingName.substring(endBracketIx);
    } else {
        newName = "[" + clipRefString + "] " + existingName;
    }
    //debuglogExt(startBracketIx, endBracketIx, newName);
    liveObject.set("name",  newName);
}

function getSelectedClip() {
    var liveObject = new LiveAPI("live_set view selected_track");
    var result = "";
    if (!liveObject) {
        debuglog('Invalid liveObject, exiting...');
        return;
    }
    //debuglog("has_audio_input", liveObject.get('has_audio_input'));
    post(liveObject.get('has_audio_input')[0]);
    if (liveObject.get('has_audio_input') < 1 && liveObject.get('has_midi_input') > 0) {
        liveObject.goto("live_set view highlighted_clip_slot");
        if (hasClip(liveObject)) {
            liveObject.goto("live_set view highlighted_clip_slot clip");
            var loopStart = liveObject.get('loop_start');
            //debuglog("loopstart", loopStart);
            post(loopStart[0]);
            var clipLength = getClipLength(liveObject);
            var looping = liveObject.get('looping');
            //debuglog("looping", looping);
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
    return clipName !== undefined && clipName.indexOf("=") >= 0;
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
                if (hasClip(liveObject)) { // todo: ...and if name of clip corresponds to a mutate4l command
                    liveObject.goto("live_set tracks " + i + " clip_slots " + s + " clip");
                    clipName = getClipName(liveObject);
                    if (containsFormula(clipName)) {
                        var expandedFormula = expandFormulaAsBytes(clipName, liveObject.id);
                        if (expandedFormula) {
                            //outlet(0, expandedFormula);
                            messageQueue[messageQueue.length] = expandedFormula;
                        } else {
                            debuglogExt("Unable to expand formula for track " + (i + 1) + " clip " + (s + 1) + " - check syntax");
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
                debuglogExt("liveobjectatclip undefined: " + target.x + "," + target.y);
                return;
            }

            //debuglog("Getting clipRef " + part);
            var clipData = getClipData(liveObjectAtClip);

            if (watchedClips[liveObjectAtClip.id] === undefined) {
//                debuglog("watchedclips at " + liveObjectAtClip.id + " set to " + ownId);
                watchedClips[liveObjectAtClip.id] = [ownId];
            } else if (watchedClips[liveObjectAtClip.id].indexOf(ownId) < 0) {
                watchedClips[liveObjectAtClip.id].push(ownId);
            }
            //debuglog("updated watchedClips for id " + liveObjectAtClip.id + ": " + watchedClips[liveObjectAtClip.id]);
            //debuglog("watchedClips now looks like", watchedClips);
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
        i;

    if (formula.length < 5) return;

    var formulaStartIndex = formula.indexOf("=");
    var formulaStopIndex = formula.indexOf(";");
    if (formulaStartIndex == -1) return; // no valid formula

    if (formulaStopIndex >= 0) {
        formula = formula.substring(formulaStartIndex + 1, formulaStopIndex).toLowerCase();
    } else {
        formula = formula.substring(formulaStartIndex + 1).toLowerCase();
    }
    var parts = formula.split(" "),
        clipDataBuffer = [],
        clipData,
        transformedPart,
        numberOfClips = 0, 
        y,
        byteBuffer = [];

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
            if (liveObjectAtClip.id == ownId) {
                debuglogExt("Recursive reference detected: A formula cannot reference itself.");
                return;
            }
            if (watchedClips[liveObjectAtClip.id] === undefined) {
                debuglogExt("watchedclips at " + liveObjectAtClip.id + " set to " + ownId);
                watchedClips[liveObjectAtClip.id] = [ownId];
            } else if (watchedClips[liveObjectAtClip.id].indexOf(ownId) < 0) {
                watchedClips[liveObjectAtClip.id].push(ownId);
            }
            //debuglog("updated watchedClips for id " + liveObjectAtClip.id + ": " + watchedClips[liveObjectAtClip.id]);
            //debuglog("Getting clipRef " + part);
            //debuglog(watchedClips, "watchedClips");
            var clipData = getClipDataAsBytes(liveObjectAtClip, target.x, target.y);
            for (var z = 0; z < clipData.length; z++) {
                clipDataBuffer[clipDataBuffer.length] = clipData[z];
            }
            transformedPart = "[" + numberOfClips + "]" + (i < parts.length - 1 ? " " : "");
            numberOfClips++;
        } else {
            transformedPart = part + (i < parts.length - 1 ? " " : "");
        }
        for (y = 0; y < transformedPart.length; y++) {
//            debuglog("Adding to bytebuffer " + transformedPart[y]);
            byteBuffer[byteBuffer.length] = transformedPart.charCodeAt(y);
        }
    }
    var currentClipLiveObject = new LiveAPI("id " + ownId);
    var metaDataBytes = new Uint8Array(4 /* id - 2 bytes, track no - 1 byte, number of inline clips - 1 byte */);
    int16ToBufferAtPos(ownId, metaDataBytes, 0);
    metaDataBytes[2] = getTrackNumber(currentClipLiveObject);
//    debuglogExt("getTrackNumber returned " + metaDataBytes[2]);
//    debuglogExt("get clip number " + getClipNumber(currentClipLiveObject));
    metaDataBytes[3] = numberOfClips;
    var metaData = [];
    for (i = 0; i < metaDataBytes.length; i++) {
        metaData[i] = metaDataBytes[i];
    }
    return metaData.concat(clipDataBuffer).concat(byteBuffer);
}

function getClipDataAsBytes(liveObject, trackNo, clipNo) {
    if (!liveObject) return;
    //debuglog("Hello from getClipDataAsBytes. trackNo is " + getTrackNumber(liveObject));

    var loopStart = liveObject.get('loop_start'),
        clipLength = getClipLength(liveObject),
        looping = liveObject.get('looping'),
        data = liveObject.call("get_notes", loopStart, 0, clipLength, 128),
        notes = [];

    //debuglog("getClipDataAsBytes clipLength = " + clipLength);

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
        debuglogExt('Invalid liveObject, exiting...');
        return;
    }
    if (liveObject.get('has_audio_input') < 1 && liveObject.get('has_midi_input') > 0) {
        liveObject.goto("live_set view highlighted_clip_slot");
        if (hasClip(liveObject)) {
            liveObject.goto("live_set view highlighted_clip_slot clip");
            var buffer = getClipDataAsBytes(liveObject);
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
    var result = new Uint8Array(floatValue.buffer);
    return result;
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

function debuglogExt(/* ... args */) {
    if (!debuglogging) return;
    var result = "";
    for (var i = 0; i < arguments.length; i++) {
        result += (i !== 0 && i < arguments.length ? " " : "") + debugPost(arguments[i], "");
    }
    var bytesToSend = [];
    for (i = 0; i < result.length; i++) {
        bytesToSend[i] = result.charCodeAt(i);
    }
    messageQueue[messageQueue.length] = stringMessageHeader.concat(bytesToSend);
}

function debuglog(/* ... args */) {
    if (!debuglogging) return;
    var result = "";
    for (var i = 0; i < arguments.length; i++) {
        result += (i !== 0 && i < arguments.length ? " " : "") + debugPost(arguments[i], "");
    }
    post(result + "\r\n");
}

function processQueue() {
    if (messageQueue.length === 0) return;
    // todo - enhancement: remove duplicate formulas, sending only the most recent one if several exist for the same cell
    outlet(0, messageQueue[0]);
    messageQueue.shift();
}

function debugPost(val, res) {
    if (Array.isArray(val)) {
        res += "[";
        for (var i = 0; i < val.length; i++) {
            var currentVal = val[i];
            if (currentVal === undefined || currentVal === null) {
                res += ".";
                continue;
            }
            res = debugPost(currentVal, res);
            if (i < val.length - 1) res += ", ";
        }
        res += "]";
    } else if ((typeof val === "object") && (val !== null)) {
        var props = Object.getOwnPropertyNames(val);
        res += "{";
        for (var ii = 0; ii < props.length; ii++) {
            res += props[ii] + ": ";
            res = debugPost(val[props[ii]], res);
            if (ii < props.length - 1) res += ", ";
        }
        res += "}";
    } else {
        res += val;
    }
    return res;
}

/*

Revised format:

2 bytes (id)
1 byte (track number where processed clip will be placed - i.e. where formula is specified)
1 byte number of inline clips

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

    Above block repeated N times

Bytes to be converted into text and parsed, where inline clips look like [x] where x is the index of the clip specified in the first part of the payload

Return format:

2 bytes (id)
4 bytes (clip length - float)
1 byte (loop state - 1/0 for on/off)
2 bytes (number of notes)
    1 byte  (pitch)
    4 bytes (start - float)
    4 bytes (duration - float)
    1 byte  (velocity)

Above block repeated N times

*/