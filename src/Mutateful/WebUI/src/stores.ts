import {readable, Subscriber, writable} from "svelte/store";

declare const signalR: any;

/*export const readableClip = readable("", (set) => {
    let connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5000/mutatefulHub")
        .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
        .build();

    connection.on("DebugMessage", (data) => {
        console.log("DebugMessage - received data: ", data);
        set("fikk data!");
    });

    connection.start()
        .then(() => {
            console.log("Connection established");
        }, () => {
            console.log("Failed to connect :(");
        });
    
    return function stop() {
        connection.stop();
    };
});*/

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

export const readableClip = (triggerOnClipRef: string) => readable("", (set: Subscriber<string>) => {
    connection.on("DebugMessage", (clipRef: string, data: number) => {
        if (clipRef === triggerOnClipRef) {
            set("fikk data!");
        }
        console.log("DebugMessage - received data: ", data);
    });


    return () => {
        console.log("readableClip end called");
        // connection.stop();
    };
});



/*export function readableSpecificClip(triggerOnClipRef: string) {
    const {subscribe} = readable(0, (set) => {
        let connection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5000/mutatefulHub")
            .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
            .build();

        connection.on("DebugMessage", (clipRef: string, data: number) => {
            console.log("DebugMessage - received data: ", clipRef, data);
            if (clipRef === triggerOnClipRef) {
                set(1);
            }
        });

        connection.start()
            .then(() => {
                console.log("Connection established");
            }, () => {
                console.log("Failed to connect :(");
            });

        return () => {
            connection.stop();
        };
    });
    return {subscribe};
}*/

/*
export const readableSpecificClip = (clipRef: string) => {
    const {subscribe, readable} = readable(0);
};*/
