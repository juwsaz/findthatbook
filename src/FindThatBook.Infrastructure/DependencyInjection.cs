using FindThatBook.Domain.Interfaces;
using FindThatBook.Infrastructure.Configuration;
using FindThatBook.Infrastructure.ExternalServices.Gemini;
using FindThatBook.Infrastructure.ExternalServices.OpenLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FindThatBook.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure settings
        services.Configure<GeminiSettings>(configuration.GetSection(GeminiSettings.SectionName));
        services.Configure<OpenLibrarySettings>(configuration.GetSection(OpenLibrarySettings.SectionName));

        // Register HTTP clients
        services.AddHttpClient<IAiExtractionService, GeminiExtractionService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IBookSearchService, OpenLibrarySearchService>(client =>
        {
            var settings = configuration.GetSection(OpenLibrarySettings.SectionName).Get<OpenLibrarySettings>()
                ?? new OpenLibrarySettings();
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("User-Agent", "FindThatBook/1.0 (InfoTrack Assessment)");
        });

        return services;
    }
}
