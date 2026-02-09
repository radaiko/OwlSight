namespace OwlSight.Core.Configuration;

public sealed class OwlSightConfig
{
    public LlmConfig Llm { get; set; } = new();
    public ReviewConfig Review { get; set; } = new();
    public string WorkingDirectory { get; set; } = Directory.GetCurrentDirectory();
    public string BaseBranch { get; set; } = "main";
    public string? OutputPath { get; set; }
}
