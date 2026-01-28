using FindThatBook.Domain.Enums;
using FindThatBook.Domain.ValueObjects.Matching;

namespace FindThatBook.Domain.Interfaces.Matching;

/// <summary>
/// Evaluates the overall match strength based on individual strategy scores.
/// </summary>
public interface IMatchStrengthEvaluator
{
    /// <summary>
    /// Determines the match strength based on strategy scores.
    /// </summary>
    /// <param name="strategyScores">Dictionary of strategy names to their scores.</param>
    /// <returns>The determined match strength.</returns>
    MatchStrength Evaluate(IReadOnlyDictionary<string, StrategyScore> strategyScores);
}
