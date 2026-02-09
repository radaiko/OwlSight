using Microsoft.Extensions.DependencyInjection;
using OwlSight.Core.Configuration;
using OwlSight.Core.Git;
using OwlSight.Core.Llm;
using OwlSight.Core.Review;
using OwlSight.Core.Rules;
using OwlSight.Core.Tools;

namespace OwlSight.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOwlSightCore(this IServiceCollection services)
    {
        // Configuration
        services.AddSingleton<ConfigLoader>();

        // Git
        services.AddSingleton<IGitService, GitCliService>();

        // Rules
        services.AddSingleton<IRulesProvider, MarkdownRulesProvider>();

        // Tools
        services.AddSingleton<ICodebaseTool, ReadFileTool>();
        services.AddSingleton<ICodebaseTool, ReadFileLinesTool>();
        services.AddSingleton<ICodebaseTool, ListFilesTool>();
        services.AddSingleton<ICodebaseTool, SearchTextTool>();
        services.AddSingleton<ICodebaseTool, GetFileStructureTool>();
        services.AddSingleton<ICodebaseTool, GetGitBlameTool>();
        services.AddSingleton<ICodebaseTool, GetGitLogTool>();
        services.AddSingleton<ToolRegistry>();

        // LLM
        services.AddSingleton<LlmClientFactory>();
        services.AddSingleton<PromptBuilder>();

        // Review
        services.AddSingleton<AgenticLoop>();
        services.AddSingleton<IReviewEngine, ReviewEngine>();

        return services;
    }
}
