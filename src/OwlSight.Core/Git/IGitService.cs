namespace OwlSight.Core.Git;

public interface IGitService
{
    Task<DiffResult> GetDiffAsync(string baseBranch, string workingDirectory, CancellationToken ct = default);
    Task<string> GetBlameAsync(string path, int? startLine, int? endLine, string workingDirectory, CancellationToken ct = default);
    Task<string> GetLogAsync(string? path, int count, string workingDirectory, CancellationToken ct = default);
}
