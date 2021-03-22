using System;
using System.Collections.Generic;
using System.Linq;
using Fleck;

namespace LocalChat.Server
{
  public class ChatRoomServer : IChatRoomServer
  {
    public void Start(IWebSocketServer webSocketServer, Action onOpen, Action onClose)
    {
      var allSockets = new List<IWebSocketConnection>();
      webSocketServer.Start(socket =>
      {
        socket.OnOpen = () =>
        {
          onOpen.Invoke();
          allSockets.Add(socket);
        };
        socket.OnClose = () =>
        {
          onClose.Invoke();
          _ = allSockets.Remove(socket);
        };
        socket.OnMessage = message => allSockets.ToList().ForEach(s => s.Send(message));
      });
    }
  }
}
