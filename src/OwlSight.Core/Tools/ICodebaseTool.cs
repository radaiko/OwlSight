using Microsoft.Extensions.AI;

namespace OwlSight.Core.Tools;

public interface ICodebaseTool
{
    string Name { get; }
    string Description { get; }
    AIFunction CreateAIFunction(string workingDirectory);
}
