using System.ComponentModel;
using Microsoft.Extensions.AI;
using OwlSight.Core.Git;

namespace OwlSight.Core.Tools;

public sealed class GetGitLogTool : ICodebaseTool
{
    private readonly IGitService _gitService;

    public GetGitLogTool(IGitService gitService) => _gitService = gitService;

    public string Name => "get_git_log";
    public string Description => "Get recent git log entries.";

    public AIFunction CreateAIFunction(string workingDirectory)
    {
        return AIFunctionFactory.Create(
            [Description("Get recent git commit log entries.")]
            async (
                [Description("Relative file path to filter log (optional)")] string? path,
                [Description("Number of log entries to return (default 10)")] int? count
            ) =>
            {
                try
                {
                    if (path is not null)
                        PathValidator.ResolveSafePath(workingDirectory, path);

                    return await _gitService.GetLogAsync(path, count ?? 10, workingDirectory);
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            },
            Name, Description);
    }
}
