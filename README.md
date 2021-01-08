# [H.Socket.IO](https://github.com/HavenDV/H.Socket.IO/) 

[![Language](https://img.shields.io/badge/language-C%23-blue.svg?style=flat-square)](https://github.com/HavenDV/H.Socket.IO/search?l=C%23&o=desc&s=&type=Code) 
[![License](https://img.shields.io/github/license/HavenDV/H.Socket.IO.svg?label=License&maxAge=86400)](LICENSE.md) 
[![Requirements](https://img.shields.io/badge/Requirements-.NET%20Standard%202.0-blue.svg)](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md)
[![Requirements](https://img.shields.io/badge/Requirements-.NET%20Framework%204.5-blue.svg)](https://github.com/microsoft/dotnet/blob/master/releases/net45/README.md)
[![Build Status](https://github.com/HavenDV/H.Socket.IO/workflows/.NET%20Core/badge.svg?branch=master)](https://github.com/HavenDV/H.Socket.IO/actions?query=workflow%3A%22.NET+Core%22)

High-performance event-based .NET Socket.IO library with a convenient interface,  
aimed at writing the smallest possible code on the user side.
  
Features:
- Supports the latest version of Socket.IO server
- Supports namespaces
- The library is null-free and does not contain NRE
- Event-based
- Completely asynchronous

### Nuget

[![NuGet](https://img.shields.io/nuget/dt/H.Socket.IO.svg?style=flat-square&label=H.Socket.IO)](https://www.nuget.org/packages/H.Socket.IO/)
[![NuGet](https://img.shields.io/nuget/dt/H.Engine.IO.svg?style=flat-square&label=H.Engine.IO)](https://www.nuget.org/packages/H.Engine.IO/)
[![NuGet](https://img.shields.io/nuget/dt/H.WebSockets.svg?style=flat-square&label=H.WebSockets)](https://www.nuget.org/packages/H.WebSockets/)

```
Install-Package H.Socket.IO
```

### Usage

```cs
using System;
using System.Threading.Tasks;
using H.Socket.IO;

#nullable enable

public class ChatMessage
{
    public string? Username { get; set; }
    public string? Message { get; set; }
    public long NumUsers { get; set; }
}
	
public async Task ConnectToChatNowShTest()
{
    await using var client = new SocketIoClient();

    client.Connected += (sender, args) => Console.WriteLine($"Connected: {args.Namespace}");
    client.Disconnected += (sender, args) => Console.WriteLine($"Disconnected. Reason: {args.Reason}, Status: {args.Status:G}");
    client.EventReceived += (sender, args) => Console.WriteLine($"EventReceived: Namespace: {args.Namespace}, Value: {args.Value}, IsHandled: {args.IsHandled}");
    client.HandledEventReceived += (sender, args) => Console.WriteLine($"HandledEventReceived: Namespace: {args.Namespace}, Value: {args.Value}");
    client.UnhandledEventReceived += (sender, args) => Console.WriteLine($"UnhandledEventReceived: Namespace: {args.Namespace}, Value: {args.Value}");
    client.ErrorReceived += (sender, args) => Console.WriteLine($"ErrorReceived: Namespace: {args.Namespace}, Value: {args.Value}");
    client.ExceptionOccurred += (sender, args) => Console.WriteLine($"ExceptionOccurred: {args.Value}");
    
    client.On("login", () =>
    {
        Console.WriteLine("You are logged in.");
    });
    client.On("login", json =>
    {
        Console.WriteLine($"You are logged in. Json: \"{json}\"");
    });
    client.On<ChatMessage>("login", message =>
    {
        Console.WriteLine($"You are logged in. Total number of users: {message.NumUsers}");
    });
    client.On<ChatMessage>("user joined", message =>
    {
        Console.WriteLine($"User joined: {message.Username}. Total number of users: {message.NumUsers}");
    });
    client.On<ChatMessage>("user left", message =>
    {
        Console.WriteLine($"User left: {message.Username}. Total number of users: {message.NumUsers}");
    });
    client.On<ChatMessage>("typing", message =>
    {
        Console.WriteLine($"User typing: {message.Username}");
    });
    client.On<ChatMessage>("stop typing", message =>
    {
        Console.WriteLine($"User stop typing: {message.Username}");
    });
    client.On<ChatMessage>("new message", message =>
    {
        Console.WriteLine($"New message from user \"{message.Username}\": {message.Message}");
    });
	
    await client.ConnectAsync(new Uri("wss://socketio-chat-h9jt.herokuapp.com/"));

    await client.Emit("add user", "C# H.Socket.IO Test User");

    await Task.Delay(TimeSpan.FromMilliseconds(200));

    await client.Emit("typing");

    await Task.Delay(TimeSpan.FromMilliseconds(200));

    await client.Emit("new message", "hello");

    await Task.Delay(TimeSpan.FromMilliseconds(200));

    await client.Emit("stop typing");

    await Task.Delay(TimeSpan.FromSeconds(2));

    await client.DisconnectAsync();
}
```

### Namespaces

```cs
// Will be sent with all messages(Unless otherwise stated).
// Also automatically connects to it.
client.DefaultNamespace = "my";

// or

// Connects to "my" namespace.
await client.ConnectAsync(new Uri(LocalCharServerUrl), namespaces: "my");
// Sends message to "my" namespace.
await client.Emit("message", "hello", "my");

```

### Custom arguments

```cs
await client.ConnectAsync(new Uri($"wss://socketio-chat-h9jt.herokuapp.com/?access_token={mAccessToken}"));
```

### Live Example

C# .NET Fiddle - https://dotnetfiddle.net/FWMpQ3 <br/>
VB.NET .NET Fiddle - https://dotnetfiddle.net/WzIdnG <br/>
Http client of the tested Socket.IO server - https://socket-io-chat.now.sh/

### Used documentation

Socket.IO Protocol - https://github.com/socketio/socket.io-protocol <br/>
Engine.IO Protocol - https://github.com/socketio/engine.io-protocol <br/>

Python implementation of Socket.IO - https://github.com/miguelgrinberg/python-socketio/blob/master/socketio/ <br/>
Python implementation of Engine.IO - https://github.com/miguelgrinberg/python-engineio/blob/master/engineio/ <br/>

### Contacts
* [mail](mailto:havendv@gmail.com)
