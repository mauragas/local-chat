using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace LocalChat.Client
{
  public interface IChatRoomClient
  {
    /// <summary>
    /// Send message to all members in the chat room.
    /// </summary>
    Task<Result> SendAsync(string message, CancellationToken cancellationToken);

    /// <summary>
    /// Wait for message from other chat members.
    /// </summary>
    Task<Result<string>> ReceiveMessageAsync(CancellationToken cancellationToken);
  }
}
