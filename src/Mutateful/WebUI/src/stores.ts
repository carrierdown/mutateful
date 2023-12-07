import {Subscriber, writable} from "svelte/store";

declare const signalR: any;

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/mutatefulHub?username=webui")
    .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
    .build();

let clipDataSetter: Subscriber<{clipRef: string, data: Uint8Array}> | null = null;

connection.on("SetClipDataOnWebUI", (clipRef: string, data: Uint8Array) => {
    console.log("DebugMessage - received data: ", data, "clipRef", clipRef);
    updateClipData(clipRef, data);
});

connection.start()
    .then(async () => {
        console.log("Connection established, got", connection.connectionId);
    }, () => {
        console.log("Failed to connect :(");
    });

const createClipDataStore = () => {
    const { subscribe, set } = writable({ clipRef: "", data: new Uint8Array() });

    clipDataSetter = set;

    return {
        subscribe,
        set: (clipRef: string, data: Uint8Array) => {
            set({ clipRef, data });
        },
    };
};

export const clipDataStore = createClipDataStore();

export const updateClipData = (clipRef: string, data: Uint8Array) => {
    clipDataSetter?.({ clipRef, data });
};

/*
    connection.on("SetClipDataOnClient", (clipRef: string, data: Uint8Array) => {
        console.log("Received data", data);
    });

    return () => {
        console.log("readableClip end called");
        // connection.stop();
    };
});*/
