using System.Diagnostics;
using FindThatBook.Application.DTOs;
using FindThatBook.Domain.Interfaces;

namespace FindThatBook.Application.UseCases;

/// <summary>
/// Use case for searching books based on a dirty query.
/// Orchestrates AI extraction, book search, and matching.
/// </summary>
public class SearchBooksUseCase
{
    private readonly IAiExtractionService _aiExtractionService;
    private readonly IBookSearchService _bookSearchService;
    private readonly IBookMatchingService _bookMatchingService;

    public SearchBooksUseCase(
        IAiExtractionService aiExtractionService,
        IBookSearchService bookSearchService,
        IBookMatchingService bookMatchingService)
    {
        _aiExtractionService = aiExtractionService;
        _bookSearchService = bookSearchService;
        _bookMatchingService = bookMatchingService;
    }

    public async Task<BookSearchResponse> ExecuteAsync(BookSearchRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // Step 1: Extract book info from dirty query using AI
        var extractedInfo = await _aiExtractionService.ExtractBookInfoAsync(request.Query, cancellationToken);

        // Step 2: Search for books using extracted info
        var books = await _bookSearchService.SearchBooksAsync(extractedInfo, maxResults: 20, cancellationToken);

        // Step 3: Rank and match books
        var candidates = _bookMatchingService.RankBooks(books, extractedInfo, request.MaxResults);

        stopwatch.Stop();

        // Map to response DTO
        return new BookSearchResponse
        {
            OriginalQuery = request.Query,
            ExtractedInfo = new ExtractedInfoDto
            {
                Title = extractedInfo.Title,
                Author = extractedInfo.Author,
                Keywords = extractedInfo.Keywords,
                Year = extractedInfo.Year
            },
            Candidates = candidates.Select(c => new BookCandidateDto
            {
                Key = c.Book.Key,
                Title = c.Book.Title,
                Authors = c.Book.Authors,
                FirstPublishYear = c.Book.FirstPublishYear,
                CoverUrl = c.Book.CoverUrl,
                OpenLibraryUrl = c.Book.OpenLibraryUrl,
                MatchStrength = c.MatchStrength.ToString(),
                MatchScore = c.MatchScore,
                MatchExplanation = c.MatchExplanation,
                MatchReasons = c.MatchReasons
            }).ToList(),
            TotalCandidates = candidates.Count,
            ProcessingTime = stopwatch.Elapsed
        };
    }
}
