using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Core.Interfaces;

/// <summary>
/// Interface for fetching repository data from GitHub.
/// Implemented by infrastructure layer using Octokit or REST API.
/// </summary>
public interface IGitHubClient
{
    /// <summary>
    /// Get all repositories for a user or organization.
    /// </summary>
    Task<IReadOnlyList<Repository>> GetRepositoriesAsync(string owner, CancellationToken ct = default);
    
    /// <summary>
    /// Get detailed information for a specific repository.
    /// </summary>
    Task<Repository?> GetRepositoryAsync(string owner, string name, CancellationToken ct = default);
    
    /// <summary>
    /// Get activity metrics for a repository.
    /// </summary>
    Task<RepositoryActivity> GetActivityAsync(string owner, string name, int days = 30, CancellationToken ct = default);
    
    /// <summary>
    /// Check rate limit status.
    /// </summary>
    Task<RateLimitInfo> GetRateLimitAsync(CancellationToken ct = default);
}

/// <summary>
/// Activity metrics for a repository within a time period.
/// </summary>
public class RepositoryActivity
{
    public int CommitCount { get; set; }
    public int PullRequestCount { get; set; }
    public int IssueCount { get; set; }
    public int ContributorCount { get; set; }
    public DateTime? LastCommit { get; set; }
    public DateTime? LastRelease { get; set; }
}

/// <summary>
/// GitHub API rate limit information.
/// </summary>
public class RateLimitInfo
{
    public int Remaining { get; set; }
    public int Limit { get; set; }
    public DateTime ResetAt { get; set; }
}
