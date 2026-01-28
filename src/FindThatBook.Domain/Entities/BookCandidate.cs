using FindThatBook.Domain.Enums;

namespace FindThatBook.Domain.Entities;

/// <summary>
/// Represents a book candidate with match information and explanation.
/// </summary>
public class BookCandidate
{
    public Book Book { get; set; } = new();
    public MatchStrength MatchStrength { get; set; }
    public double MatchScore { get; set; }
    public List<string> MatchReasons { get; set; } = new();

    public string MatchExplanation => MatchStrength switch
    {
        MatchStrength.Exact => $"Exact match: {string.Join("; ", MatchReasons)}",
        MatchStrength.Strong => $"Strong match: {string.Join("; ", MatchReasons)}",
        MatchStrength.Partial => $"Partial match: {string.Join("; ", MatchReasons)}",
        MatchStrength.Weak => $"Weak match: {string.Join("; ", MatchReasons)}",
        _ => "No significant match found"
    };
}
