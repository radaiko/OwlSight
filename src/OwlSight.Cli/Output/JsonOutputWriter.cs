using System.Text.Json;
using OwlSight.Core.Review;

namespace OwlSight.Cli.Output;

public sealed class JsonOutputWriter : IReviewOutputWriter
{
    private readonly string _outputPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonOutputWriter(string outputPath) => _outputPath = outputPath;

    public async Task WriteAsync(ReviewResult result, CancellationToken ct = default)
    {
        var dir = Path.GetDirectoryName(_outputPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        await using var stream = File.Create(_outputPath);
        await JsonSerializer.SerializeAsync(stream, result, JsonOptions, ct);
    }
}
