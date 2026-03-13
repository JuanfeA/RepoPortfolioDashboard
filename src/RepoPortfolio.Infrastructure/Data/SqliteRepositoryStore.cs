using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RepoPortfolio.Core.Interfaces;
using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Infrastructure.Data;

/// <summary>
/// SQLite implementation of IRepositoryStore.
/// </summary>
public class SqliteRepositoryStore : IRepositoryStore
{
    private readonly PortfolioDbContext _db;

    public SqliteRepositoryStore(PortfolioDbContext db)
    {
        _db = db;
    }

    #region Repositories

    public async Task<Repository?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Repositories.FindAsync([id], ct);
        return entity?.ToDomain();
    }

    public async Task<Repository?> GetByFullNameAsync(string fullName, CancellationToken ct = default)
    {
        var entity = await _db.Repositories.FirstOrDefaultAsync(r => r.FullName == fullName, ct);
        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<Repository>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _db.Repositories.ToListAsync(ct);
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<Repository>> GetByTagAsync(string tag, CancellationToken ct = default)
    {
        var entities = await _db.Repositories
            .Where(r => r.Tags.Contains(tag))
            .ToListAsync(ct);
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task SaveAsync(Repository repository, CancellationToken ct = default)
    {
        var existing = await _db.Repositories.FindAsync([repository.Id], ct);
        if (existing != null)
        {
            _db.Entry(existing).CurrentValues.SetValues(RepositoryEntity.FromDomain(repository));
        }
        else
        {
            _db.Repositories.Add(RepositoryEntity.FromDomain(repository));
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveManyAsync(IEnumerable<Repository> repositories, CancellationToken ct = default)
    {
        foreach (var repo in repositories)
        {
            var entity = RepositoryEntity.FromDomain(repo);
            var existing = await _db.Repositories.FirstOrDefaultAsync(r => r.FullName == repo.FullName, ct);
            
            if (existing != null)
            {
                entity.Id = existing.Id;
                _db.Entry(existing).CurrentValues.SetValues(entity);
            }
            else
            {
                _db.Repositories.Add(entity);
            }
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Repositories.FindAsync([id], ct);
        if (entity != null)
        {
            _db.Repositories.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region Scoring Criteria

    public async Task<IReadOnlyList<ScoringCriteria>> GetAllCriteriaAsync(CancellationToken ct = default)
    {
        var entities = await _db.ScoringCriteria.ToListAsync(ct);
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<ScoringCriteria>> GetActiveCriteriaAsync(CancellationToken ct = default)
    {
        var entities = await _db.ScoringCriteria.Where(c => c.IsActive).ToListAsync(ct);
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task SaveCriteriaAsync(ScoringCriteria criteria, CancellationToken ct = default)
    {
        var existing = await _db.ScoringCriteria.FindAsync([criteria.Id], ct);
        if (existing != null)
        {
            _db.Entry(existing).CurrentValues.SetValues(ScoringCriteriaEntity.FromDomain(criteria));
        }
        else
        {
            _db.ScoringCriteria.Add(ScoringCriteriaEntity.FromDomain(criteria));
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveManyCriteriaAsync(IEnumerable<ScoringCriteria> criteria, CancellationToken ct = default)
    {
        foreach (var c in criteria)
        {
            var entity = ScoringCriteriaEntity.FromDomain(c);
            var existing = await _db.ScoringCriteria.FirstOrDefaultAsync(x => x.Name == c.Name, ct);
            
            if (existing != null)
            {
                entity.Id = existing.Id;
                _db.Entry(existing).CurrentValues.SetValues(entity);
            }
            else
            {
                _db.ScoringCriteria.Add(entity);
            }
        }
        await _db.SaveChangesAsync(ct);
    }

    #endregion

    #region Scores

    public async Task<Score?> GetLatestScoreAsync(Guid repositoryId, CancellationToken ct = default)
    {
        var entity = await _db.Scores
            .Where(s => s.RepositoryId == repositoryId)
            .OrderByDescending(s => s.CalculatedAt)
            .FirstOrDefaultAsync(ct);
        return entity != null ? ToScoreDomain(entity) : null;
    }

    public async Task<IReadOnlyList<Score>> GetScoreHistoryAsync(Guid repositoryId, int limit = 30, CancellationToken ct = default)
    {
        var entities = await _db.Scores
            .Where(s => s.RepositoryId == repositoryId)
            .OrderByDescending(s => s.CalculatedAt)
            .Take(limit)
            .ToListAsync(ct);
        return entities.Select(ToScoreDomain).ToList();
    }

    public async Task SaveScoreAsync(Score score, CancellationToken ct = default)
    {
        _db.Scores.Add(ToScoreEntity(score));
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveManyScoresAsync(IEnumerable<Score> scores, CancellationToken ct = default)
    {
        _db.Scores.AddRange(scores.Select(ToScoreEntity));
        await _db.SaveChangesAsync(ct);
    }

    private static ScoreEntity ToScoreEntity(Score score) => new()
    {
        Id = score.Id,
        RepositoryId = score.RepositoryId,
        TotalScore = score.TotalScore,
        BreakdownJson = JsonSerializer.Serialize(score.Breakdown),
        ActivityScore = score.Categories.Activity,
        QualityScore = score.Categories.Quality,
        MaturityScore = score.Categories.Maturity,
        RiskScore = score.Categories.Risk,
        CalculatedAt = score.CalculatedAt
    };

    private static Score ToScoreDomain(ScoreEntity entity) => new()
    {
        Id = entity.Id,
        RepositoryId = entity.RepositoryId,
        TotalScore = entity.TotalScore,
        Breakdown = JsonSerializer.Deserialize<List<CriterionScore>>(entity.BreakdownJson) ?? [],
        Categories = new CategoryScores
        {
            Activity = entity.ActivityScore,
            Quality = entity.QualityScore,
            Maturity = entity.MaturityScore,
            Risk = entity.RiskScore
        },
        CalculatedAt = entity.CalculatedAt
    };

    #endregion
}
