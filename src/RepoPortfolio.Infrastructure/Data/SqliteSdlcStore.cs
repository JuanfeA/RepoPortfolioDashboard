using Microsoft.EntityFrameworkCore;
using RepoPortfolio.Core.Interfaces;
using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Infrastructure.Data;

/// <summary>
/// SQLite implementation of ISdlcStore.
/// </summary>
public class SqliteSdlcStore : ISdlcStore
{
    private readonly SdlcDbContext _db;

    public SqliteSdlcStore(SdlcDbContext db)
    {
        _db = db;
    }

    #region Insights

    public async Task SaveInsightsAsync(RepoInsights insights, CancellationToken ct = default)
    {
        var entity = RepoInsightsEntity.FromDomain(insights);
        _db.Insights.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<RepoInsights?> GetLatestInsightsAsync(Guid repositoryId, CancellationToken ct = default)
    {
        var entity = await _db.Insights
            .Where(i => i.RepositoryId == repositoryId)
            .OrderByDescending(i => i.AnalyzedAt)
            .FirstOrDefaultAsync(ct);
        
        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<RepoInsights>> GetInsightsHistoryAsync(
        Guid repositoryId, 
        int limit = 10, 
        CancellationToken ct = default)
    {
        var entities = await _db.Insights
            .Where(i => i.RepositoryId == repositoryId)
            .OrderByDescending(i => i.AnalyzedAt)
            .Take(limit)
            .ToListAsync(ct);
        
        return entities.Select(e => e.ToDomain()).ToList();
    }

    #endregion

    #region Status Reports

    public async Task SaveReportAsync(RepoStatusReport report, CancellationToken ct = default)
    {
        var entity = RepoStatusReportEntity.FromDomain(report);
        _db.Reports.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<RepoStatusReport?> GetLatestReportAsync(string repositoryFullName, CancellationToken ct = default)
    {
        var entity = await _db.Reports
            .Where(r => r.RepositoryFullName == repositoryFullName)
            .OrderByDescending(r => r.ReportedAt)
            .FirstOrDefaultAsync(ct);
        
        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<RepoStatusReport>> GetReportsAsync(
        string repositoryFullName, 
        int limit = 10, 
        CancellationToken ct = default)
    {
        var entities = await _db.Reports
            .Where(r => r.RepositoryFullName == repositoryFullName)
            .OrderByDescending(r => r.ReportedAt)
            .Take(limit)
            .ToListAsync(ct);
        
        return entities.Select(e => e.ToDomain()).ToList();
    }

    #endregion

    #region Health Summaries

    public async Task<IReadOnlyList<RepoHealthSummary>> GetHealthSummariesAsync(CancellationToken ct = default)
    {
        // This requires joining with repository data - implemented in PortfolioService
        throw new NotImplementedException("Use PortfolioService.GetHealthSummariesAsync instead");
    }

    public async Task<RepoHealthSummary?> GetHealthSummaryAsync(Guid repositoryId, CancellationToken ct = default)
    {
        // This requires joining with repository data - implemented in PortfolioService
        throw new NotImplementedException("Use PortfolioService.GetHealthSummaryAsync instead");
    }

    #endregion
}

#region SDLC Entities

/// <summary>
/// Database entity for RepoInsights.
/// </summary>
public class RepoInsightsEntity
{
    public Guid Id { get; set; }
    public Guid RepositoryId { get; set; }
    
    public int Phase { get; set; }
    public string? PhaseRationale { get; set; }
    public int MaturityScore { get; set; }
    public string? MaturityRationale { get; set; }
    
    public string? ActiveDevelopmentAreas { get; set; }
    public string? RecentFocusAreas { get; set; }
    public string? TechnicalDebtIndicators { get; set; }
    
    public string? RiskFactors { get; set; }
    public string? SecurityConcerns { get; set; }
    
    public string? RecommendedActions { get; set; }
    public string? ImprovementSuggestions { get; set; }
    
    public int? EstimatedSprintCount { get; set; }
    public string? MilestonesSuggested { get; set; }
    public string? BlockersPredicted { get; set; }
    
    public string? AnalysisModel { get; set; }
    public string? AnalysisPromptVersion { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public long AnalysisDurationTicks { get; set; }

    public RepoInsights ToDomain() => new()
    {
        Id = Id,
        RepositoryId = RepositoryId,
        Phase = (SdlcPhase)Phase,
        PhaseRationale = PhaseRationale,
        MaturityScore = MaturityScore,
        MaturityRationale = MaturityRationale,
        ActiveDevelopmentAreas = ActiveDevelopmentAreas,
        RecentFocusAreas = RecentFocusAreas,
        TechnicalDebtIndicators = TechnicalDebtIndicators,
        RiskFactors = RiskFactors,
        SecurityConcerns = SecurityConcerns,
        RecommendedActions = RecommendedActions,
        ImprovementSuggestions = ImprovementSuggestions,
        EstimatedSprintCount = EstimatedSprintCount,
        MilestonesSuggested = MilestonesSuggested,
        BlockersPredicted = BlockersPredicted,
        AnalysisModel = AnalysisModel,
        AnalysisPromptVersion = AnalysisPromptVersion,
        AnalyzedAt = AnalyzedAt,
        AnalysisDuration = TimeSpan.FromTicks(AnalysisDurationTicks)
    };

    public static RepoInsightsEntity FromDomain(RepoInsights i) => new()
    {
        Id = i.Id,
        RepositoryId = i.RepositoryId,
        Phase = (int)i.Phase,
        PhaseRationale = i.PhaseRationale,
        MaturityScore = i.MaturityScore,
        MaturityRationale = i.MaturityRationale,
        ActiveDevelopmentAreas = i.ActiveDevelopmentAreas,
        RecentFocusAreas = i.RecentFocusAreas,
        TechnicalDebtIndicators = i.TechnicalDebtIndicators,
        RiskFactors = i.RiskFactors,
        SecurityConcerns = i.SecurityConcerns,
        RecommendedActions = i.RecommendedActions,
        ImprovementSuggestions = i.ImprovementSuggestions,
        EstimatedSprintCount = i.EstimatedSprintCount,
        MilestonesSuggested = i.MilestonesSuggested,
        BlockersPredicted = i.BlockersPredicted,
        AnalysisModel = i.AnalysisModel,
        AnalysisPromptVersion = i.AnalysisPromptVersion,
        AnalyzedAt = i.AnalyzedAt,
        AnalysisDurationTicks = i.AnalysisDuration.Ticks
    };
}

/// <summary>
/// Database entity for RepoStatusReport.
/// </summary>
public class RepoStatusReportEntity
{
    public Guid Id { get; set; }
    public required string RepositoryFullName { get; set; }
    
    public string? CommitSha { get; set; }
    public string? Branch { get; set; }
    public string? EventType { get; set; }
    
    public int? ReportedPhase { get; set; }
    public int? ReportedMaturity { get; set; }
    public string? ReportedBlockersJson { get; set; }
    public string? ReportedTagsJson { get; set; }
    
    public int? Commits30Days { get; set; }
    public int? OpenIssues { get; set; }
    public int? OpenPrs { get; set; }
    public bool? HasReadme { get; set; }
    public bool? HasLicense { get; set; }
    public bool? HasTests { get; set; }
    public bool? HasCi { get; set; }
    public double? TestCoverage { get; set; }
    public string? CiBuildStatus { get; set; }
    
    public DateTime ReportedAt { get; set; }
    public string? ReporterVersion { get; set; }

    public RepoStatusReport ToDomain() => new()
    {
        Id = Id,
        RepositoryFullName = RepositoryFullName,
        CommitSha = CommitSha,
        Branch = Branch,
        EventType = EventType,
        ReportedPhase = ReportedPhase.HasValue ? (SdlcPhase)ReportedPhase : null,
        ReportedMaturity = ReportedMaturity,
        ReportedBlockers = DeserializeList(ReportedBlockersJson),
        ReportedTags = DeserializeList(ReportedTagsJson),
        Commits30Days = Commits30Days,
        OpenIssues = OpenIssues,
        OpenPrs = OpenPrs,
        HasReadme = HasReadme,
        HasLicense = HasLicense,
        HasTests = HasTests,
        HasCi = HasCi,
        TestCoverage = TestCoverage,
        CiBuildStatus = CiBuildStatus,
        ReportedAt = ReportedAt,
        ReporterVersion = ReporterVersion
    };

    public static RepoStatusReportEntity FromDomain(RepoStatusReport r) => new()
    {
        Id = r.Id,
        RepositoryFullName = r.RepositoryFullName,
        CommitSha = r.CommitSha,
        Branch = r.Branch,
        EventType = r.EventType,
        ReportedPhase = r.ReportedPhase.HasValue ? (int)r.ReportedPhase : null,
        ReportedMaturity = r.ReportedMaturity,
        ReportedBlockersJson = SerializeList(r.ReportedBlockers),
        ReportedTagsJson = SerializeList(r.ReportedTags),
        Commits30Days = r.Commits30Days,
        OpenIssues = r.OpenIssues,
        OpenPrs = r.OpenPrs,
        HasReadme = r.HasReadme,
        HasLicense = r.HasLicense,
        HasTests = r.HasTests,
        HasCi = r.HasCi,
        TestCoverage = r.TestCoverage,
        CiBuildStatus = r.CiBuildStatus,
        ReportedAt = r.ReportedAt,
        ReporterVersion = r.ReporterVersion
    };

    private static string? SerializeList(List<string>? list) =>
        list?.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(list) : null;

    private static List<string> DeserializeList(string? json) =>
        string.IsNullOrEmpty(json) 
            ? [] 
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? [];
}

#endregion

/// <summary>
/// Separate DbContext for SDLC data (insights and reports).
/// </summary>
public class SdlcDbContext : DbContext
{
    public DbSet<RepoInsightsEntity> Insights => Set<RepoInsightsEntity>();
    public DbSet<RepoStatusReportEntity> Reports => Set<RepoStatusReportEntity>();

    public SdlcDbContext(DbContextOptions<SdlcDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RepoInsightsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RepositoryId, e.AnalyzedAt });
        });

        modelBuilder.Entity<RepoStatusReportEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RepositoryFullName, e.ReportedAt });
        });
    }
}
