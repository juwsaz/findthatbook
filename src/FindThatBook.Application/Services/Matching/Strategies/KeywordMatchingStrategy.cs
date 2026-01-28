using FindThatBook.Domain.Constants;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Interfaces.Matching;
using FindThatBook.Domain.ValueObjects;
using FindThatBook.Domain.ValueObjects.Matching;

namespace FindThatBook.Application.Services.Matching.Strategies;

/// <summary>
/// Strategy for matching books by keywords.
/// </summary>
public class KeywordMatchingStrategy : IMatchingStrategy
{
    private readonly ITextNormalizer _normalizer;

    public KeywordMatchingStrategy(ITextNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public string StrategyName => "Keyword";

    public double Weight => DomainConstants.Matching.KeywordWeight;

    public bool CanEvaluate(ExtractedBookInfo extractedInfo)
    {
        return extractedInfo.HasKeywords;
    }

    public StrategyScore Evaluate(Book book, ExtractedBookInfo extractedInfo)
    {
        if (!CanEvaluate(extractedInfo))
            return StrategyScore.NoMatch(Weight);

        var keywords = extractedInfo.Keywords;
        var matchedKeywords = new List<string>();

        var searchableText = _normalizer.Normalize(
            $"{book.Title} {string.Join(" ", book.Authors)} {string.Join(" ", book.Subjects)}");

        foreach (var keyword in keywords)
        {
            var normalizedKeyword = _normalizer.Normalize(keyword);
            if (searchableText.Contains(normalizedKeyword))
            {
                matchedKeywords.Add(keyword);
            }
        }

        if (matchedKeywords.Count > 0)
        {
            var ratio = (double)matchedKeywords.Count / keywords.Count;
            return StrategyScore.Create(
                ratio,
                Weight,
                $"Keywords matched: {string.Join(", ", matchedKeywords)}");
        }

        return StrategyScore.NoMatch(Weight);
    }
}
