namespace OwlSight.Core.Rules;

public sealed class ReviewRule
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public string Severity { get; init; } = "Warning";
    public string Category { get; init; } = "General";
    public required string Content { get; init; }
}
