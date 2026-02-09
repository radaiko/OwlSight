using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OwlSight.Cli.Commands;
using OwlSight.Core;

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
services.AddOwlSightCore();
var serviceProvider = services.BuildServiceProvider();

var rootCommand = new RootCommand("OwlSight — AI-powered code review tool");
rootCommand.Subcommands.Add(new ReviewCommand(serviceProvider));
rootCommand.Subcommands.Add(new InitCommand());

return await rootCommand.Parse(args).InvokeAsync();
