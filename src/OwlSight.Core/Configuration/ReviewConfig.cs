namespace OwlSight.Core.Configuration;

public sealed class ReviewConfig
{
    public string MinSeverity { get; set; } = "Info";
    public List<string> ExcludePatterns { get; set; } = [];
    public int MaxFilesPerBatch { get; set; } = 10;
}
