using System.Text;

namespace LocalChat.Shared.Extensions
{
  public static class EncodingExtensions
  {
    /// <summary>
    /// Get Unicode string from byte array.
    /// </summary>
    public static string GetString(this byte[] byteArray) =>
      Encoding.UTF8.GetString(byteArray);

    /// <summary>
    /// Get byte array from string.
    /// </summary>
    public static byte[] GetBytes(this string text) =>
      Encoding.UTF8.GetBytes(text);
  }
}
