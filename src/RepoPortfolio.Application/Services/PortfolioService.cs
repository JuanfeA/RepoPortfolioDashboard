using RepoPortfolio.Core.Interfaces;
using RepoPortfolio.Core.Models;
using RepoPortfolio.Core.Scoring;

namespace RepoPortfolio.Application.Services;

/// <summary>
/// Main service for portfolio operations.
/// Orchestrates Core logic with Infrastructure.
/// </summary>
public class PortfolioService
{
    private readonly IRepositoryStore _store;
    private readonly IGitHubClient _github;
    private readonly IScoringEngine _scoring;

    public PortfolioService(
        IRepositoryStore store,
        IGitHubClient github,
        IScoringEngine scoring)
    {
        _store = store;
        _github = github;
        _scoring = scoring;
    }

    /// <summary>
    /// Sync repositories from GitHub and calculate scores.
    /// </summary>
    public async Task<SyncResult> SyncRepositoriesAsync(string owner, CancellationToken ct = default)
    {
        var result = new SyncResult { Owner = owner, StartedAt = DateTime.UtcNow };

        try
        {
            // Check rate limit first
            var rateLimit = await _github.GetRateLimitAsync(ct);
            if (rateLimit.Remaining < 10)
            {
                result.Error = $"Rate limit too low: {rateLimit.Remaining} remaining";
                return result;
            }

            // Fetch repositories from GitHub
            var repos = await _github.GetRepositoriesAsync(owner, ct);
            result.FetchedCount = repos.Count;

            // Save repositories
            await _store.SaveManyAsync(repos, ct);

            // Calculate and save scores
            var criteria = await _store.GetActiveCriteriaAsync(ct);
            var scores = _scoring.CalculateScores(repos, criteria).ToList();
            await _store.SaveManyScoresAsync(scores, ct);

            result.ScoredCount = scores.Count;
            result.CompletedAt = DateTime.UtcNow;
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Get portfolio overview with all repositories and their latest scores.
    /// </summary>
    public async Task<PortfolioOverview> GetOverviewAsync(CancellationToken ct = default)
    {
        var repos = await _store.GetAllAsync(ct);
        var items = new List<RepositorySummary>();

        foreach (var repo in repos)
        {
            var score = await _store.GetLatestScoreAsync(repo.Id, ct);
            items.Add(new RepositorySummary
            {
                Repository = repo,
                LatestScore = score
            });
        }

        return new PortfolioOverview
        {
            Repositories = items,
            TotalCount = items.Count,
            AverageScore = items.Where(i => i.LatestScore != null)
                               .Select(i => i.LatestScore!.TotalScore)
                               .DefaultIfEmpty(0)
                               .Average(),
            HealthDistribution = items
                .Where(i => i.LatestScore != null)
                .GroupBy(i => i.LatestScore!.Health)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    /// <summary>
    /// Get detailed information for a single repository.
    /// </summary>
    public async Task<RepositoryDetail?> GetRepositoryDetailAsync(Guid id, CancellationToken ct = default)
    {
        var repo = await _store.GetByIdAsync(id, ct);
        if (repo == null) return null;

        var latestScore = await _store.GetLatestScoreAsync(id, ct);
        var scoreHistory = await _store.GetScoreHistoryAsync(id, 30, ct);

        return new RepositoryDetail
        {
            Repository = repo,
            LatestScore = latestScore,
            ScoreHistory = scoreHistory.ToList()
        };
    }

    /// <summary>
    /// Recalculate scores for all repositories with current criteria.
    /// </summary>
    public async Task<int> RecalculateAllScoresAsync(CancellationToken ct = default)
    {
        var repos = await _store.GetAllAsync(ct);
        var criteria = await _store.GetActiveCriteriaAsync(ct);
        var scores = _scoring.CalculateScores(repos, criteria).ToList();
        await _store.SaveManyScoresAsync(scores, ct);
        return scores.Count;
    }

    /// <summary>
    /// Initialize default scoring criteria if none exist.
    /// </summary>
    public async Task InitializeDefaultCriteriaAsync(CancellationToken ct = default)
    {
        var existing = await _store.GetAllCriteriaAsync(ct);
        if (existing.Count == 0)
        {
            var defaults = DefaultCriteriaFactory.CreateDefaultCriteria();
            await _store.SaveManyCriteriaAsync(defaults, ct);
        }
    }

    /// <summary>
    /// Seed demo repositories for testing purposes.
    /// </summary>
    public async Task<int> SeedDemoRepositoriesAsync(CancellationToken ct = default)
    {
        var existing = await _store.GetAllAsync(ct);
        if (existing.Count > 0) return 0; // Already has data

        var demoRepos = CreateDemoRepositories();
        await _store.SaveManyAsync(demoRepos, ct);

        // Calculate and save scores
        var criteria = await _store.GetActiveCriteriaAsync(ct);
        var scores = _scoring.CalculateScores(demoRepos, criteria).ToList();
        await _store.SaveManyScoresAsync(scores, ct);

        return demoRepos.Count;
    }

    private static List<Repository> CreateDemoRepositories()
    {
        return
        [
            // High quality, mature project
            new Repository
            {
                Id = Guid.NewGuid(),
                Name = "enterprise-api",
                FullName = "myorg/enterprise-api",
                Description = "Production-grade REST API with full CI/CD, excellent documentation",
                PrimaryLanguage = "C#",
                Topics = ["api", "dotnet", "enterprise", "production"],
                CommitCount = 1250,
                OpenIssueCount = 5,
                OpenPullRequestCount = 2,
                ContributorCount = 8,
                LastCommitDate = DateTime.UtcNow.AddDays(-2),
                LastReleaseDate = DateTime.UtcNow.AddDays(-14),
                HasCiCd = true,
                HasTests = true,
                HasReadme = true,
                HasLicense = true,
                HasContributing = true,
                TestCoverage = 85.5,
                VulnerabilityCount = 0,
                OutdatedDependencyCount = 2,
                IsArchived = false,
                IsFork = false,
                DefaultBranch = "main",
                StarCount = 245,
                ForkCount = 45,
                CreatedAt = DateTime.UtcNow.AddYears(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                SyncedAt = DateTime.UtcNow,
                Maturity = MaturityLevel.Production
            },

            // Active development, good quality
            new Repository
            {
                Id = Guid.NewGuid(),
                Name = "web-dashboard",
                FullName = "myorg/web-dashboard",
                Description = "React dashboard with TypeScript, actively developed",
                PrimaryLanguage = "TypeScript",
                Topics = ["react", "typescript", "dashboard", "frontend"],
                CommitCount = 580,
                OpenIssueCount = 12,
                OpenPullRequestCount = 4,
                ContributorCount = 4,
                LastCommitDate = DateTime.UtcNow.AddDays(-1),
                LastReleaseDate = DateTime.UtcNow.AddDays(-30),
                HasCiCd = true,
                HasTests = true,
                HasReadme = true,
                HasLicense = true,
                HasContributing = false,
                TestCoverage = 72.0,
                VulnerabilityCount = 1,
                OutdatedDependencyCount = 5,
                IsArchived = false,
                IsFork = false,
                DefaultBranch = "main",
                StarCount = 89,
                ForkCount = 12,
                CreatedAt = DateTime.UtcNow.AddYears(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                SyncedAt = DateTime.UtcNow,
                Maturity = MaturityLevel.Beta
            },

            // Early stage, needs work
            new Repository
            {
                Id = Guid.NewGuid(),
                Name = "cli-tools",
                FullName = "myorg/cli-tools",
                Description = "Command line utilities - work in progress",
                PrimaryLanguage = "Go",
                Topics = ["cli", "tools", "golang"],
                CommitCount = 85,
                OpenIssueCount = 8,
                OpenPullRequestCount = 1,
                ContributorCount = 2,
                LastCommitDate = DateTime.UtcNow.AddDays(-15),
                LastReleaseDate = null,
                HasCiCd = false,
                HasTests = true,
                HasReadme = true,
                HasLicense = false,
                HasContributing = false,
                TestCoverage = 45.0,
                VulnerabilityCount = 0,
                OutdatedDependencyCount = 1,
                IsArchived = false,
                IsFork = false,
                DefaultBranch = "main",
                StarCount = 12,
                ForkCount = 3,
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-15),
                SyncedAt = DateTime.UtcNow,
                Maturity = MaturityLevel.Development
            },

            // Stale, needs attention
            new Repository
            {
                Id = Guid.NewGuid(),
                Name = "legacy-service",
                FullName = "myorg/legacy-service",
                Description = "Old microservice, needs migration",
                PrimaryLanguage = "Java",
                Topics = ["legacy", "microservice"],
                CommitCount = 420,
                OpenIssueCount = 34,
                OpenPullRequestCount = 0,
                ContributorCount = 1,
                LastCommitDate = DateTime.UtcNow.AddMonths(-8),
                LastReleaseDate = DateTime.UtcNow.AddYears(-1),
                HasCiCd = true,
                HasTests = true,
                HasReadme = true,
                HasLicense = true,
                HasContributing = false,
                TestCoverage = 55.0,
                VulnerabilityCount = 5,
                OutdatedDependencyCount = 23,
                IsArchived = false,
                IsFork = false,
                DefaultBranch = "master",
                StarCount = 45,
                ForkCount = 8,
                CreatedAt = DateTime.UtcNow.AddYears(-5),
                UpdatedAt = DateTime.UtcNow.AddMonths(-8),
                SyncedAt = DateTime.UtcNow,
                Maturity = MaturityLevel.Maintenance
            },

            // Fork with minimal changes
            new Repository
            {
                Id = Guid.NewGuid(),
                Name = "awesome-library-fork",
                FullName = "myorg/awesome-library-fork",
                Description = "Fork of awesome-library with custom patches",
                PrimaryLanguage = "Python",
                Topics = ["fork", "python"],
                CommitCount = 12,
                OpenIssueCount = 0,
                OpenPullRequestCount = 0,
                ContributorCount = 1,
                LastCommitDate = DateTime.UtcNow.AddMonths(-2),
                LastReleaseDate = null,
                HasCiCd = false,
                HasTests = false,
                HasReadme = true,
                HasLicense = true,
                HasContributing = false,
                TestCoverage = null,
                VulnerabilityCount = 0,
                OutdatedDependencyCount = 0,
                IsArchived = false,
                IsFork = true,
                DefaultBranch = "main",
                StarCount = 0,
                ForkCount = 0,
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UpdatedAt = DateTime.UtcNow.AddMonths(-2),
                SyncedAt = DateTime.UtcNow,
                Maturity = MaturityLevel.Unknown
            },

            // Archived project
            new Repository
            {
                Id = Guid.NewGuid(),
                Name = "deprecated-sdk",
                FullName = "myorg/deprecated-sdk",
                Description = "ARCHIVED: Old SDK, use v2 instead",
                PrimaryLanguage = "JavaScript",
                Topics = ["archived", "deprecated", "sdk"],
                CommitCount = 890,
                OpenIssueCount = 45,
                OpenPullRequestCount = 12,
                ContributorCount = 3,
                LastCommitDate = DateTime.UtcNow.AddYears(-2),
                LastReleaseDate = DateTime.UtcNow.AddYears(-2),
                HasCiCd = true,
                HasTests = true,
                HasReadme = true,
                HasLicense = true,
                HasContributing = true,
                TestCoverage = 68.0,
                VulnerabilityCount = 12,
                OutdatedDependencyCount = 45,
                IsArchived = true,
                IsFork = false,
                DefaultBranch = "master",
                StarCount = 234,
                ForkCount = 67,
                CreatedAt = DateTime.UtcNow.AddYears(-6),
                UpdatedAt = DateTime.UtcNow.AddYears(-2),
                SyncedAt = DateTime.UtcNow,
                Maturity = MaturityLevel.Deprecated
            },

            // New experimental project
            new Repository
            {
                Id = Guid.NewGuid(),
                Name = "ml-experiment",
                FullName = "myorg/ml-experiment",
                Description = "Machine learning experiments and prototypes",
                PrimaryLanguage = "Python",
                Topics = ["ml", "ai", "experiment", "jupyter"],
                CommitCount = 45,
                OpenIssueCount = 2,
                OpenPullRequestCount = 1,
                ContributorCount = 2,
                LastCommitDate = DateTime.UtcNow.AddDays(-5),
                LastReleaseDate = null,
                HasCiCd = false,
                HasTests = false,
                HasReadme = true,
                HasLicense = false,
                HasContributing = false,
                TestCoverage = null,
                VulnerabilityCount = 0,
                OutdatedDependencyCount = 3,
                IsArchived = false,
                IsFork = false,
                DefaultBranch = "main",
                StarCount = 5,
                ForkCount = 1,
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                SyncedAt = DateTime.UtcNow,
                Maturity = MaturityLevel.Prototype
            },

            // Documentation repo
            new Repository
            {
                Id = Guid.NewGuid(),
                Name = "docs",
                FullName = "myorg/docs",
                Description = "Organization documentation and guides",
                PrimaryLanguage = "Markdown",
                Topics = ["documentation", "guides"],
                CommitCount = 320,
                OpenIssueCount = 3,
                OpenPullRequestCount = 2,
                ContributorCount = 6,
                LastCommitDate = DateTime.UtcNow.AddDays(-3),
                LastReleaseDate = null,
                HasCiCd = true,
                HasTests = false,
                HasReadme = true,
                HasLicense = true,
                HasContributing = true,
                TestCoverage = null,
                VulnerabilityCount = 0,
                OutdatedDependencyCount = 0,
                IsArchived = false,
                IsFork = false,
                DefaultBranch = "main",
                StarCount = 28,
                ForkCount = 15,
                CreatedAt = DateTime.UtcNow.AddYears(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                SyncedAt = DateTime.UtcNow,
                Maturity = MaturityLevel.Production
            }
        ];
    }

    #region SDLC & Reports

    /// <summary>
    /// Process a status report from a repository's CI/CD pipeline.
    /// </summary>
    public async Task<ReportResult> ProcessReportAsync(RepoStatusReport report, CancellationToken ct = default)
    {
        // Find or create the repository
        var repos = await _store.GetAllAsync(ct);
        var repo = repos.FirstOrDefault(r => 
            r.FullName.Equals(report.RepositoryFullName, StringComparison.OrdinalIgnoreCase));

        if (repo == null)
        {
            // Unknown repo - store report but can't score
            return new ReportResult
            {
                Success = true,
                Message = $"Report received for unknown repository {report.RepositoryFullName}. Consider syncing first."
            };
        }

        // Update repo with reported metrics
        bool updated = false;
        
        if (report.HasReadme.HasValue) { repo.HasReadme = report.HasReadme.Value; updated = true; }
        if (report.HasLicense.HasValue) { repo.HasLicense = report.HasLicense.Value; updated = true; }
        if (report.HasTests.HasValue) { repo.HasTests = report.HasTests.Value; updated = true; }
        if (report.HasCi.HasValue) { repo.HasCiCd = report.HasCi.Value; updated = true; }
        if (report.OpenIssues.HasValue) { repo.OpenIssueCount = report.OpenIssues.Value; updated = true; }
        if (report.OpenPrs.HasValue) { repo.OpenPullRequestCount = report.OpenPrs.Value; updated = true; }
        if (report.TestCoverage.HasValue) { repo.TestCoverage = report.TestCoverage.Value; updated = true; }
        if (report.Commits30Days.HasValue) { repo.CommitCount = report.Commits30Days.Value; updated = true; }
        
        if (report.ReportedPhase.HasValue)
        {
            repo.Maturity = report.ReportedPhase.Value switch
            {
                SdlcPhase.Ideation or SdlcPhase.Planning => MaturityLevel.Prototype,
                SdlcPhase.Development => MaturityLevel.Development,
                SdlcPhase.Testing => MaturityLevel.Beta,
                SdlcPhase.Release or SdlcPhase.Maintenance => MaturityLevel.Production,
                SdlcPhase.Deprecated => MaturityLevel.Deprecated,
                _ => repo.Maturity
            };
            updated = true;
        }

        if (updated)
        {
            repo.SyncedAt = DateTime.UtcNow;
            await _store.SaveAsync(repo, ct);
            
            // Recalculate score
            var criteria = await _store.GetActiveCriteriaAsync(ct);
            var scores = _scoring.CalculateScores([repo], criteria).ToList();
            if (scores.Count > 0)
            {
                await _store.SaveManyScoresAsync(scores, ct);
                return new ReportResult
                {
                    Success = true,
                    NewScore = scores[0].TotalScore,
                    Message = "Repository updated and rescored"
                };
            }
        }

        return new ReportResult
        {
            Success = true,
            Message = "Report received"
        };
    }

    /// <summary>
    /// Get status reports for a repository.
    /// </summary>
    public Task<IReadOnlyList<RepoStatusReport>> GetReportsAsync(
        string repositoryFullName, 
        int limit = 10, 
        CancellationToken ct = default)
    {
        // TODO: Implement report storage
        return Task.FromResult<IReadOnlyList<RepoStatusReport>>([]);
    }

    /// <summary>
    /// Get health summary for a repository.
    /// </summary>
    public async Task<RepoHealthSummary?> GetHealthSummaryAsync(
        string repositoryFullName, 
        CancellationToken ct = default)
    {
        var repos = await _store.GetAllAsync(ct);
        var repo = repos.FirstOrDefault(r => 
            r.FullName.Equals(repositoryFullName, StringComparison.OrdinalIgnoreCase));

        if (repo == null) return null;

        var score = await _store.GetLatestScoreAsync(repo.Id, ct);

        return new RepoHealthSummary
        {
            Repository = repo,
            LatestScore = score,
            LatestInsights = null, // TODO: Implement insights storage
            LatestReport = null    // TODO: Implement report storage
        };
    }

    /// <summary>
    /// Analyze a repository using LLM.
    /// </summary>
    public Task<RepoInsights?> AnalyzeRepositoryAsync(
        string repositoryFullName, 
        bool force = false, 
        CancellationToken ct = default)
    {
        // TODO: Implement LLM analysis
        throw new InvalidOperationException("LLM analysis not configured. Set OPENAI_API_KEY or ANTHROPIC_API_KEY.");
    }

    #endregion
}

public class ReportResult
{
    public bool Success { get; set; }
    public double? NewScore { get; set; }
    public string? Message { get; set; }
}

#region DTOs

public class SyncResult
{
    public required string Owner { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int FetchedCount { get; set; }
    public int ScoredCount { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public class PortfolioOverview
{
    public required List<RepositorySummary> Repositories { get; set; }
    public int TotalCount { get; set; }
    public double AverageScore { get; set; }
    public Dictionary<HealthStatus, int> HealthDistribution { get; set; } = [];
}

public class RepositorySummary
{
    public required Repository Repository { get; set; }
    public Score? LatestScore { get; set; }
}

public class RepositoryDetail
{
    public required Repository Repository { get; set; }
    public Score? LatestScore { get; set; }
    public List<Score> ScoreHistory { get; set; } = [];
}

#endregion
