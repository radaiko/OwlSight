using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.AI;

namespace OwlSight.Core.Tools;

public sealed class GetFileStructureTool : ICodebaseTool
{
    public string Name => "get_file_structure";
    public string Description => "Get the directory tree structure of the repository.";

    public AIFunction CreateAIFunction(string workingDirectory)
    {
        return AIFunctionFactory.Create(
            [Description("Get the directory tree structure. Returns files and directories up to 3 levels deep.")]
            (
                [Description("Relative directory path (defaults to root)")] string? path
            ) =>
            {
                var dir = string.IsNullOrWhiteSpace(path)
                    ? workingDirectory
                    : PathValidator.ResolveSafePath(workingDirectory, path);

                if (!Directory.Exists(dir))
                    return $"Error: Directory not found: {path ?? "."}";

                var sb = new StringBuilder();
                BuildTree(dir, workingDirectory, sb, "", 0, maxDepth: 3);
                return sb.ToString();
            },
            Name, Description);
    }

    private static void BuildTree(string dir, string root, StringBuilder sb, string indent, int depth, int maxDepth)
    {
        if (depth >= maxDepth) return;

        var entries = Directory.GetFileSystemEntries(dir)
            .Where(e => !Path.GetFileName(e).StartsWith('.'))
            .OrderBy(e => !Directory.Exists(e))
            .ThenBy(e => Path.GetFileName(e))
            .ToList();

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var name = Path.GetFileName(entry);
            var isLast = i == entries.Count - 1;
            var connector = isLast ? "└── " : "├── ";
            var childIndent = indent + (isLast ? "    " : "│   ");

            if (Directory.Exists(entry))
            {
                sb.AppendLine($"{indent}{connector}{name}/");
                BuildTree(entry, root, sb, childIndent, depth + 1, maxDepth);
            }
            else
            {
                sb.AppendLine($"{indent}{connector}{name}");
            }
        }
    }
}
