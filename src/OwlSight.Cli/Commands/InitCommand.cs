using System.CommandLine;
using Spectre.Console;

namespace OwlSight.Cli.Commands;

public sealed class InitCommand : Command
{
    public InitCommand() : base("init", "Initialize OwlSight configuration in the current directory")
    {
        var workingDirOption = new Option<string?>("--working-dir", "-d") { Description = "Working directory (defaults to current)" };
        Options.Add(workingDirOption);

        this.SetAction(async (parseResult, ct) =>
        {
            var workingDir = parseResult.GetValue(workingDirOption) ?? Directory.GetCurrentDirectory();

            var owlsightDir = Path.Combine(workingDir, ".owlsight");
            var rulesDir = Path.Combine(owlsightDir, "rules");

            if (Directory.Exists(owlsightDir))
            {
                AnsiConsole.MarkupLine("[yellow]Warning:[/] .owlsight directory already exists.");
            }

            Directory.CreateDirectory(rulesDir);

            var configPath = Path.Combine(owlsightDir, "config.json");
            if (!File.Exists(configPath))
            {
                await File.WriteAllTextAsync(configPath, """
                    {
                      "llm": {
                        "baseUrl": "https://api.openai.com/v1",
                        "model": "gpt-4o",
                        "maxTokens": 4096,
                        "temperature": 0.2,
                        "maxToolRoundtrips": 15
                      },
                      "review": {
                        "minSeverity": "Info",
                        "excludePatterns": [
                          "**/bin/**",
                          "**/obj/**",
                          "**/*.generated.cs",
                          "**/node_modules/**"
                        ],
                        "maxFilesPerBatch": 10
                      }
                    }
                    """, ct);
            }

            var exampleRulePath = Path.Combine(rulesDir, "no-console-writeline.md");
            if (!File.Exists(exampleRulePath))
            {
                await File.WriteAllTextAsync(exampleRulePath, """
                    ---
                    title: No Console.WriteLine in Production Code
                    severity: Warning
                    category: Code Quality
                    description: Console.WriteLine should not be used in production code; use proper logging instead.
                    ---
                    # No Console.WriteLine in Production Code

                    Production code should use structured logging (e.g., `ILogger`) instead of `Console.WriteLine`.

                    ## Why
                    - `Console.WriteLine` output cannot be filtered, structured, or routed to different sinks.
                    - It is not thread-safe in all scenarios.
                    - It bypasses the application's logging configuration.

                    ## Exceptions
                    - CLI tools where console output is the intended interface.
                    - Test projects.
                    """, ct);
            }

            AnsiConsole.MarkupLine("[green bold]Initialized .owlsight/ directory[/]");
            AnsiConsole.MarkupLine($"  [dim]Config:[/]  {Path.GetRelativePath(workingDir, configPath)}");
            AnsiConsole.MarkupLine($"  [dim]Rules:[/]   {Path.GetRelativePath(workingDir, rulesDir)}/");
            AnsiConsole.MarkupLine("\nEdit [bold].owlsight/config.json[/] to customize your settings.");
            AnsiConsole.MarkupLine("Add review rules as markdown files in [bold].owlsight/rules/[/].");

            return 0;
        });
    }
}
