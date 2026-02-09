using System.ComponentModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.FileSystemGlobbing;

namespace OwlSight.Core.Tools;

public sealed class ListFilesTool : ICodebaseTool
{
    public string Name => "list_files";
    public string Description => "List files in a directory, optionally filtered by glob pattern.";

    public AIFunction CreateAIFunction(string workingDirectory)
    {
        return AIFunctionFactory.Create(
            [Description("List files in a directory, optionally filtered by a glob pattern.")]
            (
                [Description("Relative directory path (defaults to root)")] string? directory,
                [Description("Glob pattern to filter files (e.g. '*.cs', '**/*.json')")] string? pattern
            ) =>
            {
                var dir = string.IsNullOrWhiteSpace(directory)
                    ? workingDirectory
                    : PathValidator.ResolveSafePath(workingDirectory, directory);

                if (!Directory.Exists(dir))
                    return $"Error: Directory not found: {directory ?? "."}";

                if (!string.IsNullOrWhiteSpace(pattern))
                {
                    var matcher = new Matcher();
                    matcher.AddInclude(pattern);
                    var result = matcher.GetResultsInFullPath(dir);
                    var relativePaths = result
                        .Select(f => Path.GetRelativePath(workingDirectory, f))
                        .OrderBy(f => f)
                        .Take(200);
                    return string.Join('\n', relativePaths);
                }

                var entries = Directory.GetFileSystemEntries(dir)
                    .Select(f => Path.GetRelativePath(workingDirectory, f))
                    .OrderBy(f => f)
                    .Take(200);
                return string.Join('\n', entries);
            },
            Name, Description);
    }
}
