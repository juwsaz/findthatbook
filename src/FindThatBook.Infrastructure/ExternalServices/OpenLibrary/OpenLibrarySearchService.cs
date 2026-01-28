using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Web;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Exceptions;
using FindThatBook.Domain.Interfaces;
using FindThatBook.Domain.ValueObjects;
using FindThatBook.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FindThatBook.Infrastructure.ExternalServices.OpenLibrary;

public class OpenLibrarySearchService : IBookSearchService
{
    private readonly HttpClient _httpClient;
    private readonly OpenLibrarySettings _settings;
    private readonly ILogger<OpenLibrarySearchService> _logger;

    public OpenLibrarySearchService(
        HttpClient httpClient,
        IOptions<OpenLibrarySettings> settings,
        ILogger<OpenLibrarySearchService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<List<Book>> SearchBooksAsync(
        ExtractedBookInfo bookInfo,
        int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        var searchQueries = BuildSearchQueries(bookInfo);
        var allBooks = new List<Book>();
        var seenKeys = new HashSet<string>();

        foreach (var query in searchQueries)
        {
            if (allBooks.Count >= maxResults)
                break;

            try
            {
                var books = await ExecuteSearchAsync(query, maxResults - allBooks.Count, cancellationToken);

                foreach (var book in books)
                {
                    if (seenKeys.Add(book.Key))
                    {
                        allBooks.Add(book);
                    }
                }
            }
            catch (BookSearchException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "HTTP error for search query: {Query}", query);
                throw BookSearchException.ApiCallFailed("Open Library", ex);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Search query failed: {Query}", query);
            }
        }

        _logger.LogInformation("Found {Count} books for query: {Query}", allBooks.Count, bookInfo.OriginalQuery);
        return allBooks;
    }

    private List<string> BuildSearchQueries(ExtractedBookInfo bookInfo)
    {
        var queries = new List<string>();

        // Strategy 1: Combined title and author search
        if (bookInfo.HasTitle && bookInfo.HasAuthor)
        {
            queries.Add($"title={Encode(bookInfo.Title!)}&author={Encode(bookInfo.Author!)}");
        }

        // Strategy 2: Title only search
        if (bookInfo.HasTitle)
        {
            queries.Add($"title={Encode(bookInfo.Title!)}");
        }

        // Strategy 3: Author only search
        if (bookInfo.HasAuthor)
        {
            queries.Add($"author={Encode(bookInfo.Author!)}");
        }

        // Strategy 4: General search with keywords
        if (bookInfo.HasKeywords)
        {
            var keywordQuery = string.Join(" ", bookInfo.Keywords);
            queries.Add($"q={Encode(keywordQuery)}");
        }

        // Strategy 5: Fallback to original query
        if (!string.IsNullOrWhiteSpace(bookInfo.OriginalQuery) && queries.Count == 0)
        {
            queries.Add($"q={Encode(bookInfo.OriginalQuery)}");
        }

        return queries;
    }

    private async Task<List<Book>> ExecuteSearchAsync(string queryParams, int limit, CancellationToken cancellationToken)
    {
        var url = $"{_settings.BaseUrl}/search.json?{queryParams}&limit={limit}&fields=key,title,author_name,first_publish_year,cover_i,subject,publisher,isbn,number_of_pages_median,language";

        _logger.LogDebug("Executing Open Library search: {Url}", url);

        try
        {
            var response = await _httpClient.GetFromJsonAsync<OpenLibrarySearchResponse>(url, cancellationToken);

            if (response?.Docs == null)
                return new List<Book>();

            return response.Docs.Select(MapToBook).ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error executing search: {Url}", url);
            throw BookSearchException.ApiCallFailed("Open Library", ex);
        }
    }

    private static Book MapToBook(OpenLibraryDoc doc)
    {
        return new Book
        {
            Key = doc.Key ?? string.Empty,
            Title = doc.Title ?? "Unknown Title",
            Authors = doc.AuthorName?.ToList() ?? new List<string>(),
            FirstPublishYear = doc.FirstPublishYear,
            CoverId = doc.CoverId?.ToString(),
            Subjects = doc.Subject?.Take(10).ToList() ?? new List<string>(),
            Publishers = doc.Publisher?.Take(5).ToList() ?? new List<string>(),
            Isbn = doc.Isbn?.FirstOrDefault(),
            NumberOfPages = doc.NumberOfPagesMedian,
            Languages = doc.Language?.ToList() ?? new List<string>()
        };
    }

    private static string Encode(string value) => HttpUtility.UrlEncode(value);

    #region Open Library API Models

    private class OpenLibrarySearchResponse
    {
        [JsonPropertyName("numFound")]
        public int NumFound { get; set; }

        [JsonPropertyName("docs")]
        public List<OpenLibraryDoc>? Docs { get; set; }
    }

    private class OpenLibraryDoc
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("author_name")]
        public string[]? AuthorName { get; set; }

        [JsonPropertyName("first_publish_year")]
        public int? FirstPublishYear { get; set; }

        [JsonPropertyName("cover_i")]
        public int? CoverId { get; set; }

        [JsonPropertyName("subject")]
        public string[]? Subject { get; set; }

        [JsonPropertyName("publisher")]
        public string[]? Publisher { get; set; }

        [JsonPropertyName("isbn")]
        public string[]? Isbn { get; set; }

        [JsonPropertyName("number_of_pages_median")]
        public int? NumberOfPagesMedian { get; set; }

        [JsonPropertyName("language")]
        public string[]? Language { get; set; }
    }

    #endregion
}
