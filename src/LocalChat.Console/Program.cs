using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Fleck;
using Serilog;
using Serilog.Exceptions;

namespace LocalChat.Console
{
  internal class Program
  {
    private static readonly CancellationTokenSource CancellationTokenSource =
      new();

    private static readonly ILogger Log = new LoggerConfiguration()
        .Enrich.WithExceptionDetails()
        .WriteTo.Console()
        .CreateLogger();

    private static async Task Main(string[] args)
    {
      OverrideFleckLogger();

      try
      {
        var options = GetOptions(args);
        if (options.IsFailure)
        {
          Log.Fatal(options.Error);
          return;
        }
        var chatController = new ChatController(Log, CancellationTokenSource);
        await chatController.StartAsync(options.Value, CancellationTokenSource.Token);
      }
      catch (Exception e)
      {
        Log.Fatal("Application failed: {Message}", e.Message);
        Log.Verbose(e, "Application failed with unhandled exception.");
      }
      CancellationTokenSource.Cancel();
    }

    private static Result<Options> GetOptions(string[] args)
    {
      var appConfiguration = new AppConfiguration(Log);
      var parsedArguments = appConfiguration.GetConfigurationOptions(args);
      if (parsedArguments.IsFailure)
      {
        return Result.Failure<Options>(parsedArguments.Error);
      }
      Log.Information("Configuration values:\n{@Options}", parsedArguments.Value);
      return parsedArguments;
    }

    private static void OverrideFleckLogger()
    {
      var logger = Log.ForContext<FleckLog>();
      FleckLog.LogAction = (level, message, exception) =>
      {
        switch (level)
        {
          case LogLevel.Debug:
            logger.Debug(message, exception);
            break;
          case LogLevel.Error:
            logger.Error(message, exception);
            break;
          case LogLevel.Warn:
            logger.Warning(message, exception);
            break;
          default:
            logger.Information(message, exception);
            break;
        }
      };
    }
  }
}
