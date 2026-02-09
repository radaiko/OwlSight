namespace OwlSight.Core.Tools;

internal static class PathValidator
{
    public static string ResolveSafePath(string workingDirectory, string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(workingDirectory, relativePath));
        var normalizedBase = Path.GetFullPath(workingDirectory);

        if (!fullPath.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException($"Access denied: path '{relativePath}' resolves outside the working directory.");

        return fullPath;
    }
}
