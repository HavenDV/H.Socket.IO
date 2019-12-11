## [SimpleSocketIoClient](https://github.com/HavenDV/SimpleSocketIoClient/) 

[![NuGet](https://img.shields.io/nuget/v/SimpleSocketIoClient.svg?style=flat-square)](https://www.nuget.org/packages/SimpleSocketIoClient/)
[![Language](https://img.shields.io/badge/language-C%23-blue.svg?style=flat-square)](https://github.com/HavenDV/SimpleSocketIoClient/search?l=C%23&o=desc&s=&type=Code) 
[![License](https://img.shields.io/github/license/HavenDV/SimpleSocketIoClient.svg?label=License&maxAge=86400)](LICENSE.md) 
[![Requirements](https://img.shields.io/badge/Requirements-.NET%20Standard%202.0-blue.svg)](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md)
[![Build Status](https://github.com/HavenDV/SimpleSocketIoClient/workflows/.NET%20Core/badge.svg?branch=master)](https://github.com/HavenDV/SimpleSocketIoClient/actions?query=workflow%3A%22.NET+Core%22)

This is the Socket.IO client for .NET, which is base on `ClientWebSocket`, provide a simple way to connect to the Socket.IO server. The target framework is **.NET Standard 2.0**

### Nuget

```
Install-Package SimpleSocketIoClient
```

### Usage

```cs
using System;
using System.Threading.Tasks;
using SimpleSocketIoClient;

public class ChatMessage
{
    public string Username { get; set; }
    public string Message { get; set; }
    public long NumUsers { get; set; }
}

public async Task ConnectToChatNowShTest()
{
#if NETCOREAPP3_0 || NETCOREAPP3_1
    await using var client = new SocketIoClient();
#else
    using var client = new SocketIoClient();
#endif

    client.Connected += (sender, args) => Console.WriteLine("Connected");
    client.Disconnected += (sender, args) => Console.WriteLine($"Disconnected. Reason: {args.Reason}, Status: {args.Status:G}");
    client.AfterEvent += (sender, args) => Console.WriteLine($"AfterEvent: {args.Value}");
    client.AfterUnhandledEvent += (sender, args) => Console.WriteLine($"AfterUnhandledEvent: {args.Value}");
    client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");

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
	
    await client.ConnectAsync(new Uri("wss://socket-io-chat.now.sh/"));

    await client.Emit("add user", "C# SimpleSocketIoClient Test User");

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

### Live Example

.NET Fiddle - https://dotnetfiddle.net/YUPt3x <br/>
Http client of the tested Socket.IO server - https://socket-io-chat.now.sh/

### Used documentation

Socket.IO Protocol - https://github.com/socketio/socket.io-protocol <br/>
Engine.IO Protocol - https://github.com/socketio/engine.io-protocol

### Contacts
* [mail](mailto:havendv@gmail.com)