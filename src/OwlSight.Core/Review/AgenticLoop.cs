using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace OwlSight.Core.Review;

public sealed class AgenticLoop
{
    private readonly ILogger<AgenticLoop> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AgenticLoop(ILogger<AgenticLoop> logger) => _logger = logger;

    public async Task<List<ReviewFinding>> RunAsync(
        IChatClient client,
        IList<ChatMessage> messages,
        IList<AITool> tools,
        int maxRoundtrips,
        ChatOptions chatOptions,
        CancellationToken ct = default)
    {
        for (var iteration = 0; iteration < maxRoundtrips; iteration++)
        {
            _logger.LogDebug("Agentic loop iteration {Iteration}/{Max}", iteration + 1, maxRoundtrips);

            var response = await client.GetResponseAsync(messages, chatOptions, ct);

            // Add assistant response to messages
            messages.Add(new ChatMessage(ChatRole.Assistant, response.Messages.SelectMany(m => m.Contents).ToList()));

            var toolCalls = response.Messages
                .SelectMany(m => m.Contents)
                .OfType<FunctionCallContent>()
                .ToList();

            if (toolCalls.Count > 0)
            {
                _logger.LogDebug("Processing {Count} tool calls", toolCalls.Count);

                var toolResultContents = new List<AIContent>();
                foreach (var toolCall in toolCalls)
                {
                    var result = await ExecuteToolCallAsync(toolCall, tools, ct);
                    toolResultContents.Add(result);
                }

                messages.Add(new ChatMessage(ChatRole.Tool, toolResultContents));
                continue;
            }

            // No tool calls — try to parse the final response as findings
            var textContent = string.Join("", response.Messages
                .SelectMany(m => m.Contents)
                .OfType<TextContent>()
                .Select(t => t.Text));

            if (!string.IsNullOrWhiteSpace(textContent))
            {
                var findings = ParseFindings(textContent);
                if (findings is not null)
                    return findings;

                _logger.LogWarning("Failed to parse findings from LLM response, continuing loop");
            }
        }

        _logger.LogWarning("Agentic loop reached max iterations ({Max}) without final response", maxRoundtrips);
        return [];
    }

    private async Task<FunctionResultContent> ExecuteToolCallAsync(
        FunctionCallContent toolCall,
        IList<AITool> tools,
        CancellationToken ct)
    {
        var tool = tools
            .OfType<AIFunction>()
            .FirstOrDefault(t => t.Name == toolCall.Name);

        if (tool is null)
        {
            _logger.LogWarning("Unknown tool requested: {ToolName}", toolCall.Name);
            return new FunctionResultContent(toolCall.CallId, $"Error: Unknown tool '{toolCall.Name}'");
        }

        try
        {
            _logger.LogDebug("Executing tool: {ToolName}", toolCall.Name);
            var args = toolCall.Arguments is not null
                ? new AIFunctionArguments(toolCall.Arguments)
                : null;
            var result = await tool.InvokeAsync(args, ct);
            var resultStr = result?.ToString() ?? string.Empty;

            // Truncate very long results
            if (resultStr.Length > 30_000)
                resultStr = resultStr[..30_000] + "\n... (truncated)";

            return new FunctionResultContent(toolCall.CallId, resultStr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool {ToolName} failed", toolCall.Name);
            return new FunctionResultContent(toolCall.CallId, $"Error executing tool: {ex.Message}");
        }
    }

    internal static List<ReviewFinding>? ParseFindings(string text)
    {
        // Try to extract JSON from the response
        var json = ExtractJson(text);
        if (json is null)
            return null;

        try
        {
            // Try parsing as { "findings": [...] }
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("findings", out var findingsArray))
            {
                return JsonSerializer.Deserialize<List<ReviewFinding>>(findingsArray.GetRawText(), JsonOptions) ?? [];
            }

            // Try parsing as direct array
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<List<ReviewFinding>>(json, JsonOptions) ?? [];
            }
        }
        catch (JsonException)
        {
            // Fall through
        }

        return null;
    }

    private static string? ExtractJson(string text)
    {
        // Try the whole text first
        var trimmed = text.Trim();
        if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
            return trimmed;

        // Try to find JSON in markdown code blocks
        var start = text.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
        if (start >= 0)
        {
            start = text.IndexOf('\n', start) + 1;
            var end = text.IndexOf("```", start, StringComparison.Ordinal);
            if (end > start)
                return text[start..end].Trim();
        }

        // Try to find bare JSON block
        start = text.IndexOf('{');
        if (start >= 0)
        {
            var end = text.LastIndexOf('}');
            if (end > start)
                return text[start..(end + 1)];
        }

        return null;
    }
}
