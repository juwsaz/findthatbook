using FindThatBook.Domain.Entities;

namespace FindThatBook.Tests.Common.Builders;

public class BookBuilder
{
    private string _key = "/works/OL123";
    private string _title = "Test Book";
    private List<string> _authors = new() { "Test Author" };
    private int? _firstPublishYear = 2000;
    private string? _coverId = "12345";
    private List<string> _subjects = new();
    private List<string> _publishers = new();
    private string? _isbn;
    private int? _numberOfPages;
    private List<string> _languages = new();

    public BookBuilder WithKey(string key)
    {
        _key = key;
        return this;
    }

    public BookBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public BookBuilder WithAuthor(string author)
    {
        _authors = new List<string> { author };
        return this;
    }

    public BookBuilder WithAuthors(params string[] authors)
    {
        _authors = authors.ToList();
        return this;
    }

    public BookBuilder WithYear(int? year)
    {
        _firstPublishYear = year;
        return this;
    }

    public BookBuilder WithCoverId(string? coverId)
    {
        _coverId = coverId;
        return this;
    }

    public BookBuilder WithSubjects(params string[] subjects)
    {
        _subjects = subjects.ToList();
        return this;
    }

    public BookBuilder WithPublishers(params string[] publishers)
    {
        _publishers = publishers.ToList();
        return this;
    }

    public BookBuilder WithIsbn(string? isbn)
    {
        _isbn = isbn;
        return this;
    }

    public BookBuilder WithNumberOfPages(int? pages)
    {
        _numberOfPages = pages;
        return this;
    }

    public BookBuilder WithLanguages(params string[] languages)
    {
        _languages = languages.ToList();
        return this;
    }

    public Book Build() => new()
    {
        Key = _key,
        Title = _title,
        Authors = _authors,
        FirstPublishYear = _firstPublishYear,
        CoverId = _coverId,
        Subjects = _subjects,
        Publishers = _publishers,
        Isbn = _isbn,
        NumberOfPages = _numberOfPages,
        Languages = _languages
    };

    public static BookBuilder Default() => new();

    public static Book TheHobbit() => new BookBuilder()
        .WithKey("/works/OL27479W")
        .WithTitle("The Hobbit")
        .WithAuthor("J.R.R. Tolkien")
        .WithYear(1937)
        .WithCoverId("6335835")
        .WithSubjects("Fantasy", "Adventure")
        .Build();

    public static Book HuckleberryFinn() => new BookBuilder()
        .WithKey("/works/OL53908W")
        .WithTitle("The Adventures of Huckleberry Finn")
        .WithAuthor("Mark Twain")
        .WithYear(1884)
        .WithCoverId("8242128")
        .WithSubjects("Fiction", "American Literature")
        .Build();
}
