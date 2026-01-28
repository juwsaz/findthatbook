using FindThatBook.Application.DTOs;
using FindThatBook.Application.UseCases;
using FindThatBook.Domain.Constants;
using FindThatBook.Domain.Guards;
using Microsoft.AspNetCore.Mvc;

namespace FindThatBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly SearchBooksUseCase _searchBooksUseCase;
    private readonly ILogger<BooksController> _logger;

    public BooksController(SearchBooksUseCase searchBooksUseCase, ILogger<BooksController> logger)
    {
        _searchBooksUseCase = searchBooksUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Searches for books based on a dirty/unstructured query.
    /// Uses AI to extract title, author, and keywords, then searches Open Library.
    /// </summary>
    /// <param name="request">The search request containing the query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Up to 5 book candidates with match explanations.</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(BookSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookSearchResponse>> SearchBooks(
        [FromBody] BookSearchRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Guard.AgainstNullOrEmpty(request.Query, nameof(request.Query));
        Guard.AgainstOutOfRange(
            request.MaxResults,
            DomainConstants.Api.MinMaxResults,
            DomainConstants.Api.MaxMaxResults,
            nameof(request.MaxResults));

        _logger.LogInformation("Received search request: {Query}", request.Query);

        var result = await _searchBooksUseCase.ExecuteAsync(request, cancellationToken);

        _logger.LogInformation(
            "Search completed: found {Count} candidates in {Time}ms",
            result.TotalCandidates,
            result.ProcessingTime.TotalMilliseconds);

        return Ok(result);
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
