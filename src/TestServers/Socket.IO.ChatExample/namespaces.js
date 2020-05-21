var app = require("express")();
var server = require("http").createServer(app);
var io = require("socket.io")(server);
var port = 1465;

server.listen(port, () => {
	console.log("Server listening at port %d", port);
});

var nsp = io.of("/my");
nsp.on("connection", function (socket) {
	console.log("someone connected");

	socket.on("message", (data) => {
		console.log(`new message: ${data}`);
	    socket.emit("message", {
		    message: data
	    });
    });
});