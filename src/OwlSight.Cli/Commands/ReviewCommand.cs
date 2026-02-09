using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OwlSight.Cli.Output;
using OwlSight.Core.Configuration;
using OwlSight.Core.Review;

namespace OwlSight.Cli.Commands;

public sealed class ReviewCommand : Command
{
    public ReviewCommand(IServiceProvider services) : base("review", "Run AI-powered code review on changes")
    {
        var baseOption = new Option<string>("--base", "-b") { Description = "Base branch to diff against", Required = true };
        var baseUrlOption = new Option<string?>("--base-url") { Description = "LLM API base URL" };
        var apiKeyOption = new Option<string?>("--api-key") { Description = "LLM API key" };
        var modelOption = new Option<string?>("--model", "-m") { Description = "LLM model name" };
        var outputOption = new Option<string?>("--output", "-o") { Description = "JSON output file path" };
        var minSeverityOption = new Option<string?>("--min-severity") { Description = "Minimum severity to report (Critical, Warning, Info, Nitpick)" };
        var maxFilesOption = new Option<int?>("--max-files-per-batch") { Description = "Maximum files per review batch" };
        var maxRoundtripsOption = new Option<int?>("--max-tool-roundtrips") { Description = "Maximum tool-calling iterations" };
        var workingDirOption = new Option<string?>("--working-dir", "-d") { Description = "Working directory (defaults to current)" };

        Options.Add(baseOption);
        Options.Add(baseUrlOption);
        Options.Add(apiKeyOption);
        Options.Add(modelOption);
        Options.Add(outputOption);
        Options.Add(minSeverityOption);
        Options.Add(maxFilesOption);
        Options.Add(maxRoundtripsOption);
        Options.Add(workingDirOption);

        this.SetAction(async (parseResult, ct) =>
        {
            var baseBranch = parseResult.GetValue(baseOption)!;
            var baseUrl = parseResult.GetValue(baseUrlOption);
            var apiKey = parseResult.GetValue(apiKeyOption);
            var model = parseResult.GetValue(modelOption);
            var output = parseResult.GetValue(outputOption);
            var minSeverity = parseResult.GetValue(minSeverityOption);
            var maxFiles = parseResult.GetValue(maxFilesOption);
            var maxRoundtrips = parseResult.GetValue(maxRoundtripsOption);
            var workingDir = parseResult.GetValue(workingDirOption);

            var configLoader = services.GetRequiredService<ConfigLoader>();
            var engine = services.GetRequiredService<IReviewEngine>();
            var logger = services.GetRequiredService<ILogger<ReviewCommand>>();

            var config = configLoader.Load(
                baseBranch: baseBranch,
                baseUrl: baseUrl,
                apiKey: apiKey,
                model: model,
                outputPath: output,
                minSeverity: minSeverity,
                maxFilesPerBatch: maxFiles,
                maxToolRoundtrips: maxRoundtrips,
                workingDir: workingDir);

            if (string.IsNullOrEmpty(config.Llm.ApiKey))
            {
                logger.LogError("API key is required. Use --api-key or set OWLSIGHT_API_KEY environment variable.");
                Console.Error.WriteLine("Error: API key is required. Use --api-key or set OWLSIGHT_API_KEY environment variable.");
                return 2;
            }

            try
            {
                var result = await engine.RunAsync(config, ct);

                var consoleWriter = new ConsoleOutputWriter();
                await consoleWriter.WriteAsync(result, ct);

                if (!string.IsNullOrEmpty(config.OutputPath))
                {
                    var jsonWriter = new JsonOutputWriter(config.OutputPath);
                    await jsonWriter.WriteAsync(result, ct);
                    logger.LogInformation("Report written to {Path}", config.OutputPath);
                }

                return result.HasCriticalFindings ? 1 : 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Review failed");
                return 2;
            }
        });
    }
}
