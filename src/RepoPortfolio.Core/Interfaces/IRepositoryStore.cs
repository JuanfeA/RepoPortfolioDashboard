using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Core.Interfaces;

/// <summary>
/// Interface for persisting and retrieving repositories.
/// Implemented by infrastructure layer (SQLite, PostgreSQL, etc.)
/// </summary>
public interface IRepositoryStore
{
    // Repositories
    Task<Repository?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Repository?> GetByFullNameAsync(string fullName, CancellationToken ct = default);
    Task<IReadOnlyList<Repository>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Repository>> GetByTagAsync(string tag, CancellationToken ct = default);
    Task SaveAsync(Repository repository, CancellationToken ct = default);
    Task SaveManyAsync(IEnumerable<Repository> repositories, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    
    // Scoring Criteria
    Task<IReadOnlyList<ScoringCriteria>> GetAllCriteriaAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ScoringCriteria>> GetActiveCriteriaAsync(CancellationToken ct = default);
    Task SaveCriteriaAsync(ScoringCriteria criteria, CancellationToken ct = default);
    Task SaveManyCriteriaAsync(IEnumerable<ScoringCriteria> criteria, CancellationToken ct = default);
    
    // Scores
    Task<Score?> GetLatestScoreAsync(Guid repositoryId, CancellationToken ct = default);
    Task<IReadOnlyList<Score>> GetScoreHistoryAsync(Guid repositoryId, int limit = 30, CancellationToken ct = default);
    Task SaveScoreAsync(Score score, CancellationToken ct = default);
    Task SaveManyScoresAsync(IEnumerable<Score> scores, CancellationToken ct = default);
}
