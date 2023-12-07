const maxApi = require("max-api");
const signalR = require("@microsoft/signalr");
const signalRMsgPack = require("@microsoft/signalr-protocol-msgpack");
const MESSAGE_TYPES = maxApi.MESSAGE_TYPES;

const commandSignifiers = {
    logMessage: 1,
    setClipData: 2,
    setFormula: 3,
    setAndEvaluateFormula: 4,
    setAndEvaluateClipData: 5,
    evaluateFormulas: 6,
    resetUpdatedInternally: 7,
};

const live11Flag = 128; // 0x80

let connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/mutatefulHub?username=live")
    .withHubProtocol(new signalRMsgPack.MessagePackHubProtocol())
    .build();

connection.on("SetClipDataOnClient", (isLive11, data) => {
    isLive11 = isLive11 > 0;
    debuglog("SetClipDataOnClient: Received data, Live 11: ", isLive11);
    maxApi.outlet(["handleIncomingData", ...data]);
    // maxApi.outlet(isLive11, data);
});

maxApi.addHandler(MESSAGE_TYPES.LIST, async (...args) => {
    //maxApi.post(`received list: ${args.join(", ")}`);
    if (args.length === 0) return;
    let commandSignifier = args[0];
    let isLive11 = (commandSignifier & live11Flag) > 0;
    try {
        switch (commandSignifier & 0x0f) {
            case commandSignifiers.setClipData:
                await connection.invoke("SetClipData", isLive11, new Uint8Array(args.slice(1)));
                break;
            case commandSignifiers.setFormula:
                await connection.invoke("SetFormula", new Uint8Array(args.slice(1)));
                break;
            case commandSignifiers.setAndEvaluateClipData:
                await connection.invoke("SetAndEvaluateClipData", isLive11, new Uint8Array(args.slice(1)));
                break;
            case commandSignifiers.setAndEvaluateFormula:
                await connection.invoke("SetAndEvaluateFormula", isLive11, new Uint8Array(args.slice(1)));
                break;
            case commandSignifiers.evaluateFormulas:
                await connection.invoke("EvaluateFormulas", isLive11);
                break;
            case commandSignifiers.logMessage:
                await connection.invoke("LogMessage", new Uint8Array(args.slice(1)));
                break;
        }
    } catch (err) {
        maxApi.post(`Error occurred when invoking command with id ${commandSignifier & 0x0f}`);
    }
});

connection.start()
    .then(() => {
        maxApi.post("Connection active.");
        // connection.invoke("GetMessageFromClient", "Hello server this is max4l");
        // connection.invoke("LogMessage", new Uint8Array([60,61,62,63,64,65]));
    }, () => {
        maxApi.post("Failed to connect. Make sure mutateful app is running.");
    });

function debuglog(/* ... args */) {
    let result = "";
    for (let i = 0; i < arguments.length; i++) {
        result += (i !== 0 && i < arguments.length ? " " : "") + debugPost(arguments[i], "");
    }
    maxApi.post(result + "\r\n");
}

function debugPost(val, res) {
    if (Array.isArray(val)) {
        res += "[";
        for (let i = 0; i < val.length; i++) {
            let currentVal = val[i];
            if (currentVal === undefined || currentVal === null) {
                res += ".";
                continue;
            }
            res = debugPost(currentVal, res);
            if (i < val.length - 1) res += ", ";
        }
        res += "]";
    } else if ((typeof val === "object") && (val !== null)) {
        let props = Object.getOwnPropertyNames(val);
        res += "{";
        for (let ii = 0; ii < props.length; ii++) {
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
