using FindThatBook.Domain.ValueObjects;
using FluentAssertions;

namespace FindThatBook.Tests.Domain;

public class ExtractedBookInfoTests
{
    [Fact]
    public void HasTitle_WithTitle_ReturnsTrue()
    {
        // Arrange
        var info = new ExtractedBookInfo { Title = "The Hobbit" };

        // Assert
        info.HasTitle.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HasTitle_WithEmptyOrNullTitle_ReturnsFalse(string? title)
    {
        // Arrange
        var info = new ExtractedBookInfo { Title = title };

        // Assert
        info.HasTitle.Should().BeFalse();
    }

    [Fact]
    public void HasAuthor_WithAuthor_ReturnsTrue()
    {
        // Arrange
        var info = new ExtractedBookInfo { Author = "J.R.R. Tolkien" };

        // Assert
        info.HasAuthor.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HasAuthor_WithEmptyOrNullAuthor_ReturnsFalse(string? author)
    {
        // Arrange
        var info = new ExtractedBookInfo { Author = author };

        // Assert
        info.HasAuthor.Should().BeFalse();
    }

    [Fact]
    public void HasKeywords_WithKeywords_ReturnsTrue()
    {
        // Arrange
        var info = new ExtractedBookInfo { Keywords = new List<string> { "fantasy", "illustrated" } };

        // Assert
        info.HasKeywords.Should().BeTrue();
    }

    [Fact]
    public void HasKeywords_WithEmptyList_ReturnsFalse()
    {
        // Arrange
        var info = new ExtractedBookInfo { Keywords = new List<string>() };

        // Assert
        info.HasKeywords.Should().BeFalse();
    }

    [Fact]
    public void DefaultKeywords_IsEmptyList()
    {
        // Arrange
        var info = new ExtractedBookInfo();

        // Assert
        info.Keywords.Should().BeEmpty();
    }
}
