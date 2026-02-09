namespace OwlSight.Core.Git;

public sealed class DiffHunk
{
    public required int OldStart { get; init; }
    public required int OldCount { get; init; }
    public required int NewStart { get; init; }
    public required int NewCount { get; init; }
    public required string Header { get; init; }
    public required string Content { get; init; }
}
