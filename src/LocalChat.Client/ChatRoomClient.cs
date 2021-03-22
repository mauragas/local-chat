using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using LocalChat.Shared.Extensions;

namespace LocalChat.Client
{
  public class ChatRoomClient : IChatRoomClient, IDisposable
  {
    internal readonly ClientWebSocket ClientWebSocket;

    internal ChatRoomClient(ClientWebSocket clientWebSocket) =>
      this.ClientWebSocket = clientWebSocket;

    /// <summary>
    /// Enter to chat room with specified room number.
    /// </summary>
    public static async Task<Result<IChatRoomClient>> EnterAsync(int roomNumber, ClientWebSocket socket, CancellationToken cancellationToken)
    {
      try
      {
        await socket.ConnectAsync(new Uri($"ws://localhost:{roomNumber}/"), cancellationToken);
        if (socket.State != WebSocketState.Open)
          return Result.Failure<IChatRoomClient>($"Not able to enter to chat room: {roomNumber}");
        return Result.Success<IChatRoomClient>(new ChatRoomClient(socket));
      }
      catch (WebSocketException ex)
      {
        return Result
          .Failure<IChatRoomClient>($"Chat is not started, new chat can be established: {ex.Message}");
      }
    }

    public async Task<Result> SendAsync(string message, CancellationToken cancellationToken)
    {
      if (this.ClientWebSocket.State != WebSocketState.Open)
        return Result.Failure("Chat room is not open");
      var sendBuffer = new ArraySegment<byte>(message.GetBytes());
      await this.ClientWebSocket
        .SendAsync(sendBuffer, WebSocketMessageType.Text, true, cancellationToken);
      return Result.Success();
    }

    public async Task<Result<string>> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
      var fullMessage = new StringBuilder();
      var isCompleteMessage = false;
      try
      {
        while (!isCompleteMessage)
        {
          var buffer = new byte[2048];
          var receivedBuffer = new ArraySegment<byte>(buffer);
          var result = await this.ClientWebSocket.ReceiveAsync(receivedBuffer, cancellationToken);
          _ = fullMessage.Append(receivedBuffer.ToArray().GetString().Replace("\0", string.Empty));
          isCompleteMessage = result.EndOfMessage;
        }
      }
      catch (Exception ex)
      {
        return Result.Failure<string>($"Failed to receive message: {ex.Message}");
      }
      return Result.Success(fullMessage.ToString());
    }

    public void Dispose()
    {
      this.ClientWebSocket.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
