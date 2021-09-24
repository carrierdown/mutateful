import {readable, Subscriber, writable} from "svelte/store";

declare const signalR: any;

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/mutatefulHub")
    .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
    .build();

connection.start()
    .then(() => {
        console.log("Connection established");
    }, () => {
        console.log("Failed to connect :(");
    });

export const clipDataStore = (triggerOnClipRef: string) => readable(new Uint8Array(), (set: Subscriber<Uint8Array>) => {
    connection.on("SetClipDataOnClient", (clipRef: string, data: Uint8Array) => {
        if (clipRef === triggerOnClipRef) {
            console.log("DebugMessage - received data: ", data);
            set(data);
        }
    });

    return () => {
        console.log("readableClip end called");
        // connection.stop();
    };
});