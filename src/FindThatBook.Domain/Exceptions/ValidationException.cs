namespace FindThatBook.Domain.Exceptions;

/// <summary>
/// Exception thrown when input validation fails.
/// </summary>
public class ValidationException : FindThatBookException
{
    private const string DefaultErrorCode = "VALIDATION_FAILED";

    public string PropertyName { get; }

    public ValidationException(string message, string propertyName)
        : base(message, DefaultErrorCode)
    {
        PropertyName = propertyName;
    }

    public static ValidationException NullOrEmpty(string propertyName)
        => new($"{propertyName} cannot be null or empty", propertyName);

    public static ValidationException OutOfRange(string propertyName, int min, int max, int actual)
        => new($"{propertyName} must be between {min} and {max}, but was {actual}", propertyName);
}
