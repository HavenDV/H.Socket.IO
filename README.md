# [SimpleSocketIoClient](https://github.com/HavenDV/SimpleSocketIoClient/) [![Language](https://img.shields.io/badge/language-C%23-blue.svg?style=flat-square)](https://github.com/HavenDV/SimpleSocketIoClient/search?l=C%23&o=desc&s=&type=Code) [![License](https://img.shields.io/github/license/HavenDV/SimpleSocketIoClient.svg?label=License&maxAge=86400)](LICENSE.md) [![Requirements](https://img.shields.io/badge/Requirements-.NET%20Standard%202.1-blue.svg)](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md)

This is the Socket.IO client for .NET, which is base on `ClientWebSocket`, provide a simple way to connect to the Socket.IO server. The target framework is **.NET Standard 2.1**

## Nuget

```
Install-Package SimpleSocketIoClient
```

## Usage

```cs
await using var client = new SocketIoClient();

client.AfterEvent += (sender, args) => Console.WriteLine($"AfterEvent: {args.Value}");
client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");

await client.ConnectAsync(new Uri("https://socket-io-chat.now.sh/"));

await Task.Delay(TimeSpan.FromSeconds(5));

await client.DisconnectAsync();
```

# Branches

|   master(stable)  |
|-------------------|
| Github Actions CI |  
|-------------------|
| [![Build Status](https://github.com/HavenDV/SimpleSocketIoClient/workflows/.NET%20Core/badge.svg?branch=master)](https://github.com/HavenDV/SimpleSocketIoClient/actions?query=workflow%3A%22.NET+Core%22) |

# Contacts
* [mail](mailto:havendv@gmail.com)