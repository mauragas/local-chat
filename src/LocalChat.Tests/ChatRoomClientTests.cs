using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using LocalChat.Client;
using LocalChat.Server;
using NUnit.Framework;

namespace LocalChat.Tests
{
  public class ChatRoomClientTests
  {
    [Test]
    public async Task Test_EnterAsync_FailedToEnterToRoom()
    {
      // Arrange
      var roomNumber = 8181;
      using var socket = new ClientWebSocket();

      // Act
      var chatRoomClient = await ChatRoomClient.EnterAsync(roomNumber, socket, CancellationToken.None);

      // Assert
      // Fails to enter because there is no open chat room
      Assert.That(chatRoomClient.IsFailure, Is.EqualTo(true));
    }

    [Test]
    public async Task Test_EnterAsync_EnteredToChatRoom()
    {
      // Arrange
      var roomNumber = 8181;
      using var webSocketServer = new WebSocketServer($"ws://0.0.0.0:{roomNumber}");
      using var socket = new ClientWebSocket();
      var chatRoomServer = new ChatRoomServer();
      static void doNothing()
      { }
      chatRoomServer.Start(webSocketServer, doNothing, doNothing);

      // Act
      var chatRoomClient = await ChatRoomClient.EnterAsync(roomNumber, socket, CancellationToken.None);

      // Assert
      Assert.That(chatRoomClient.IsSuccess, Is.EqualTo(true));
    }

    [Test]
    public async Task Test_SendAndReceive_BigMessageIsProcessed()
    {
      // Arrange
      var messageToSend = GetRandomMessage(10000);
      var roomNumber = 8181;
      using var webSocketServer = new WebSocketServer($"ws://0.0.0.0:{roomNumber}");
      using var socket = new ClientWebSocket();
      var chatRoomServer = new ChatRoomServer();
      static void doNothing()
      { }
      chatRoomServer.Start(webSocketServer, doNothing, doNothing);

      // Act
      var chatRoomClient = await ChatRoomClient.EnterAsync(roomNumber, socket, CancellationToken.None);
      var sendResult = await chatRoomClient.Value.SendAsync(messageToSend, CancellationToken.None);
      var receivedMessage = await chatRoomClient.Value.ReceiveMessageAsync(CancellationToken.None);

      // Assert
      Assert.That(sendResult.IsSuccess, Is.EqualTo(true));
      Assert.That(receivedMessage.Value.Length, Is.EqualTo(messageToSend.Length));
      Assert.That(receivedMessage.Value, Is.EqualTo(messageToSend));
    }

    private static string GetRandomMessage(int length)
    {
      var random = new Random();
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }
  }
}
