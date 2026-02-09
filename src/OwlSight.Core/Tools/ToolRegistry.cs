using Microsoft.Extensions.AI;

namespace OwlSight.Core.Tools;

public sealed class ToolRegistry
{
    private readonly IEnumerable<ICodebaseTool> _tools;

    public ToolRegistry(IEnumerable<ICodebaseTool> tools) => _tools = tools;

    public IList<AITool> CreateTools(string workingDirectory)
    {
        return _tools
            .Select(t => (AITool)t.CreateAIFunction(workingDirectory))
            .ToList();
    }
}
