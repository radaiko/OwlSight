using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace OwlSight.Core.Tools;

public sealed class ReadFileLinesTool : ICodebaseTool
{
    public string Name => "read_file_lines";
    public string Description => "Read specific lines from a file.";

    public AIFunction CreateAIFunction(string workingDirectory)
    {
        return AIFunctionFactory.Create(
            [Description("Read specific lines from a file (1-indexed, inclusive).")]
            (
                [Description("Relative path to the file")] string path,
                [Description("Start line number (1-indexed)")] int startLine,
                [Description("End line number (inclusive)")] int endLine
            ) =>
            {
                var fullPath = PathValidator.ResolveSafePath(workingDirectory, path);
                if (!File.Exists(fullPath))
                    return $"Error: File not found: {path}";

                var lines = File.ReadAllLines(fullPath);
                var start = Math.Max(1, startLine) - 1;
                var end = Math.Min(lines.Length, endLine);

                if (start >= lines.Length)
                    return $"Error: Start line {startLine} is beyond file length ({lines.Length} lines).";

                var selected = lines[start..end];
                return string.Join('\n', selected.Select((line, i) => $"{start + i + 1}: {line}"));
            },
            Name, Description);
    }
}
