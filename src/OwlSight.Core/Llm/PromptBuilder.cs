using System.Text;
using Microsoft.Extensions.AI;
using OwlSight.Core.Git;
using OwlSight.Core.Rules;

namespace OwlSight.Core.Llm;

public sealed class PromptBuilder
{
    public IList<ChatMessage> Build(IReadOnlyList<ReviewRule> rules, IReadOnlyList<ChangedFile> files)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, BuildSystemPrompt(rules)),
            new(ChatRole.User, BuildUserPrompt(files))
        };
        return messages;
    }

    private static string BuildSystemPrompt(IReadOnlyList<ReviewRule> rules)
    {
        var sb = new StringBuilder();

        sb.AppendLine("""
            You are OwlSight, an expert AI code reviewer. Your job is to review code changes (diffs) and produce actionable findings.

            ## Instructions
            1. Carefully analyze the provided diffs for bugs, security issues, performance problems, code quality, and adherence to best practices.
            2. Use the provided tools to inspect surrounding code context when needed — read files, search for patterns, check blame info, etc.
            3. Focus on the CHANGED lines, but consider surrounding context.
            4. Do NOT report issues in unchanged code unless the changed code creates a problem with it.
            5. Be specific: include exact file paths and line numbers.

            ## Output Format
            When you have completed your review, respond with a JSON object in this EXACT format (no markdown fencing):
            {
              "findings": [
                {
                  "file": "relative/path/to/file.cs",
                  "line": 42,
                  "endLine": 45,
                  "severity": "Critical|Warning|Info|Nitpick",
                  "title": "Short title describing the issue",
                  "description": "Detailed explanation of why this is a problem",
                  "suggestion": "How to fix it (optional)",
                  "ruleId": "matching-rule-id-if-applicable (optional)"
                }
              ]
            }

            ## Severity Levels
            - **Critical**: Bugs, security vulnerabilities, data loss risks — must be fixed before merge
            - **Warning**: Code smells, potential issues, poor patterns — should be fixed
            - **Info**: Suggestions for improvement — nice to have
            - **Nitpick**: Style/formatting issues — minor

            If there are no issues found, return: {"findings": []}
            """);

        if (rules.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Project-Specific Review Rules");
            foreach (var rule in rules)
            {
                sb.AppendLine($"\n### Rule: {rule.Id} ({rule.Severity})");
                sb.AppendLine($"**{rule.Title}**");
                if (!string.IsNullOrEmpty(rule.Description))
                    sb.AppendLine(rule.Description);
                sb.AppendLine(rule.Content);
            }
        }

        return sb.ToString();
    }

    private static string BuildUserPrompt(IReadOnlyList<ChangedFile> files)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Please review the following code changes:\n");

        foreach (var file in files)
        {
            sb.AppendLine($"## File: {file.Path} ({file.Status})");
            sb.AppendLine("```diff");
            sb.AppendLine(file.RawDiff);
            sb.AppendLine("```");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
