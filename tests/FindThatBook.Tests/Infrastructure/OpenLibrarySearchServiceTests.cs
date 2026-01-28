using System.Net;
using FindThatBook.Domain.Exceptions;
using FindThatBook.Domain.ValueObjects;
using FindThatBook.Infrastructure.Configuration;
using FindThatBook.Infrastructure.ExternalServices.OpenLibrary;
using FindThatBook.Tests.Common.Builders;
using FindThatBook.Tests.Common.Fixtures;
using FindThatBook.Tests.Common.Mocks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FindThatBook.Tests.Infrastructure;

public class OpenLibrarySearchServiceTests
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly OpenLibrarySearchService _sut;

    public OpenLibrarySearchServiceTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(_mockHandler)
        {
            BaseAddress = new Uri("https://openlibrary.org")
        };

        var settings = Options.Create(new OpenLibrarySettings
        {
            BaseUrl = "https://openlibrary.org",
            TimeoutSeconds = 30
        });

        _sut = new OpenLibrarySearchService(
            httpClient,
            settings,
            NullLogger<OpenLibrarySearchService>.Instance);
    }

    [Fact]
    public async Task SearchBooksAsync_WithValidResponse_ReturnsBooks()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.OpenLibrary.SingleBookResponse(
                "/works/OL27479W", "The Hobbit", "J.R.R. Tolkien", 1937));

        var bookInfo = ExtractedBookInfoBuilder.Default()
            .WithTitle("The Hobbit")
            .WithAuthor("J.R.R. Tolkien")
            .Build();

        // Act
        var result = await _sut.SearchBooksAsync(bookInfo);

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("The Hobbit");
        result[0].Authors.Should().Contain("J.R.R. Tolkien");
        result[0].FirstPublishYear.Should().Be(1937);
    }

    [Fact]
    public async Task SearchBooksAsync_WithMultipleBooks_ReturnsAllBooks()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.OpenLibrary.SearchResponse(
                ("/works/OL1", "Book 1", new[] { "Author 1" }, 2000, null),
                ("/works/OL2", "Book 2", new[] { "Author 2" }, 2001, null),
                ("/works/OL3", "Book 3", new[] { "Author 3" }, 2002, null)));

        var bookInfo = ExtractedBookInfoBuilder.Default()
            .WithKeywords("test")
            .Build();

        // Act
        var result = await _sut.SearchBooksAsync(bookInfo);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task SearchBooksAsync_WithEmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.OpenLibrary.EmptySearchResponse());

        var bookInfo = ExtractedBookInfoBuilder.Default()
            .WithTitle("Nonexistent Book")
            .Build();

        // Act
        var result = await _sut.SearchBooksAsync(bookInfo);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchBooksAsync_WithHttpError_ThrowsBookSearchException()
    {
        // Arrange
        _mockHandler.QueueErrorResponse(HttpStatusCode.InternalServerError, "Server error");

        var bookInfo = ExtractedBookInfoBuilder.Default()
            .WithTitle("Test")
            .Build();

        // Act
        var act = () => _sut.SearchBooksAsync(bookInfo);

        // Assert
        await act.Should().ThrowAsync<BookSearchException>();
    }

    [Fact]
    public async Task SearchBooksAsync_WithTitleAndAuthor_SearchesBothFields()
    {
        // Arrange
        _mockHandler
            .QueueSuccessResponse(HttpResponseFixtures.OpenLibrary.SingleBookResponse(
                "/works/OL1", "The Hobbit", "J.R.R. Tolkien"))
            .QueueSuccessResponse(HttpResponseFixtures.OpenLibrary.EmptySearchResponse())
            .QueueSuccessResponse(HttpResponseFixtures.OpenLibrary.EmptySearchResponse());

        var bookInfo = ExtractedBookInfoBuilder.Default()
            .WithTitle("The Hobbit")
            .WithAuthor("Tolkien")
            .Build();

        // Act
        await _sut.SearchBooksAsync(bookInfo);

        // Assert
        _mockHandler.ReceivedRequests.Should().HaveCountGreaterThan(0);
        var firstRequest = _mockHandler.ReceivedRequests[0];
        firstRequest.RequestUri!.ToString().Should().Contain("title=");
        firstRequest.RequestUri.ToString().Should().Contain("author=");
    }

    [Fact]
    public async Task SearchBooksAsync_DeduplicatesResults()
    {
        // Arrange
        _mockHandler
            .QueueSuccessResponse(HttpResponseFixtures.OpenLibrary.SingleBookResponse(
                "/works/OL1", "The Hobbit", "J.R.R. Tolkien"))
            .QueueSuccessResponse(HttpResponseFixtures.OpenLibrary.SingleBookResponse(
                "/works/OL1", "The Hobbit", "J.R.R. Tolkien"));

        var bookInfo = ExtractedBookInfoBuilder.Default()
            .WithTitle("The Hobbit")
            .WithAuthor("Tolkien")
            .Build();

        // Act
        var result = await _sut.SearchBooksAsync(bookInfo);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchBooksAsync_RespectsMaxResults()
    {
        // Arrange - API will receive limit parameter, mock returns limited books
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.OpenLibrary.SearchResponse(
                ("/works/OL1", "Book 1", new[] { "Author" }, 2001, null),
                ("/works/OL2", "Book 2", new[] { "Author" }, 2002, null),
                ("/works/OL3", "Book 3", new[] { "Author" }, 2003, null)));

        var bookInfo = ExtractedBookInfoBuilder.Default()
            .WithKeywords("test")
            .Build();

        // Act
        var result = await _sut.SearchBooksAsync(bookInfo, maxResults: 3);

        // Assert
        result.Should().HaveCount(3);
        // Verify the URL contained the limit parameter
        var request = _mockHandler.ReceivedRequests[0];
        request.RequestUri!.ToString().Should().Contain("limit=3");
    }

    [Fact]
    public async Task SearchBooksAsync_WithOnlyKeywords_UsesGeneralSearch()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.OpenLibrary.SingleBookResponse(
                "/works/OL1", "Fantasy Book", "Author"));

        var bookInfo = ExtractedBookInfoBuilder.Default()
            .WithKeywords("fantasy", "adventure")
            .Build();

        // Act
        await _sut.SearchBooksAsync(bookInfo);

        // Assert
        var request = _mockHandler.ReceivedRequests[0];
        request.RequestUri!.ToString().Should().Contain("q=");
    }

    [Fact]
    public async Task SearchBooksAsync_MapsAllBookProperties()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.OpenLibrary.SearchResponse(
                ("/works/OL123", "Test Book", new[] { "Author One", "Author Two" }, 2020, 12345)));

        var bookInfo = ExtractedBookInfoBuilder.Default()
            .WithTitle("Test")
            .Build();

        // Act
        var result = await _sut.SearchBooksAsync(bookInfo);

        // Assert
        result.Should().HaveCount(1);
        var book = result[0];
        book.Key.Should().Be("/works/OL123");
        book.Title.Should().Be("Test Book");
        book.Authors.Should().HaveCount(2);
        book.FirstPublishYear.Should().Be(2020);
        book.CoverId.Should().Be("12345");
    }

    [Fact]
    public async Task SearchBooksAsync_WithServiceUnavailable_ThrowsBookSearchException()
    {
        // Arrange
        _mockHandler.QueueErrorResponse(HttpStatusCode.ServiceUnavailable, "Service down");

        var bookInfo = ExtractedBookInfoBuilder.Default()
            .WithTitle("Test")
            .Build();

        // Act
        var act = () => _sut.SearchBooksAsync(bookInfo);

        // Assert
        await act.Should().ThrowAsync<BookSearchException>();
    }

    [Fact]
    public async Task SearchBooksAsync_WithNullBookYear_HandlesGracefully()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.OpenLibrary.SingleBookResponse(
                "/works/OL1", "Book Without Year", "Author", null));

        var bookInfo = ExtractedBookInfoBuilder.Default()
            .WithTitle("Book")
            .Build();

        // Act
        var result = await _sut.SearchBooksAsync(bookInfo);

        // Assert
        result[0].FirstPublishYear.Should().BeNull();
    }

    [Fact]
    public async Task SearchBooksAsync_WithOriginalQueryFallback_SearchesOriginalQuery()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.OpenLibrary.SingleBookResponse("/works/OL1", "Test", "Author"));

        var bookInfo = new ExtractedBookInfo
        {
            OriginalQuery = "some random query"
        };

        // Act
        await _sut.SearchBooksAsync(bookInfo);

        // Assert
        var request = _mockHandler.ReceivedRequests[0];
        request.RequestUri!.ToString().Should().Contain("q=some");
    }

    [Fact]
    public async Task SearchBooksAsync_EncodesSpecialCharacters()
    {
        // Arrange
        _mockHandler.QueueSuccessResponse(
            HttpResponseFixtures.OpenLibrary.EmptySearchResponse());

        var bookInfo = ExtractedBookInfoBuilder.Default()
            .WithTitle("Book & Title")
            .Build();

        // Act
        await _sut.SearchBooksAsync(bookInfo);

        // Assert
        var request = _mockHandler.ReceivedRequests[0];
        request.RequestUri!.ToString().Should().Contain("%26"); // & encoded
    }
}
