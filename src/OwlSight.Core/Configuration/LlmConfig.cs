namespace OwlSight.Core.Configuration;

public sealed class LlmConfig
{
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o";
    public int MaxTokens { get; set; } = 4096;
    public float Temperature { get; set; } = 0.2f;
    public int MaxToolRoundtrips { get; set; } = 15;
}
