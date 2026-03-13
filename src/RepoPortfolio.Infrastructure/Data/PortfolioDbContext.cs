using Microsoft.EntityFrameworkCore;
using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for SQLite storage.
/// </summary>
public class PortfolioDbContext : DbContext
{
    public DbSet<RepositoryEntity> Repositories => Set<RepositoryEntity>();
    public DbSet<ScoringCriteriaEntity> ScoringCriteria => Set<ScoringCriteriaEntity>();
    public DbSet<ScoreEntity> Scores => Set<ScoreEntity>();

    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RepositoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FullName).IsUnique();
            entity.Property(e => e.Topics).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            entity.Property(e => e.Tags).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        });

        modelBuilder.Entity<ScoringCriteriaEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<ScoreEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RepositoryId, e.CalculatedAt });
            entity.Property(e => e.BreakdownJson); // Store as JSON string
        });
    }
}

#region Entity Classes

/// <summary>
/// Database entity for Repository.
/// </summary>
public class RepositoryEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string FullName { get; set; }
    public string? Description { get; set; }
    public string? PrimaryLanguage { get; set; }
    public List<string> Topics { get; set; } = [];
    
    public int CommitCount { get; set; }
    public int OpenIssueCount { get; set; }
    public int OpenPullRequestCount { get; set; }
    public int ContributorCount { get; set; }
    public DateTime? LastCommitDate { get; set; }
    public DateTime? LastReleaseDate { get; set; }
    
    public bool HasCiCd { get; set; }
    public bool HasTests { get; set; }
    public bool HasReadme { get; set; }
    public bool HasLicense { get; set; }
    public bool HasContributing { get; set; }
    public double? TestCoverage { get; set; }
    
    public int VulnerabilityCount { get; set; }
    public int OutdatedDependencyCount { get; set; }
    public bool IsArchived { get; set; }
    public bool IsFork { get; set; }
    
    public string? DefaultBranch { get; set; }
    public int StarCount { get; set; }
    public int ForkCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime SyncedAt { get; set; }
    
    public int Maturity { get; set; }
    public List<string> Tags { get; set; } = [];

    public Repository ToDomain() => new()
    {
        Id = Id,
        Name = Name,
        FullName = FullName,
        Description = Description,
        PrimaryLanguage = PrimaryLanguage,
        Topics = Topics,
        CommitCount = CommitCount,
        OpenIssueCount = OpenIssueCount,
        OpenPullRequestCount = OpenPullRequestCount,
        ContributorCount = ContributorCount,
        LastCommitDate = LastCommitDate,
        LastReleaseDate = LastReleaseDate,
        HasCiCd = HasCiCd,
        HasTests = HasTests,
        HasReadme = HasReadme,
        HasLicense = HasLicense,
        HasContributing = HasContributing,
        TestCoverage = TestCoverage,
        VulnerabilityCount = VulnerabilityCount,
        OutdatedDependencyCount = OutdatedDependencyCount,
        IsArchived = IsArchived,
        IsFork = IsFork,
        DefaultBranch = DefaultBranch,
        StarCount = StarCount,
        ForkCount = ForkCount,
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt,
        SyncedAt = SyncedAt,
        Maturity = (MaturityLevel)Maturity,
        Tags = Tags
    };

    public static RepositoryEntity FromDomain(Repository repo) => new()
    {
        Id = repo.Id,
        Name = repo.Name,
        FullName = repo.FullName,
        Description = repo.Description,
        PrimaryLanguage = repo.PrimaryLanguage,
        Topics = repo.Topics,
        CommitCount = repo.CommitCount,
        OpenIssueCount = repo.OpenIssueCount,
        OpenPullRequestCount = repo.OpenPullRequestCount,
        ContributorCount = repo.ContributorCount,
        LastCommitDate = repo.LastCommitDate,
        LastReleaseDate = repo.LastReleaseDate,
        HasCiCd = repo.HasCiCd,
        HasTests = repo.HasTests,
        HasReadme = repo.HasReadme,
        HasLicense = repo.HasLicense,
        HasContributing = repo.HasContributing,
        TestCoverage = repo.TestCoverage,
        VulnerabilityCount = repo.VulnerabilityCount,
        OutdatedDependencyCount = repo.OutdatedDependencyCount,
        IsArchived = repo.IsArchived,
        IsFork = repo.IsFork,
        DefaultBranch = repo.DefaultBranch,
        StarCount = repo.StarCount,
        ForkCount = repo.ForkCount,
        CreatedAt = repo.CreatedAt,
        UpdatedAt = repo.UpdatedAt,
        SyncedAt = repo.SyncedAt,
        Maturity = (int)repo.Maturity,
        Tags = repo.Tags
    };
}

public class ScoringCriteriaEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public string? Description { get; set; }
    public double Weight { get; set; }
    public int CalculationType { get; set; }
    public double ThresholdLow { get; set; }
    public double ThresholdMedium { get; set; }
    public double ThresholdHigh { get; set; }
    public int? WarningDays { get; set; }
    public int? CriticalDays { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ScoringCriteria ToDomain() => new()
    {
        Id = Id,
        Name = Name,
        Category = Category,
        Description = Description,
        Weight = Weight,
        CalculationType = (CalculationType)CalculationType,
        Thresholds = new Thresholds
        {
            Low = ThresholdLow,
            Medium = ThresholdMedium,
            High = ThresholdHigh,
            WarningDays = WarningDays,
            CriticalDays = CriticalDays
        },
        IsActive = IsActive,
        Version = Version,
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt
    };

    public static ScoringCriteriaEntity FromDomain(ScoringCriteria c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Category = c.Category,
        Description = c.Description,
        Weight = c.Weight,
        CalculationType = (int)c.CalculationType,
        ThresholdLow = c.Thresholds.Low,
        ThresholdMedium = c.Thresholds.Medium,
        ThresholdHigh = c.Thresholds.High,
        WarningDays = c.Thresholds.WarningDays,
        CriticalDays = c.Thresholds.CriticalDays,
        IsActive = c.IsActive,
        Version = c.Version,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}

public class ScoreEntity
{
    public Guid Id { get; set; }
    public Guid RepositoryId { get; set; }
    public double TotalScore { get; set; }
    public string BreakdownJson { get; set; } = "[]";
    public double ActivityScore { get; set; }
    public double QualityScore { get; set; }
    public double MaturityScore { get; set; }
    public double RiskScore { get; set; }
    public DateTime CalculatedAt { get; set; }
}

#endregion
