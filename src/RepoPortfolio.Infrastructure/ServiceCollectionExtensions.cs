using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RepoPortfolio.Application.Services;
using RepoPortfolio.Core.Interfaces;
using RepoPortfolio.Core.Scoring;
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
        string? gitHubToken = null)
    {
        var dbPath = databasePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RepoPortfolio",
            "portfolio.db");
        
        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        
        // Register DbContext
        services.AddDbContext<PortfolioDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));  
        
        // Register Core services
        services.AddSingleton<IScoringEngine, ScoringEngine>();
        
        // Register Infrastructure services
        services.AddScoped<IRepositoryStore, SqliteRepositoryStore>();
        services.AddSingleton<IGitHubClient>(_ => new OctokitGitHubClient(gitHubToken));
        
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
