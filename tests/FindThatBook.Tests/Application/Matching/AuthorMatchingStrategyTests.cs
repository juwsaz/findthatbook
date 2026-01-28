using FindThatBook.Application.Services.Matching;
using FindThatBook.Application.Services.Matching.Strategies;
using FindThatBook.Domain.Constants;
using FindThatBook.Tests.Common.Builders;
using FluentAssertions;

namespace FindThatBook.Tests.Application.Matching;

public class AuthorMatchingStrategyTests
{
    private readonly AuthorMatchingStrategy _sut;

    public AuthorMatchingStrategyTests()
    {
        _sut = new AuthorMatchingStrategy(new TextNormalizer());
    }

    [Fact]
    public void StrategyName_ReturnsAuthor()
    {
        _sut.StrategyName.Should().Be("Author");
    }

    [Fact]
    public void Weight_ReturnsAuthorWeight()
    {
        _sut.Weight.Should().Be(DomainConstants.Matching.AuthorWeight);
    }

    [Fact]
    public void CanEvaluate_WithAuthor_ReturnsTrue()
    {
        var info = ExtractedBookInfoBuilder.Default()
            .WithAuthor("J.R.R. Tolkien")
            .Build();

        _sut.CanEvaluate(info).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CanEvaluate_WithoutAuthor_ReturnsFalse(string? author)
    {
        var info = ExtractedBookInfoBuilder.Default()
            .WithAuthor(author)
            .Build();

        _sut.CanEvaluate(info).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_WithExactMatch_ReturnsFullScore()
    {
        var book = BookBuilder.Default()
            .WithAuthor("J.R.R. Tolkien")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithAuthor("J.R.R. Tolkien")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(DomainConstants.Matching.AuthorExactScore);
        result.Reason.Should().Contain("exact match");
    }

    [Fact]
    public void Evaluate_WithLastNameOnly_ReturnsHighScore()
    {
        var book = BookBuilder.Default()
            .WithAuthor("J.R.R. Tolkien")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithAuthor("Tolkien")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(DomainConstants.Matching.AuthorLastNameOnlyScore);
        result.Reason.Should().Contain("Author match");
    }

    [Fact]
    public void Evaluate_WithFirstAndLastName_ReturnsVeryHighScore()
    {
        var book = BookBuilder.Default()
            .WithAuthor("George Orwell")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithAuthor("George Orwell")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(DomainConstants.Matching.AuthorExactScore);
    }

    [Fact]
    public void Evaluate_WithMultipleAuthors_MatchesFirst()
    {
        // When there are multiple authors, the strategy returns the first match found
        var book = BookBuilder.Default()
            .WithAuthors("J.R.R. Tolkien", "Christopher Tolkien")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithAuthor("Tolkien")
            .Build();

        var result = _sut.Evaluate(book, info);

        // Should match "Tolkien" as last name
        result.Score.Should().Be(DomainConstants.Matching.AuthorLastNameOnlyScore);
        result.Reason.Should().Contain("Author match");
    }

    [Fact]
    public void Evaluate_WithSecondAuthorExactMatch_MatchesFirst()
    {
        // When the second author would be an exact match but first is partial,
        // we get the first match (this is expected behavior based on iteration order)
        var book = BookBuilder.Default()
            .WithAuthors("Different Author", "J.R.R. Tolkien")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithAuthor("J.R.R. Tolkien")
            .Build();

        var result = _sut.Evaluate(book, info);

        // First author "Different Author" has no match, second is exact
        result.Score.Should().Be(DomainConstants.Matching.AuthorExactScore);
    }

    [Fact]
    public void Evaluate_WithNoMatch_ReturnsZeroScore()
    {
        var book = BookBuilder.Default()
            .WithAuthor("Stephen King")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithAuthor("J.K. Rowling")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(0);
        result.HasMatch.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_WithNoBookAuthors_ReturnsZeroScore()
    {
        var book = BookBuilder.Default()
            .WithAuthors()
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithAuthor("Some Author")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(0);
    }

    [Fact]
    public void Evaluate_IgnoresCase()
    {
        var book = BookBuilder.Default()
            .WithAuthor("MARK TWAIN")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithAuthor("mark twain")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(DomainConstants.Matching.AuthorExactScore);
    }

    [Fact]
    public void Evaluate_WithPartialNameMatch_ReturnsPartialScore()
    {
        var book = BookBuilder.Default()
            .WithAuthor("John Ronald Reuel Tolkien")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithAuthor("J Tolkien")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().BeGreaterThan(0);
    }
}
