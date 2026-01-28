using FindThatBook.Domain.Constants;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Interfaces.Matching;
using FindThatBook.Domain.ValueObjects;
using FindThatBook.Domain.ValueObjects.Matching;

namespace FindThatBook.Application.Services.Matching.Strategies;

/// <summary>
/// Strategy for matching books by title.
/// </summary>
public class TitleMatchingStrategy : IMatchingStrategy
{
    private readonly ITextNormalizer _normalizer;

    public TitleMatchingStrategy(ITextNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public string StrategyName => "Title";

    public double Weight => DomainConstants.Matching.TitleWeight;

    public bool CanEvaluate(ExtractedBookInfo extractedInfo)
    {
        return extractedInfo.HasTitle;
    }

    public StrategyScore Evaluate(Book book, ExtractedBookInfo extractedInfo)
    {
        if (!CanEvaluate(extractedInfo))
            return StrategyScore.NoMatch(Weight);

        var normalizedBookTitle = _normalizer.Normalize(book.Title);
        var normalizedSearchTitle = _normalizer.Normalize(extractedInfo.Title!);

        // Exact match
        if (normalizedBookTitle == normalizedSearchTitle)
        {
            return StrategyScore.Create(
                DomainConstants.Matching.TitleExactScore,
                Weight,
                $"Title exact match: \"{book.Title}\"");
        }

        // Contains full search title
        if (normalizedBookTitle.Contains(normalizedSearchTitle))
        {
            return StrategyScore.Create(
                DomainConstants.Matching.TitleContainsScore,
                Weight,
                $"Title contains search term: \"{extractedInfo.Title}\"");
        }

        // Search title contains book title
        if (normalizedSearchTitle.Contains(normalizedBookTitle))
        {
            return StrategyScore.Create(
                DomainConstants.Matching.TitleContainedScore,
                Weight,
                $"Search term contains title: \"{book.Title}\"");
        }

        // Word-level matching
        return EvaluateWordMatch(book.Title, normalizedBookTitle, normalizedSearchTitle);
    }

    private StrategyScore EvaluateWordMatch(string originalTitle, string normalizedBookTitle, string normalizedSearchTitle)
    {
        var searchWords = normalizedSearchTitle.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleWords = normalizedBookTitle.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var matchedWords = searchWords.Count(sw =>
            titleWords.Any(tw => tw.Contains(sw) || sw.Contains(tw)));

        if (matchedWords > 0)
        {
            var wordMatchRatio = (double)matchedWords / searchWords.Length;

            if (wordMatchRatio >= DomainConstants.Matching.TitleWordMatchRatioThreshold)
            {
                var score = DomainConstants.Matching.TitleWordMatchBaseScore +
                           (wordMatchRatio * DomainConstants.Matching.TitleWordMatchMultiplier);

                return StrategyScore.Create(
                    score,
                    Weight,
                    $"Title word match: {matchedWords}/{searchWords.Length} words matched");
            }

            return StrategyScore.Create(
                wordMatchRatio * DomainConstants.Matching.TitleLowWordMatchMultiplier,
                Weight,
                $"Title partial word match: {matchedWords}/{searchWords.Length} words");
        }

        return StrategyScore.NoMatch(Weight);
    }
}
