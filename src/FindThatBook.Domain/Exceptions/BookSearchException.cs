namespace FindThatBook.Domain.Exceptions;

/// <summary>
/// Exception thrown when book search service fails.
/// </summary>
public class BookSearchException : FindThatBookException
{
    private const string DefaultErrorCode = "BOOK_SEARCH_FAILED";

    public BookSearchException(string message)
        : base(message, DefaultErrorCode)
    {
    }

    public BookSearchException(string message, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
    }

    public static BookSearchException ApiCallFailed(string serviceName, Exception innerException)
        => new($"Failed to call {serviceName} API for book search", innerException);

    public static BookSearchException InvalidResponse(string reason)
        => new($"Invalid search response: {reason}");

    public static BookSearchException ServiceUnavailable(string serviceName)
        => new($"{serviceName} service is currently unavailable");
}
