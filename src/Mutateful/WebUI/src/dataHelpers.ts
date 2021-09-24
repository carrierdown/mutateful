import {Clip, NoteEvent} from "./clip";

const sizeOfOneNoteInBytesLive11 = 25;

export function decodeClip(data: Uint8Array): Clip
{
    let length = getFloat32FromByteArray(data, 2);
    let clip = new Clip(length);
    let numNotes = getUint16FromByteArray(data, 7);
    let offset = 9;
    for (let i = 0; i < numNotes; i++)
    {
        clip.notes.push(
            new NoteEvent(
                data[offset],
                getFloat32FromByteArray(data, offset + 1),
                getFloat32FromByteArray(data, offset + 5),
                getFloat32FromByteArray(data, offset + 9)
            )
        );
        offset += sizeOfOneNoteInBytesLive11;
    }
    return clip;
}

function getFloat32FromByteArray(bytes: Uint8Array, start = 0): number {
    let temp = new Uint8Array(4);
    for (let i = 0; i < 4; i++) {
        temp[i] = bytes[i + start];
    }
    let value = new Float32Array(temp.buffer);
    return value[0];
}

function getUint16FromByteArray(bytes: Uint8Array, start = 0): number {
    let temp = new Uint8Array(2);
    temp[0] = bytes[start];
    temp[1] = bytes[start + 1];
    let value = new Uint16Array(temp.buffer);
    return value[0];
}