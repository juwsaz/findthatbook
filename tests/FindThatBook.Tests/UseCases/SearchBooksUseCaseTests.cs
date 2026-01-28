using FindThatBook.Application.DTOs;
using FindThatBook.Application.UseCases;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Enums;
using FindThatBook.Domain.Interfaces;
using FindThatBook.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FindThatBook.Tests.UseCases;

public class SearchBooksUseCaseTests
{
    private readonly Mock<IAiExtractionService> _aiExtractionServiceMock;
    private readonly Mock<IBookSearchService> _bookSearchServiceMock;
    private readonly Mock<IBookMatchingService> _bookMatchingServiceMock;
    private readonly SearchBooksUseCase _sut;

    public SearchBooksUseCaseTests()
    {
        _aiExtractionServiceMock = new Mock<IAiExtractionService>();
        _bookSearchServiceMock = new Mock<IBookSearchService>();
        _bookMatchingServiceMock = new Mock<IBookMatchingService>();

        _sut = new SearchBooksUseCase(
            _aiExtractionServiceMock.Object,
            _bookSearchServiceMock.Object,
            _bookMatchingServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_CallsAllServicesInCorrectOrder()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "tolkien hobbit", MaxResults = 5 };

        var extractedInfo = new ExtractedBookInfo
        {
            Title = "The Hobbit",
            Author = "J.R.R. Tolkien",
            OriginalQuery = request.Query
        };

        var books = new List<Book>
        {
            new() { Key = "/works/OL1", Title = "The Hobbit", Authors = new List<string> { "J.R.R. Tolkien" } }
        };

        var candidates = new List<BookCandidate>
        {
            new()
            {
                Book = books[0],
                MatchStrength = MatchStrength.Exact,
                MatchScore = 95,
                MatchReasons = new List<string> { "Title exact match", "Author exact match" }
            }
        };

        _aiExtractionServiceMock
            .Setup(x => x.ExtractBookInfoAsync(request.Query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractedInfo);

        _bookSearchServiceMock
            .Setup(x => x.SearchBooksAsync(extractedInfo, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        _bookMatchingServiceMock
            .Setup(x => x.RankBooks(books, extractedInfo, request.MaxResults))
            .Returns(candidates);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        _aiExtractionServiceMock.Verify(x => x.ExtractBookInfoAsync(request.Query, It.IsAny<CancellationToken>()), Times.Once);
        _bookSearchServiceMock.Verify(x => x.SearchBooksAsync(extractedInfo, 20, It.IsAny<CancellationToken>()), Times.Once);
        _bookMatchingServiceMock.Verify(x => x.RankBooks(books, extractedInfo, request.MaxResults), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCorrectResponse()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "orwell 1984", MaxResults = 5 };

        var extractedInfo = new ExtractedBookInfo
        {
            Title = "1984",
            Author = "George Orwell",
            Year = 1949,
            Keywords = new List<string> { "dystopian" },
            OriginalQuery = request.Query
        };

        var books = new List<Book>
        {
            new()
            {
                Key = "/works/OL1",
                Title = "1984",
                Authors = new List<string> { "George Orwell" },
                FirstPublishYear = 1949,
                CoverId = "12345"
            }
        };

        var candidates = new List<BookCandidate>
        {
            new()
            {
                Book = books[0],
                MatchStrength = MatchStrength.Exact,
                MatchScore = 95,
                MatchReasons = new List<string> { "Title exact match", "Author exact match", "Year exact match" }
            }
        };

        _aiExtractionServiceMock
            .Setup(x => x.ExtractBookInfoAsync(request.Query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractedInfo);

        _bookSearchServiceMock
            .Setup(x => x.SearchBooksAsync(extractedInfo, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        _bookMatchingServiceMock
            .Setup(x => x.RankBooks(books, extractedInfo, request.MaxResults))
            .Returns(candidates);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.OriginalQuery.Should().Be(request.Query);
        result.ExtractedInfo.Title.Should().Be("1984");
        result.ExtractedInfo.Author.Should().Be("George Orwell");
        result.ExtractedInfo.Year.Should().Be(1949);
        result.Candidates.Should().HaveCount(1);
        result.Candidates[0].Title.Should().Be("1984");
        result.Candidates[0].MatchStrength.Should().Be("Exact");
        result.TotalCandidates.Should().Be(1);
        result.ProcessingTime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMatches_ReturnsEmptyCandidates()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "nonexistent book xyz", MaxResults = 5 };

        var extractedInfo = new ExtractedBookInfo
        {
            Title = "nonexistent book xyz",
            OriginalQuery = request.Query
        };

        _aiExtractionServiceMock
            .Setup(x => x.ExtractBookInfoAsync(request.Query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractedInfo);

        _bookSearchServiceMock
            .Setup(x => x.SearchBooksAsync(extractedInfo, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Book>());

        _bookMatchingServiceMock
            .Setup(x => x.RankBooks(It.IsAny<List<Book>>(), extractedInfo, request.MaxResults))
            .Returns(new List<BookCandidate>());

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.Candidates.Should().BeEmpty();
        result.TotalCandidates.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_MapsBookPropertiesCorrectly()
    {
        // Arrange
        var request = new BookSearchRequest { Query = "test", MaxResults = 5 };

        var extractedInfo = new ExtractedBookInfo { OriginalQuery = request.Query };

        var books = new List<Book>
        {
            new()
            {
                Key = "/works/OL12345",
                Title = "Test Book",
                Authors = new List<string> { "Author One", "Author Two" },
                FirstPublishYear = 2020,
                CoverId = "98765"
            }
        };

        var candidates = new List<BookCandidate>
        {
            new()
            {
                Book = books[0],
                MatchStrength = MatchStrength.Strong,
                MatchScore = 80,
                MatchReasons = new List<string> { "Title match" }
            }
        };

        _aiExtractionServiceMock
            .Setup(x => x.ExtractBookInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractedInfo);

        _bookSearchServiceMock
            .Setup(x => x.SearchBooksAsync(It.IsAny<ExtractedBookInfo>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        _bookMatchingServiceMock
            .Setup(x => x.RankBooks(It.IsAny<List<Book>>(), It.IsAny<ExtractedBookInfo>(), It.IsAny<int>()))
            .Returns(candidates);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        var candidate = result.Candidates[0];
        candidate.Key.Should().Be("/works/OL12345");
        candidate.Title.Should().Be("Test Book");
        candidate.Authors.Should().BeEquivalentTo(new[] { "Author One", "Author Two" });
        candidate.FirstPublishYear.Should().Be(2020);
        candidate.CoverUrl.Should().Contain("98765");
        candidate.OpenLibraryUrl.Should().Contain("/works/OL12345");
        candidate.MatchStrength.Should().Be("Strong");
        candidate.MatchScore.Should().Be(80);
        candidate.MatchReasons.Should().Contain("Title match");
    }
}
