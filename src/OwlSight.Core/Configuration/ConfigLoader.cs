using System.Text.Json;

namespace OwlSight.Core.Configuration;

/// <summary>
/// Loads configuration with priority: CLI args > env vars > .owlsight/config.json > defaults.
/// </summary>
public sealed class ConfigLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public OwlSightConfig Load(
        string? baseBranch = null,
        string? baseUrl = null,
        string? apiKey = null,
        string? model = null,
        string? outputPath = null,
        string? minSeverity = null,
        int? maxFilesPerBatch = null,
        int? maxToolRoundtrips = null,
        string? workingDir = null)
    {
        var effectiveWorkingDir = workingDir ?? Directory.GetCurrentDirectory();

        // Start with defaults
        var config = new OwlSightConfig { WorkingDirectory = effectiveWorkingDir };

        // Layer: .owlsight/config.json
        var configFilePath = Path.Combine(effectiveWorkingDir, ".owlsight", "config.json");
        if (File.Exists(configFilePath))
        {
            var json = File.ReadAllText(configFilePath);
            var fileConfig = JsonSerializer.Deserialize<OwlSightConfig>(json, JsonOptions);
            if (fileConfig is not null)
            {
                config = fileConfig;
                config.WorkingDirectory = effectiveWorkingDir;
            }
        }

        // Layer: environment variables
        ApplyEnvironmentVariables(config);

        // Layer: CLI args (highest priority)
        if (baseBranch is not null) config.BaseBranch = baseBranch;
        if (baseUrl is not null) config.Llm.BaseUrl = baseUrl;
        if (apiKey is not null) config.Llm.ApiKey = apiKey;
        if (model is not null) config.Llm.Model = model;
        if (outputPath is not null) config.OutputPath = outputPath;
        if (minSeverity is not null) config.Review.MinSeverity = minSeverity;
        if (maxFilesPerBatch.HasValue) config.Review.MaxFilesPerBatch = maxFilesPerBatch.Value;
        if (maxToolRoundtrips.HasValue) config.Llm.MaxToolRoundtrips = maxToolRoundtrips.Value;

        return config;
    }

    private static void ApplyEnvironmentVariables(OwlSightConfig config)
    {
        var apiKey = Environment.GetEnvironmentVariable("OWLSIGHT_API_KEY");
        if (!string.IsNullOrEmpty(apiKey)) config.Llm.ApiKey = apiKey;

        var baseUrl = Environment.GetEnvironmentVariable("OWLSIGHT_BASE_URL");
        if (!string.IsNullOrEmpty(baseUrl)) config.Llm.BaseUrl = baseUrl;

        var model = Environment.GetEnvironmentVariable("OWLSIGHT_MODEL");
        if (!string.IsNullOrEmpty(model)) config.Llm.Model = model;
    }
}
