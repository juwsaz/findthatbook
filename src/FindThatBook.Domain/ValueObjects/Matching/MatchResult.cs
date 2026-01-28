using FindThatBook.Domain.Enums;

namespace FindThatBook.Domain.ValueObjects.Matching;

/// <summary>
/// Represents the aggregated result of all matching strategies.
/// </summary>
public record MatchResult
{
    /// <summary>
    /// Individual strategy scores keyed by strategy name.
    /// </summary>
    public IReadOnlyDictionary<string, StrategyScore> StrategyScores { get; init; } =
        new Dictionary<string, StrategyScore>();

    /// <summary>
    /// The total weighted score (0-100).
    /// </summary>
    public double TotalScore { get; init; }

    /// <summary>
    /// The determined match strength based on strategy scores.
    /// </summary>
    public MatchStrength MatchStrength { get; init; }

    /// <summary>
    /// All match reasons from strategies that produced matches.
    /// </summary>
    public IReadOnlyList<string> Reasons { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the score for a specific strategy.
    /// </summary>
    public StrategyScore? GetScore(string strategyName)
    {
        return StrategyScores.TryGetValue(strategyName, out var score) ? score : null;
    }
}
