namespace RepoPortfolio.Core.Models;

/// <summary>
/// Represents a GitHub repository with metadata for scoring.
/// </summary>
public class Repository
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string FullName { get; set; } // e.g., "owner/repo"
    public string? Description { get; set; }
    public string? PrimaryLanguage { get; set; }
    public List<string> Topics { get; set; } = [];
    
    // Activity metrics
    public int CommitCount { get; set; }
    public int OpenIssueCount { get; set; }
    public int OpenPullRequestCount { get; set; }
    public int ContributorCount { get; set; }
    public DateTime? LastCommitDate { get; set; }
    public DateTime? LastReleaseDate { get; set; }
    
    // Quality indicators
    public bool HasCiCd { get; set; }
    public bool HasTests { get; set; }
    public bool HasReadme { get; set; }
    public bool HasLicense { get; set; }
    public bool HasContributing { get; set; }
    public double? TestCoverage { get; set; } // 0.0 to 1.0
    
    // Risk indicators
    public int VulnerabilityCount { get; set; }
    public int OutdatedDependencyCount { get; set; }
    public bool IsArchived { get; set; }
    public bool IsFork { get; set; }
    
    // Metadata
    public string? DefaultBranch { get; set; }
    public int StarCount { get; set; }
    public int ForkCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
    
    // Classification
    public MaturityLevel Maturity { get; set; } = MaturityLevel.Unknown;
    public List<string> Tags { get; set; } = [];
}

public enum MaturityLevel
{
    Unknown = 0,
    Prototype = 1,
    Development = 2,
    Beta = 3,
    Production = 4,
    Maintenance = 5,
    Deprecated = 6
}
