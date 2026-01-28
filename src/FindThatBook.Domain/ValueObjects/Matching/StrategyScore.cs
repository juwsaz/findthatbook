namespace FindThatBook.Domain.ValueObjects.Matching;

/// <summary>
/// Represents the result of evaluating a single matching strategy.
/// </summary>
public record StrategyScore
{
    /// <summary>
    /// The score from 0.0 to 1.0 representing the match quality.
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// The weight of this strategy in the overall score calculation.
    /// </summary>
    public double Weight { get; init; }

    /// <summary>
    /// Explanation of why this score was assigned.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Gets the weighted contribution to the total score.
    /// </summary>
    public double WeightedScore => Score * Weight * 100;

    /// <summary>
    /// Indicates whether this strategy produced a meaningful match.
    /// </summary>
    public bool HasMatch => Score > 0;

    /// <summary>
    /// Creates a zero score (no match).
    /// </summary>
    public static StrategyScore NoMatch(double weight) => new()
    {
        Score = 0,
        Weight = weight,
        Reason = null
    };

    /// <summary>
    /// Creates a score with the given value and reason.
    /// </summary>
    public static StrategyScore Create(double score, double weight, string reason) => new()
    {
        Score = Math.Clamp(score, 0, 1),
        Weight = weight,
        Reason = reason
    };
}
