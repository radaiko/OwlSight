using FluentAssertions;
using OwlSight.Core.Tools;

namespace OwlSight.Core.Tests.Tools;

public class PathValidatorTests
{
    [Fact]
    public void ResolveSafePath_WithValidRelativePath_ReturnsFullPath()
    {
        var workingDir = "/tmp/test-repo";
        var result = PathValidator.ResolveSafePath(workingDir, "src/file.cs");
        result.Should().Be(Path.GetFullPath(Path.Combine(workingDir, "src/file.cs")));
    }

    [Fact]
    public void ResolveSafePath_WithTraversalAttempt_ThrowsUnauthorizedAccess()
    {
        var workingDir = "/tmp/test-repo";
        var act = () => PathValidator.ResolveSafePath(workingDir, "../../etc/passwd");
        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void ResolveSafePath_WithAbsolutePathOutside_ThrowsUnauthorizedAccess()
    {
        var workingDir = "/tmp/test-repo";
        var act = () => PathValidator.ResolveSafePath(workingDir, "/etc/passwd");
        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void ResolveSafePath_WithCurrentDirReference_ReturnsValidPath()
    {
        var workingDir = "/tmp/test-repo";
        var result = PathValidator.ResolveSafePath(workingDir, "./src/../src/file.cs");
        result.Should().StartWith(Path.GetFullPath(workingDir));
    }
}
