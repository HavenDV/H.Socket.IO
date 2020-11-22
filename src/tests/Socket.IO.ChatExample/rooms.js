var app = require("express")();
var server = require("http").createServer(app);
var io = require("socket.io")(server);
var port = 1465;

server.listen(port, () => {
    console.log("Server listening at port %d", port);
});

io.on("connection", (socket) => {
    console.log("someone connected");

    socket.join("room");

    socket.on("message", (data) => {
        console.log("message received");

        io.to("room").emit("message");
    });
});
