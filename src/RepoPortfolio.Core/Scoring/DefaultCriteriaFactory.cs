using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Core.Scoring;

/// <summary>
/// Factory for creating default scoring criteria.
/// </summary>
public static class DefaultCriteriaFactory
{
    public static IEnumerable<ScoringCriteria> CreateDefaultCriteria()
    {
        // Activity criteria (total weight: 0.40)
        yield return new ScoringCriteria
        {
            Name = "days_since_commit",
            Category = "activity",
            Description = "Days since last commit (recent = higher score)",
            Weight = 0.15,
            CalculationType = CalculationType.DaysSince,
            Thresholds = new Thresholds { WarningDays = 90, CriticalDays = 180 }
        };

        yield return new ScoringCriteria
        {
            Name = "commit_frequency",
            Category = "activity",
            Description = "Number of commits in the repository",
            Weight = 0.10,
            CalculationType = CalculationType.Count,
            Thresholds = new Thresholds { Low = 0, Medium = 50, High = 200 }
        };

        yield return new ScoringCriteria
        {
            Name = "open_prs",
            Category = "activity",
            Description = "Open pull requests (some activity is good, too many is bad)",
            Weight = 0.08,
            CalculationType = CalculationType.Count,
            Thresholds = new Thresholds { Low = 0, Medium = 3, High = 10 }
        };

        yield return new ScoringCriteria
        {
            Name = "contributors",
            Category = "activity",
            Description = "Number of contributors",
            Weight = 0.07,
            CalculationType = CalculationType.Count,
            Thresholds = new Thresholds { Low = 1, Medium = 3, High = 10 }
        };

        // Quality criteria (total weight: 0.30)
        yield return new ScoringCriteria
        {
            Name = "has_ci_cd",
            Category = "quality",
            Description = "Repository has CI/CD configured",
            Weight = 0.10,
            CalculationType = CalculationType.Boolean,
            Thresholds = new Thresholds()
        };

        yield return new ScoringCriteria
        {
            Name = "has_tests",
            Category = "quality",
            Description = "Repository has test files",
            Weight = 0.08,
            CalculationType = CalculationType.Boolean,
            Thresholds = new Thresholds()
        };

        yield return new ScoringCriteria
        {
            Name = "has_readme",
            Category = "quality",
            Description = "Repository has a README file",
            Weight = 0.05,
            CalculationType = CalculationType.Boolean,
            Thresholds = new Thresholds()
        };

        yield return new ScoringCriteria
        {
            Name = "has_license",
            Category = "quality",
            Description = "Repository has a license file",
            Weight = 0.04,
            CalculationType = CalculationType.Boolean,
            Thresholds = new Thresholds()
        };

        yield return new ScoringCriteria
        {
            Name = "test_coverage",
            Category = "quality",
            Description = "Test coverage percentage",
            Weight = 0.03,
            CalculationType = CalculationType.Percentage,
            Thresholds = new Thresholds { Low = 0, Medium = 60, High = 80 }
        };

        // Maturity criteria (total weight: 0.20)
        yield return new ScoringCriteria
        {
            Name = "maturity_level",
            Category = "maturity",
            Description = "Repository maturity classification",
            Weight = 0.10,
            CalculationType = CalculationType.Count,
            Thresholds = new Thresholds { Low = 0, Medium = 2, High = 4 }
        };

        yield return new ScoringCriteria
        {
            Name = "days_since_release",
            Category = "maturity",
            Description = "Days since last release",
            Weight = 0.06,
            CalculationType = CalculationType.DaysSince,
            Thresholds = new Thresholds { WarningDays = 180, CriticalDays = 365 }
        };

        yield return new ScoringCriteria
        {
            Name = "stars",
            Category = "maturity",
            Description = "GitHub stars (indicates adoption)",
            Weight = 0.04,
            CalculationType = CalculationType.Count,
            Thresholds = new Thresholds { Low = 0, Medium = 10, High = 100 }
        };

        // Risk criteria (total weight: 0.10)
        yield return new ScoringCriteria
        {
            Name = "vulnerabilities",
            Category = "risk",
            Description = "Known security vulnerabilities (fewer is better)",
            Weight = 0.06,
            CalculationType = CalculationType.InverseCount,
            Thresholds = new Thresholds { Low = 0, Medium = 3, High = 10 }
        };

        yield return new ScoringCriteria
        {
            Name = "outdated_dependencies",
            Category = "risk",
            Description = "Outdated dependencies (fewer is better)",
            Weight = 0.04,
            CalculationType = CalculationType.InverseCount,
            Thresholds = new Thresholds { Low = 0, Medium = 5, High = 20 }
        };
    }
}
