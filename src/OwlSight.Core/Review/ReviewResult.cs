namespace OwlSight.Core.Review;

public sealed class ReviewResult
{
    public string Version { get; init; } = "1.0.0";
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public required ReviewSummary Summary { get; init; }
    public required List<ReviewFinding> Findings { get; init; }
    public bool HasCriticalFindings => Findings.Any(f => f.Severity == ReviewSeverity.Critical);
}
