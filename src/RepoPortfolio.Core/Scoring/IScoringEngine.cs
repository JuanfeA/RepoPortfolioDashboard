using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Core.Scoring;

/// <summary>
/// Engine for calculating repository scores based on configurable criteria.
/// This is the core business logic - no external dependencies.
/// </summary>
public interface IScoringEngine
{
    /// <summary>
    /// Calculate a score for a single repository.
    /// </summary>
    Score CalculateScore(Repository repository, IEnumerable<ScoringCriteria> criteria);
    
    /// <summary>
    /// Calculate scores for multiple repositories.
    /// </summary>
    IEnumerable<Score> CalculateScores(IEnumerable<Repository> repositories, IEnumerable<ScoringCriteria> criteria);
    
    /// <summary>
    /// Get the raw value for a specific criterion from repository data.
    /// </summary>
    double ExtractRawValue(Repository repository, ScoringCriteria criterion);
    
    /// <summary>
    /// Normalize a raw value to a 0-100 score based on thresholds.
    /// </summary>
    double NormalizeValue(double rawValue, ScoringCriteria criterion);
}
