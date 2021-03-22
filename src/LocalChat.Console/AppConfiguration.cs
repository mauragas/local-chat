using System.Linq;
using System;
using CommandLine;
using Serilog;
using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace LocalChat.Console
{
  public class AppConfiguration
  {
    private readonly ILogger log;

    public AppConfiguration(ILogger logger) => this.log = logger;

    /// <summary>
    /// Get configuration options from given command line arguments.
    /// In case of failure during argument parsing returns <see cref="Result.IsFailure"/> true.
    /// </summary>
    public Result<Options> GetConfigurationOptions(string[] commandLineArguments)
    {
      Options? parsedArguments = null;
      _ = Parser.Default.ParseArguments<Options>(commandLineArguments)
        .WithParsed(options => parsedArguments = options)
        .WithNotParsed(errors => HandleParseError(errors));
      if (parsedArguments is null)
        return Result.Failure<Options>("Failed to parse command arguments.");
      return parsedArguments;
    }

    /// <summary>
    /// In case of errors or --help or --version
    /// </summary>
    private void HandleParseError(IEnumerable<Error> errors)
    {
      if (!errors.Any(e => e is HelpRequestedError || e is VersionRequestedError))
      {
        this.log.Error("Failed to parse command line arguments {ErrorTags}", errors.Select(e => e.Tag));
        Environment.Exit(0);
      }
    }
  }
}
