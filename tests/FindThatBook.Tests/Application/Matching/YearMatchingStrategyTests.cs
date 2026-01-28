using FindThatBook.Application.Services.Matching.Strategies;
using FindThatBook.Domain.Constants;
using FindThatBook.Tests.Common.Builders;
using FluentAssertions;

namespace FindThatBook.Tests.Application.Matching;

public class YearMatchingStrategyTests
{
    private readonly YearMatchingStrategy _sut = new();

    [Fact]
    public void StrategyName_ReturnsYear()
    {
        _sut.StrategyName.Should().Be("Year");
    }

    [Fact]
    public void Weight_ReturnsYearWeight()
    {
        _sut.Weight.Should().Be(DomainConstants.Matching.YearWeight);
    }

    [Fact]
    public void CanEvaluate_WithYear_ReturnsTrue()
    {
        var info = ExtractedBookInfoBuilder.Default()
            .WithYear(1937)
            .Build();

        _sut.CanEvaluate(info).Should().BeTrue();
    }

    [Fact]
    public void CanEvaluate_WithoutYear_ReturnsFalse()
    {
        var info = ExtractedBookInfoBuilder.Default()
            .WithYear(null)
            .Build();

        _sut.CanEvaluate(info).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_WithExactYear_ReturnsFullScore()
    {
        var book = BookBuilder.Default()
            .WithYear(1937)
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithYear(1937)
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(DomainConstants.Matching.YearExactScore);
        result.Reason.Should().Contain("exact match");
    }

    [Theory]
    [InlineData(1936)]
    [InlineData(1938)]
    [InlineData(1935)]
    [InlineData(1939)]
    public void Evaluate_WithCloseYear_ReturnsHighScore(int searchYear)
    {
        var book = BookBuilder.Default()
            .WithYear(1937)
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithYear(searchYear)
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(DomainConstants.Matching.YearCloseScore);
        result.Reason.Should().Contain("close match");
    }

    [Theory]
    [InlineData(1933)]
    [InlineData(1934)]
    [InlineData(1940)]
    [InlineData(1941)]
    [InlineData(1942)]
    public void Evaluate_WithApproximateYear_ReturnsMediumScore(int searchYear)
    {
        var book = BookBuilder.Default()
            .WithYear(1937)
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithYear(searchYear)
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(DomainConstants.Matching.YearApproximateScore);
        result.Reason.Should().Contain("approximate");
    }

    [Theory]
    [InlineData(1900)]
    [InlineData(1950)]
    [InlineData(2020)]
    public void Evaluate_WithDistantYear_ReturnsZeroScore(int searchYear)
    {
        var book = BookBuilder.Default()
            .WithYear(1937)
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithYear(searchYear)
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(0);
        result.HasMatch.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_WithNoBookYear_ReturnsZeroScore()
    {
        var book = BookBuilder.Default()
            .WithYear(null)
            .Build();

        var info = ExtractedBookInfoBuilder.Default()
            .WithYear(1937)
            .Build();

        var result = _sut.Evaluate(book, info);

        result.Score.Should().Be(0);
    }
}
