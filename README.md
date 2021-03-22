# Local chat application

Simple `localhost` chat console application using [Fleck](https://github.com/statianzo/Fleck) library to initialize single chat room server.

Console application have two modes:

- `Chat server and client application` - if there is no already existing open chat room, application initialize new one with specified room (port) number and joins the chat itself.
- `Client application` - subscribes to chat room if these is already open chat room (port) already opened by other application.

There is one input parameter:

- `--Port` - chat room (port) number, used to open WEB socket for `localhost` computer.

**NOTE:** words `port number` and `room number` are used interchangeably and means the same thing.

## Examples

Navigate to project `LocalChat.Console` folder and execute command:

- `dotnet run --Port=8181` - join to existing chat room with number 8181 or initialize new one.

## Current project structure

Solution consist of 5 projects:

- `LocalChat.Client` - chat room client class library used to connect to existing chat room.
- `LocalChat.Console` - console user interface.
- `LocalChat.Server` - chat server class library.
- `LocalChat.Shared` - class library shared between projects.
- `LocalChat.Tests` - various integrations / unit tests.

## Limitations and missing features

- Solution requires at least instance to be as a server application while other instances can be only clients.
- There is no data and log persistance.
- Clients do not have names therefore is not possible to track in console UI from who message is received.

## Known issues

- In case server application is restarted client applications are not able automatically rejoin the chat again.
