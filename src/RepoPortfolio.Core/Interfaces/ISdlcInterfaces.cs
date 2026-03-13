using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Core.Interfaces;

/// <summary>
/// Interface for LLM-powered repository analysis.
/// </summary>
public interface IRepoAnalyzer
{
    /// <summary>
    /// Analyze a repository using LLM to generate insights.
    /// </summary>
    Task<RepoInsights> AnalyzeAsync(Repository repo, CancellationToken ct = default);
    
    /// <summary>
    /// Analyze multiple repositories in batch.
    /// </summary>
    Task<IReadOnlyList<RepoInsights>> AnalyzeBatchAsync(
        IEnumerable<Repository> repos, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Get cached insights if available and not stale.
    /// </summary>
    Task<RepoInsights?> GetCachedInsightsAsync(Guid repositoryId, CancellationToken ct = default);
    
    /// <summary>
    /// Check if analysis is available (LLM configured).
    /// </summary>
    bool IsAvailable { get; }
}

/// <summary>
/// Interface for storing SDLC-related data.
/// </summary>
public interface ISdlcStore
{
    // Insights
    Task SaveInsightsAsync(RepoInsights insights, CancellationToken ct = default);
    Task<RepoInsights?> GetLatestInsightsAsync(Guid repositoryId, CancellationToken ct = default);
    Task<IReadOnlyList<RepoInsights>> GetInsightsHistoryAsync(Guid repositoryId, int limit = 10, CancellationToken ct = default);
    
    // Status Reports
    Task SaveReportAsync(RepoStatusReport report, CancellationToken ct = default);
    Task<RepoStatusReport?> GetLatestReportAsync(string repositoryFullName, CancellationToken ct = default);
    Task<IReadOnlyList<RepoStatusReport>> GetReportsAsync(string repositoryFullName, int limit = 10, CancellationToken ct = default);
    
    // Health Summaries
    Task<IReadOnlyList<RepoHealthSummary>> GetHealthSummariesAsync(CancellationToken ct = default);
    Task<RepoHealthSummary?> GetHealthSummaryAsync(Guid repositoryId, CancellationToken ct = default);
}
