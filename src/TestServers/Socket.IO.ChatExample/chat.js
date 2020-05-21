var app = require("express")();
var server = require("http").createServer(app);
var io = require("socket.io")(server);
var port = 1465;

server.listen(port, () => {
    console.log("Server listening at port %d", port);
});

var numUsers = 0;

io.on("connection", (socket) => {
    var addedUser = false;

    socket.on("new message", (data) => {
        socket.broadcast.emit("new message", {
            username: socket.username,
            message: data
        });
    });

    socket.on("add user", (username) => {
        if (addedUser) return;

        socket.username = username;
        ++numUsers;
        addedUser = true;
        socket.emit("login", {
            numUsers: numUsers
        });
        socket.broadcast.emit("user joined", {
            username: socket.username,
            numUsers: numUsers
        });
    });

    socket.on("typing", () => {
        socket.broadcast.emit("typing", {
            username: socket.username
        });
    });

    socket.on("stop typing", () => {
        socket.broadcast.emit("stop typing", {
            username: socket.username
        });
    });

    socket.on("disconnect", () => {
        if (addedUser) {
            --numUsers;

            socket.broadcast.emit("user left", {
                username: socket.username,
                numUsers: numUsers
            });
        }
    });
});
