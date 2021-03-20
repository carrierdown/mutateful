outlets = 3;
inlets = 3;

// global vars
var debuglogging = true;
var selectedClipObserver = {};
var clipNameObserver = {};
var clipContentsObserver = {};
var nameCallback = new ObservableCallback(-1);
var notesCallback = new ObservableCallback(-1);
var messageQueue = [];

var typedDataFirstByte = 127;
var typedDataSecondByte = 126;
var typedDataThirdByte = 125;
var typedDataThirdByteL11 = 128;
var stringDataSignifier = 124;
var setClipDataSignifier = 255;
var setFormulaSignifier = 254;
var evaluateFormulasSignifier = 253;
var setAndEvaluateClipDataSignifier = 252;
var setAndEvaluateFormulaSignifier = 251;

var stringMessageHeader = [0 /* dummy id which is removed prior to sending, only used for duplicate removal with clip ids for formulas and clip data */, 
                            typedDataFirstByte, typedDataSecondByte, typedDataThirdByte, stringDataSignifier];
var setClipDataHeader = [0, typedDataFirstByte, typedDataSecondByte, typedDataThirdByteL11, setClipDataSignifier];
var setFormulaHeader = [0, typedDataFirstByte, typedDataSecondByte, typedDataThirdByte, setFormulaSignifier];
var evaluateFormulasHeader = [0, typedDataFirstByte, typedDataSecondByte, typedDataThirdByteL11, evaluateFormulasSignifier];
var setAndEvaluateClipDataHeader = [0, typedDataFirstByte, typedDataSecondByte, typedDataThirdByteL11, setAndEvaluateClipDataSignifier];
var setAndEvaluateFormulaHeader = [0, typedDataFirstByte, typedDataSecondByte, typedDataThirdByteL11, setAndEvaluateFormulaSignifier];

var SIZE_OF_ONE_NOTE_IN_BYTES = 1 /* pitch */ + 4 /* start */ + 4 /* duration */ + 4 /* velocity */ + 4 /* probability */ + 4 /* velocity_deviation */ + 4 /* release velocity */;
var SIZE_OF_HEADER_IN_BYTES = 1 /* track number */ + 1 /* clip number */ + 4 /* clipLength */ + 1 /* looping */ + 2 /* numNotes */;

// -* Public API: Main functions called via patcher *-

function onInit() {
    selectedClipObserver = new LiveAPI(onSelectedClipChanged, "live_set view");
    selectedClipObserver.property = "detail_clip";
    sendAllClipData();
}

function handleIncomingData(/* ... arguments */) {
    // debuglogExt("incoming data");
    var args = [].slice.call(arguments);
    if (args.length < 13) {
        post("Error - expected bigger payload");
    }
    if (args[0] === typedDataFirstByte &&
        args[1] === typedDataSecondByte &&
        args[2] === typedDataThirdByte) {

        if (args[3] === setClipDataSignifier) {
            onClipDataFromServer(args.slice(4));
        }
    }
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

function processQueue() {
    if (messageQueue.length === 0) return;
    var dataToSend = messageQueue[0];
    dataToSend.shift(); // remove first item of data, since this is used only for duplicate removal in addFormulaToQueue
    outlet(0, dataToSend);
    messageQueue.shift();
}

// The following handlers are called indirectly, i.e. via deferlow-object in patcher to get around bug in M4L API

function onSelectedClipWithoutName(clipId, maybeName) {
    // debuglogExt("onSelectedClipWithoutName");
    var name = maybeName || "";
    if (name.indexOf("[") === -1 && name.indexOf("]") === -1) {
        var clipSlot = new LiveAPI("id " + clipId);
        enumerateClip(getTrackNumber(clipSlot), getClipNumber(clipSlot) + 1, clipSlot);
    }
}

function onSelectedClipWasCopied(clipId, maybeName) {
    debuglogExt("onSelectedClipWasCopied", maybeName);
    var name = maybeName || "";
    var clipSlot = new LiveAPI("id " + clipId);
    enumerateClip(getTrackNumber(clipSlot), getClipNumber(clipSlot) + 1, clipSlot);
    setAndEvaluateClipDataOrFormula(clipSlot);
}

function onSelectedClipRenamed(arg1) {
    debuglogExt("onSelectedClipRenamed");
    var selectedClipSlot = new LiveAPI("id " + arg1);
    setAndEvaluateClipDataOrFormula(selectedClipSlot);
}

function onSelectedClipNotesChanged(arg1) {
    // debuglogExt("onSelectedClipNotesChanged");
    var selectedClipSlot = new LiveAPI("id " + arg1);
    setAndEvaluateClipDataOrFormula(selectedClipSlot);
}

function onSelectedClipChanged(args) {
    // debuglogExt("onSelectedClipChanged", args[args.length - 1]);
    var id = args[args.length - 1];
    if (id === 0) return; // return on empty clip
    // update watchers
    outlet(1, ["updateObserversOnClipChange", id]); // Max does not support creating LiveAPI objects in custom callbacks, so this is handled by piping data back into itself via a deferlow object
}

function resetInternalUpdateState() {
    notesCallback.updatedInternally = false;
}

function updateObserversOnClipChange(rawId) {
    var id = "id " + rawId;
    clipNameObserver.property = "";
    clipNameObserver = null;
    nameCallback.name = getClipNameFromId(rawId);
    nameCallback.id = rawId;
    clipNameObserver = new LiveAPI(nameCallback.getCallback().onNameChanged, id);
    nameCallback.skipFirstNameCallback = true; // quirk: see below
    clipNameObserver.property = "name";
    // nameCallback.setLiveApi(clipNameObserver);

    clipContentsObserver.property = "";
    clipContentsObserver = null;
    notesCallback.name = getClipNameFromId(rawId);
    notesCallback.id = rawId;
    clipContentsObserver = new LiveAPI(notesCallback.getCallback().onNotesChanged, id);
    notesCallback.skipFirstNotesCallback = true; // quirk: When a new observer is hooked up, the callback fires immediately even though no change occurred. We work around this with this variable.
    notesCallback.updatedInternally = false;
    clipContentsObserver.property = "notes";
    // notesCallback.setLiveApi(clipContentsObserver);
}

// -* ObservableCallback class *- 

function ObservableCallback(id) {
    this.id = id;
    this.name = "<not set>";
    this.skipFirstNotesCallback = true;
    this.skipFirstNameCallback = true;
    this.updatedInternally = false;
}

ObservableCallback.prototype.getCallback = function() {
    var self = this;
    return {
        onNameChanged: function(arg) {
            var name = "";
            if (arg.indexOf("name") < 0) {
                return;
            }
            name = arg[arg.indexOf("name") + 1];
            // quirk: name is contained in "" if it contains spaces, otherwise not
            if (name.indexOf("\"") === 0) {
                name = name.substr(1, name.length - 2);
            }
            if (self.skipFirstNameCallback === true) {
                // debuglogExt("onNameChanged skipped");
                self.skipFirstNameCallback = false;
                return;
            }
            // debuglogExt("Name changed");
            if (name.length > 0) {
                if (self.name === name) {
                    // clip was probably copied
                    outlet(2, ["onSelectedClipWasCopied", parseInt(self.id, 10), name]);
                } else {
                    // name of selected clip changed
                    self.name = name;
                    outlet(2, ["onSelectedClipRenamed", parseInt(self.id, 10), name]);
                }
            }
            if (name.length === 0) {
                outlet(2, ["onSelectedClipWithoutName", parseInt(self.id, 10), name]);
            }
        },
        onNotesChanged: function(arg) {
            if (self.updatedInternally === true) {
                // debuglogExt("onNotesChanged terminated");
                self.updatedInternally = false;
                return;
            }
            // debuglogExt("onNotesChanged");
            /*
             typically called twice with self data from ableton:
             onNotesChanged [id, 112]
             onNotesChanged [notes, bang]
             */
            if (self.skipFirstNotesCallback === true) {
                self.skipFirstNotesCallback = false;
                // debuglogExt("onNotesChanged skipped");
                return;
            }
            if (arg.indexOf("notes") >= 0) {
                outlet(2, ["onSelectedClipNotesChanged", parseInt(self.id, 10)]);
                self.skipFirstNotesCallback = false;
            }
        }
    };
};

/*ObservableCallback.prototype.setLiveApi = function(api) {
    this.api = api;
};*/

// -* Internal handlers and M4L API helpers *-

function onClipDataFromServer(data) {
    // debuglogExt("incoming clip data");
    var trackNo = data[0];
    var clipNo = data[1];
    var clipLength = getFloat32FromByteArray(data, 2);
    var isLooping = data[6];
    var numNotes = getUint16FromByteArray(data, 7);
    var startOffset = 9;
    var liveObject = getOrCreateClipAtPosition(trackNo, clipNo);

    liveObject.set('loop_start', 0);
    liveObject.set('loop_end', clipLength);
    liveObject.set('end_marker', clipLength);
    liveObject.set('looping', isLooping);
    liveObject.call('remove_notes_extended', 0, 128, 0, clipLength);
    
    // todo: fill dict with note objects - convert to json then send with add_new_notes function
    var noteDict = {notes:[]};
    
    // liveObject.call('notes', numNotes);
    for (var c = 0; c < numNotes; c++) {
        var pitch = data[startOffset];
        var start = getFloat32FromByteArray(data, startOffset + 1);
        var duration = getFloat32FromByteArray(data, startOffset + 5);
        var velocity = getFloat32FromByteArray(data, startOffset + 9);
        var probability = getFloat32FromByteArray(data, startOffset + 13);
        var velocityDeviation = getFloat32FromByteArray(data, startOffset + 17);
        var releaseVelocity = getFloat32FromByteArray(data, startOffset + 21);
        
        // liveObject.call('note', pitch, start, duration, velocity, 0 /* not muted */);
        var note = {
            pitch: pitch, start_time: start, duration: duration, velocity: velocity, probability: probability, 
            velocity_deviation: velocityDeviation, release_velocity: releaseVelocity
        };
        noteDict.notes.push(note);
        startOffset += SIZE_OF_ONE_NOTE_IN_BYTES;
    }
    notesCallback.updatedInternally = true; // avoid firing callback on internal updates
    liveObject.call('add_new_notes', JSON.stringify(noteDict));
    outlet(1, ["resetInternalUpdateState"]);
}

function getOrCreateClipAtPosition(trackNo, clipNo) {
    var liveObject = new LiveAPI("live_set tracks " + trackNo);

    if (!liveObject) {
        debuglogExt('Invalid liveObject at [' + trackNo + ', ' + clipNo + '], exiting...');
        return;
    }
    if (!isMidiTrack(liveObject)) {
        debuglogExt('Clip at [' + trackNo + ', ' + clipNo + '] is not a midi track :(');
        return;
    }
    liveObject.goto("live_set tracks " + trackNo + " clip_slots " + clipNo);

    if (!hasClip(liveObject)) {
        liveObject.call('create_clip', 4);
    }

    liveObject.goto("live_set tracks " + trackNo + " clip_slots " + clipNo + " clip");

    return liveObject;
}

function enumerateClip(trackNo, clipNo, liveObject) {
    var existingName = getClipName(liveObject);
    var newName = "";
    var clipRefString = String.fromCharCode(65 + trackNo) + clipNo;
    if (existingName.indexOf("[" + clipRefString + "]") >= 0) return;
    var startBracketIx = existingName.indexOf("[");
    var endBracketIx = existingName.indexOf("]", startBracketIx);
    if (startBracketIx >= 0 && endBracketIx >= 0) {
        newName = existingName.substring(0, startBracketIx + 1) + clipRefString + existingName.substring(endBracketIx);
    } else {
        newName = "[" + clipRefString + "] " + existingName;
    }
    liveObject.set("name",  newName);
}

function sendAllClipData() {
    var liveObject = new LiveAPI("live_set"),
        numScenes = getNumberOfScenes(liveObject),
        numTracks = getNumberOfTracks(liveObject),
        clipName = "",
        payload;

    for (var i = 0; i < numTracks; i++) {
        liveObject.goto("live_set tracks " + i);
        if (liveObject.get('has_audio_input') < 1 && liveObject.get('has_midi_input') > 0) {
            for (var s = 0; s < numScenes; s++) {
                liveObject.goto("live_set tracks " + i + " clip_slots " + s);
                if (hasClip(liveObject)) {
                    liveObject.goto("live_set tracks " + i + " clip_slots " + s + " clip");
                    clipName = getClipName(liveObject);
                    if (containsFormula(clipName)) {
                        var formula = extractFormula(clipName);
                        var trackNo = getTrackNumber(liveObject);
                        var clipNo = getClipNumber(liveObject);
                        if (trackNo === false || clipNo === false) {
                            post("Unable to get trackNo or clipNo");
                            continue;
                        }
                        if (formula.length == 0) continue;
                        payload = setFormulaHeader.concat(
                            [trackNo, clipNo],
                            asciiStringToArray(formula));
                    } else {
                        payload = setClipDataHeader.concat(
                            [].slice.call(getClipDataAsBytes(liveObject, i, s)));
                    }
                    messageQueue.push(payload);
                }
            }
        }
    }
    messageQueue.push([].concat(evaluateFormulasHeader)); // clone array prior to sending
}

function setAndEvaluateClipDataOrFormula(liveObject) {
    var clipName = getClipName(liveObject);
    var payload = [];
    var trackNo = getTrackNumber(liveObject);
    var clipNo = getClipNumber(liveObject);
    // debuglogExt("clipname", clipName);

    if (containsFormula(clipName)) {
        var formula = extractFormula(clipName);
        // debuglogExt("clipNo", clipNo);
        if (trackNo === false || clipNo === false) {
            post("Unable to get trackNo or clipNo");
            return;
        }
        if (formula.length === 0) return;
        payload = setAndEvaluateFormulaHeader.concat(
            [trackNo, clipNo],
            asciiStringToArray(formula));
    } else {
        payload = setAndEvaluateClipDataHeader.concat(
            [].slice.call(getClipDataAsBytes(liveObject, trackNo, clipNo)));
    }
    messageQueue.push(payload);
}

function getClipDataAsBytes(liveObject, trackNo, clipNo) {
    if (!liveObject) return;
    //debuglog("Hello from getClipDataAsBytes. trackNo is " + getTrackNumber(liveObject));
    
    var loopStart = liveObject.get('loop_start'),
        clipLength = getClipLength(liveObject),
        looping = liveObject.get('looping'),
        data = JSON.parse(liveObject.call("get_notes_extended", 0, 128, loopStart, clipLength)),
        notes = [],
        note, i;

    // filter muted notes
    for (i = 0; i < data.notes.length; i++) {
        note = data.notes[i];
        if (note.mute === 1) continue;
        notes.push(note);
    }
    
    var resultBuffer = new Uint8Array(SIZE_OF_HEADER_IN_BYTES + (notes.length * SIZE_OF_ONE_NOTE_IN_BYTES)),
        currentNoteOffset = SIZE_OF_HEADER_IN_BYTES;

    resultBuffer[0] = trackNo;
    resultBuffer[1] = clipNo;
    floatToBufferAtPos(clipLength, resultBuffer, 2);
    resultBuffer[6] = looping ? 1 : 0;
    int16ToBufferAtPos(notes.length, resultBuffer, 7);

    for (i = 0; i < notes.length; i++) {
        note = notes[i];
        resultBuffer[currentNoteOffset] = note.pitch;
        floatToBufferAtPos(note.start_time, resultBuffer, currentNoteOffset + 1);
        floatToBufferAtPos(note.duration, resultBuffer, currentNoteOffset + 5);
        floatToBufferAtPos(note.velocity, resultBuffer, currentNoteOffset + 9);
        floatToBufferAtPos(note.probability, resultBuffer, currentNoteOffset + 13);
        floatToBufferAtPos(note.velocity_deviation, resultBuffer, currentNoteOffset + 17);
        floatToBufferAtPos(note.release_velocity, resultBuffer, currentNoteOffset + 21);
        
        currentNoteOffset += SIZE_OF_ONE_NOTE_IN_BYTES;
    }
    // debuglogExt("got clip with id: ", liveObject.id);
    return resultBuffer;
}

// currently not in use, but could be useful when sending clip data or formula changes that trigger evaluation immediately
function addFormulaToQueue(id, formulaAsBytes) {
    if (formulaAsBytes.length > 16384) {
        debuglogExt("Dropping formula " + formula + " since it takes up more than the maximum allowed packet size of 16384 bytes. This will be fixed in a later version of mutate4l, but for now, use shorter and/or fewer clips per formula.");
        return;
    }
    var found = false;
    // search message queue from formulas starting with same id. If found, we replace it with the new one. Otherwise, add to the end of the list.
    for (var i = messageQueue.length - 1; i >= 0; i--) {
        if (messageQueue[i].length > 0 && messageQueue[i][0] === id) {
            messageQueue[i] = [id].concat(formulaAsBytes);
            found = true;
            break;
        }
    }
    if (!found) {
        messageQueue[messageQueue.length] = [id].concat(formulaAsBytes);
    }
}

// -* Utility functions *-

function floatToByteArray(value) {
    var floatValue = new Float32Array(1);
    floatValue[0] = value;
    return new Uint8Array(floatValue.buffer);
}

function asciiStringToArray(input) {
    var bytes = [];
    for (var i = 0; i < input.length; i++)
    {
        charCode = input.charCodeAt(i);
        bytes[i] = charCode & 0xFF;
    }
    return bytes;
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

function getClipName(liveObject) {
    var clipName = liveObject.get("name");
    if (!clipName.length || clipName.length === 0) return "";
    return clipName[0] + ""; // if name is numeric, the live api turns it into a number instead of a string, so we need to coerce it.
}

function getClipNameFromId(id) {
    return getClipName(new LiveAPI("id " + id));
}

function getClipLength(liveObject) {
    var clipLength = liveObject.get("length");
    if (clipLength.length > 0) return clipLength[0];
    return clipLength;
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

function containsFormula(clipName) {
    return clipName !== undefined && clipName.indexOf("=") >= 0;
}

function getNumberOfScenes(liveObject) {
    // divide by 2 since this yields an array with two entries per scene
    return liveObject.get("scenes").length / 2;
}

function getNumberOfTracks(liveObject) {
    // divide by 2 since this yields an array with two entries per track
    return liveObject.get("tracks").length / 2;
}

function extractFormula(clipName) {
    if (clipName.length < 5) return;

    var formulaStartIndex = clipName.indexOf("=");
    var formulaStopIndex = clipName.indexOf(";");
    var formula = "";

    if (formulaStartIndex == -1) return ""; // no valid formula

    if (formulaStopIndex >= 0) {
        formula = clipName.substring(formulaStartIndex + 1, formulaStopIndex).toLowerCase();
    } else {
        formula = clipName.substring(formulaStartIndex + 1).toLowerCase();
    }
    return formula;
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

// -* Logging utils *-

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