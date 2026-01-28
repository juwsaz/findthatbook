namespace FindThatBook.Domain.Interfaces.Matching;

/// <summary>
/// Normalizes text for comparison in matching operations.
/// </summary>
public interface ITextNormalizer
{
    /// <summary>
    /// Normalizes text by removing punctuation and converting to lowercase.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <returns>The normalized text.</returns>
    string Normalize(string text);

    /// <summary>
    /// Splits normalized text into words.
    /// </summary>
    /// <param name="text">The text to split.</param>
    /// <returns>Array of words.</returns>
    string[] SplitIntoWords(string text);
}
