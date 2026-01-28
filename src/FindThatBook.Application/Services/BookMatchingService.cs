using FindThatBook.Application.Services.Matching;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Enums;
using FindThatBook.Domain.Interfaces;
using FindThatBook.Domain.ValueObjects;

namespace FindThatBook.Application.Services;

/// <summary>
/// Service for matching and ranking books based on extracted information.
/// Uses the Strategy Pattern to delegate matching logic to individual strategies.
/// </summary>
public class BookMatchingService : IBookMatchingService
{
    private readonly MatchingStrategyRegistry _strategyRegistry;

    public BookMatchingService(MatchingStrategyRegistry strategyRegistry)
    {
        _strategyRegistry = strategyRegistry;
    }

    public List<BookCandidate> RankBooks(List<Book> books, ExtractedBookInfo extractedInfo, int maxCandidates = 5)
    {
        return books
            .Select(book => EvaluateBook(book, extractedInfo))
            .Where(c => c.MatchStrength != MatchStrength.None)
            .OrderByDescending(c => c.MatchStrength)
            .ThenByDescending(c => c.MatchScore)
            .Take(maxCandidates)
            .ToList();
    }

    private BookCandidate EvaluateBook(Book book, ExtractedBookInfo extractedInfo)
    {
        var result = _strategyRegistry.Evaluate(book, extractedInfo);

        return new BookCandidate
        {
            Book = book,
            MatchStrength = result.MatchStrength,
            MatchScore = result.TotalScore,
            MatchReasons = result.Reasons.ToList()
        };
    }
}
