using FindThatBook.Domain.Constants;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Interfaces.Matching;
using FindThatBook.Domain.ValueObjects;
using FindThatBook.Domain.ValueObjects.Matching;

namespace FindThatBook.Application.Services.Matching.Strategies;

/// <summary>
/// Strategy for matching books by author.
/// </summary>
public class AuthorMatchingStrategy : IMatchingStrategy
{
    private readonly ITextNormalizer _normalizer;

    public AuthorMatchingStrategy(ITextNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public string StrategyName => "Author";

    public double Weight => DomainConstants.Matching.AuthorWeight;

    public bool CanEvaluate(ExtractedBookInfo extractedInfo)
    {
        return extractedInfo.HasAuthor;
    }

    public StrategyScore Evaluate(Book book, ExtractedBookInfo extractedInfo)
    {
        if (!CanEvaluate(extractedInfo) || book.Authors.Count == 0)
            return StrategyScore.NoMatch(Weight);

        var normalizedSearchAuthor = _normalizer.Normalize(extractedInfo.Author!);
        var searchAuthorParts = normalizedSearchAuthor.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var author in book.Authors)
        {
            var score = EvaluateAuthor(author, normalizedSearchAuthor, searchAuthorParts);
            if (score.HasMatch)
                return score;
        }

        return StrategyScore.NoMatch(Weight);
    }

    private StrategyScore EvaluateAuthor(string author, string normalizedSearchAuthor, string[] searchAuthorParts)
    {
        var normalizedAuthor = _normalizer.Normalize(author);
        var authorParts = normalizedAuthor.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Exact match
        if (normalizedAuthor == normalizedSearchAuthor)
        {
            return StrategyScore.Create(
                DomainConstants.Matching.AuthorExactScore,
                Weight,
                $"Author exact match: \"{author}\"");
        }

        // Last name match (common search pattern)
        var lastNameMatch = authorParts.Any(ap =>
            searchAuthorParts.Any(sp => ap == sp && ap.Length > DomainConstants.Matching.AuthorMinPartLength));

        if (lastNameMatch)
        {
            // Check if first name/initial also matches
            var firstNameMatch = authorParts.Any(ap =>
                searchAuthorParts.Any(sp =>
                    (ap.StartsWith(sp) || sp.StartsWith(ap)) && ap != authorParts.Last()));

            if (firstNameMatch || searchAuthorParts.Length == 1)
            {
                var score = searchAuthorParts.Length == 1
                    ? DomainConstants.Matching.AuthorLastNameOnlyScore
                    : DomainConstants.Matching.AuthorLastNameWithFirstScore;

                return StrategyScore.Create(score, Weight, $"Author match: \"{author}\"");
            }
        }

        // Partial match - any part matches
        var matchedParts = searchAuthorParts.Count(sp =>
            authorParts.Any(ap => ap.Contains(sp) || sp.Contains(ap)));

        if (matchedParts > 0)
        {
            var ratio = (double)matchedParts / searchAuthorParts.Length;
            if (ratio >= DomainConstants.Matching.AuthorPartialRatioThreshold)
            {
                var score = DomainConstants.Matching.AuthorPartialBaseScore +
                           (ratio * DomainConstants.Matching.AuthorPartialMultiplier);

                return StrategyScore.Create(score, Weight, $"Author partial match: \"{author}\"");
            }
        }

        return StrategyScore.NoMatch(Weight);
    }
}
