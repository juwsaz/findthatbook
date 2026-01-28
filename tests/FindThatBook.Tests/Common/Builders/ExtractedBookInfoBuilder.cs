using FindThatBook.Domain.ValueObjects;

namespace FindThatBook.Tests.Common.Builders;

public class ExtractedBookInfoBuilder
{
    private string? _title;
    private string? _author;
    private int? _year;
    private List<string> _keywords = new();
    private string? _originalQuery;

    public ExtractedBookInfoBuilder WithTitle(string? title)
    {
        _title = title;
        return this;
    }

    public ExtractedBookInfoBuilder WithAuthor(string? author)
    {
        _author = author;
        return this;
    }

    public ExtractedBookInfoBuilder WithYear(int? year)
    {
        _year = year;
        return this;
    }

    public ExtractedBookInfoBuilder WithKeywords(params string[] keywords)
    {
        _keywords = keywords.ToList();
        return this;
    }

    public ExtractedBookInfoBuilder WithOriginalQuery(string? query)
    {
        _originalQuery = query;
        return this;
    }

    public ExtractedBookInfo Build() => new()
    {
        Title = _title,
        Author = _author,
        Year = _year,
        Keywords = _keywords,
        OriginalQuery = _originalQuery
    };

    public static ExtractedBookInfoBuilder Default() => new();

    public static ExtractedBookInfo ForHobbit() => new ExtractedBookInfoBuilder()
        .WithTitle("The Hobbit")
        .WithAuthor("J.R.R. Tolkien")
        .WithYear(1937)
        .WithOriginalQuery("hobbit tolkien 1937")
        .Build();

    public static ExtractedBookInfo ForHuckleberryFinn() => new ExtractedBookInfoBuilder()
        .WithTitle("The Adventures of Huckleberry Finn")
        .WithAuthor("Mark Twain")
        .WithOriginalQuery("huckleberry finn mark twain")
        .Build();
}
