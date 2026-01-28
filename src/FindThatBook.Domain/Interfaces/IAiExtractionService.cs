using FindThatBook.Domain.ValueObjects;

namespace FindThatBook.Domain.Interfaces;

/// <summary>
/// Interface for AI-based extraction of book information from dirty queries.
/// </summary>
public interface IAiExtractionService
{
    /// <summary>
    /// Extracts structured book information from a dirty text query.
    /// </summary>
    /// <param name="dirtyQuery">The unstructured search query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Extracted book information.</returns>
    Task<ExtractedBookInfo> ExtractBookInfoAsync(string dirtyQuery, CancellationToken cancellationToken = default);
}
