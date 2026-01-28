using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Enums;
using FluentAssertions;

namespace FindThatBook.Tests.Domain;

public class BookCandidateTests
{
    [Theory]
    [InlineData(MatchStrength.Exact, "Exact match:")]
    [InlineData(MatchStrength.Strong, "Strong match:")]
    [InlineData(MatchStrength.Partial, "Partial match:")]
    [InlineData(MatchStrength.Weak, "Weak match:")]
    [InlineData(MatchStrength.None, "No significant match found")]
    public void MatchExplanation_ReturnsCorrectPrefix_ForMatchStrength(MatchStrength strength, string expectedPrefix)
    {
        // Arrange
        var candidate = new BookCandidate
        {
            MatchStrength = strength,
            MatchReasons = new List<string> { "Title matched", "Author matched" }
        };

        // Act
        var explanation = candidate.MatchExplanation;

        // Assert
        explanation.Should().StartWith(expectedPrefix);
    }

    [Fact]
    public void MatchExplanation_IncludesAllReasons()
    {
        // Arrange
        var reasons = new List<string> { "Title exact match", "Author matched", "Year matched" };
        var candidate = new BookCandidate
        {
            MatchStrength = MatchStrength.Exact,
            MatchReasons = reasons
        };

        // Act
        var explanation = candidate.MatchExplanation;

        // Assert
        foreach (var reason in reasons)
        {
            explanation.Should().Contain(reason);
        }
    }
}

public class BookTests
{
    [Fact]
    public void CoverUrl_WithCoverId_ReturnsCorrectUrl()
    {
        // Arrange
        var book = new Book { CoverId = "12345" };

        // Act
        var coverUrl = book.CoverUrl;

        // Assert
        coverUrl.Should().Be("https://covers.openlibrary.org/b/id/12345-M.jpg");
    }

    [Fact]
    public void CoverUrl_WithoutCoverId_ReturnsEmptyString()
    {
        // Arrange
        var book = new Book { CoverId = null };

        // Act
        var coverUrl = book.CoverUrl;

        // Assert
        coverUrl.Should().BeEmpty();
    }

    [Fact]
    public void OpenLibraryUrl_ReturnsCorrectUrl()
    {
        // Arrange
        var book = new Book { Key = "/works/OL12345W" };

        // Act
        var url = book.OpenLibraryUrl;

        // Assert
        url.Should().Be("https://openlibrary.org/works/OL12345W");
    }
}
