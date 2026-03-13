using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Infrastructure.AI;

/// <summary>
/// OpenAI-compatible API analyzer (works with OpenAI, Azure OpenAI, and compatible APIs).
/// </summary>
public class OpenAiAnalyzer : AiAnalyzerBase
{
    private readonly HttpClient _http;
    private readonly string _endpoint;

    public OpenAiAnalyzer(AiAnalyzerOptions options, HttpClient? httpClient = null) 
        : base(options)
    {
        _http = httpClient ?? new HttpClient();
        _http.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", options.ApiKey);
        
        _endpoint = options.BaseUrl ?? "https://api.openai.com/v1/chat/completions";
    }

    public override bool IsAvailable => 
        Options.Provider is AiProvider.OpenAI or AiProvider.AzureOpenAI or AiProvider.LocalLLM 
        && !string.IsNullOrWhiteSpace(Options.ApiKey);

    public override string ProviderName => Options.Provider switch
    {
        AiProvider.AzureOpenAI => "Azure OpenAI",
        AiProvider.LocalLLM => "Local LLM",
        _ => "OpenAI"
    };

    protected override async Task<string> SendPromptAsync(string prompt, CancellationToken ct)
    {
        var request = new
        {
            model = Options.Model,
            messages = new[]
            {
                new { role = "system", content = "You are an expert software engineer analyzing GitHub repositories. Respond only with valid JSON." },
                new { role = "user", content = prompt }
            },
            max_tokens = Options.MaxTokens,
            temperature = Options.Temperature
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _http.PostAsync(_endpoint, content, ct);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(responseJson);
        
        // Extract the message content from OpenAI response
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";
    }
}

/// <summary>
/// Anthropic Claude API analyzer.
/// </summary>
public class AnthropicAnalyzer : AiAnalyzerBase
{
    private readonly HttpClient _http;
    private const string Endpoint = "https://api.anthropic.com/v1/messages";

    public AnthropicAnalyzer(AiAnalyzerOptions options, HttpClient? httpClient = null) 
        : base(options)
    {
        _http = httpClient ?? new HttpClient();
        _http.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public override bool IsAvailable => 
        Options.Provider == AiProvider.Anthropic 
        && !string.IsNullOrWhiteSpace(Options.ApiKey);

    public override string ProviderName => "Anthropic Claude";

    protected override async Task<string> SendPromptAsync(string prompt, CancellationToken ct)
    {
        var request = new
        {
            model = Options.Model,
            max_tokens = Options.MaxTokens,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            system = "You are an expert software engineer analyzing GitHub repositories. Respond only with valid JSON."
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _http.PostAsync(Endpoint, content, ct);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(responseJson);
        
        // Extract the message content from Anthropic response
        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "";
    }
}

/// <summary>
/// Null object pattern - analyzer that always returns basic insights.
/// Used when no AI provider is configured.
/// </summary>
public class NullAnalyzer : AiAnalyzerBase
{
    public NullAnalyzer() : base(new AiAnalyzerOptions()) { }

    public override bool IsAvailable => false;
    public override string ProviderName => "None (AI not configured)";

    protected override Task<string> SendPromptAsync(string prompt, CancellationToken ct)
    {
        throw new InvalidOperationException("AI analysis is not configured. Set OPENAI_API_KEY or ANTHROPIC_API_KEY.");
    }

    public new Task<RepoInsights> AnalyzeAsync(Repository repo, CancellationToken ct = default)
    {
        // Return rule-based insights when AI is not available
        var insights = GenerateRuleBasedInsights(repo);
        return Task.FromResult(insights);
    }

    private static RepoInsights GenerateRuleBasedInsights(Repository repo)
    {
        var phase = DeterminePhase(repo);
        var maturity = CalculateMaturity(repo);
        
        return new RepoInsights
        {
            RepositoryId = repo.Id,
            Phase = phase,
            PhaseRationale = GetPhaseRationale(phase, repo),
            MaturityScore = maturity,
            MaturityRationale = GetMaturityRationale(maturity, repo),
            ActiveDevelopmentAreas = null,
            TechnicalDebtIndicators = GetTechnicalDebtIndicators(repo),
            RiskFactors = GetRiskFactors(repo),
            RecommendedActions = GetRecommendedActions(repo),
            AnalysisModel = "rule-based",
            AnalysisPromptVersion = "1.0"
        };
    }

    private static SdlcPhase DeterminePhase(Repository repo)
    {
        if (repo.IsArchived) return SdlcPhase.Deprecated;
        
        var daysSinceCommit = repo.LastCommitDate.HasValue 
            ? (DateTime.UtcNow - repo.LastCommitDate.Value).TotalDays 
            : 365;
        
        if (daysSinceCommit > 365) return SdlcPhase.Deprecated;
        if (daysSinceCommit > 180) return SdlcPhase.Maintenance;
        
        if (!repo.HasReadme && !repo.HasLicense && repo.CommitCount < 10)
            return SdlcPhase.Ideation;
        
        if (!repo.HasTests && !repo.HasCiCd && repo.CommitCount < 50)
            return SdlcPhase.Development;
        
        if (repo.HasTests && !repo.LastReleaseDate.HasValue)
            return SdlcPhase.Testing;
        
        if (repo.LastReleaseDate.HasValue)
        {
            var daysSinceRelease = (DateTime.UtcNow - repo.LastReleaseDate.Value).TotalDays;
            if (daysSinceRelease < 90 && daysSinceCommit < 30)
                return SdlcPhase.Release;
            return SdlcPhase.Maintenance;
        }
        
        return SdlcPhase.Development;
    }

    private static int CalculateMaturity(Repository repo)
    {
        var score = 0;
        
        if (repo.HasReadme) score += 15;
        if (repo.HasLicense) score += 10;
        if (repo.HasCiCd) score += 15;
        if (repo.HasTests) score += 15;
        if (repo.HasContributing) score += 5;
        if (repo.LastReleaseDate.HasValue) score += 15;
        if (repo.ContributorCount > 1) score += 5;
        if (repo.StarCount > 0) score += 5;
        if (repo.VulnerabilityCount == 0) score += 10;
        if (repo.OutdatedDependencyCount == 0) score += 5;
        
        return Math.Min(100, score);
    }

    private static string GetPhaseRationale(SdlcPhase phase, Repository repo) => phase switch
    {
        SdlcPhase.Deprecated => repo.IsArchived 
            ? "Repository is archived" 
            : $"No commits in over a year (last: {repo.LastCommitDate?.ToString("yyyy-MM-dd") ?? "unknown"})",
        SdlcPhase.Maintenance => "Repository receives infrequent updates, typical of maintenance mode",
        SdlcPhase.Ideation => "Early stage: missing documentation and basic setup",
        SdlcPhase.Planning => "Has documentation but limited implementation",
        SdlcPhase.Development => "Active development without test coverage or releases",
        SdlcPhase.Testing => "Has tests but no formal releases yet",
        SdlcPhase.Release => "Recently released with active development",
        _ => "Unable to determine phase from available data"
    };

    private static string GetMaturityRationale(int maturity, Repository repo)
    {
        var missing = new List<string>();
        if (!repo.HasReadme) missing.Add("README");
        if (!repo.HasLicense) missing.Add("LICENSE");
        if (!repo.HasCiCd) missing.Add("CI/CD");
        if (!repo.HasTests) missing.Add("tests");
        
        if (missing.Count == 0)
            return "Repository has all major quality indicators";
        
        return $"Missing: {string.Join(", ", missing)}";
    }

    private static string? GetTechnicalDebtIndicators(Repository repo)
    {
        var indicators = new List<string>();
        if (!repo.HasTests) indicators.Add("No automated tests");
        if (!repo.HasCiCd) indicators.Add("No CI/CD pipeline");
        if (repo.OutdatedDependencyCount > 0) indicators.Add($"{repo.OutdatedDependencyCount} outdated dependencies");
        if (repo.VulnerabilityCount > 0) indicators.Add($"{repo.VulnerabilityCount} security vulnerabilities");
        
        return indicators.Count > 0 
            ? System.Text.Json.JsonSerializer.Serialize(indicators) 
            : null;
    }

    private static string? GetRiskFactors(Repository repo)
    {
        var risks = new List<string>();
        if (repo.ContributorCount == 1) risks.Add("Single contributor (bus factor)");
        if (!repo.HasLicense) risks.Add("No license (legal risk)");
        if (repo.VulnerabilityCount > 0) risks.Add("Known vulnerabilities");
        if (repo.IsFork) risks.Add("Fork - may diverge from upstream");
        
        return risks.Count > 0 
            ? System.Text.Json.JsonSerializer.Serialize(risks) 
            : null;
    }

    private static string? GetRecommendedActions(Repository repo)
    {
        var actions = new List<string>();
        if (!repo.HasReadme) actions.Add("Add a README.md file");
        if (!repo.HasLicense) actions.Add("Add a LICENSE file");
        if (!repo.HasTests) actions.Add("Set up automated testing");
        if (!repo.HasCiCd) actions.Add("Configure CI/CD pipeline");
        if (!repo.HasContributing) actions.Add("Add CONTRIBUTING.md");
        if (repo.VulnerabilityCount > 0) actions.Add("Address security vulnerabilities");
        if (repo.OutdatedDependencyCount > 0) actions.Add("Update outdated dependencies");
        
        return actions.Count > 0 
            ? System.Text.Json.JsonSerializer.Serialize(actions) 
            : null;
    }
}
