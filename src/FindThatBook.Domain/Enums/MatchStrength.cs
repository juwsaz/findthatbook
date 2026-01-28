namespace FindThatBook.Domain.Enums;

/// <summary>
/// Represents the strength of a book match based on the matching criteria hierarchy.
/// </summary>
public enum MatchStrength
{
    /// <summary>
    /// No significant match found.
    /// </summary>
    None = 0,

    /// <summary>
    /// Weak match - only some keywords matched.
    /// </summary>
    Weak = 1,

    /// <summary>
    /// Partial match - either title or author matched, but not both.
    /// </summary>
    Partial = 2,

    /// <summary>
    /// Strong match - both title and author matched with high confidence.
    /// </summary>
    Strong = 3,

    /// <summary>
    /// Exact match - title and author matched exactly.
    /// </summary>
    Exact = 4
}
