namespace OwlSight.Core.Review;

public sealed class ReviewFinding
{
    public required string File { get; init; }
    public int? Line { get; init; }
    public int? EndLine { get; init; }
    public required ReviewSeverity Severity { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public string? Suggestion { get; init; }
    public string? RuleId { get; init; }
}
