using FindThatBook.Domain.Constants;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Interfaces.Matching;
using FindThatBook.Domain.ValueObjects;
using FindThatBook.Domain.ValueObjects.Matching;

namespace FindThatBook.Application.Services.Matching.Strategies;

/// <summary>
/// Strategy for matching books by publication year.
/// </summary>
public class YearMatchingStrategy : IMatchingStrategy
{
    public string StrategyName => "Year";

    public double Weight => DomainConstants.Matching.YearWeight;

    public bool CanEvaluate(ExtractedBookInfo extractedInfo)
    {
        return extractedInfo.Year.HasValue;
    }

    public StrategyScore Evaluate(Book book, ExtractedBookInfo extractedInfo)
    {
        if (!CanEvaluate(extractedInfo) || !book.FirstPublishYear.HasValue)
            return StrategyScore.NoMatch(Weight);

        var bookYear = book.FirstPublishYear.Value;
        var searchYear = extractedInfo.Year!.Value;
        var yearDiff = Math.Abs(bookYear - searchYear);

        if (yearDiff == 0)
        {
            return StrategyScore.Create(
                DomainConstants.Matching.YearExactScore,
                Weight,
                $"Year exact match: {bookYear}");
        }

        if (yearDiff <= DomainConstants.Matching.YearCloseRange)
        {
            return StrategyScore.Create(
                DomainConstants.Matching.YearCloseScore,
                Weight,
                $"Year close match: {bookYear} (searched: {searchYear})");
        }

        if (yearDiff <= DomainConstants.Matching.YearApproximateRange)
        {
            return StrategyScore.Create(
                DomainConstants.Matching.YearApproximateScore,
                Weight,
                $"Year approximate match: {bookYear}");
        }

        return StrategyScore.NoMatch(Weight);
    }
}
