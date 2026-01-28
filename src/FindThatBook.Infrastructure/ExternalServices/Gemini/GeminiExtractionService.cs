using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FindThatBook.Domain.Exceptions;
using FindThatBook.Domain.Interfaces;
using FindThatBook.Domain.ValueObjects;
using FindThatBook.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FindThatBook.Infrastructure.ExternalServices.Gemini;

public class GeminiExtractionService : IAiExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiExtractionService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GeminiExtractionService(
        HttpClient httpClient,
        IOptions<GeminiSettings> settings,
        ILogger<GeminiExtractionService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ExtractedBookInfo> ExtractBookInfoAsync(string dirtyQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = CreateExtractionPrompt(dirtyQuery);
            var response = await CallGeminiApiAsync(prompt, cancellationToken);
            var extractedInfo = ParseGeminiResponse(response, dirtyQuery);

            _logger.LogInformation(
                "Extracted book info from query '{Query}': Title='{Title}', Author='{Author}', Keywords=[{Keywords}]",
                dirtyQuery, extractedInfo.Title, extractedInfo.Author, string.Join(", ", extractedInfo.Keywords));

            return extractedInfo;
        }
        catch (AiExtractionException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Gemini API for query: {Query}", dirtyQuery);
            throw AiExtractionException.ApiCallFailed("Gemini", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout calling Gemini API for query: {Query}", dirtyQuery);
            throw AiExtractionException.ApiCallFailed("Gemini", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract book info from query: {Query}", dirtyQuery);
            // Fallback: return basic extraction instead of throwing
            return CreateFallbackExtraction(dirtyQuery);
        }
    }

    private static string CreateExtractionPrompt(string query)
    {
        return $@"You are a book identification assistant. Extract structured information from the following book search query.

Query: ""{query}""

Extract the following information and respond ONLY with a valid JSON object (no markdown, no code blocks):
{{
    ""title"": ""extracted book title or null if not identifiable"",
    ""author"": ""extracted author name or null if not identifiable"",
    ""year"": extracted year as number or null if not present,
    ""keywords"": [""array"", ""of"", ""relevant"", ""keywords""]
}}

Rules:
- For author names, use the full name if possible (e.g., ""Mark Twain"" not ""Twain"")
- Keywords should include any descriptive terms like ""illustrated"", ""first edition"", etc.
- If the query seems to contain a misspelling, try to identify the correct title/author
- Common patterns: ""author title"", ""title author"", ""title year"", etc.
- Examples of queries and expected extractions:
  - ""mark huckleberry"" -> title: ""The Adventures of Huckleberry Finn"", author: ""Mark Twain""
  - ""tolkien hobbit illustrated 1937"" -> title: ""The Hobbit"", author: ""J.R.R. Tolkien"", year: 1937, keywords: [""illustrated""]

Respond with ONLY the JSON object, nothing else.";
    }

    private async Task<string> CallGeminiApiAsync(string prompt, CancellationToken cancellationToken)
    {
        var url = $"{_settings.BaseUrl}/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

        var request = new GeminiRequest
        {
            Contents = new[]
            {
                new GeminiContent
                {
                    Parts = new[] { new GeminiPart { Text = prompt } }
                }
            },
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = 0.1,
                MaxOutputTokens = 256
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, request, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw AiExtractionException.InvalidResponse($"API returned status {response.StatusCode}: {content}");
        }

        var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, cancellationToken);

        return geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
            ?? throw AiExtractionException.InvalidResponse("Empty response from Gemini API");
    }

    private ExtractedBookInfo ParseGeminiResponse(string response, string originalQuery)
    {
        try
        {
            // Clean up response (remove any markdown code blocks if present)
            var cleanJson = response
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var parsed = JsonSerializer.Deserialize<GeminiExtractedInfo>(cleanJson, JsonOptions);

            return new ExtractedBookInfo
            {
                Title = parsed?.Title,
                Author = parsed?.Author,
                Year = parsed?.Year,
                Keywords = parsed?.Keywords ?? new List<string>(),
                OriginalQuery = originalQuery
            };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Gemini response: {Response}", response);
            throw AiExtractionException.ParsingFailed(response, ex);
        }
    }

    private static ExtractedBookInfo CreateFallbackExtraction(string query)
    {
        // Simple fallback: split by spaces and try to identify parts
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return new ExtractedBookInfo
        {
            Title = null,
            Author = null,
            Keywords = words.ToList(),
            OriginalQuery = query
        };
    }

    #region Gemini API Models

    private class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public GeminiContent[] Contents { get; set; } = Array.Empty<GeminiContent>();

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public GeminiPart[] Parts { get; set; } = Array.Empty<GeminiPart>();
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private class GeminiGenerationConfig
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; set; }
    }

    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public GeminiCandidate[]? Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private class GeminiExtractedInfo
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Keywords { get; set; }
    }

    #endregion
}
