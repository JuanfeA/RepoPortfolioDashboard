using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using RepoPortfolio.Application.Services;
using RepoPortfolio.Core.Interfaces;
using RepoPortfolio.Core.Scoring;
using RepoPortfolio.Infrastructure.AI;
using RepoPortfolio.Infrastructure.Data;
using RepoPortfolio.Infrastructure.GitHub;

namespace RepoPortfolio.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all RepoPortfolio services with SQLite storage.
    /// </summary>
    public static IServiceCollection AddRepoPortfolio(
        this IServiceCollection services,
        string? databasePath = null,
        string? gitHubToken = null,
        Action<AiAnalyzerOptions>? configureAi = null)
    {
        var dbPath = databasePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RepoPortfolio",
            "portfolio.db");
        
        var sdlcDbPath = Path.Combine(
            Path.GetDirectoryName(dbPath)!,
            "sdlc.db");
        
        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        
        // Register DbContext
        services.AddDbContext<PortfolioDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));
        
        // Register SDLC DbContext
        services.AddDbContext<SdlcDbContext>(options =>
            options.UseSqlite($"Data Source={sdlcDbPath}"));
        
        // Register Core services
        services.AddSingleton<IScoringEngine, ScoringEngine>();
        
        // Register Infrastructure services
        services.AddScoped<IRepositoryStore, SqliteRepositoryStore>();
        services.AddScoped<ISdlcStore, SqliteSdlcStore>();
        services.AddSingleton<IGitHubClient>(_ => new OctokitGitHubClient(gitHubToken));
        
        // Register Memory Cache
        services.AddMemoryCache();
        
        // Register AI Analyzer
        var aiOptions = new AiAnalyzerOptions();
        configureAi?.Invoke(aiOptions);
        
        // Auto-configure from environment if not explicitly set
        if (!aiOptions.IsConfigured)
        {
            var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            var anthropicKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            
            if (!string.IsNullOrWhiteSpace(openAiKey))
            {
                aiOptions.Provider = AiProvider.OpenAI;
                aiOptions.ApiKey = openAiKey;
            }
            else if (!string.IsNullOrWhiteSpace(anthropicKey))
            {
                aiOptions.Provider = AiProvider.Anthropic;
                aiOptions.ApiKey = anthropicKey;
            }
        }
        
        services.AddSingleton(aiOptions);
        services.AddScoped<IRepoAnalyzer>(sp =>
        {
            var options = sp.GetRequiredService<AiAnalyzerOptions>();
            var cache = sp.GetRequiredService<IMemoryCache>();
            var store = sp.GetRequiredService<ISdlcStore>();
            return new AiRepoAnalyzer(options, cache, store);
        });
        
        // Register Application services
        services.AddScoped<PortfolioService>();
        
        return services;
    }

    /// <summary>
    /// Initialize the database (create if not exists) and seed demo data.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, bool seedDemoData = true)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        await db.Database.EnsureCreatedAsync();
        
        // Initialize SDLC database
        var sdlcDb = scope.ServiceProvider.GetRequiredService<SdlcDbContext>();
        await sdlcDb.Database.EnsureCreatedAsync();
        
        // Initialize default criteria
        var portfolio = scope.ServiceProvider.GetRequiredService<PortfolioService>();
        await portfolio.InitializeDefaultCriteriaAsync();

        // Seed demo data if enabled and no data exists
        if (seedDemoData)
        {
            var seeded = await portfolio.SeedDemoRepositoriesAsync();
            if (seeded > 0)
            {
                Console.WriteLine($"Seeded {seeded} demo repositories");
            }
        }
    }
}
