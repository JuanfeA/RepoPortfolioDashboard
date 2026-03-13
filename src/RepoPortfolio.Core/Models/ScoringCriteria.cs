namespace RepoPortfolio.Core.Models;

/// <summary>
/// Defines a scoring criterion with configurable weight and thresholds.
/// </summary>
public class ScoringCriteria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Category { get; set; } // activity, quality, maturity, risk
    public string? Description { get; set; }
    
    /// <summary>
    /// Weight of this criterion in overall score (0.0 to 1.0).
    /// All active criteria weights should sum to 1.0.
    /// </summary>
    public double Weight { get; set; }
    
    /// <summary>
    /// How to calculate the raw value for this criterion.
    /// </summary>
    public CalculationType CalculationType { get; set; }
    
    /// <summary>
    /// Thresholds for converting raw values to normalized scores (0-100).
    /// </summary>
    public Thresholds Thresholds { get; set; } = new();
    
    /// <summary>
    /// Whether this criterion is currently active in scoring.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Version number for tracking changes.
    /// </summary>
    public int Version { get; set; } = 1;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum CalculationType
{
    /// <summary>Count of items (commits, PRs, issues)</summary>
    Count,
    
    /// <summary>Boolean flag (has CI, has tests)</summary>
    Boolean,
    
    /// <summary>Days since an event (last commit, last release)</summary>
    DaysSince,
    
    /// <summary>Percentage value (test coverage)</summary>
    Percentage,
    
    /// <summary>Inverse count (fewer is better - vulnerabilities)</summary>
    InverseCount
}

/// <summary>
/// Thresholds for normalizing raw values to scores.
/// </summary>
public class Thresholds
{
    /// <summary>Value considered "low" or poor.</summary>
    public double Low { get; set; }
    
    /// <summary>Value considered "medium" or acceptable.</summary>
    public double Medium { get; set; }
    
    /// <summary>Value considered "high" or excellent.</summary>
    public double High { get; set; }
    
    /// <summary>For DaysSince: days before warning (e.g., 90 days inactive)</summary>
    public int? WarningDays { get; set; }
    
    /// <summary>For DaysSince: days before critical (e.g., 180 days inactive)</summary>
    public int? CriticalDays { get; set; }
}
