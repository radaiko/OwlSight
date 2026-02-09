using Microsoft.Extensions.AI;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using OwlSight.Core.Configuration;
using OwlSight.Core.Git;
using OwlSight.Core.Llm;
using OwlSight.Core.Rules;
using OwlSight.Core.Tools;

namespace OwlSight.Core.Review;

public sealed class ReviewEngine : IReviewEngine
{
    private readonly IGitService _gitService;
    private readonly IRulesProvider _rulesProvider;
    private readonly ToolRegistry _toolRegistry;
    private readonly LlmClientFactory _llmClientFactory;
    private readonly PromptBuilder _promptBuilder;
    private readonly AgenticLoop _agenticLoop;
    private readonly ILogger<ReviewEngine> _logger;

    public ReviewEngine(
        IGitService gitService,
        IRulesProvider rulesProvider,
        ToolRegistry toolRegistry,
        LlmClientFactory llmClientFactory,
        PromptBuilder promptBuilder,
        AgenticLoop agenticLoop,
        ILogger<ReviewEngine> logger)
    {
        _gitService = gitService;
        _rulesProvider = rulesProvider;
        _toolRegistry = toolRegistry;
        _llmClientFactory = llmClientFactory;
        _promptBuilder = promptBuilder;
        _agenticLoop = agenticLoop;
        _logger = logger;
    }

    public async Task<ReviewResult> RunAsync(OwlSightConfig config, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting review against base branch '{BaseBranch}'", config.BaseBranch);

        // Load rules
        var rules = await _rulesProvider.LoadRulesAsync(config.WorkingDirectory, ct);
        _logger.LogInformation("Loaded {Count} review rules", rules.Count);

        // Get diff
        var diff = await _gitService.GetDiffAsync(config.BaseBranch, config.WorkingDirectory, ct);
        _logger.LogInformation("Found {Count} changed files", diff.Files.Count);

        if (diff.Files.Count == 0)
        {
            _logger.LogInformation("No changed files found");
            return CreateEmptyResult(0);
        }

        // Filter files
        var filteredFiles = FilterFiles(diff.Files, config.Review.ExcludePatterns);
        _logger.LogInformation("{Count} files after filtering", filteredFiles.Count);

        if (filteredFiles.Count == 0)
        {
            _logger.LogInformation("All changed files were excluded by patterns");
            return CreateEmptyResult(0);
        }

        // Batch files
        var batches = BatchFiles(filteredFiles, config.Review.MaxFilesPerBatch);
        _logger.LogInformation("Processing {BatchCount} batches", batches.Count);

        // Create LLM client and tools
        using var chatClient = _llmClientFactory.Create(config.Llm);
        var tools = _toolRegistry.CreateTools(config.WorkingDirectory);

        var chatOptions = new ChatOptions
        {
            Tools = tools,
            Temperature = config.Llm.Temperature,
            MaxOutputTokens = config.Llm.MaxTokens,
            ModelId = config.Llm.Model
        };

        // Process batches
        var allFindings = new List<ReviewFinding>();
        for (var i = 0; i < batches.Count; i++)
        {
            _logger.LogInformation("Processing batch {Current}/{Total}", i + 1, batches.Count);
            var messages = _promptBuilder.Build(rules, batches[i]);
            var findings = await _agenticLoop.RunAsync(
                chatClient, messages, tools, config.Llm.MaxToolRoundtrips, chatOptions, ct);
            allFindings.AddRange(findings);
        }

        // Filter by minimum severity
        if (Enum.TryParse<ReviewSeverity>(config.Review.MinSeverity, true, out var minSeverity))
        {
            allFindings = allFindings.Where(f => f.Severity >= minSeverity).ToList();
        }

        return new ReviewResult
        {
            Summary = BuildSummary(allFindings, filteredFiles.Count, batches.Count),
            Findings = allFindings
        };
    }

    private static List<ChangedFile> FilterFiles(List<ChangedFile> files, List<string> excludePatterns)
    {
        if (excludePatterns.Count == 0)
            return files;

        var matcher = new Matcher();
        foreach (var pattern in excludePatterns)
            matcher.AddInclude(pattern);

        return files.Where(f => !matcher.Match(f.Path).HasMatches).ToList();
    }

    private static List<List<ChangedFile>> BatchFiles(List<ChangedFile> files, int batchSize)
    {
        var batches = new List<List<ChangedFile>>();
        for (var i = 0; i < files.Count; i += batchSize)
        {
            batches.Add(files.GetRange(i, Math.Min(batchSize, files.Count - i)));
        }
        return batches;
    }

    private static ReviewSummary BuildSummary(List<ReviewFinding> findings, int filesCount, int batchCount)
    {
        var bySeverity = findings
            .GroupBy(f => f.Severity)
            .ToDictionary(g => g.Key, g => g.Count());

        return new ReviewSummary
        {
            TotalFindings = findings.Count,
            BySeverity = bySeverity,
            ReviewedFilesCount = filesCount,
            BatchCount = batchCount
        };
    }

    private static ReviewResult CreateEmptyResult(int filesCount) => new()
    {
        Summary = new ReviewSummary
        {
            TotalFindings = 0,
            ReviewedFilesCount = filesCount,
            BatchCount = 0
        },
        Findings = []
    };
}
