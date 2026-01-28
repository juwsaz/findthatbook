using FindThatBook.Domain.Interfaces.Matching;

namespace FindThatBook.Application.Services.Matching;

/// <summary>
/// Normalizes text for comparison in matching operations.
/// </summary>
public class TextNormalizer : ITextNormalizer
{
    /// <inheritdoc />
    public string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return text.ToLowerInvariant()
            .Replace(".", "")
            .Replace(",", "")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("-", " ")
            .Replace("_", " ")
            .Trim();
    }

    /// <inheritdoc />
    public string[] SplitIntoWords(string text)
    {
        var normalized = Normalize(text);
        return normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}
