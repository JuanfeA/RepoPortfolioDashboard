# Desktop App Architecture — Modular Monolith Design

## 🎯 Goals
1. **Local-first Windows desktop app** — no mandatory cloud dependency
2. **Reusable core** — internals can be exposed as API/library for monetization
3. **Single codebase** — no duplication between desktop and potential SaaS version

---

## 🏗️ Recommended Architecture: Clean Architecture with Optional API

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         PRESENTATION LAYER                              │
│  ┌─────────────────┐              ┌─────────────────┐                  │
│  │  Desktop App    │              │  Web API        │  (Optional)      │
│  │  (WPF/WinUI 3)  │              │  (ASP.NET Core) │                  │
│  └────────┬────────┘              └────────┬────────┘                  │
│           │                                │                            │
│           └──────────────┬─────────────────┘                           │
│                          ▼                                              │
├─────────────────────────────────────────────────────────────────────────┤
│                      APPLICATION LAYER                                  │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  RepoPortfolio.Application (Class Library)                      │   │
│  │  - Use Cases / Commands / Queries                               │   │
│  │  - DTOs, Interfaces                                             │   │
│  │  - Orchestration logic                                          │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                          ▼                                              │
├─────────────────────────────────────────────────────────────────────────┤
│                        DOMAIN LAYER                                     │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  RepoPortfolio.Core (Class Library) ← MONETIZABLE               │   │
│  │  - Scoring Engine                                               │   │
│  │  - Domain Models (Repo, Criteria, Score)                        │   │
│  │  - Business Rules                                               │   │
│  │  - NO external dependencies                                     │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                          ▼                                              │
├─────────────────────────────────────────────────────────────────────────┤
│                    INFRASTRUCTURE LAYER                                 │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐      │
│  │ RepoPortfolio    │  │ RepoPortfolio    │  │ RepoPortfolio    │      │
│  │ .Data.SQLite     │  │ .Data.PostgreSQL │  │ .GitHub          │      │
│  │ (Local DB)       │  │ (Cloud DB)       │  │ (API Client)     │      │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘      │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📦 Project Structure

```
RepoPortfolio/
├── src/
│   ├── RepoPortfolio.Core/              # Domain logic (PACKAGEABLE AS NUGET)
│   │   ├── Models/
│   │   │   ├── Repository.cs
│   │   │   ├── ScoringCriteria.cs
│   │   │   └── Score.cs
│   │   ├── Scoring/
│   │   │   ├── IScoringEngine.cs
│   │   │   ├── ScoringEngine.cs
│   │   │   └── ScoreCalculator.cs
│   │   └── Interfaces/
│   │       ├── IRepositoryStore.cs
│   │       └── IGitHubClient.cs
│   │
│   ├── RepoPortfolio.Application/       # Use cases, orchestration
│   │   ├── Commands/
│   │   │   ├── SyncRepositoriesCommand.cs
│   │   │   └── UpdateScoringWeightsCommand.cs
│   │   ├── Queries/
│   │   │   ├── GetPortfolioOverviewQuery.cs
│   │   │   └── GetRepoDetailsQuery.cs
│   │   └── Services/
│   │       └── PortfolioService.cs
│   │
│   ├── RepoPortfolio.Infrastructure/    # External integrations
│   │   ├── GitHub/
│   │   │   └── GitHubApiClient.cs
│   │   ├── Data/
│   │   │   ├── SqliteDbContext.cs
│   │   │   └── Repositories/
│   │   └── Scheduling/
│   │       └── SyncScheduler.cs
│   │
│   ├── RepoPortfolio.Desktop/           # WPF or WinUI 3 app
│   │   ├── Views/
│   │   ├── ViewModels/
│   │   ├── App.xaml
│   │   └── MainWindow.xaml
│   │
│   └── RepoPortfolio.Api/               # OPTIONAL: Web API for remote access
│       ├── Controllers/
│       └── Program.cs
│
├── tests/
│   ├── RepoPortfolio.Core.Tests/
│   └── RepoPortfolio.Application.Tests/
│
└── RepoPortfolio.sln
```

---

## 🖥️ Desktop UI Framework Comparison

| Framework | Pros | Cons | Best For |
|-----------|------|------|----------|
| **WPF** | Mature, extensive docs, MVVM support, rich controls | Windows-only, older tech | Complex data-heavy apps, dashboards |
| **WinUI 3** | Modern UI, Fluent design, future of Windows dev | Windows 10+ only, newer (less docs) | Modern Windows apps |
| **MAUI** | Cross-platform (Win/Mac/iOS/Android) | Larger binary, some rough edges | If you might need Mac support |
| **Avalonia** | Cross-platform, XAML-based like WPF | Smaller ecosystem | Linux support needed |

**Recommendation**: **WPF** for a dashboard-focused app. It has the best data grid controls, charting libraries, and MVVM tooling. If you want modern Fluent UI, consider **WinUI 3**.

---

## 💾 Local Database Options

| Option | Pros | Cons |
|--------|------|------|
| **SQLite** | Zero config, single file, portable | Limited concurrent writes |
| **LiteDB** | NoSQL, embedded, .NET native | Less SQL familiarity |
| **SQL Server LocalDB** | Full SQL Server features | Requires installation |
| **SQLite + EF Core** | Best of both: SQLite simplicity + ORM | Slightly more setup |

**Recommendation**: **SQLite with EF Core** — simple, portable, familiar SQL, easy to migrate to PostgreSQL/Azure SQL later.

---

## 🔄 Reusability / Monetization Paths

### Path 1: NuGet Package (Core Library)
```
RepoPortfolio.Core → NuGet.org (or private feed)
```
- License the scoring engine to other developers
- They integrate it into their own apps
- Revenue: Per-seat license or subscription

### Path 2: Desktop App (Licensed)
```
RepoPortfolio.Desktop → Microsoft Store / Direct Download
```
- Sell the complete desktop application
- Revenue: One-time purchase or subscription

### Path 3: SaaS API (Cloud)
```
RepoPortfolio.Api → Azure/AWS hosted
```
- Same core logic, exposed via REST API
- Revenue: API calls / subscription tiers

### Path 4: Hybrid (Desktop + Cloud Sync)
```
Desktop App ←→ Cloud API ←→ Shared Database
```
- Desktop works offline with local SQLite
- Optional cloud sync for teams
- Revenue: Free local + paid cloud features

---

## 🧱 Code Example: Clean Separation

### Core Layer (No Dependencies)
```csharp
// RepoPortfolio.Core/Scoring/ScoringEngine.cs
namespace RepoPortfolio.Core.Scoring;

public class ScoringEngine : IScoringEngine
{
    public Score CalculateScore(Repository repo, IEnumerable<ScoringCriteria> criteria)
    {
        var totalScore = 0.0;
        var breakdown = new Dictionary<string, double>();

        foreach (var criterion in criteria.Where(c => c.IsActive))
        {
            var value = criterion.Category switch
            {
                "activity" => CalculateActivityScore(repo, criterion),
                "quality" => CalculateQualityScore(repo, criterion),
                "maturity" => CalculateMaturityScore(repo, criterion),
                "risk" => CalculateRiskScore(repo, criterion),
                _ => 0
            };
            
            breakdown[criterion.Name] = value;
            totalScore += value * criterion.Weight;
        }

        return new Score
        {
            RepoId = repo.Id,
            TotalScore = totalScore,
            Breakdown = breakdown,
            CalculatedAt = DateTime.UtcNow
        };
    }
    
    // ... calculation methods
}
```

### Application Layer (Orchestration)
```csharp
// RepoPortfolio.Application/Services/PortfolioService.cs
namespace RepoPortfolio.Application.Services;

public class PortfolioService
{
    private readonly IRepositoryStore _repoStore;
    private readonly IGitHubClient _github;
    private readonly IScoringEngine _scoring;

    public PortfolioService(
        IRepositoryStore repoStore, 
        IGitHubClient github, 
        IScoringEngine scoring)
    {
        _repoStore = repoStore;
        _github = github;
        _scoring = scoring;
    }

    public async Task<SyncResult> SyncRepositoriesAsync(string owner)
    {
        var repos = await _github.GetRepositoriesAsync(owner);
        var criteria = await _repoStore.GetActiveCriteriaAsync();
        
        foreach (var repo in repos)
        {
            var score = _scoring.CalculateScore(repo, criteria);
            await _repoStore.SaveScoreAsync(score);
        }
        
        return new SyncResult { SyncedCount = repos.Count };
    }
}
```

### Desktop Consumes Application Layer
```csharp
// RepoPortfolio.Desktop/ViewModels/DashboardViewModel.cs
public class DashboardViewModel : ObservableObject
{
    private readonly PortfolioService _portfolio;
    
    public ObservableCollection<RepoSummary> Repositories { get; } = new();
    
    [RelayCommand]
    private async Task SyncAsync()
    {
        IsSyncing = true;
        var result = await _portfolio.SyncRepositoriesAsync("myorg");
        await LoadRepositoriesAsync();
        IsSyncing = false;
    }
}
```

### API Consumes Same Application Layer
```csharp
// RepoPortfolio.Api/Controllers/PortfolioController.cs
[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly PortfolioService _portfolio;

    [HttpPost("sync")]
    public async Task<ActionResult<SyncResult>> Sync([FromBody] SyncRequest request)
    {
        var result = await _portfolio.SyncRepositoriesAsync(request.Owner);
        return Ok(result);
    }
}
```

**Key insight**: Both Desktop and API use the **exact same** `PortfolioService` — no duplication!

---

## 📋 Recommended Tech Stack for Desktop-First

| Layer | Technology | Reason |
|-------|------------|--------|
| **UI** | WPF + CommunityToolkit.Mvvm | Mature, great for dashboards |
| **Charts** | LiveCharts2 or OxyPlot | Free, good performance |
| **Data Grid** | Built-in or Syncfusion (free community) | Rich features |
| **Core** | .NET 8 Class Library | Latest LTS, no dependencies |
| **Data** | SQLite + EF Core | Portable, upgradable |
| **GitHub** | Octokit.NET | Official SDK |
| **DI** | Microsoft.Extensions.DependencyInjection | Standard |
| **Scheduling** | Quartz.NET or simple Timer | Background sync |

---

## 🚀 Getting Started Steps

1. Create solution with layered projects
2. Implement Core models + scoring engine (test-driven)
3. Add Infrastructure with SQLite + GitHub client
4. Build minimal WPF dashboard
5. Later: Add optional API project using same Application layer

---

## ❓ Decision: Embedded API or Separate?

### Option A: Embedded Web Server in Desktop
```csharp
// Desktop app hosts its own API
var builder = WebApplication.CreateBuilder();
builder.WebHost.UseUrls("http://localhost:5000");
// ... configure services
var app = builder.Build();
app.MapControllers();
app.RunAsync(); // Run alongside WPF
```
**Pro**: Single exe, simple deployment  
**Con**: Always running, port conflicts possible

### Option B: Separate API Project
```
RepoPortfolio.Desktop.exe  (local use)
RepoPortfolio.Api.exe      (team/remote use)
```
**Pro**: Clean separation, scale independently  
**Con**: Two deployments

### Option C: Shared Library + Multiple Hosts
```
RepoPortfolio.Core.dll     → NuGet package
RepoPortfolio.Desktop.exe  → Uses Core directly
RepoPortfolio.Api          → Uses Core via DI
```
**Pro**: Maximum flexibility, true reuse  
**Con**: More initial setup

**Recommendation**: Start with **Option C** — it's the cleanest for monetization and doesn't require more code, just better organization.
