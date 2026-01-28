namespace FindThatBook.Domain.Entities;

/// <summary>
/// Represents a book from the Open Library API.
/// </summary>
public class Book
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<string> Authors { get; set; } = new();
    public int? FirstPublishYear { get; set; }
    public string? CoverId { get; set; }
    public List<string> Subjects { get; set; } = new();
    public List<string> Publishers { get; set; } = new();
    public string? Isbn { get; set; }
    public int? NumberOfPages { get; set; }
    public List<string> Languages { get; set; } = new();

    public string CoverUrl => !string.IsNullOrEmpty(CoverId)
        ? $"https://covers.openlibrary.org/b/id/{CoverId}-M.jpg"
        : string.Empty;

    public string OpenLibraryUrl => $"https://openlibrary.org{Key}";
}
