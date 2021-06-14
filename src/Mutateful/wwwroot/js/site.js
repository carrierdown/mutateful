const connection = new signalR
    .HubConnectionBuilder()
    .withUrl("/mutatefulHub")
    .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
    .build();

connection.start().then(function (msg) {
    console.log("Connection active");
    connection.invoke("GetMessageFromClient", "Hello server");
});

connection.on("FromServerMessage", function (message) {
    console.log("received: ", message);
});

connection.on("FromServerByteArray", data => {
    console.log(data);
});