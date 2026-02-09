using FluentAssertions;
using OwlSight.Core.Rules;

namespace OwlSight.Core.Tests.Rules;

public class MarkdownRulesParsingTests
{
    [Fact]
    public void ExtractFrontMatter_WithValidFrontMatter_ReturnsKeyValues()
    {
        var content = """
            ---
            title: Test Rule
            severity: Critical
            category: Security
            ---
            # Test Rule

            Body content here.
            """;

        var result = MarkdownRulesProvider.ExtractFrontMatter(content, out var body);

        result.Should().ContainKey("title").WhoseValue.Should().Be("Test Rule");
        result.Should().ContainKey("severity").WhoseValue.Should().Be("Critical");
        result.Should().ContainKey("category").WhoseValue.Should().Be("Security");
        body.Should().Contain("# Test Rule");
        body.Should().Contain("Body content here.");
    }

    [Fact]
    public void ExtractFrontMatter_WithNoFrontMatter_ReturnsEmptyDict()
    {
        var content = """
            # Test Rule

            Just a regular markdown file.
            """;

        var result = MarkdownRulesProvider.ExtractFrontMatter(content, out var body);

        result.Should().BeEmpty();
        body.Should().Be(content);
    }

    [Fact]
    public void ParseRuleFile_WithFullContent_CreatesCompleteRule()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var filePath = Path.Combine(tempDir, "no-hardcoded-secrets.md");
            File.WriteAllText(filePath, """
                ---
                title: No Hardcoded Secrets
                severity: Critical
                category: Security
                description: Do not hardcode API keys, passwords, or other secrets.
                ---
                # No Hardcoded Secrets

                Check for hardcoded secrets in code.
                """);

            var rule = MarkdownRulesProvider.ParseRuleFile(filePath);

            rule.Should().NotBeNull();
            rule!.Id.Should().Be("no-hardcoded-secrets");
            rule.Title.Should().Be("No Hardcoded Secrets");
            rule.Severity.Should().Be("Critical");
            rule.Category.Should().Be("Security");
            rule.Description.Should().Be("Do not hardcode API keys, passwords, or other secrets.");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
