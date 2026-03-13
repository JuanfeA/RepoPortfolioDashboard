using Octokit;
using RepoPortfolio.Core.Interfaces;
using RepoPortfolio.Core.Models;
using Repository = RepoPortfolio.Core.Models.Repository;
using IGitHubClient = RepoPortfolio.Core.Interfaces.IGitHubClient;

namespace RepoPortfolio.Infrastructure.GitHub;

/// <summary>
/// GitHub API client implementation using Octokit.
/// </summary>
public class OctokitGitHubClient : IGitHubClient
{
    private readonly GitHubClient _client;

    public OctokitGitHubClient(string? token = null, string appName = "RepoPortfolioDashboard")
    {
        _client = new GitHubClient(new ProductHeaderValue(appName));
        
        if (!string.IsNullOrEmpty(token))
        {
            _client.Credentials = new Credentials(token);
        }
    }

    public async Task<IReadOnlyList<Repository>> GetRepositoriesAsync(string owner, CancellationToken ct = default)
    {
        var repos = new List<Repository>();

        try
        {
            // Try as organization first, fall back to user
            IReadOnlyList<Octokit.Repository> ghRepos;
            try
            {
                ghRepos = await _client.Repository.GetAllForOrg(owner);
            }
            catch (NotFoundException)
            {
                ghRepos = await _client.Repository.GetAllForUser(owner);
            }

            foreach (var ghRepo in ghRepos)
            {
                repos.Add(await MapToRepositoryAsync(ghRepo, ct));
            }
        }
        catch (RateLimitExceededException ex)
        {
            throw new InvalidOperationException($"GitHub rate limit exceeded. Resets at {ex.Reset}", ex);
        }

        return repos;
    }

    public async Task<Repository?> GetRepositoryAsync(string owner, string name, CancellationToken ct = default)
    {
        try
        {
            var ghRepo = await _client.Repository.Get(owner, name);
            return await MapToRepositoryAsync(ghRepo, ct);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task<RepositoryActivity> GetActivityAsync(string owner, string name, int days = 30, CancellationToken ct = default)
    {
        var since = DateTimeOffset.UtcNow.AddDays(-days);
        var activity = new RepositoryActivity();

        try
        {
            // Get commits
            var commits = await _client.Repository.Commit.GetAll(owner, name, new CommitRequest
            {
                Since = since
            });
            activity.CommitCount = commits.Count;
            activity.LastCommit = commits.FirstOrDefault()?.Commit?.Author?.Date.DateTime;

            // Get contributors
            var contributors = await _client.Repository.GetAllContributors(owner, name);
            activity.ContributorCount = contributors.Count;

            // Get PRs
            var prs = await _client.PullRequest.GetAllForRepository(owner, name, new PullRequestRequest
            {
                State = ItemStateFilter.All
            });
            activity.PullRequestCount = prs.Count(pr => pr.CreatedAt >= since);

            // Get issues
            var issues = await _client.Issue.GetAllForRepository(owner, name, new RepositoryIssueRequest
            {
                State = ItemStateFilter.All,
                Since = since
            });
            activity.IssueCount = issues.Count(i => i.PullRequest == null); // Exclude PRs

            // Get latest release
            try
            {
                var releases = await _client.Repository.Release.GetAll(owner, name);
                activity.LastRelease = releases.FirstOrDefault()?.PublishedAt?.DateTime;
            }
            catch (NotFoundException) { }
        }
        catch (RateLimitExceededException ex)
        {
            throw new InvalidOperationException($"GitHub rate limit exceeded. Resets at {ex.Reset}", ex);
        }

        return activity;
    }

    public async Task<RateLimitInfo> GetRateLimitAsync(CancellationToken ct = default)
    {
        var rateLimit = await _client.RateLimit.GetRateLimits();
        return new RateLimitInfo
        {
            Remaining = rateLimit.Resources.Core.Remaining,
            Limit = rateLimit.Resources.Core.Limit,
            ResetAt = rateLimit.Resources.Core.Reset.DateTime
        };
    }

    private async Task<Repository> MapToRepositoryAsync(Octokit.Repository ghRepo, CancellationToken ct)
    {
        var repo = new Repository
        {
            Name = ghRepo.Name,
            FullName = ghRepo.FullName,
            Description = ghRepo.Description,
            PrimaryLanguage = ghRepo.Language,
            Topics = ghRepo.Topics?.ToList() ?? [],
            DefaultBranch = ghRepo.DefaultBranch,
            StarCount = ghRepo.StargazersCount,
            ForkCount = ghRepo.ForksCount,
            OpenIssueCount = ghRepo.OpenIssuesCount, // Includes PRs in GitHub API
            IsArchived = ghRepo.Archived,
            IsFork = ghRepo.Fork,
            CreatedAt = ghRepo.CreatedAt.DateTime,
            UpdatedAt = ghRepo.UpdatedAt.DateTime,
            SyncedAt = DateTime.UtcNow
        };

        // Try to get additional metadata
        try
        {
            // Check for common quality indicators
            await TryGetQualityIndicatorsAsync(ghRepo.Owner.Login, ghRepo.Name, repo, ct);
        }
        catch
        {
            // Non-critical, continue with basic data
        }

        return repo;
    }

    private async Task TryGetQualityIndicatorsAsync(string owner, string name, Repository repo, CancellationToken ct)
    {
        try
        {
            var contents = await _client.Repository.Content.GetAllContents(owner, name);
            var rootFiles = contents.Select(c => c.Name.ToLowerInvariant()).ToHashSet();

            repo.HasReadme = rootFiles.Any(f => f.StartsWith("readme"));
            repo.HasLicense = rootFiles.Any(f => f.StartsWith("license"));
            repo.HasContributing = rootFiles.Any(f => f.StartsWith("contributing"));
        }
        catch (NotFoundException) { }

        // Check for CI/CD
        try
        {
            var workflows = await _client.Repository.Content.GetAllContents(owner, name, ".github/workflows");
            repo.HasCiCd = workflows.Count > 0;
        }
        catch (NotFoundException)
        {
            // Check for other CI systems
            try
            {
                var contents = await _client.Repository.Content.GetAllContents(owner, name);
                var rootFiles = contents.Select(c => c.Name.ToLowerInvariant()).ToHashSet();
                repo.HasCiCd = rootFiles.Contains(".travis.yml") ||
                              rootFiles.Contains("azure-pipelines.yml") ||
                              rootFiles.Contains("jenkinsfile") ||
                              rootFiles.Contains(".circleci");
            }
            catch { }
        }

        // Check for tests
        try
        {
            var contents = await _client.Repository.Content.GetAllContents(owner, name);
            var folderNames = contents.Where(c => c.Type == ContentType.Dir)
                                     .Select(c => c.Name.ToLowerInvariant())
                                     .ToHashSet();
            repo.HasTests = folderNames.Contains("tests") ||
                           folderNames.Contains("test") ||
                           folderNames.Contains("__tests__") ||
                           folderNames.Contains("spec");
        }
        catch (NotFoundException) { }

        // Note: Vulnerability alerts require GraphQL API or GitHub App permissions
        // For now, we skip this - can be added later with proper auth

        // Get commit count and last commit
        try
        {
            var commits = await _client.Repository.Commit.GetAll(owner, name, new CommitRequest());
            repo.CommitCount = commits.Count;
            repo.LastCommitDate = commits.FirstOrDefault()?.Commit?.Author?.Date.DateTime;
        }
        catch { }

        // Get last release
        try
        {
            var releases = await _client.Repository.Release.GetAll(owner, name);
            repo.LastReleaseDate = releases.FirstOrDefault()?.PublishedAt?.DateTime;
        }
        catch { }

        // Get contributor count
        try
        {
            var contributors = await _client.Repository.GetAllContributors(owner, name);
            repo.ContributorCount = contributors.Count;
        }
        catch { }

        // Get open PR count (separate from issues)
        try
        {
            var prs = await _client.PullRequest.GetAllForRepository(owner, name, new PullRequestRequest
            {
                State = ItemStateFilter.Open
            });
            repo.OpenPullRequestCount = prs.Count;
            repo.OpenIssueCount -= prs.Count; // Adjust issue count
        }
        catch { }
    }
}
