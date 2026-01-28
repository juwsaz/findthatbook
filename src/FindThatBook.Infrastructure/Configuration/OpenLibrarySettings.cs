namespace FindThatBook.Infrastructure.Configuration;

public class OpenLibrarySettings
{
    public const string SectionName = "OpenLibrary";

    public string BaseUrl { get; set; } = "https://openlibrary.org";
    public int TimeoutSeconds { get; set; } = 30;
}
