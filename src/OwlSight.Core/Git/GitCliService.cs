using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace OwlSight.Core.Git;

public sealed partial class GitCliService : IGitService
{
    public async Task<DiffResult> GetDiffAsync(string baseBranch, string workingDirectory, CancellationToken ct = default)
    {
        var diffOutput = await RunGitAsync($"diff {baseBranch} --unified=5", workingDirectory, ct);
        var files = ParseUnifiedDiff(diffOutput);
        return new DiffResult { Files = files, BaseBranch = baseBranch };
    }

    public async Task<string> GetBlameAsync(string path, int? startLine, int? endLine, string workingDirectory, CancellationToken ct = default)
    {
        var args = new StringBuilder("blame");
        if (startLine.HasValue && endLine.HasValue)
            args.Append($" -L {startLine},{endLine}");
        else if (startLine.HasValue)
            args.Append($" -L {startLine},+1");
        args.Append($" -- \"{path}\"");

        return await RunGitAsync(args.ToString(), workingDirectory, ct);
    }

    public async Task<string> GetLogAsync(string? path, int count, string workingDirectory, CancellationToken ct = default)
    {
        var args = $"log --oneline -n {count}";
        if (path is not null)
            args += $" -- \"{path}\"";

        return await RunGitAsync(args, workingDirectory, ct);
    }

    internal static List<ChangedFile> ParseUnifiedDiff(string diffOutput)
    {
        var files = new List<ChangedFile>();
        if (string.IsNullOrWhiteSpace(diffOutput))
            return files;

        var fileSections = DiffFileHeaderRegex().Split(diffOutput);

        // fileSections[0] is empty/preamble, then pairs of (header-match, content)
        var headers = DiffFileHeaderRegex().Matches(diffOutput);

        for (var i = 0; i < headers.Count; i++)
        {
            var header = headers[i];
            var content = i + 1 < fileSections.Length ? fileSections[i + 1] : string.Empty;

            var filePath = ExtractFilePath(header.Value, content);
            var status = DetermineStatus(header.Value, content);
            var hunks = ParseHunks(content);

            files.Add(new ChangedFile
            {
                Path = filePath,
                Status = status,
                Hunks = hunks,
                RawDiff = header.Value + content
            });
        }

        return files;
    }

    private static string ExtractFilePath(string header, string content)
    {
        // Try to extract from +++ b/path
        var match = PlusPlusPathRegex().Match(content);
        if (match.Success && match.Groups[1].Value != "/dev/null")
            return match.Groups[1].Value;

        // Fallback: extract from --- a/path
        match = MinusMinusPathRegex().Match(content);
        if (match.Success && match.Groups[1].Value != "/dev/null")
            return match.Groups[1].Value;

        // Fallback: extract from diff --git a/path b/path
        match = DiffGitPathRegex().Match(header);
        return match.Success ? match.Groups[2].Value : "unknown";
    }

    private static string DetermineStatus(string header, string content)
    {
        if (content.Contains("new file mode")) return "Added";
        if (content.Contains("deleted file mode")) return "Deleted";
        if (content.Contains("rename from")) return "Renamed";
        return "Modified";
    }

    private static List<DiffHunk> ParseHunks(string content)
    {
        var hunks = new List<DiffHunk>();
        var hunkMatches = HunkHeaderRegex().Matches(content);

        for (var i = 0; i < hunkMatches.Count; i++)
        {
            var match = hunkMatches[i];
            var oldStart = int.Parse(match.Groups[1].Value);
            var oldCount = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1;
            var newStart = int.Parse(match.Groups[3].Value);
            var newCount = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 1;
            var hunkHeader = match.Groups[5].Value.Trim();

            var startIdx = match.Index + match.Length;
            var endIdx = i + 1 < hunkMatches.Count ? hunkMatches[i + 1].Index : content.Length;
            var hunkContent = content[startIdx..endIdx].TrimEnd();

            hunks.Add(new DiffHunk
            {
                OldStart = oldStart,
                OldCount = oldCount,
                NewStart = newStart,
                NewCount = newCount,
                Header = hunkHeader,
                Content = hunkContent
            });
        }

        return hunks;
    }

    internal static async Task<string> RunGitAsync(string arguments, string workingDirectory, CancellationToken ct = default)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(ct);
        var error = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException($"git {arguments} failed (exit {process.ExitCode}): {error.Trim()}");

        return output;
    }

    [GeneratedRegex(@"^diff --git ", RegexOptions.Multiline)]
    private static partial Regex DiffFileHeaderRegex();

    [GeneratedRegex(@"\+\+\+ b/(.+)")]
    private static partial Regex PlusPlusPathRegex();

    [GeneratedRegex(@"--- a/(.+)")]
    private static partial Regex MinusMinusPathRegex();

    [GeneratedRegex(@"diff --git a/(.+) b/(.+)")]
    private static partial Regex DiffGitPathRegex();

    [GeneratedRegex(@"^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@(.*)$", RegexOptions.Multiline)]
    private static partial Regex HunkHeaderRegex();
}
