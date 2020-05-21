var app = require("express")();
var server = require("http").createServer(app);
var io = require("socket.io")(server);
var port = 1465;

server.listen(port, () => {
    console.log("Server listening at port %d", port);
});

var numUsers = 0;

io.on("connection", (socket) => {
    console.log("someone connected");

    var addedUser = false;

    socket.join("some room");

    socket.on("new message", (data) => {
        io.to("some room").emit("new message", {
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
        io.to("some room").emit("user joined", {
            username: socket.username,
            numUsers: numUsers
        });
    });

    socket.on("typing", () => {
        io.to("some room").emit("typing", {
            username: socket.username
        });
    });

    socket.on("stop typing", () => {
        io.to("some room").emit("stop typing", {
            username: socket.username
        });
    });

    socket.on("disconnect", () => {
        if (addedUser) {
            --numUsers;

            io.to("some room").emit("user left", {
                username: socket.username,
                numUsers: numUsers
            });
        }
    });
});
