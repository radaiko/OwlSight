using System.Text.RegularExpressions;

namespace OwlSight.Core.Rules;

public sealed partial class MarkdownRulesProvider : IRulesProvider
{
    public Task<IReadOnlyList<ReviewRule>> LoadRulesAsync(string workingDirectory, CancellationToken ct = default)
    {
        var rulesDir = Path.Combine(workingDirectory, ".owlsight", "rules");
        if (!Directory.Exists(rulesDir))
            return Task.FromResult<IReadOnlyList<ReviewRule>>([]);

        var rules = new List<ReviewRule>();
        foreach (var file in Directory.GetFiles(rulesDir, "*.md"))
        {
            ct.ThrowIfCancellationRequested();
            var rule = ParseRuleFile(file);
            if (rule is not null)
                rules.Add(rule);
        }

        return Task.FromResult<IReadOnlyList<ReviewRule>>(rules);
    }

    internal static ReviewRule? ParseRuleFile(string filePath)
    {
        var content = File.ReadAllText(filePath);
        if (string.IsNullOrWhiteSpace(content))
            return null;

        var id = Path.GetFileNameWithoutExtension(filePath);
        var frontMatter = ExtractFrontMatter(content, out var body);

        var title = frontMatter.GetValueOrDefault("title")
                    ?? ExtractFirstHeading(body)
                    ?? id;

        var severity = frontMatter.GetValueOrDefault("severity") ?? "Warning";
        var category = frontMatter.GetValueOrDefault("category") ?? "General";
        var description = frontMatter.GetValueOrDefault("description") ?? string.Empty;

        return new ReviewRule
        {
            Id = id,
            Title = title,
            Description = description,
            Severity = severity,
            Category = category,
            Content = body.Trim()
        };
    }

    internal static Dictionary<string, string> ExtractFrontMatter(string content, out string body)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        body = content;

        var match = FrontMatterRegex().Match(content);
        if (!match.Success) return result;

        var frontMatterText = match.Groups[1].Value;
        body = content[(match.Index + match.Length)..];

        foreach (var line in frontMatterText.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var colonIdx = line.IndexOf(':');
            if (colonIdx <= 0) continue;

            var key = line[..colonIdx].Trim();
            var value = line[(colonIdx + 1)..].Trim();
            result[key] = value;
        }

        return result;
    }

    private static string? ExtractFirstHeading(string markdown)
    {
        var match = HeadingRegex().Match(markdown);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    [GeneratedRegex(@"^---\s*\n(.*?)\n---\s*\n", RegexOptions.Singleline)]
    private static partial Regex FrontMatterRegex();

    [GeneratedRegex(@"^#\s+(.+)$", RegexOptions.Multiline)]
    private static partial Regex HeadingRegex();
}
