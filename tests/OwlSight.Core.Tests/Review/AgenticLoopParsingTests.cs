using FluentAssertions;
using OwlSight.Core.Review;

namespace OwlSight.Core.Tests.Review;

public class AgenticLoopParsingTests
{
    [Fact]
    public void ParseFindings_WithValidJson_ReturnsFindings()
    {
        var json = """
            {
              "findings": [
                {
                  "file": "src/Example.cs",
                  "line": 42,
                  "severity": "Warning",
                  "title": "Unused variable",
                  "description": "Variable x is assigned but never used."
                }
              ]
            }
            """;

        var findings = AgenticLoop.ParseFindings(json);

        findings.Should().NotBeNull();
        findings.Should().HaveCount(1);
        findings![0].File.Should().Be("src/Example.cs");
        findings[0].Line.Should().Be(42);
        findings[0].Severity.Should().Be(ReviewSeverity.Warning);
        findings[0].Title.Should().Be("Unused variable");
    }

    [Fact]
    public void ParseFindings_WithEmptyFindings_ReturnsEmptyList()
    {
        var json = """{"findings": []}""";

        var findings = AgenticLoop.ParseFindings(json);

        findings.Should().NotBeNull();
        findings.Should().BeEmpty();
    }

    [Fact]
    public void ParseFindings_WithMarkdownCodeBlock_ExtractsJson()
    {
        var text = """
            Here are my findings:
            ```json
            {"findings": [{"file": "test.cs", "line": 1, "severity": "Info", "title": "Test", "description": "Desc"}]}
            ```
            """;

        var findings = AgenticLoop.ParseFindings(text);

        findings.Should().NotBeNull();
        findings.Should().HaveCount(1);
    }

    [Fact]
    public void ParseFindings_WithInvalidJson_ReturnsNull()
    {
        var text = "This is not JSON at all.";

        var findings = AgenticLoop.ParseFindings(text);

        findings.Should().BeNull();
    }

    [Fact]
    public void ParseFindings_WithAllSeverities_ParsesCorrectly()
    {
        var json = """
            {
              "findings": [
                {"file": "a.cs", "line": 1, "severity": "Critical", "title": "T1", "description": "D1"},
                {"file": "b.cs", "line": 2, "severity": "Warning", "title": "T2", "description": "D2"},
                {"file": "c.cs", "line": 3, "severity": "Info", "title": "T3", "description": "D3"},
                {"file": "d.cs", "line": 4, "severity": "Nitpick", "title": "T4", "description": "D4"}
              ]
            }
            """;

        var findings = AgenticLoop.ParseFindings(json);

        findings.Should().HaveCount(4);
        findings![0].Severity.Should().Be(ReviewSeverity.Critical);
        findings[1].Severity.Should().Be(ReviewSeverity.Warning);
        findings[2].Severity.Should().Be(ReviewSeverity.Info);
        findings[3].Severity.Should().Be(ReviewSeverity.Nitpick);
    }
}
