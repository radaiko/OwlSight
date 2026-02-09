using OwlSight.Core.Review;

namespace OwlSight.Cli.Output;

public interface IReviewOutputWriter
{
    Task WriteAsync(ReviewResult result, CancellationToken ct = default);
}
