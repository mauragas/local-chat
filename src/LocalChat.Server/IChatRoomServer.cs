using System;
using Fleck;

namespace LocalChat.Server
{
  public interface IChatRoomServer
  {
    void Start(IWebSocketServer webSocketServer, Action onOpen, Action onClose);
  }
}
