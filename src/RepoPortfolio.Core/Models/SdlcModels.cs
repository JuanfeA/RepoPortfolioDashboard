namespace RepoPortfolio.Core.Models;

/// <summary>
/// SDLC phase of a repository.
/// </summary>
public enum SdlcPhase
{
    Unknown = 0,
    Ideation = 1,
    Planning = 2,
    Development = 3,
    Testing = 4,
    Release = 5,
    Maintenance = 6,
    Deprecated = 7
}

/// <summary>
/// AI-generated insights about a repository.
/// </summary>
public class RepoInsights
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RepositoryId { get; set; }
    
    // SDLC Analysis
    public SdlcPhase Phase { get; set; } = SdlcPhase.Unknown;
    public string? PhaseRationale { get; set; }
    public int MaturityScore { get; set; } // 0-100
    public string? MaturityRationale { get; set; }
    
    // Development Activity
    public string? ActiveDevelopmentAreas { get; set; } // JSON array of feature areas
    public string? RecentFocusAreas { get; set; } // Based on recent commits
    public string? TechnicalDebtIndicators { get; set; } // JSON array
    
    // Risk Assessment
    public string? RiskFactors { get; set; } // JSON array
    public string? SecurityConcerns { get; set; } // JSON array
    
    // Recommendations
    public string? RecommendedActions { get; set; } // JSON array of next steps
    public string? ImprovementSuggestions { get; set; } // JSON array
    
    // Project Management
    public int? EstimatedSprintCount { get; set; }
    public string? MilestonesSuggested { get; set; } // JSON array
    public string? BlockersPredicted { get; set; } // JSON array
    
    // Analysis Metadata
    public string? AnalysisModel { get; set; } // e.g., "gpt-4", "claude-3"
    public string? AnalysisPromptVersion { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan AnalysisDuration { get; set; }
    public bool IsStale => DateTime.UtcNow - AnalyzedAt > TimeSpan.FromDays(7);
}

/// <summary>
/// Report submitted by a repository via CI/CD or webhook.
/// </summary>
public class RepoStatusReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string RepositoryFullName { get; set; }
    
    // Git Context
    public string? CommitSha { get; set; }
    public string? Branch { get; set; }
    public string? EventType { get; set; } // push, pull_request, schedule, etc.
    
    // Self-Reported SDLC
    public SdlcPhase? ReportedPhase { get; set; }
    public int? ReportedMaturity { get; set; }
    public List<string> ReportedBlockers { get; set; } = [];
    public List<string> ReportedTags { get; set; } = [];
    
    // Computed Metrics from CI
    public int? Commits30Days { get; set; }
    public int? OpenIssues { get; set; }
    public int? OpenPrs { get; set; }
    public bool? HasReadme { get; set; }
    public bool? HasLicense { get; set; }
    public bool? HasTests { get; set; }
    public bool? HasCi { get; set; }
    public double? TestCoverage { get; set; }
    public string? CiBuildStatus { get; set; } // success, failure, etc.
    
    // Metadata
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    public string? ReporterVersion { get; set; } // Version of the reporting workflow
}

/// <summary>
/// Summary of repository health for dashboard display.
/// </summary>
public class RepoHealthSummary
{
    public required Repository Repository { get; set; }
    public Score? LatestScore { get; set; }
    public RepoInsights? LatestInsights { get; set; }
    public RepoStatusReport? LatestReport { get; set; }
    
    // Computed properties
    public HealthStatus OverallHealth => LatestScore?.Health ?? HealthStatus.Critical;
    public SdlcPhase CurrentPhase => LatestInsights?.Phase ?? 
        (LatestReport?.ReportedPhase ?? SdlcPhase.Unknown);
    public bool NeedsAttention => 
        OverallHealth == HealthStatus.Critical || 
        OverallHealth == HealthStatus.AtRisk ||
        (LatestInsights?.IsStale ?? true);
    public bool HasRecentReport => 
        LatestReport != null && 
        DateTime.UtcNow - LatestReport.ReportedAt < TimeSpan.FromDays(1);
}
