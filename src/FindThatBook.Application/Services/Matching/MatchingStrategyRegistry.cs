using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Interfaces.Matching;
using FindThatBook.Domain.ValueObjects;
using FindThatBook.Domain.ValueObjects.Matching;

namespace FindThatBook.Application.Services.Matching;

/// <summary>
/// Registry that holds all matching strategies and coordinates their execution.
/// </summary>
public class MatchingStrategyRegistry
{
    private readonly IReadOnlyList<IMatchingStrategy> _strategies;
    private readonly IMatchStrengthEvaluator _strengthEvaluator;

    public MatchingStrategyRegistry(
        IEnumerable<IMatchingStrategy> strategies,
        IMatchStrengthEvaluator strengthEvaluator)
    {
        _strategies = strategies.ToList();
        _strengthEvaluator = strengthEvaluator;
    }

    /// <summary>
    /// Gets all registered strategies.
    /// </summary>
    public IReadOnlyList<IMatchingStrategy> Strategies => _strategies;

    /// <summary>
    /// Evaluates a book against extracted information using all applicable strategies.
    /// </summary>
    /// <param name="book">The book to evaluate.</param>
    /// <param name="extractedInfo">The extracted book information.</param>
    /// <returns>The aggregated match result.</returns>
    public MatchResult Evaluate(Book book, ExtractedBookInfo extractedInfo)
    {
        var strategyScores = new Dictionary<string, StrategyScore>();
        var reasons = new List<string>();
        double totalScore = 0;

        foreach (var strategy in _strategies)
        {
            var score = strategy.CanEvaluate(extractedInfo)
                ? strategy.Evaluate(book, extractedInfo)
                : StrategyScore.NoMatch(strategy.Weight);

            strategyScores[strategy.StrategyName] = score;
            totalScore += score.WeightedScore;

            if (score.HasMatch && !string.IsNullOrEmpty(score.Reason))
            {
                reasons.Add(score.Reason);
            }
        }

        var matchStrength = _strengthEvaluator.Evaluate(strategyScores);

        return new MatchResult
        {
            StrategyScores = strategyScores,
            TotalScore = Math.Round(totalScore, 2),
            MatchStrength = matchStrength,
            Reasons = reasons
        };
    }
}
