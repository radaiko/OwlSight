namespace OwlSight.Core.Git;

public sealed class DiffResult
{
    public required List<ChangedFile> Files { get; init; }
    public required string BaseBranch { get; init; }
}
