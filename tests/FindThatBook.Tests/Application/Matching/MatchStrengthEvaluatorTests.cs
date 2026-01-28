using FindThatBook.Application.Services.Matching;
using FindThatBook.Domain.Constants;
using FindThatBook.Domain.Enums;
using FindThatBook.Domain.ValueObjects.Matching;
using FluentAssertions;

namespace FindThatBook.Tests.Application.Matching;

public class MatchStrengthEvaluatorTests
{
    private readonly MatchStrengthEvaluator _sut = new();

    [Fact]
    public void Evaluate_WithExactTitleAndAuthor_ReturnsExact()
    {
        var scores = CreateScores(
            titleScore: 1.0,
            authorScore: 1.0,
            yearScore: 0,
            keywordScore: 0);

        var result = _sut.Evaluate(scores);

        result.Should().Be(MatchStrength.Exact);
    }

    [Fact]
    public void Evaluate_WithHighTitleAndAuthor_ReturnsStrong()
    {
        var scores = CreateScores(
            titleScore: 0.75,
            authorScore: 0.75,
            yearScore: 0,
            keywordScore: 0);

        var result = _sut.Evaluate(scores);

        result.Should().Be(MatchStrength.Strong);
    }

    [Fact]
    public void Evaluate_WithVeryHighTitleAndModerateAuthor_ReturnsStrong()
    {
        var scores = CreateScores(
            titleScore: 0.90,
            authorScore: 0.55,
            yearScore: 0,
            keywordScore: 0);

        var result = _sut.Evaluate(scores);

        result.Should().Be(MatchStrength.Strong);
    }

    [Fact]
    public void Evaluate_WithGoodTitle_ReturnsPartial()
    {
        var scores = CreateScores(
            titleScore: 0.65,
            authorScore: 0,
            yearScore: 0,
            keywordScore: 0);

        var result = _sut.Evaluate(scores);

        result.Should().Be(MatchStrength.Partial);
    }

    [Fact]
    public void Evaluate_WithGoodAuthor_ReturnsPartial()
    {
        var scores = CreateScores(
            titleScore: 0,
            authorScore: 0.65,
            yearScore: 0,
            keywordScore: 0);

        var result = _sut.Evaluate(scores);

        result.Should().Be(MatchStrength.Partial);
    }

    [Fact]
    public void Evaluate_WithModerateMatchAndYearConfirmation_ReturnsPartial()
    {
        var scores = CreateScores(
            titleScore: 0.45,
            authorScore: 0,
            yearScore: 0.85,
            keywordScore: 0);

        var result = _sut.Evaluate(scores);

        result.Should().Be(MatchStrength.Partial);
    }

    [Fact]
    public void Evaluate_WithSomeKeywords_ReturnsWeak()
    {
        var scores = CreateScores(
            titleScore: 0,
            authorScore: 0,
            yearScore: 0,
            keywordScore: 0.35);

        var result = _sut.Evaluate(scores);

        result.Should().Be(MatchStrength.Weak);
    }

    [Fact]
    public void Evaluate_WithLowTitle_ReturnsWeak()
    {
        var scores = CreateScores(
            titleScore: 0.35,
            authorScore: 0,
            yearScore: 0,
            keywordScore: 0);

        var result = _sut.Evaluate(scores);

        result.Should().Be(MatchStrength.Weak);
    }

    [Fact]
    public void Evaluate_WithNoMatches_ReturnsNone()
    {
        var scores = CreateScores(
            titleScore: 0,
            authorScore: 0,
            yearScore: 0,
            keywordScore: 0);

        var result = _sut.Evaluate(scores);

        result.Should().Be(MatchStrength.None);
    }

    [Fact]
    public void Evaluate_WithVeryLowScores_ReturnsNone()
    {
        var scores = CreateScores(
            titleScore: 0.1,
            authorScore: 0.1,
            yearScore: 0.1,
            keywordScore: 0.1);

        var result = _sut.Evaluate(scores);

        result.Should().Be(MatchStrength.None);
    }

    [Fact]
    public void Evaluate_WithMissingStrategies_HandlesGracefully()
    {
        var scores = new Dictionary<string, StrategyScore>
        {
            ["Title"] = StrategyScore.Create(0.5, 0.4, "test")
        };

        var result = _sut.Evaluate(scores);

        result.Should().NotBe(MatchStrength.None);
    }

    private static Dictionary<string, StrategyScore> CreateScores(
        double titleScore, double authorScore, double yearScore, double keywordScore)
    {
        return new Dictionary<string, StrategyScore>
        {
            ["Title"] = StrategyScore.Create(titleScore, DomainConstants.Matching.TitleWeight, "Title reason"),
            ["Author"] = StrategyScore.Create(authorScore, DomainConstants.Matching.AuthorWeight, "Author reason"),
            ["Year"] = StrategyScore.Create(yearScore, DomainConstants.Matching.YearWeight, "Year reason"),
            ["Keyword"] = StrategyScore.Create(keywordScore, DomainConstants.Matching.KeywordWeight, "Keyword reason")
        };
    }
}
