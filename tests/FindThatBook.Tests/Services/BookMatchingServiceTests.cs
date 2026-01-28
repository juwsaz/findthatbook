using FindThatBook.Application.Services;
using FindThatBook.Application.Services.Matching;
using FindThatBook.Application.Services.Matching.Strategies;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Enums;
using FindThatBook.Domain.Interfaces.Matching;
using FindThatBook.Domain.ValueObjects;
using FluentAssertions;

namespace FindThatBook.Tests.Services;

public class BookMatchingServiceTests
{
    private readonly BookMatchingService _sut;

    public BookMatchingServiceTests()
    {
        var textNormalizer = new TextNormalizer();
        var strategies = new IMatchingStrategy[]
        {
            new TitleMatchingStrategy(textNormalizer),
            new AuthorMatchingStrategy(textNormalizer),
            new YearMatchingStrategy(),
            new KeywordMatchingStrategy(textNormalizer)
        };
        var strengthEvaluator = new MatchStrengthEvaluator();
        var registry = new MatchingStrategyRegistry(strategies, strengthEvaluator);

        _sut = new BookMatchingService(registry);
    }

    [Fact]
    public void RankBooks_WithExactTitleAndAuthorMatch_ReturnsExactMatchStrength()
    {
        // Arrange
        var books = new List<Book>
        {
            new()
            {
                Key = "/works/OL123",
                Title = "The Adventures of Huckleberry Finn",
                Authors = new List<string> { "Mark Twain" },
                FirstPublishYear = 1884
            }
        };

        var extractedInfo = new ExtractedBookInfo
        {
            Title = "The Adventures of Huckleberry Finn",
            Author = "Mark Twain",
            OriginalQuery = "huckleberry finn mark twain"
        };

        // Act
        var result = _sut.RankBooks(books, extractedInfo);

        // Assert
        result.Should().HaveCount(1);
        result[0].MatchStrength.Should().Be(MatchStrength.Exact);
        result[0].MatchScore.Should().BeGreaterThan(70);
    }

    [Fact]
    public void RankBooks_WithPartialTitleMatch_ReturnsPartialMatchStrength()
    {
        // Arrange
        var books = new List<Book>
        {
            new()
            {
                Key = "/works/OL456",
                Title = "The Hobbit",
                Authors = new List<string> { "J.R.R. Tolkien" },
                FirstPublishYear = 1937
            }
        };

        var extractedInfo = new ExtractedBookInfo
        {
            Title = "Hobbit",
            Author = null,
            OriginalQuery = "hobbit"
        };

        // Act
        var result = _sut.RankBooks(books, extractedInfo);

        // Assert
        result.Should().HaveCount(1);
        result[0].MatchStrength.Should().BeOneOf(MatchStrength.Partial, MatchStrength.Strong);
    }

    [Fact]
    public void RankBooks_WithAuthorOnlyMatch_ReturnsPartialMatchStrength()
    {
        // Arrange
        var books = new List<Book>
        {
            new()
            {
                Key = "/works/OL789",
                Title = "1984",
                Authors = new List<string> { "George Orwell" },
                FirstPublishYear = 1949
            }
        };

        var extractedInfo = new ExtractedBookInfo
        {
            Title = null,
            Author = "George Orwell",
            OriginalQuery = "orwell"
        };

        // Act
        var result = _sut.RankBooks(books, extractedInfo);

        // Assert
        result.Should().HaveCount(1);
        result[0].MatchStrength.Should().BeOneOf(MatchStrength.Partial, MatchStrength.Weak);
        result[0].MatchReasons.Should().Contain(r => r.Contains("Author"));
    }

    [Fact]
    public void RankBooks_WithKeywordMatch_ReturnsWeakMatchStrength()
    {
        // Arrange
        var books = new List<Book>
        {
            new()
            {
                Key = "/works/OL111",
                Title = "The Lord of the Rings: Illustrated Edition",
                Authors = new List<string> { "J.R.R. Tolkien" },
                Subjects = new List<string> { "Fantasy", "Adventure" }
            }
        };

        var extractedInfo = new ExtractedBookInfo
        {
            Title = null,
            Author = null,
            Keywords = new List<string> { "illustrated", "fantasy" },
            OriginalQuery = "illustrated fantasy"
        };

        // Act
        var result = _sut.RankBooks(books, extractedInfo);

        // Assert
        result.Should().HaveCount(1);
        result[0].MatchReasons.Should().Contain(r => r.Contains("Keywords"));
    }

    [Fact]
    public void RankBooks_WithNoMatch_ReturnsEmptyList()
    {
        // Arrange
        var books = new List<Book>
        {
            new()
            {
                Key = "/works/OL222",
                Title = "Cooking Recipes for Beginners",
                Authors = new List<string> { "Chef Gordon" }
            }
        };

        var extractedInfo = new ExtractedBookInfo
        {
            Title = "Advanced Quantum Physics",
            Author = "Dr. Einstein",
            OriginalQuery = "quantum physics"
        };

        // Act
        var result = _sut.RankBooks(books, extractedInfo);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void RankBooks_WithYearMatch_IncludesYearInReasons()
    {
        // Arrange
        var books = new List<Book>
        {
            new()
            {
                Key = "/works/OL333",
                Title = "The Hobbit",
                Authors = new List<string> { "J.R.R. Tolkien" },
                FirstPublishYear = 1937
            }
        };

        var extractedInfo = new ExtractedBookInfo
        {
            Title = "The Hobbit",
            Author = "Tolkien",
            Year = 1937,
            OriginalQuery = "hobbit tolkien 1937"
        };

        // Act
        var result = _sut.RankBooks(books, extractedInfo);

        // Assert
        result.Should().HaveCount(1);
        result[0].MatchReasons.Should().Contain(r => r.Contains("Year"));
    }

    [Fact]
    public void RankBooks_ReturnsMaxCandidatesSpecified()
    {
        // Arrange
        var books = Enumerable.Range(1, 10).Select(i => new Book
        {
            Key = $"/works/OL{i}",
            Title = $"Book {i} with Tolkien",
            Authors = new List<string> { "J.R.R. Tolkien" }
        }).ToList();

        var extractedInfo = new ExtractedBookInfo
        {
            Author = "Tolkien",
            OriginalQuery = "tolkien"
        };

        // Act
        var result = _sut.RankBooks(books, extractedInfo, maxCandidates: 3);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void RankBooks_SortsByMatchStrengthThenScore()
    {
        // Arrange
        var books = new List<Book>
        {
            new()
            {
                Key = "/works/OL1",
                Title = "Some Book",
                Authors = new List<string> { "Mark Twain" }
            },
            new()
            {
                Key = "/works/OL2",
                Title = "The Adventures of Huckleberry Finn",
                Authors = new List<string> { "Mark Twain" }
            }
        };

        var extractedInfo = new ExtractedBookInfo
        {
            Title = "The Adventures of Huckleberry Finn",
            Author = "Mark Twain",
            OriginalQuery = "huckleberry finn twain"
        };

        // Act
        var result = _sut.RankBooks(books, extractedInfo);

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(1);
        result[0].Book.Title.Should().Be("The Adventures of Huckleberry Finn");
    }

    [Fact]
    public void RankBooks_WithLastNameAuthorSearch_FindsMatch()
    {
        // Arrange
        var books = new List<Book>
        {
            new()
            {
                Key = "/works/OL444",
                Title = "Pride and Prejudice",
                Authors = new List<string> { "Jane Austen" }
            }
        };

        var extractedInfo = new ExtractedBookInfo
        {
            Author = "Austen",
            OriginalQuery = "austen"
        };

        // Act
        var result = _sut.RankBooks(books, extractedInfo);

        // Assert
        result.Should().HaveCount(1);
        result[0].MatchReasons.Should().Contain(r => r.Contains("Author"));
    }
}
