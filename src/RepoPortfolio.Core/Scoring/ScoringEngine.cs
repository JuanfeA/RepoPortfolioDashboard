using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Core.Scoring;

/// <summary>
/// Default implementation of the scoring engine.
/// Pure business logic with no external dependencies.
/// </summary>
public class ScoringEngine : IScoringEngine
{
    public Score CalculateScore(Repository repository, IEnumerable<ScoringCriteria> criteria)
    {
        var activeCriteria = criteria.Where(c => c.IsActive).ToList();
        var breakdown = new List<CriterionScore>();
        var categoryTotals = new Dictionary<string, (double weighted, double weight)>
        {
            ["activity"] = (0, 0),
            ["quality"] = (0, 0),
            ["maturity"] = (0, 0),
            ["risk"] = (0, 0)
        };

        foreach (var criterion in activeCriteria)
        {
            var rawValue = ExtractRawValue(repository, criterion);
            var normalizedScore = NormalizeValue(rawValue, criterion);
            var weightedScore = normalizedScore * criterion.Weight;

            breakdown.Add(new CriterionScore
            {
                CriteriaId = criterion.Id,
                CriteriaName = criterion.Name,
                Category = criterion.Category,
                RawValue = rawValue,
                NormalizedScore = normalizedScore,
                WeightedScore = weightedScore,
                Weight = criterion.Weight
            });

            var cat = criterion.Category.ToLowerInvariant();
            if (categoryTotals.ContainsKey(cat))
            {
                var (weighted, weight) = categoryTotals[cat];
                categoryTotals[cat] = (weighted + weightedScore, weight + criterion.Weight);
            }
        }

        var totalScore = breakdown.Sum(b => b.WeightedScore);

        return new Score
        {
            RepositoryId = repository.Id,
            TotalScore = Math.Round(totalScore, 2),
            Breakdown = breakdown,
            Categories = new CategoryScores
            {
                Activity = CalculateCategoryScore(categoryTotals, "activity"),
                Quality = CalculateCategoryScore(categoryTotals, "quality"),
                Maturity = CalculateCategoryScore(categoryTotals, "maturity"),
                Risk = CalculateCategoryScore(categoryTotals, "risk")
            },
            CalculatedAt = DateTime.UtcNow
        };
    }

    public IEnumerable<Score> CalculateScores(IEnumerable<Repository> repositories, IEnumerable<ScoringCriteria> criteria)
    {
        var criteriaList = criteria.ToList();
        return repositories.Select(repo => CalculateScore(repo, criteriaList));
    }

    public double ExtractRawValue(Repository repository, ScoringCriteria criterion)
    {
        return criterion.Name.ToLowerInvariant() switch
        {
            // Activity metrics
            "commit_frequency" or "commits" => repository.CommitCount,
            "open_issues" => repository.OpenIssueCount,
            "open_prs" or "pull_requests" => repository.OpenPullRequestCount,
            "contributors" => repository.ContributorCount,
            "days_since_commit" => DaysSince(repository.LastCommitDate),
            "days_since_release" => DaysSince(repository.LastReleaseDate),
            
            // Quality metrics
            "has_ci_cd" or "ci_cd" => repository.HasCiCd ? 1 : 0,
            "has_tests" or "tests" => repository.HasTests ? 1 : 0,
            "has_readme" or "readme" => repository.HasReadme ? 1 : 0,
            "has_license" or "license" => repository.HasLicense ? 1 : 0,
            "has_contributing" => repository.HasContributing ? 1 : 0,
            "test_coverage" => (repository.TestCoverage ?? 0) * 100,
            
            // Risk metrics
            "vulnerabilities" => repository.VulnerabilityCount,
            "outdated_dependencies" => repository.OutdatedDependencyCount,
            
            // Popularity (can be used for maturity)
            "stars" => repository.StarCount,
            "forks" => repository.ForkCount,
            
            // Maturity
            "maturity_level" => (int)repository.Maturity,
            
            _ => 0
        };
    }

    public double NormalizeValue(double rawValue, ScoringCriteria criterion)
    {
        return criterion.CalculationType switch
        {
            CalculationType.Boolean => rawValue > 0 ? 100 : 0,
            
            CalculationType.Count => NormalizeCount(rawValue, criterion.Thresholds),
            
            CalculationType.DaysSince => NormalizeDaysSince(rawValue, criterion.Thresholds),
            
            CalculationType.Percentage => Math.Clamp(rawValue, 0, 100),
            
            CalculationType.InverseCount => NormalizeInverseCount(rawValue, criterion.Thresholds),
            
            _ => 0
        };
    }

    private static double NormalizeCount(double value, Thresholds t)
    {
        if (value >= t.High) return 100;
        if (value <= t.Low) return 0;
        if (value >= t.Medium) return 50 + (value - t.Medium) / (t.High - t.Medium) * 50;
        return (value - t.Low) / (t.Medium - t.Low) * 50;
    }

    private static double NormalizeDaysSince(double days, Thresholds t)
    {
        var warningDays = t.WarningDays ?? 90;
        var criticalDays = t.CriticalDays ?? 180;

        if (days <= 7) return 100;        // Very recent
        if (days <= 30) return 90;        // Recent
        if (days <= warningDays) return 70;
        if (days <= criticalDays) return 40;
        return Math.Max(0, 20 - (days - criticalDays) / 30 * 5);
    }

    private static double NormalizeInverseCount(double value, Thresholds t)
    {
        // Fewer is better (vulnerabilities, outdated deps)
        if (value <= t.Low) return 100;   // Low = good
        if (value >= t.High) return 0;    // High = bad
        return 100 - (value - t.Low) / (t.High - t.Low) * 100;
    }

    private static double DaysSince(DateTime? date)
    {
        if (!date.HasValue) return 365; // Assume very old if unknown
        return (DateTime.UtcNow - date.Value).TotalDays;
    }

    private static double CalculateCategoryScore(Dictionary<string, (double weighted, double weight)> totals, string category)
    {
        var (weighted, weight) = totals.GetValueOrDefault(category, (0, 0));
        return weight > 0 ? Math.Round(weighted / weight * 100, 2) : 0;
    }
}
