namespace FindThatBook.Domain.ValueObjects;

/// <summary>
/// Represents the extracted book information from a dirty query using AI.
/// </summary>
public record ExtractedBookInfo
{
    public string? Title { get; init; }
    public string? Author { get; init; }
    public List<string> Keywords { get; init; } = new();
    public int? Year { get; init; }
    public string? OriginalQuery { get; init; }

    public bool HasTitle => !string.IsNullOrWhiteSpace(Title);
    public bool HasAuthor => !string.IsNullOrWhiteSpace(Author);
    public bool HasKeywords => Keywords.Count > 0;
}
