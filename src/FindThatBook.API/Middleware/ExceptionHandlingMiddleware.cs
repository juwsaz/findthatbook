using System.Net;
using System.Text.Json;
using FindThatBook.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FindThatBook.API.Middleware;

/// <summary>
/// Global exception handling middleware that converts exceptions to ProblemDetails responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                CreateProblemDetails(
                    "Validation Error",
                    validationEx.Message,
                    StatusCodes.Status400BadRequest,
                    validationEx.ErrorCode,
                    new Dictionary<string, object> { ["property"] = validationEx.PropertyName })),

            AiExtractionException aiEx => (
                HttpStatusCode.ServiceUnavailable,
                CreateProblemDetails(
                    "AI Service Unavailable",
                    "The AI extraction service is temporarily unavailable. Please try again later.",
                    StatusCodes.Status503ServiceUnavailable,
                    aiEx.ErrorCode)),

            BookSearchException searchEx => (
                HttpStatusCode.BadGateway,
                CreateProblemDetails(
                    "Book Search Failed",
                    "Unable to search for books at this time. Please try again later.",
                    StatusCodes.Status502BadGateway,
                    searchEx.ErrorCode)),

            ArgumentNullException argNullEx => (
                HttpStatusCode.BadRequest,
                CreateProblemDetails(
                    "Invalid Request",
                    $"Missing required parameter: {argNullEx.ParamName}",
                    StatusCodes.Status400BadRequest,
                    "ARGUMENT_NULL")),

            _ => (
                HttpStatusCode.InternalServerError,
                CreateProblemDetails(
                    "Internal Server Error",
                    "An unexpected error occurred. Please try again later.",
                    StatusCodes.Status500InternalServerError,
                    "INTERNAL_ERROR"))
        };

        LogException(exception, statusCode);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
    }

    private static ProblemDetails CreateProblemDetails(
        string title,
        string detail,
        int status,
        string errorCode,
        IDictionary<string, object>? extensions = null)
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = status,
            Type = $"https://findthatbook.api/errors/{errorCode.ToLowerInvariant()}"
        };

        problemDetails.Extensions["errorCode"] = errorCode;

        if (extensions != null)
        {
            foreach (var extension in extensions)
            {
                problemDetails.Extensions[extension.Key] = extension.Value;
            }
        }

        return problemDetails;
    }

    private void LogException(Exception exception, HttpStatusCode statusCode)
    {
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);
        }
    }
}

/// <summary>
/// Extension methods for registering exception handling middleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
