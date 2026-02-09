using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace OwlSight.Core.Tools;

public sealed class ReadFileTool : ICodebaseTool
{
    public string Name => "read_file";
    public string Description => "Read the entire contents of a file.";

    public AIFunction CreateAIFunction(string workingDirectory)
    {
        return AIFunctionFactory.Create(
            [Description("Read the entire contents of a file at the given path.")]
            (
                [Description("Relative path to the file")] string path
            ) =>
            {
                var fullPath = PathValidator.ResolveSafePath(workingDirectory, path);
                if (!File.Exists(fullPath))
                    return $"Error: File not found: {path}";
                var content = File.ReadAllText(fullPath);
                return content.Length > 50_000
                    ? content[..50_000] + "\n... (truncated)"
                    : content;
            },
            Name, Description);
    }
}
