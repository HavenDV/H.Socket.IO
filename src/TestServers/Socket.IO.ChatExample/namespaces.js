var app = require("express")();
var server = require("http").createServer(app);
var io = require("socket.io")(server);
var port = 1465;

server.listen(port, () => {
	console.log("Server listening at port %d", port);
});

app.get("/", function (req, res) {
	res.sendfile("index.html");
});

var nsp = io.of("/my");
nsp.on("connection", function (socket) {
	console.log("someone connected");
	//nsp.emit("hi", "Hello everyone!");

	socket.on("message", (data) => {
		console.log(`new message: ${data}`);
	    socket.emit("message", {
		    message: data
	    });
    });
});