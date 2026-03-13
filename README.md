# Repo Portfolio Dashboard

A Windows desktop application to monitor and score your GitHub repositories, with a reusable core that can be exposed as an API or packaged as a NuGet library.

## 🏗️ Architecture

```
RepoPortfolio/
├── src/
│   ├── RepoPortfolio.Core/          # Domain logic (ZERO dependencies - monetizable)
│   │   ├── Models/                  # Repository, Score, ScoringCriteria
│   │   ├── Scoring/                 # ScoringEngine, DefaultCriteriaFactory
│   │   └── Interfaces/              # IRepositoryStore, IGitHubClient
│   │
│   ├── RepoPortfolio.Application/   # Use cases and orchestration
│   │   └── Services/                # PortfolioService
│   │
│   ├── RepoPortfolio.Infrastructure/# External integrations
│   │   ├── Data/                    # SQLite storage (EF Core)
│   │   └── GitHub/                  # Octokit client
│   │
│   ├── RepoPortfolio.Desktop/       # WPF application
│   │   ├── ViewModels/              # MVVM view models
│   │   └── MainWindow.xaml          # UI
│   │
│   └── RepoPortfolio.Api/           # Optional Web API (same Core)
│
└── tests/
    └── RepoPortfolio.Core.Tests/    # Unit tests for scoring engine
```

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK or later
- Windows 10/11 (for Desktop app)

### Run the Desktop App
```bash
cd src/RepoPortfolio.Desktop
dotnet run
```

### Configure GitHub Token (Optional)
Set your GitHub token for higher API rate limits:
```bash
# PowerShell
$env:GITHUB_TOKEN = "ghp_your_token_here"

# Or create a User environment variable via Windows Settings
```

### Run Tests
```bash
dotnet test
```

## 📊 Scoring System

The scoring engine evaluates repositories across 4 categories:

| Category | Weight | Criteria |
|----------|--------|----------|
| **Activity** | 40% | Commit frequency, last commit date, open PRs, contributors |
| **Quality** | 30% | CI/CD, tests, README, license, test coverage |
| **Maturity** | 20% | Maturity level, last release, stars |
| **Risk** | 10% | Vulnerabilities, outdated dependencies |

### Health Levels
- **Excellent** (80-100): Well-maintained, high quality
- **Good** (60-79): Healthy, minor improvements possible
- **Needs Attention** (40-59): Some concerns to address
- **At Risk** (20-39): Significant issues
- **Critical** (0-19): Abandoned or severely unmaintained

## 🔌 Reusability

The `RepoPortfolio.Core` library has **zero external dependencies** and can be:

1. **Used in the Desktop app** (current)
2. **Exposed via Web API** (included in solution)
3. **Packaged as NuGet** for other developers
4. **Licensed** to enterprises

### Example: Using Core in your own app
```csharp
// Register services
services.AddSingleton<IScoringEngine, ScoringEngine>();

// Calculate scores
var engine = new ScoringEngine();
var criteria = DefaultCriteriaFactory.CreateDefaultCriteria();
var score = engine.CalculateScore(repository, criteria);

Console.WriteLine($"Total: {score.TotalScore}, Health: {score.Health}");
```

## 🛠️ Customization

### Add Custom Scoring Criteria
```csharp
var customCriteria = new ScoringCriteria
{
    Name = "documentation_quality",
    Category = "quality",
    Weight = 0.05,
    CalculationType = CalculationType.Percentage,
    Thresholds = new Thresholds { Low = 0, Medium = 50, High = 80 }
};
```

### Adjust Weights
Weights are stored in the SQLite database and can be modified via the UI or directly.

## 📂 Data Storage

Repository data and scores are stored locally in SQLite:
```
%LocalAppData%\RepoPortfolio\portfolio.db
```

## 📝 License

MIT License - See LICENSE file for details.
