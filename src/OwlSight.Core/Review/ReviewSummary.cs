namespace OwlSight.Core.Review;

public sealed class ReviewSummary
{
    public int TotalFindings { get; init; }
    public Dictionary<ReviewSeverity, int> BySeverity { get; init; } = new();
    public int ReviewedFilesCount { get; init; }
    public int BatchCount { get; init; }
}
