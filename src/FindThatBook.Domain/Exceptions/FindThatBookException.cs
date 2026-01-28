namespace FindThatBook.Domain.Exceptions;

/// <summary>
/// Base exception for all FindThatBook domain exceptions.
/// </summary>
public abstract class FindThatBookException : Exception
{
    public string ErrorCode { get; }

    protected FindThatBookException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    protected FindThatBookException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
