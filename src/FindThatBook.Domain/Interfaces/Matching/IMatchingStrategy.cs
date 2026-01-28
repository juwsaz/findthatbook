using FindThatBook.Domain.Entities;
using FindThatBook.Domain.ValueObjects;
using FindThatBook.Domain.ValueObjects.Matching;

namespace FindThatBook.Domain.Interfaces.Matching;

/// <summary>
/// Defines a strategy for matching books against extracted information.
/// </summary>
public interface IMatchingStrategy
{
    /// <summary>
    /// Gets the unique name identifying this strategy.
    /// </summary>
    string StrategyName { get; }

    /// <summary>
    /// Gets the weight of this strategy in the overall score (0.0 to 1.0).
    /// </summary>
    double Weight { get; }

    /// <summary>
    /// Evaluates the match between a book and extracted information.
    /// </summary>
    /// <param name="book">The book to evaluate.</param>
    /// <param name="extractedInfo">The extracted book information from the query.</param>
    /// <returns>A strategy score representing the match quality.</returns>
    StrategyScore Evaluate(Book book, ExtractedBookInfo extractedInfo);

    /// <summary>
    /// Determines if this strategy can evaluate the given extracted information.
    /// </summary>
    /// <param name="extractedInfo">The extracted information to check.</param>
    /// <returns>True if the strategy has the required data to evaluate.</returns>
    bool CanEvaluate(ExtractedBookInfo extractedInfo);
}
