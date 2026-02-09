namespace OwlSight.Core.Git;

public sealed class ChangedFile
{
    public required string Path { get; init; }
    public required string Status { get; init; }
    public required List<DiffHunk> Hunks { get; init; }
    public required string RawDiff { get; init; }
}
