const maxApi = require("max-api");
const signalR = require("@microsoft/signalr");
const signalRMsgPack = require("@microsoft/signalr-protocol-msgpack");
const MESSAGE_TYPES = maxApi.MESSAGE_TYPES;

/*const stringDataSignifier                 = 129;  // 0b1000_0001;
const resetUpdatedInternallyIdentifier    =   7;  // 0b0000_0111;
const setClipDataOnClientSignifier        =  66;  // 0b0100_0010;
const setFormulaOnServerSignifier         = 131;  // 0b1000_0011;
const setFormulaOnClientSignifier         =   3;  // 0b0000_0011;
const setLive11ClipDataOnServerSignifier  = 194;  // 0b1100_0010;
const evaluateL11FormulasSignifier        = 196;  // 0b1100_0100;
const setAndEvaluateL11ClipDataSignifier  = 197;  // 0b1100_0101;
const setAndEvaluateL11FormulaSignifier   = 198;  // 0b1100_0110;
const setClipDataOnServerSignifier        = 130;  // 0b1000_0010;
const evaluateFormulasSignifier           = 132;  // 0b1000_0100;
const setAndEvaluateClipDataSignifier     = 133;  // 0b1000_0101;
const setAndEvaluateFormulaSignifier      = 134;  // 0b1000_0110;*/

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

/*const logMessageSignifier                 =   1;
const resetUpdatedInternallyIdentifier    =   7;
const setFormulaSignifier                 = 131;  // 0b1000_0011;
const setFormulaOnClientSignifier         =   3;  // 0b0000_0011;
const setLive11ClipDataOnServerSignifier  = 194;  // 0b1100_0010;
const evaluateL11FormulasSignifier        = 196;  // 0b1100_0100;
const setAndEvaluateL11ClipDataSignifier  = 197;  // 0b1100_0101;
const setAndEvaluateL11FormulaSignifier   = 198;  // 0b1100_0110;
const setClipDataOnServerSignifier        = 130;  // 0b1000_0010;
const evaluateFormulasSignifier           = 132;  // 0b1000_0100;
const setAndEvaluateClipDataSignifier     = 133;  // 0b1000_0101;
const setAndEvaluateFormulaSignifier      = 134;  // 0b1000_0110;*/
maxApi.post("Heisannn!");

maxApi.addHandler(MESSAGE_TYPES.LIST, async (...args) => {
    maxApi.post(`received list: ${args.join(", ")}`);
    if (args.length === 0) return;
    let commandSignifier = args[0];
    switch (commandSignifier & 0x0f) {
        case commandSignifiers.setClipData:
            await connection.invoke("SetClipData", commandSignifier & live11Flag > 0 ? true : false, args.slice(1));
            break;        
        case commandSignifiers.setFormula:
            await connection.invoke("SetFormula", args.slice(1));
            break;
        case commandSignifiers.setAndEvaluateClipData:
            await connection.invoke("SetAndEvaluateClipData", commandSignifier & live11Flag > 0 ? true : false, args.slice(1));
            break;
        case commandSignifiers.setAndEvaluateFormula:
            await connection.invoke("SetAndEvaluateFormula", commandSignifier & live11Flag > 0 ? true : false, args.slice(1));
            break;
        case commandSignifiers.evaluateFormulas:
            await connection.invoke("EvaluateFormulas", commandSignifier & live11Flag > 0 ? true : false);
            break;
        case commandSignifiers.logMessage:
            await connection.invoke("LogMessage", args.slice(1));
            break;
    }
});

let connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/mutatefulHub")
    .withHubProtocol(new signalRMsgPack.MessagePackHubProtocol())
    .build();

connection.on("FromServerMessage", msg => {
    maxApi.post(msg);
});

connection.on("FromServerByteArray", data => {
    debuglog(data);
});

connection.start()
    .then(() => {
        maxApi.post("Connection active.");
        // connection.invoke("GetMessageFromClient", "Hello server this is max4l");
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