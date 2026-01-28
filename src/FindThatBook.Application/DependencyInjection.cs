using FindThatBook.Application.Services;
using FindThatBook.Application.Services.Matching;
using FindThatBook.Application.Services.Matching.Strategies;
using FindThatBook.Application.UseCases;
using FindThatBook.Domain.Interfaces;
using FindThatBook.Domain.Interfaces.Matching;
using Microsoft.Extensions.DependencyInjection;

namespace FindThatBook.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register text normalizer
        services.AddSingleton<ITextNormalizer, TextNormalizer>();

        // Register matching strategies
        services.AddSingleton<IMatchingStrategy, TitleMatchingStrategy>();
        services.AddSingleton<IMatchingStrategy, AuthorMatchingStrategy>();
        services.AddSingleton<IMatchingStrategy, YearMatchingStrategy>();
        services.AddSingleton<IMatchingStrategy, KeywordMatchingStrategy>();

        // Register match strength evaluator
        services.AddSingleton<IMatchStrengthEvaluator, MatchStrengthEvaluator>();

        // Register strategy registry
        services.AddSingleton<MatchingStrategyRegistry>();

        // Register application services
        services.AddScoped<IBookMatchingService, BookMatchingService>();
        services.AddScoped<SearchBooksUseCase>();

        return services;
    }
}
