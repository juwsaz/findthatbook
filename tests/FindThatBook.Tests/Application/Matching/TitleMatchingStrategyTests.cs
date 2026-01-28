using FindThatBook.Application.Services.Matching;
using FindThatBook.Application.Services.Matching.Strategies;
using FindThatBook.Domain.Constants;
using FindThatBook.Tests.Common.Builders;
using FluentAssertions;

namespace FindThatBook.Tests.Application.Matching;

public class TitleMatchingStrategyTests
{
    private readonly TitleMatchingStrategy _sut;

    public TitleMatchingStrategyTests()
    {
        _sut = new TitleMatchingStrategy(new TextNormalizer());
    }

    [Fact]
    public void StrategyName_ReturnsTitle()
    {
        _sut.StrategyName.Should().Be("Title");
    }

    [Fact]
    public void Weight_ReturnsTitleWeight()
    {
        _sut.Weight.Should().Be(DomainConstants.Matching.TitleWeight);
    }

    [Fact]
    public void CanEvaluate_WithTitle_ReturnsTrue()
    {
        var info = ExtractedBookInfoBuilder.Default()
            .WithTitle("Test Title")
            .Build();

        _sut.CanEvaluate(info).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CanEvaluate_WithoutTitle_ReturnsFalse(string? title)
    {
        var info = ExtractedBookInfoBuilder.Default()
            .WithTitle(title)
            .Build();

        _sut.CanEvaluate(info).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_WithExactMatch_ReturnsFullScore()
    {
        var book = BookBuilder.Default()
            .WithTitle("The Hobbit")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithTitle("The Hobbit")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(DomainConstants.Matching.TitleExactScore);
        result.Reason.Should().Contain("exact match");
    }

    [Fact]
    public void Evaluate_WithBookTitleContainingSearchTitle_ReturnsHighScore()
    {
        var book = BookBuilder.Default()
            .WithTitle("The Hobbit: An Unexpected Journey")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithTitle("The Hobbit")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(DomainConstants.Matching.TitleContainsScore);
        result.Reason.Should().Contain("contains");
    }

    [Fact]
    public void Evaluate_WithSearchTitleContainingBookTitle_ReturnsMediumScore()
    {
        var book = BookBuilder.Default()
            .WithTitle("Hobbit")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithTitle("The Hobbit Book")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(DomainConstants.Matching.TitleContainedScore);
    }

    [Fact]
    public void Evaluate_WithPartialWordMatch_ReturnsPartialScore()
    {
        var book = BookBuilder.Default()
            .WithTitle("The Adventures of Huckleberry Finn")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithTitle("Huckleberry Adventures")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().BeGreaterThan(0);
        result.Reason.Should().Contain("word match");
    }

    [Fact]
    public void Evaluate_WithNoMatch_ReturnsZeroScore()
    {
        var book = BookBuilder.Default()
            .WithTitle("Cooking Recipes")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithTitle("Quantum Physics")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(0);
        result.HasMatch.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_IgnoresCase()
    {
        var book = BookBuilder.Default()
            .WithTitle("THE HOBBIT")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithTitle("the hobbit")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(DomainConstants.Matching.TitleExactScore);
    }

    [Fact]
    public void Evaluate_IgnoresPunctuation()
    {
        var book = BookBuilder.Default()
            .WithTitle("The Hobbit, or There and Back Again")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithTitle("The Hobbit or There and Back Again")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(DomainConstants.Matching.TitleExactScore);
    }
}
