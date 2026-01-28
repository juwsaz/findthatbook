using FindThatBook.Domain.Exceptions;

namespace FindThatBook.Domain.Guards;

/// <summary>
/// Guard clauses for input validation.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Throws ValidationException if the string is null or empty.
    /// </summary>
    public static void AgainstNullOrEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw ValidationException.NullOrEmpty(parameterName);
        }
    }

    /// <summary>
    /// Throws ValidationException if the value is outside the specified range.
    /// </summary>
    public static void AgainstOutOfRange(int value, int min, int max, string parameterName)
    {
        if (value < min || value > max)
        {
            throw ValidationException.OutOfRange(parameterName, min, max, value);
        }
    }

    /// <summary>
    /// Throws ArgumentNullException if the value is null.
    /// </summary>
    public static T AgainstNull<T>(T? value, string parameterName) where T : class
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        return value;
    }

    /// <summary>
    /// Throws ValidationException if the collection is null or empty.
    /// </summary>
    public static void AgainstNullOrEmptyCollection<T>(ICollection<T>? collection, string parameterName)
    {
        if (collection == null || collection.Count == 0)
        {
            throw ValidationException.NullOrEmpty(parameterName);
        }
    }
}
