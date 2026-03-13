using RepoPortfolio.Core.Models;
using RepoPortfolio.Core.Scoring;

namespace RepoPortfolio.Core.Tests;

public class ScoringEngineTests
{
    private readonly ScoringEngine _engine = new();

    [Fact]
    public void CalculateScore_WithActiveCriteria_ReturnsScore()
    {
        // Arrange
        var repo = new Repository
        {
            Name = "test-repo",
            FullName = "owner/test-repo",
            CommitCount = 100,
            LastCommitDate = DateTime.UtcNow.AddDays(-5),
            HasCiCd = true,
            HasTests = true,
            HasReadme = true
        };

        var criteria = DefaultCriteriaFactory.CreateDefaultCriteria().ToList();

        // Act
        var score = _engine.CalculateScore(repo, criteria);

        // Assert
        Assert.NotNull(score);
        Assert.Equal(repo.Id, score.RepositoryId);
        Assert.True(score.TotalScore > 0);
        Assert.NotEmpty(score.Breakdown);
    }

    [Fact]
    public void CalculateScore_WithNoActivity_ReturnsLowScore()
    {
        // Arrange
        var repo = new Repository
        {
            Name = "abandoned-repo",
            FullName = "owner/abandoned-repo",
            CommitCount = 0,
            LastCommitDate = DateTime.UtcNow.AddDays(-365),
            HasCiCd = false,
            HasTests = false
        };

        var criteria = DefaultCriteriaFactory.CreateDefaultCriteria().ToList();

        // Act
        var score = _engine.CalculateScore(repo, criteria);

        // Assert
        Assert.True(score.TotalScore < 50);
        Assert.True(score.Health == HealthStatus.AtRisk || score.Health == HealthStatus.Critical);
    }

    [Fact]
    public void NormalizeValue_Boolean_ReturnsCorrectValue()
    {
        var criterion = new ScoringCriteria
        {
            Name = "has_ci_cd",
            Category = "quality",
            CalculationType = CalculationType.Boolean,
            Weight = 0.1
        };

        Assert.Equal(100, _engine.NormalizeValue(1, criterion));
        Assert.Equal(0, _engine.NormalizeValue(0, criterion));
    }

    [Fact]
    public void NormalizeValue_Count_InterpolatesCorrectly()
    {
        var criterion = new ScoringCriteria
        {
            Name = "commits",
            Category = "activity",
            CalculationType = CalculationType.Count,
            Weight = 0.1,
            Thresholds = new Thresholds { Low = 0, Medium = 50, High = 100 }
        };

        Assert.Equal(0, _engine.NormalizeValue(0, criterion));
        Assert.Equal(50, _engine.NormalizeValue(50, criterion));
        Assert.Equal(100, _engine.NormalizeValue(100, criterion));
        Assert.Equal(100, _engine.NormalizeValue(200, criterion)); // Capped at 100
    }

    [Fact]
    public void ExtractRawValue_CommitCount_ReturnsCorrectValue()
    {
        var repo = new Repository
        {
            Name = "test",
            FullName = "owner/test",
            CommitCount = 42
        };

        var criterion = new ScoringCriteria
        {
            Name = "commits",
            Category = "activity",
            CalculationType = CalculationType.Count,
            Weight = 0.1
        };

        Assert.Equal(42, _engine.ExtractRawValue(repo, criterion));
    }
}
