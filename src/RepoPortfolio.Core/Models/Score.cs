namespace RepoPortfolio.Core.Models;

/// <summary>
/// Represents a calculated score for a repository.
/// </summary>
public class Score
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RepositoryId { get; set; }
    
    /// <summary>
    /// Overall weighted score (0-100).
    /// </summary>
    public double TotalScore { get; set; }
    
    /// <summary>
    /// Individual criterion scores.
    /// </summary>
    public List<CriterionScore> Breakdown { get; set; } = [];
    
    /// <summary>
    /// Category-level aggregated scores.
    /// </summary>
    public CategoryScores Categories { get; set; } = new();
    
    /// <summary>
    /// Health classification based on total score.
    /// </summary>
    public HealthStatus Health => TotalScore switch
    {
        >= 80 => HealthStatus.Excellent,
        >= 60 => HealthStatus.Good,
        >= 40 => HealthStatus.NeedsAttention,
        >= 20 => HealthStatus.AtRisk,
        _ => HealthStatus.Critical
    };
    
    /// <summary>
    /// When this score was calculated.
    /// </summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class CriterionScore
{
    public Guid CriteriaId { get; set; }
    public required string CriteriaName { get; set; }
    public required string Category { get; set; }
    
    /// <summary>Raw value from repository data.</summary>
    public double RawValue { get; set; }
    
    /// <summary>Normalized score (0-100).</summary>
    public double NormalizedScore { get; set; }
    
    /// <summary>Weighted contribution to total score.</summary>
    public double WeightedScore { get; set; }
    
    /// <summary>Weight applied.</summary>
    public double Weight { get; set; }
}

public class CategoryScores
{
    public double Activity { get; set; }
    public double Quality { get; set; }
    public double Maturity { get; set; }
    public double Risk { get; set; }
}

public enum HealthStatus
{
    Critical,
    AtRisk,
    NeedsAttention,
    Good,
    Excellent
}
