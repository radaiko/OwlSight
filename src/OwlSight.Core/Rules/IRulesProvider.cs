namespace OwlSight.Core.Rules;

public interface IRulesProvider
{
    Task<IReadOnlyList<ReviewRule>> LoadRulesAsync(string workingDirectory, CancellationToken ct = default);
}
