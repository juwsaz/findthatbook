using FindThatBook.Application.Services.Matching;
using FindThatBook.Application.Services.Matching.Strategies;
using FindThatBook.Domain.Constants;
using FindThatBook.Tests.Common.Builders;
using FluentAssertions;

namespace FindThatBook.Tests.Application.Matching;

public class KeywordMatchingStrategyTests
{
    private readonly KeywordMatchingStrategy _sut;

    public KeywordMatchingStrategyTests()
    {
        _sut = new KeywordMatchingStrategy(new TextNormalizer());
    }

    [Fact]
    public void StrategyName_ReturnsKeyword()
    {
        _sut.StrategyName.Should().Be("Keyword");
    }

    [Fact]
    public void Weight_ReturnsKeywordWeight()
    {
        _sut.Weight.Should().Be(DomainConstants.Matching.KeywordWeight);
    }

    [Fact]
    public void CanEvaluate_WithKeywords_ReturnsTrue()
    {
        var info = ExtractedBookInfoBuilder.Default()
            .WithKeywords("fantasy", "adventure")
            .Build();

        _sut.CanEvaluate(info).Should().BeTrue();
    }

    [Fact]
    public void CanEvaluate_WithEmptyKeywords_ReturnsFalse()
    {
        var info = ExtractedBookInfoBuilder.Default()
            .WithKeywords()
            .Build();

        _sut.CanEvaluate(info).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_WithAllKeywordsMatching_ReturnsFullScore()
    {
        var book = BookBuilder.Default()
            .WithTitle("The Hobbit")
            .WithSubjects("Fantasy", "Adventure")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithKeywords("fantasy", "adventure")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(1.0);
        result.Reason.Should().Contain("Keywords matched");
    }

    [Fact]
    public void Evaluate_WithPartialKeywordsMatching_ReturnsPartialScore()
    {
        var book = BookBuilder.Default()
            .WithTitle("The Hobbit")
            .WithSubjects("Fantasy")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithKeywords("fantasy", "sci-fi")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(0.5);
    }

    [Fact]
    public void Evaluate_WithNoKeywordsMatching_ReturnsZeroScore()
    {
        var book = BookBuilder.Default()
            .WithTitle("Cooking Book")
            .WithSubjects("Cooking", "Recipes")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithKeywords("fantasy", "adventure")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(0);
        result.HasMatch.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_MatchesKeywordsInTitle()
    {
        var book = BookBuilder.Default()
            .WithTitle("The Illustrated Hobbit")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithKeywords("illustrated")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(1.0);
    }

    [Fact]
    public void Evaluate_MatchesKeywordsInAuthor()
    {
        var book = BookBuilder.Default()
            .WithTitle("Some Book")
            .WithAuthor("J.R.R. Tolkien")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithKeywords("tolkien")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(1.0);
    }

    [Fact]
    public void Evaluate_MatchesKeywordsInSubjects()
    {
        var book = BookBuilder.Default()
            .WithTitle("Some Book")
            .WithSubjects("Science Fiction", "Space Opera")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithKeywords("science", "fiction")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(1.0);
    }

    [Fact]
    public void Evaluate_IgnoresCase()
    {
        var book = BookBuilder.Default()
            .WithTitle("The Hobbit")
            .WithSubjects("FANTASY")
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithKeywords("fantasy")
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(1.0);
    }
}
