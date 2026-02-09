using FluentAssertions;
using OwlSight.Core.Git;

namespace OwlSight.Core.Tests.Git;

public class GitDiffParsingTests
{
    private const string SampleDiff = """
        diff --git a/src/Example.cs b/src/Example.cs
        index abc1234..def5678 100644
        --- a/src/Example.cs
        +++ b/src/Example.cs
        @@ -10,6 +10,8 @@ namespace Example
             public void DoSomething()
             {
                 var x = 1;
        +        var y = 2;
        +        Console.WriteLine(x + y);
                 return;
             }
         }
        """;

    [Fact]
    public void ParseUnifiedDiff_WithSingleFile_ReturnsOneChangedFile()
    {
        var files = GitCliService.ParseUnifiedDiff(SampleDiff);

        files.Should().HaveCount(1);
        files[0].Path.Should().Be("src/Example.cs");
        files[0].Status.Should().Be("Modified");
    }

    [Fact]
    public void ParseUnifiedDiff_WithSingleFile_ParsesHunks()
    {
        var files = GitCliService.ParseUnifiedDiff(SampleDiff);

        files[0].Hunks.Should().HaveCount(1);
        files[0].Hunks[0].OldStart.Should().Be(10);
        files[0].Hunks[0].OldCount.Should().Be(6);
        files[0].Hunks[0].NewStart.Should().Be(10);
        files[0].Hunks[0].NewCount.Should().Be(8);
    }

    [Fact]
    public void ParseUnifiedDiff_WithEmptyInput_ReturnsEmptyList()
    {
        var files = GitCliService.ParseUnifiedDiff("");
        files.Should().BeEmpty();
    }

    [Fact]
    public void ParseUnifiedDiff_WithNewFile_DetectsAddedStatus()
    {
        var diff = """
            diff --git a/src/NewFile.cs b/src/NewFile.cs
            new file mode 100644
            index 0000000..abc1234
            --- /dev/null
            +++ b/src/NewFile.cs
            @@ -0,0 +1,5 @@
            +namespace Example;
            +
            +public class NewFile
            +{
            +}
            """;

        var files = GitCliService.ParseUnifiedDiff(diff);

        files.Should().HaveCount(1);
        files[0].Path.Should().Be("src/NewFile.cs");
        files[0].Status.Should().Be("Added");
    }

    [Fact]
    public void ParseUnifiedDiff_WithMultipleFiles_ReturnsAll()
    {
        var diff = """
            diff --git a/file1.cs b/file1.cs
            index abc..def 100644
            --- a/file1.cs
            +++ b/file1.cs
            @@ -1,3 +1,4 @@
             line1
            +added
             line2
             line3
            diff --git a/file2.cs b/file2.cs
            index abc..def 100644
            --- a/file2.cs
            +++ b/file2.cs
            @@ -1,3 +1,4 @@
             line1
            +added
             line2
             line3
            """;

        var files = GitCliService.ParseUnifiedDiff(diff);
        files.Should().HaveCount(2);
        files[0].Path.Should().Be("file1.cs");
        files[1].Path.Should().Be("file2.cs");
    }
}
