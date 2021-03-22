using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using LocalChat.Server;
using Moq;
using NUnit.Framework;

namespace LocalChat.Tests
{
  public class ChatRoomServerTests
  {
    [Test]
    public void Test_Start_WebSocketServerStartIsCalled()
    {
      // Arrange
      var webSocketServerMock = new Mock<IWebSocketServer>();
      var chatRoomServer = new ChatRoomServer();

      // Act
      chatRoomServer.Start(webSocketServerMock.Object, null, null);

      // Assert
      webSocketServerMock.Verify(mock => mock.Start(It.IsAny<Action<IWebSocketConnection>>()), Times.Once);
    }

    [Test]
    public async Task Test_Start_OnOpenAndCloseActionsAreCalled()
    {
      // Arrange
      var roomNumber = 8181;
      using var webSocketServer = new WebSocketServer($"ws://0.0.0.0:{roomNumber}");
      var isOnOpenActionCalled = false;
      void onOpenAction() => isOnOpenActionCalled = true;
      var isOnCloseActionCalled = false;
      void onCloseAction() => isOnCloseActionCalled = true;
      var chatRoomServer = new ChatRoomServer();

      // Act
      chatRoomServer.Start(webSocketServer, onOpenAction, onCloseAction);
      var socket = new ClientWebSocket();
      await socket.ConnectAsync(new Uri($"ws://localhost:{roomNumber}/"), CancellationToken.None);
      await socket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);

      // Assert
      Assert.That(isOnOpenActionCalled, Is.EqualTo(true));
      Assert.That(isOnCloseActionCalled, Is.EqualTo(true));
    }
  }
}
