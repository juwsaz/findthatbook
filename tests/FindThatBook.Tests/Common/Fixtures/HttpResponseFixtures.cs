using System.Text.Json;

namespace FindThatBook.Tests.Common.Fixtures;

public static class HttpResponseFixtures
{
    public static class Gemini
    {
        public static string SuccessfulExtraction(string title, string author, int? year = null, params string[] keywords)
        {
            var innerJson = new
            {
                title,
                author,
                year,
                keywords = keywords.ToList()
            };
            var innerJsonText = JsonSerializer.Serialize(innerJson);

            return $$"""
            {
                "candidates": [
                    {
                        "content": {
                            "parts": [
                                {
                                    "text": {{JsonSerializer.Serialize(innerJsonText)}}
                                }
                            ]
                        }
                    }
                ]
            }
            """;
        }

        public static string EmptyResponse() => """
            {
                "candidates": []
            }
            """;

        public static string InvalidJsonResponse() => """
            {
                "candidates": [
                    {
                        "content": {
                            "parts": [
                                {
                                    "text": "This is not valid JSON"
                                }
                            ]
                        }
                    }
                ]
            }
            """;

        public static string MarkdownWrappedResponse(string title, string author)
        {
            var innerJson = new
            {
                title,
                author,
                year = (int?)null,
                keywords = new List<string>()
            };
            var innerJsonText = "```json\n" + JsonSerializer.Serialize(innerJson) + "\n```";

            return $$"""
            {
                "candidates": [
                    {
                        "content": {
                            "parts": [
                                {
                                    "text": {{JsonSerializer.Serialize(innerJsonText)}}
                                }
                            ]
                        }
                    }
                ]
            }
            """;
        }
    }

    public static class OpenLibrary
    {
        public static string SearchResponse(params (string key, string title, string[] authors, int? year, int? coverId)[] books)
        {
            var docs = books.Select(b => $$"""
                {
                    "key": "{{b.key}}",
                    "title": "{{b.title}}",
                    "author_name": [{{string.Join(", ", b.authors.Select(a => $"\"{a}\""))}}],
                    "first_publish_year": {{(b.year.HasValue ? b.year.Value.ToString() : "null")}},
                    "cover_i": {{(b.coverId.HasValue ? b.coverId.Value.ToString() : "null")}}
                }
                """);

            return $$"""
            {
                "numFound": {{books.Length}},
                "docs": [{{string.Join(", ", docs)}}]
            }
            """;
        }

        public static string EmptySearchResponse() => """
            {
                "numFound": 0,
                "docs": []
            }
            """;

        public static string SingleBookResponse(string key, string title, string author, int? year = null)
        {
            return SearchResponse((key, title, new[] { author }, year, null));
        }
    }
}
