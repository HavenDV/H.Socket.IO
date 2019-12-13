var app = require("express")();
var http = require("http").Server(app);
var io = require("socket.io")(http);

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

http.listen(1465, function () {
	console.log("listening on localhost:1465");
});