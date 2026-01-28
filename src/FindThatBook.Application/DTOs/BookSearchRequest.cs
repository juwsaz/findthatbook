using System.ComponentModel.DataAnnotations;

namespace FindThatBook.Application.DTOs;

/// <summary>
/// Request DTO for book search.
/// </summary>
public record BookSearchRequest
{
    /// <summary>
    /// The dirty/unstructured search query (e.g., "mark huckleberry", "tolkien hobbit illustrated 1937").
    /// </summary>
    [Required]
    [MinLength(2, ErrorMessage = "Query must be at least 2 characters long.")]
    [MaxLength(500, ErrorMessage = "Query cannot exceed 500 characters.")]
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Maximum number of candidates to return (1-10).
    /// </summary>
    [Range(1, 10)]
    public int MaxResults { get; init; } = 5;
}
