using System.Net;
using FindThatBook.Domain.Exceptions;
using FindThatBook.Infrastructure.Configuration;
using FindThatBook.Infrastructure.ExternalServices.Gemini;
using FindThatBook.Tests.Common.Fixtures;
using FindThatBook.Tests.Common.Mocks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FindThatBook.Tests.Infrastructure;

public class GeminiExtractionServiceTests
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly GeminiExtractionService _sut;

    public GeminiExtractionServiceTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(_mockHandler)
        {
            BaseAddress = new Uri("https://api.test.com")
        };

        var settings = Options.Create(new GeminiSettings
        {
            BaseUrl = "https://api.test.com",
            Model = "gemini-pro",
            ApiKey = "test-key"
        });

        _sut = new GeminiExtractionService(
            httpClient,
            settings,
            NullLogger<GeminiExtractionService>.Instance);
    }

    [Fact]
    public async Task ExtractBookInfoAsync_WithValidResponse_ReturnsExtractedInfo()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.Gemini.SuccessfulExtraction("The Hobbit", "J.R.R. Tolkien", 1937));

        // Act
        var result = await _sut.ExtractBookInfoAsync("hobbit tolkien");

        // Assert
        result.Title.Should().Be("The Hobbit");
        result.Author.Should().Be("J.R.R. Tolkien");
        result.Year.Should().Be(1937);
        result.OriginalQuery.Should().Be("hobbit tolkien");
    }

    [Fact]
    public async Task ExtractBookInfoAsync_WithKeywords_ExtractsKeywords()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.Gemini.SuccessfulExtraction(
                "The Hobbit", "J.R.R. Tolkien", 1937, "illustrated", "fantasy"));

        // Act
        var result = await _sut.ExtractBookInfoAsync("illustrated hobbit");

        // Assert
        result.Keywords.Should().Contain("illustrated");
        result.Keywords.Should().Contain("fantasy");
    }

    [Fact]
    public async Task ExtractBookInfoAsync_WithMarkdownWrappedJson_ParsesCorrectly()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.Gemini.MarkdownWrappedResponse("The Hobbit", "J.R.R. Tolkien"));

        // Act
        var result = await _sut.ExtractBookInfoAsync("hobbit tolkien");

        // Assert
        result.Title.Should().Be("The Hobbit");
        result.Author.Should().Be("J.R.R. Tolkien");
    }

    [Fact]
    public async Task ExtractBookInfoAsync_WithEmptyResponse_ThrowsAiExtractionException()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(HttpResponseFixtures.Gemini.EmptyResponse());

        // Act
        var act = () => _sut.ExtractBookInfoAsync("hobbit tolkien");

        // Assert - Empty response from API should throw
        await act.Should().ThrowAsync<AiExtractionException>()
            .WithMessage("*Empty response*");
    }

    [Fact]
    public async Task ExtractBookInfoAsync_WithHttpError_ThrowsAiExtractionException()
    {
        // Arrange
        _mockHandler.QueueErrorResponse(HttpStatusCode.InternalServerError, "Server error");

        // Act
        var act = () => _sut.ExtractBookInfoAsync("hobbit");

        // Assert
        await act.Should().ThrowAsync<AiExtractionException>();
    }

    [Fact]
    public async Task ExtractBookInfoAsync_WithUnauthorizedError_ThrowsAiExtractionException()
    {
        // Arrange
        _mockHandler.QueueErrorResponse(HttpStatusCode.Unauthorized, "Invalid API key");

        // Act
        var act = () => _sut.ExtractBookInfoAsync("hobbit");

        // Assert
        await act.Should().ThrowAsync<AiExtractionException>();
    }

    [Fact]
    public async Task ExtractBookInfoAsync_PreservesOriginalQuery()
    {
        // Arrange
        var query = "mark twain huckleberry finn 1884";
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.Gemini.SuccessfulExtraction(
                "The Adventures of Huckleberry Finn", "Mark Twain", 1884));

        // Act
        var result = await _sut.ExtractBookInfoAsync(query);

        // Assert
        result.OriginalQuery.Should().Be(query);
    }

    [Fact]
    public async Task ExtractBookInfoAsync_WithNullYear_HandlesGracefully()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.Gemini.SuccessfulExtraction("The Hobbit", "J.R.R. Tolkien", null));

        // Act
        var result = await _sut.ExtractBookInfoAsync("hobbit");

        // Assert
        result.Year.Should().BeNull();
    }

    [Fact]
    public async Task ExtractBookInfoAsync_MakesCorrectApiCall()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.Gemini.SuccessfulExtraction("Test", "Author"));

        // Act
        await _sut.ExtractBookInfoAsync("test query");

        // Assert
        _mockHandler.ReceivedRequests.Should().HaveCount(1);
        var request = _mockHandler.ReceivedRequests[0];
        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri!.ToString().Should().Contain("gemini-pro");
        request.RequestUri.ToString().Should().Contain("key=test-key");
    }

    [Fact]
    public async Task ExtractBookInfoAsync_WithServiceUnavailable_ThrowsAiExtractionException()
    {
        // Arrange
        _mockHandler.QueueErrorResponse(HttpStatusCode.ServiceUnavailable, "Service unavailable");

        // Act
        var act = () => _sut.ExtractBookInfoAsync("test");

        // Assert
        await act.Should().ThrowAsync<AiExtractionException>();
    }

    [Fact]
    public async Task ExtractBookInfoAsync_WithRateLimitExceeded_ThrowsAiExtractionException()
    {
        // Arrange
        _mockHandler.QueueErrorResponse(HttpStatusCode.TooManyRequests, "Rate limit exceeded");

        // Act
        var act = () => _sut.ExtractBookInfoAsync("test");

        // Assert
        await act.Should().ThrowAsync<AiExtractionException>();
    }

    [Fact]
    public async Task ExtractBookInfoAsync_WithEmptyQuery_StillWorks()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.Gemini.SuccessfulExtraction("Test", "Author"));

        // Act
        var result = await _sut.ExtractBookInfoAsync("");

        // Assert
        result.Should().NotBeNull();
        result.OriginalQuery.Should().Be("");
    }

    [Fact]
    public async Task ExtractBookInfoAsync_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var query = "tolkien's hobbit \"illustrated\"";
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.Gemini.SuccessfulExtraction("The Hobbit", "J.R.R. Tolkien"));

        // Act
        var result = await _sut.ExtractBookInfoAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.OriginalQuery.Should().Be(query);
    }
}
