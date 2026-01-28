using FindThatBook.Domain.Entities;
using FindThatBook.Domain.ValueObjects;

namespace FindThatBook.Domain.Interfaces;

/// <summary>
/// Interface for matching and ranking books based on extracted information.
/// </summary>
public interface IBookMatchingService
{
    /// <summary>
    /// Evaluates and ranks books against the extracted book information.
    /// </summary>
    /// <param name="books">List of books to evaluate.</param>
    /// <param name="extractedInfo">The extracted book information to match against.</param>
    /// <param name="maxCandidates">Maximum number of candidates to return.</param>
    /// <returns>Ranked list of book candidates with match explanations.</returns>
    List<BookCandidate> RankBooks(List<Book> books, ExtractedBookInfo extractedInfo, int maxCandidates = 5);
}
