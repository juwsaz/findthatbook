namespace FindThatBook.Application.DTOs;

/// <summary>
/// Response DTO for book search results.
/// </summary>
public record BookSearchResponse
{
    public string OriginalQuery { get; init; } = string.Empty;
    public ExtractedInfoDto ExtractedInfo { get; init; } = new();
    public List<BookCandidateDto> Candidates { get; init; } = new();
    public int TotalCandidates { get; init; }
    public TimeSpan ProcessingTime { get; init; }
}

/// <summary>
/// DTO for extracted book information.
/// </summary>
public record ExtractedInfoDto
{
    public string? Title { get; init; }
    public string? Author { get; init; }
    public List<string> Keywords { get; init; } = new();
    public int? Year { get; init; }
}

/// <summary>
/// DTO for a book candidate with match information.
/// </summary>
public record BookCandidateDto
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public List<string> Authors { get; init; } = new();
    public int? FirstPublishYear { get; init; }
    public string? CoverUrl { get; init; }
    public string OpenLibraryUrl { get; init; } = string.Empty;
    public string MatchStrength { get; init; } = string.Empty;
    public double MatchScore { get; init; }
    public string MatchExplanation { get; init; } = string.Empty;
    public List<string> MatchReasons { get; init; } = new();
}
