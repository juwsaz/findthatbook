using FindThatBook.Domain.Constants;
using FindThatBook.Domain.Enums;
using FindThatBook.Domain.Interfaces.Matching;
using FindThatBook.Domain.ValueObjects.Matching;

namespace FindThatBook.Application.Services.Matching;

/// <summary>
/// Evaluates the overall match strength based on individual strategy scores.
/// </summary>
public class MatchStrengthEvaluator : IMatchStrengthEvaluator
{
    public MatchStrength Evaluate(IReadOnlyDictionary<string, StrategyScore> strategyScores)
    {
        var titleScore = GetScore(strategyScores, "Title");
        var authorScore = GetScore(strategyScores, "Author");
        var yearScore = GetScore(strategyScores, "Year");
        var keywordScore = GetScore(strategyScores, "Keyword");

        // Exact: Both title and author are exact matches
        if (titleScore >= DomainConstants.Matching.ExactMatchThreshold &&
            authorScore >= DomainConstants.Matching.ExactMatchThreshold)
        {
            return MatchStrength.Exact;
        }

        // Strong: High confidence on both title and author
        if (titleScore >= DomainConstants.Matching.StrongMatchThreshold &&
            authorScore >= DomainConstants.Matching.StrongMatchThreshold)
        {
            return MatchStrength.Strong;
        }

        // Strong: Very high on title with some author match
        if (titleScore >= DomainConstants.Matching.HighTitleThreshold &&
            authorScore >= DomainConstants.Matching.TitleWordMatchRatioThreshold)
        {
            return MatchStrength.Strong;
        }

        // Partial: Good title OR good author match
        if (titleScore >= DomainConstants.Matching.PartialMatchThreshold ||
            authorScore >= DomainConstants.Matching.PartialMatchThreshold)
        {
            return MatchStrength.Partial;
        }

        // Partial: Moderate matches with year confirmation
        if ((titleScore >= DomainConstants.Matching.ModerateMatchThreshold ||
             authorScore >= DomainConstants.Matching.ModerateMatchThreshold) &&
            yearScore >= DomainConstants.Matching.YearConfirmationThreshold)
        {
            return MatchStrength.Partial;
        }

        // Weak: Some keywords matched or moderate title/author
        if (keywordScore >= DomainConstants.Matching.WeakMatchThreshold ||
            titleScore >= DomainConstants.Matching.WeakMatchThreshold ||
            authorScore >= DomainConstants.Matching.WeakMatchThreshold)
        {
            return MatchStrength.Weak;
        }

        return MatchStrength.None;
    }

    private static double GetScore(IReadOnlyDictionary<string, StrategyScore> scores, string strategyName)
    {
        return scores.TryGetValue(strategyName, out var score) ? score.Score : 0;
    }
}
