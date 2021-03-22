using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Fleck;
using LocalChat.Client;
using LocalChat.Server;
using Serilog;

namespace LocalChat.Console
{
  public class ChatController
  {
    private const string ChatIpAddress = "0.0.0.0";
    private const string ExitCommand = "exit";
    private readonly ILogger log;
    private readonly CancellationTokenSource cancellationTokenSource;

    public ChatController(ILogger logger, CancellationTokenSource cancellationTokenSource)
    {
      this.log = logger;
      this.cancellationTokenSource = cancellationTokenSource;
    }

    public async Task StartAsync(Options options, CancellationToken cancellationToken)
    {
      if (options.Port is null)
      {
        this.log.Fatal($"Specified port is null");
        Environment.Exit(0);
      }
      var roomNumber = (int)options.Port;

      var chatRoomResult = await ConnectToChatRoom(roomNumber, cancellationToken);
      if (chatRoomResult.IsFailure)
      {
        using var webSocketServer = new WebSocketServer($"ws://{ChatIpAddress}:{roomNumber}");
        StartChatServer(webSocketServer);
        chatRoomResult = await ConnectToChatRoom(roomNumber, cancellationToken);
        if (chatRoomResult.IsFailure)
          this.log.Fatal(chatRoomResult.Error);
      }
    }

    private static void StartChatServer(IWebSocketServer webSocketServer)
    {
      static void onOpen() => System.Console.WriteLine("New chat member joined the chat!");
      static void onClose() => System.Console.WriteLine("Chat member left the chat!");
      new ChatRoomServer().Start(webSocketServer, onOpen, onClose);
    }

    private async Task<Result> ConnectToChatRoom(int roomNumber, CancellationToken cancellationToken)
    {
      using var socket = new ClientWebSocket();
      var chatRoomResult = await ChatRoomClient.EnterAsync(roomNumber, socket, cancellationToken);
      if (chatRoomResult.IsFailure)
      {
        this.log.Debug(chatRoomResult.Error);
        return chatRoomResult;
      }
      else
      {
        this.log.Information($"Connected to chat room: {roomNumber}");
      }

      using (var chatRoom = (ChatRoomClient)chatRoomResult.Value)
      {
        var receiveMessagesTask = ReceiveMessagesAsync(chatRoom, cancellationToken);
        var sendMessagesTask = SendMessagesAsync(chatRoom, cancellationToken);
        await Task.WhenAll(receiveMessagesTask, sendMessagesTask);
      }
      return chatRoomResult;
    }

    private static async Task ReceiveMessagesAsync(IChatRoomClient chatClient, CancellationToken cancellationToken) =>
      await Task.Run(async () =>
      {
        while (!cancellationToken.IsCancellationRequested)
        {
          var receivedMessage = await chatClient.ReceiveMessageAsync(cancellationToken);
          if (receivedMessage.IsSuccess)
            System.Console.WriteLine($"Received message: {receivedMessage.Value}");
        };
      }, cancellationToken);

    private async Task SendMessagesAsync(IChatRoomClient chatRoom, CancellationToken cancellationToken)
    {
      var message = System.Console.ReadLine();
      while (message != ExitCommand && !cancellationToken.IsCancellationRequested)
      {
        var result = await chatRoom.SendAsync(message, cancellationToken);
        if (result.IsFailure)
          this.log.Error($"Failed to send message. {result.Error}");
        message = System.Console.ReadLine();
      }
      // Cancel all tasks if exit command is provided
      this.cancellationTokenSource.Cancel();
    }
  }
}
