using CommandLine;

namespace LocalChat.Console
{
  public class Options
  {
    [Option('p', nameof(Port), HelpText = "Chat room port number", Required = true)]
    public int? Port { get; set; }
  }
}
