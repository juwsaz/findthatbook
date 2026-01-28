namespace FindThatBook.Domain.Exceptions;

/// <summary>
/// Exception thrown when AI extraction service fails.
/// </summary>
public class AiExtractionException : FindThatBookException
{
    private const string DefaultErrorCode = "AI_EXTRACTION_FAILED";

    public AiExtractionException(string message)
        : base(message, DefaultErrorCode)
    {
    }

    public AiExtractionException(string message, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
    }

    public static AiExtractionException ApiCallFailed(string serviceName, Exception innerException)
        => new($"Failed to call {serviceName} API for book extraction", innerException);

    public static AiExtractionException InvalidResponse(string reason)
        => new($"Invalid AI response: {reason}");

    public static AiExtractionException ParsingFailed(string content, Exception innerException)
        => new($"Failed to parse AI response: {content}", innerException);
}
