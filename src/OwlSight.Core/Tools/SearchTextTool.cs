using System.ComponentModel;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.FileSystemGlobbing;

namespace OwlSight.Core.Tools;

public sealed class SearchTextTool : ICodebaseTool
{
    public string Name => "search_text";
    public string Description => "Search for text or regex pattern in files.";

    public AIFunction CreateAIFunction(string workingDirectory)
    {
        return AIFunctionFactory.Create(
            [Description("Search for a text or regex pattern in files within the repository.")]
            (
                [Description("Regex pattern to search for")] string pattern,
                [Description("Relative directory path to search in (defaults to root)")] string? path,
                [Description("Glob pattern to filter files (e.g. '*.cs')")] string? filePattern
            ) =>
            {
                var searchDir = string.IsNullOrWhiteSpace(path)
                    ? workingDirectory
                    : PathValidator.ResolveSafePath(workingDirectory, path);

                if (!Directory.Exists(searchDir))
                    return $"Error: Directory not found: {path ?? "."}";

                Regex regex;
                try
                {
                    regex = new Regex(pattern, RegexOptions.Compiled, TimeSpan.FromSeconds(5));
                }
                catch (RegexParseException ex)
                {
                    return $"Error: Invalid regex pattern: {ex.Message}";
                }

                var files = GetFilesToSearch(searchDir, workingDirectory, filePattern);
                var results = new List<string>();
                var matchCount = 0;
                const int maxMatches = 100;

                foreach (var file in files)
                {
                    if (matchCount >= maxMatches) break;

                    try
                    {
                        var lines = File.ReadAllLines(file);
                        for (var i = 0; i < lines.Length && matchCount < maxMatches; i++)
                        {
                            if (regex.IsMatch(lines[i]))
                            {
                                var relativePath = Path.GetRelativePath(workingDirectory, file);
                                results.Add($"{relativePath}:{i + 1}: {lines[i].TrimStart()}");
                                matchCount++;
                            }
                        }
                    }
                    catch
                    {
                        // Skip binary or inaccessible files
                    }
                }

                return results.Count > 0
                    ? string.Join('\n', results)
                    : "No matches found.";
            },
            Name, Description);
    }

    private static IEnumerable<string> GetFilesToSearch(string searchDir, string workingDirectory, string? filePattern)
    {
        if (!string.IsNullOrWhiteSpace(filePattern))
        {
            var matcher = new Matcher();
            matcher.AddInclude(filePattern);
            return matcher.GetResultsInFullPath(searchDir);
        }

        return Directory.EnumerateFiles(searchDir, "*", new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            MaxRecursionDepth = 10
        });
    }
}
