using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Infrastructure.AI;

/// <summary>
/// Base class for AI provider implementations.
/// Template Method pattern for consistent analysis workflow.
/// </summary>
public abstract class AiAnalyzerBase
{
    protected readonly AiAnalyzerOptions Options;

    protected AiAnalyzerBase(AiAnalyzerOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Check if this analyzer is available.
    /// </summary>
    public abstract bool IsAvailable { get; }

    /// <summary>
    /// Get the provider name for logging.
    /// </summary>
    public abstract string ProviderName { get; }

    /// <summary>
    /// Template method: Analyze a repository.
    /// </summary>
    public async Task<RepoInsights> AnalyzeAsync(Repository repo, CancellationToken ct = default)
    {
        var startTime = DateTime.UtcNow;
        
        var prompt = BuildAnalysisPrompt(repo);
        var response = await SendPromptAsync(prompt, ct);
        var insights = ParseResponse(response, repo.Id);
        
        insights.AnalysisDuration = DateTime.UtcNow - startTime;
        insights.AnalysisModel = Options.Model;
        insights.AnalysisPromptVersion = "1.0";
        
        return insights;
    }

    /// <summary>
    /// Build the analysis prompt for a repository.
    /// </summary>
    protected virtual string BuildAnalysisPrompt(Repository repo)
    {
        var jsonTemplate = @"{
    ""phase"": ""Unknown|Ideation|Planning|Development|Testing|Release|Maintenance|Deprecated"",
    ""phaseRationale"": ""Why this phase was determined"",
    ""maturityScore"": ""0-100"",
    ""maturityRationale"": ""Why this maturity score"",
    ""activeDevelopmentAreas"": [""area1"", ""area2""],
    ""technicalDebtIndicators"": [""indicator1"", ""indicator2""],
    ""riskFactors"": [""risk1"", ""risk2""],
    ""securityConcerns"": [""concern1""],
    ""recommendedActions"": [""action1"", ""action2""],
    ""improvementSuggestions"": [""suggestion1""],
    ""estimatedSprintCount"": null,
    ""milestonesSuggested"": [""milestone1""],
    ""blockersPredicted"": [""blocker1""]
}";

        return $"""
            Analyze this GitHub repository and provide SDLC insights:
            
            ## Repository Information
            - Name: {repo.FullName}
            - Description: {repo.Description ?? "No description"}
            - Primary Language: {repo.PrimaryLanguage ?? "Unknown"}
            - Topics: {string.Join(", ", repo.Topics)}
            - Created: {repo.CreatedAt:yyyy-MM-dd}
            - Last Commit: {repo.LastCommitDate?.ToString("yyyy-MM-dd") ?? "Unknown"}
            - Last Release: {repo.LastReleaseDate?.ToString("yyyy-MM-dd") ?? "No releases"}
            
            ## Metrics
            - Commits: {repo.CommitCount}
            - Contributors: {repo.ContributorCount}
            - Open Issues: {repo.OpenIssueCount}
            - Open PRs: {repo.OpenPullRequestCount}
            - Stars: {repo.StarCount}
            - Forks: {repo.ForkCount}
            
            ## Quality Indicators
            - Has README: {repo.HasReadme}
            - Has License: {repo.HasLicense}
            - Has CI/CD: {repo.HasCiCd}
            - Has Tests: {repo.HasTests}
            - Has Contributing: {repo.HasContributing}
            - Test Coverage: {repo.TestCoverage?.ToString("F1") ?? "Unknown"}%
            - Vulnerabilities: {repo.VulnerabilityCount}
            - Outdated Dependencies: {repo.OutdatedDependencyCount}
            
            ## Flags
            - Is Fork: {repo.IsFork}
            - Is Archived: {repo.IsArchived}
            - Maturity Level: {repo.Maturity}
            
            Respond in this JSON format:
            {jsonTemplate}
            """;
    }

    /// <summary>
    /// Send prompt to AI provider. Implemented by concrete classes.
    /// </summary>
    protected abstract Task<string> SendPromptAsync(string prompt, CancellationToken ct);

    /// <summary>
    /// Parse AI response into RepoInsights.
    /// </summary>
    protected virtual RepoInsights ParseResponse(string response, Guid repositoryId)
    {
        // Try to extract JSON from response
        var jsonStart = response.IndexOf('{');
        var jsonEnd = response.LastIndexOf('}');
        
        if (jsonStart < 0 || jsonEnd < 0)
        {
            return new RepoInsights
            {
                RepositoryId = repositoryId,
                Phase = SdlcPhase.Unknown,
                PhaseRationale = "Failed to parse AI response"
            };
        }

        var json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
        
        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            
            var insights = new RepoInsights
            {
                RepositoryId = repositoryId,
                Phase = ParsePhase(GetStringOrDefault(root, "phase", "Unknown") ?? "Unknown"),
                PhaseRationale = GetStringOrDefault(root, "phaseRationale"),
                MaturityScore = GetIntOrDefault(root, "maturityScore"),
                MaturityRationale = GetStringOrDefault(root, "maturityRationale"),
                ActiveDevelopmentAreas = GetArrayAsJson(root, "activeDevelopmentAreas"),
                TechnicalDebtIndicators = GetArrayAsJson(root, "technicalDebtIndicators"),
                RiskFactors = GetArrayAsJson(root, "riskFactors"),
                SecurityConcerns = GetArrayAsJson(root, "securityConcerns"),
                RecommendedActions = GetArrayAsJson(root, "recommendedActions"),
                ImprovementSuggestions = GetArrayAsJson(root, "improvementSuggestions"),
                EstimatedSprintCount = GetIntOrNull(root, "estimatedSprintCount"),
                MilestonesSuggested = GetArrayAsJson(root, "milestonesSuggested"),
                BlockersPredicted = GetArrayAsJson(root, "blockersPredicted")
            };
            
            return insights;
        }
        catch
        {
            return new RepoInsights
            {
                RepositoryId = repositoryId,
                Phase = SdlcPhase.Unknown,
                PhaseRationale = "Failed to parse AI response JSON"
            };
        }
    }

    private static SdlcPhase ParsePhase(string phase) => phase.ToLowerInvariant() switch
    {
        "ideation" => SdlcPhase.Ideation,
        "planning" => SdlcPhase.Planning,
        "development" => SdlcPhase.Development,
        "testing" => SdlcPhase.Testing,
        "release" => SdlcPhase.Release,
        "maintenance" => SdlcPhase.Maintenance,
        "deprecated" => SdlcPhase.Deprecated,
        _ => SdlcPhase.Unknown
    };

    private static string? GetStringOrDefault(System.Text.Json.JsonElement root, string property, string? defaultValue = null)
    {
        if (root.TryGetProperty(property, out var prop) && prop.ValueKind == System.Text.Json.JsonValueKind.String)
            return prop.GetString();
        return defaultValue;
    }

    private static int GetIntOrDefault(System.Text.Json.JsonElement root, string property, int defaultValue = 0)
    {
        if (root.TryGetProperty(property, out var prop) && prop.ValueKind == System.Text.Json.JsonValueKind.Number)
            return prop.GetInt32();
        return defaultValue;
    }

    private static int? GetIntOrNull(System.Text.Json.JsonElement root, string property)
    {
        if (root.TryGetProperty(property, out var prop) && prop.ValueKind == System.Text.Json.JsonValueKind.Number)
            return prop.GetInt32();
        return null;
    }

    private static string? GetArrayAsJson(System.Text.Json.JsonElement root, string property)
    {
        if (root.TryGetProperty(property, out var prop) && prop.ValueKind == System.Text.Json.JsonValueKind.Array)
            return prop.GetRawText();
        return null;
    }
}
