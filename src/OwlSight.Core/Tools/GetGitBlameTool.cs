using System.ComponentModel;
using Microsoft.Extensions.AI;
using OwlSight.Core.Git;

namespace OwlSight.Core.Tools;

public sealed class GetGitBlameTool : ICodebaseTool
{
    private readonly IGitService _gitService;

    public GetGitBlameTool(IGitService gitService) => _gitService = gitService;

    public string Name => "get_git_blame";
    public string Description => "Get git blame information for a file.";

    public AIFunction CreateAIFunction(string workingDirectory)
    {
        return AIFunctionFactory.Create(
            [Description("Get git blame output showing who last modified each line.")]
            async (
                [Description("Relative path to the file")] string path,
                [Description("Start line number (optional)")] int? startLine,
                [Description("End line number (optional)")] int? endLine
            ) =>
            {
                try
                {
                    PathValidator.ResolveSafePath(workingDirectory, path);
                    return await _gitService.GetBlameAsync(path, startLine, endLine, workingDirectory);
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            },
            Name, Description);
    }
}
