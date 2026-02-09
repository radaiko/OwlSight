using OwlSight.Core.Configuration;

namespace OwlSight.Core.Review;

public interface IReviewEngine
{
    Task<ReviewResult> RunAsync(OwlSightConfig config, CancellationToken ct = default);
}
