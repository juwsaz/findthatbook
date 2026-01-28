using FindThatBook.API.Controllers;
using FindThatBook.Application.DTOs;
using FindThatBook.Application.UseCases;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Enums;
using FindThatBook.Domain.Exceptions;
using FindThatBook.Domain.Interfaces;
using FindThatBook.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FindThatBook.Tests.API;

public class BooksControllerTests
{
    private readonly Mock<IAiExtractionService> _aiServiceMock;
    private readonly Mock<IBookSearchService> _searchServiceMock;
    private readonly Mock<IBookMatchingService> _matchingServiceMock;
    private readonly BooksController _sut;

    public BooksControllerTests()
    {
        _aiServiceMock = new Mock<IAiExtractionService>();
        _searchServiceMock = new Mock<IBookSearchService>();
        _matchingServiceMock = new Mock<IBookMatchingService>();

        var useCase = new SearchBooksUseCase(
            _aiServiceMock.Object,
            _searchServiceMock.Object,
            _matchingServiceMock.Object);

        _sut = new BooksController(useCase, NullLogger<BooksController>.Instance);
    }

    [Fact]
    public async Task SearchBooks_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "hobbit tolkien", MaxResults = 5 };
        SetupSuccessfulMocks();

        // Act
        var result = await _sut.SearchBooks(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SearchBooks_WithValidRequest_ReturnsBookSearchResponse()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "hobbit tolkien", MaxResults = 5 };
        SetupSuccessfulMocks();

        // Act
        var result = await _sut.SearchBooks(request, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<BookSearchResponse>();
    }

    [Fact]
    public async Task SearchBooks_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _sut.SearchBooks(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SearchBooks_WithEmptyQuery_ThrowsValidationException()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "", MaxResults = 5 };

        // Act
        var act = () => _sut.SearchBooks(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SearchBooks_WithWhitespaceQuery_ThrowsValidationException()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "   ", MaxResults = 5 };

        // Act
        var act = () => _sut.SearchBooks(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SearchBooks_WithMaxResultsTooLow_ThrowsValidationException()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "test", MaxResults = 0 };

        // Act
        var act = () => _sut.SearchBooks(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SearchBooks_WithMaxResultsTooHigh_ThrowsValidationException()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "test", MaxResults = 100 };

        // Act
        var act = () => _sut.SearchBooks(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SearchBooks_ReturnsCorrectCandidateCount()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "test", MaxResults = 5 };
        SetupSuccessfulMocksWithCandidates(3);

        // Act
        var result = await _sut.SearchBooks(request, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as BookSearchResponse;
        response!.TotalCandidates.Should().Be(3);
    }

    [Fact]
    public async Task SearchBooks_CallsAiExtractionService()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "hobbit", MaxResults = 5 };
        SetupSuccessfulMocks();

        // Act
        await _sut.SearchBooks(request, CancellationToken.None);

        // Assert
        _aiServiceMock.Verify(
            x => x.ExtractBookInfoAsync("hobbit", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchBooks_CallsSearchService()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "hobbit", MaxResults = 5 };
        SetupSuccessfulMocks();

        // Act
        await _sut.SearchBooks(request, CancellationToken.None);

        // Assert
        _searchServiceMock.Verify(
            x => x.SearchBooksAsync(
                It.IsAny<ExtractedBookInfo>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchBooks_CallsMatchingService()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "hobbit", MaxResults = 5 };
        SetupSuccessfulMocks();

        // Act
        await _sut.SearchBooks(request, CancellationToken.None);

        // Assert
        _matchingServiceMock.Verify(
            x => x.RankBooks(
                It.IsAny<List<Book>>(),
                It.IsAny<ExtractedBookInfo>(),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public void Health_ReturnsOkResult()
    {
        // Act
        var result = _sut.Health();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Health_ReturnsHealthyStatus()
    {
        // Act
        var result = _sut.Health() as OkObjectResult;

        // Assert
        var value = result!.Value;
        value.Should().NotBeNull();
        value!.ToString().Should().Contain("healthy");
    }

    private void SetupSuccessfulMocks()
    {
        SetupSuccessfulMocksWithCandidates(1);
    }

    private void SetupSuccessfulMocksWithCandidates(int candidateCount)
    {
        var extractedInfo = new ExtractedBookInfo
        {
            Title = "The Hobbit",
            Author = "J.R.R. Tolkien",
            OriginalQuery = "hobbit"
        };

        var books = Enumerable.Range(1, candidateCount)
            .Select(i => new Book
            {
                Key = $"/works/OL{i}",
                Title = $"Book {i}",
                Authors = new List<string> { "Author" }
            })
            .ToList();

        var candidates = books.Select(b => new BookCandidate
        {
            Book = b,
            MatchStrength = MatchStrength.Strong,
            MatchScore = 80,
            MatchReasons = new List<string> { "Test match" }
        }).ToList();

        _aiServiceMock
            .Setup(x => x.ExtractBookInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractedInfo);

        _searchServiceMock
            .Setup(x => x.SearchBooksAsync(
                It.IsAny<ExtractedBookInfo>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        _matchingServiceMock
            .Setup(x => x.RankBooks(
                It.IsAny<List<Book>>(),
                It.IsAny<ExtractedBookInfo>(),
                It.IsAny<int>()))
            .Returns(candidates);
    }
}
