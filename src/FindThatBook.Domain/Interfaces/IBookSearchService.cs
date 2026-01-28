using FindThatBook.Domain.Entities;
using FindThatBook.Domain.ValueObjects;

namespace FindThatBook.Domain.Interfaces;

/// <summary>
/// Interface for searching books in external library APIs.
/// </summary>
public interface IBookSearchService
{
    /// <summary>
    /// Searches for books based on extracted book information.
    /// </summary>
    /// <param name="bookInfo">The extracted book information.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching books.</returns>
    Task<List<Book>> SearchBooksAsync(ExtractedBookInfo bookInfo, int maxResults = 20, CancellationToken cancellationToken = default);
}
