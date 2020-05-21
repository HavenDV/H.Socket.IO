var app = require("express")();
var server = require("http").createServer(app);
var io = require("socket.io")(server);
var port = 1465;

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

server.listen(port, function () {
	console.log("listening on localhost:1465");
});