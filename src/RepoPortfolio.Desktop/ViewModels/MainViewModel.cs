using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RepoPortfolio.Application.Services;
using RepoPortfolio.Core.Interfaces;
using RepoPortfolio.Core.Models;
using RepoPortfolio.Desktop.Services;

namespace RepoPortfolio.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly PortfolioService _portfolio;
    private readonly IRepoAnalyzer? _analyzer;

    public MainViewModel(PortfolioService portfolio, IRepoAnalyzer? analyzer = null)
    {
        _portfolio = portfolio;
        _analyzer = analyzer;
        
        // Subscribe to theme changes
        ThemeService.Instance.ThemeChanged += (_, e) => 
        {
            OnPropertyChanged(nameof(CurrentThemeName));
            OnPropertyChanged(nameof(IsClassicTheme));
        };
    }

    [ObservableProperty]
    private ObservableCollection<RepositorySummaryViewModel> _repositories = [];

    [ObservableProperty]
    private RepositorySummaryViewModel? _selectedRepository;

    [ObservableProperty]
    private string _owner = "";

    [ObservableProperty]
    private bool _isSyncing;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isAnalyzing;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private double _averageScore;

    [ObservableProperty]
    private int _totalRepositories;

    [ObservableProperty]
    private int _healthyRepos;

    [ObservableProperty]
    private int _atRiskRepos;

    // Theme properties
    public string CurrentThemeName => ThemeService.Instance.CurrentTheme == ThemeType.Classic 
        ? "Classic" : "Modern Dark";
    
    public bool IsClassicTheme => ThemeService.Instance.CurrentTheme == ThemeType.Classic;
    
    public bool IsAiAvailable => _analyzer?.IsAvailable ?? false;

    [RelayCommand]
    private async Task LoadRepositoriesAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading repositories...";

        try
        {
            var overview = await _portfolio.GetOverviewAsync();
            
            Repositories.Clear();
            foreach (var item in overview.Repositories)
            {
                Repositories.Add(new RepositorySummaryViewModel(item.Repository, item.LatestScore));
            }

            TotalRepositories = overview.TotalCount;
            AverageScore = Math.Round(overview.AverageScore, 1);
            StatusMessage = $"Loaded {TotalRepositories} repositories";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SyncRepositoriesAsync()
    {
        if (string.IsNullOrWhiteSpace(Owner))
        {
            StatusMessage = "Please enter a GitHub username or organization";
            return;
        }

        IsSyncing = true;
        StatusMessage = $"Syncing repositories for {Owner}...";

        try
        {
            var result = await _portfolio.SyncRepositoriesAsync(Owner);
            
            if (result.Success)
            {
                StatusMessage = $"Synced {result.FetchedCount} repositories, scored {result.ScoredCount}";
                await LoadRepositoriesAsync();
            }
            else
            {
                StatusMessage = $"Sync failed: {result.Error}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsSyncing = false;
        }
    }

    [RelayCommand]
    private async Task RecalculateScoresAsync()
    {
        IsLoading = true;
        StatusMessage = "Recalculating scores...";

        try
        {
            var count = await _portfolio.RecalculateAllScoresAsync();
            StatusMessage = $"Recalculated {count} scores";
            await LoadRepositoriesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        ThemeService.Instance.ToggleTheme();
        StatusMessage = $"Switched to {CurrentThemeName} theme";
    }

    [RelayCommand]
    private void ApplyDarkTheme()
    {
        ThemeService.Instance.ApplyTheme(ThemeType.Dark);
        StatusMessage = "Switched to Modern Dark theme";
    }

    [RelayCommand]
    private void ApplyClassicTheme()
    {
        ThemeService.Instance.ApplyTheme(ThemeType.Classic);
        StatusMessage = "Switched to Classic Windows theme";
    }

    [RelayCommand]
    private async Task AnalyzeSelectedAsync()
    {
        if (SelectedRepository == null)
        {
            StatusMessage = "Please select a repository to analyze";
            return;
        }

        if (_analyzer == null || !_analyzer.IsAvailable)
        {
            StatusMessage = "AI analysis not available. Configure OPENAI_API_KEY or ANTHROPIC_API_KEY.";
            return;
        }

        IsAnalyzing = true;
        StatusMessage = $"Analyzing {SelectedRepository.Name}...";

        try
        {
            var insights = await _portfolio.AnalyzeRepositoryAsync(SelectedRepository.FullName);
            if (insights != null)
            {
                SelectedRepository.UpdateInsights(insights);
                StatusMessage = $"Analysis complete: {insights.Phase} phase, {insights.MaturityScore}% maturity";
            }
            else
            {
                StatusMessage = "Analysis returned no results";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Analysis error: {ex.Message}";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    private void UpdateHealthStats()
    {
        HealthyRepos = Repositories.Count(r => r.Health is HealthStatus.Excellent or HealthStatus.Good);
        AtRiskRepos = Repositories.Count(r => r.Health is HealthStatus.AtRisk or HealthStatus.Critical);
    }
}

public partial class RepositorySummaryViewModel : ObservableObject
{
    public RepositorySummaryViewModel(Repository repo, Score? score)
    {
        Id = repo.Id;
        Name = repo.Name;
        FullName = repo.FullName;
        Description = repo.Description ?? "";
        Language = repo.PrimaryLanguage ?? "Unknown";
        IsFork = repo.IsFork;
        IsArchived = repo.IsArchived;
        
        TotalScore = score?.TotalScore ?? 0;
        Health = score?.Health ?? HealthStatus.Critical;
        ActivityScore = score?.Categories.Activity ?? 0;
        QualityScore = score?.Categories.Quality ?? 0;
        MaturityScore = score?.Categories.Maturity ?? 0;
        RiskScore = score?.Categories.Risk ?? 0;
        
        LastCommit = repo.LastCommitDate?.ToString("yyyy-MM-dd") ?? "Never";
        LastCommitDays = repo.LastCommitDate.HasValue 
            ? (int)(DateTime.UtcNow - repo.LastCommitDate.Value).TotalDays 
            : -1;
        Stars = repo.StarCount;
        Forks = repo.ForkCount;
        Issues = repo.OpenIssueCount;
        PullRequests = repo.OpenPullRequestCount;
        HasCiCd = repo.HasCiCd;
        HasTests = repo.HasTests;
        HasReadme = repo.HasReadme;
        HasLicense = repo.HasLicense;
    }

    public Guid Id { get; }
    public string Name { get; }
    public string FullName { get; }
    public string Description { get; }
    public string Language { get; }
    public bool IsFork { get; }
    public bool IsArchived { get; }
    
    public double TotalScore { get; }
    public HealthStatus Health { get; }
    public double ActivityScore { get; }
    public double QualityScore { get; }
    public double MaturityScore { get; }
    public double RiskScore { get; }
    
    public string LastCommit { get; }
    public int LastCommitDays { get; }
    public int Stars { get; }
    public int Forks { get; }
    public int Issues { get; }
    public int PullRequests { get; }
    public bool HasCiCd { get; }
    public bool HasTests { get; }
    public bool HasReadme { get; }
    public bool HasLicense { get; }

    // SDLC Insights (populated by AI analysis)
    [ObservableProperty]
    private SdlcPhase _phase = SdlcPhase.Unknown;

    [ObservableProperty]
    private string? _phaseRationale;

    [ObservableProperty]
    private int _aiMaturityScore;

    [ObservableProperty]
    private string? _recommendedActions;

    [ObservableProperty]
    private string? _riskFactors;

    [ObservableProperty]
    private bool _hasInsights;

    public void UpdateInsights(RepoInsights insights)
    {
        Phase = insights.Phase;
        PhaseRationale = insights.PhaseRationale;
        AiMaturityScore = insights.MaturityScore;
        RecommendedActions = insights.RecommendedActions;
        RiskFactors = insights.RiskFactors;
        HasInsights = true;
    }

    public string HealthColor => Health switch
    {
        HealthStatus.Excellent => "#22C55E",
        HealthStatus.Good => "#84CC16",
        HealthStatus.NeedsAttention => "#EAB308",
        HealthStatus.AtRisk => "#F97316",
        HealthStatus.Critical => "#EF4444",
        _ => "#6B7280"
    };

    public string PhaseColor => Phase switch
    {
        SdlcPhase.Ideation => "#9333EA",
        SdlcPhase.Planning => "#3B82F6",
        SdlcPhase.Development => "#22C55E",
        SdlcPhase.Testing => "#F59E0B",
        SdlcPhase.Release => "#10B981",
        SdlcPhase.Maintenance => "#6366F1",
        SdlcPhase.Deprecated => "#6B7280",
        _ => "#6B7280"
    };

    public string PhaseIcon => Phase switch
    {
        SdlcPhase.Ideation => "💡",
        SdlcPhase.Planning => "📋",
        SdlcPhase.Development => "🔨",
        SdlcPhase.Testing => "🧪",
        SdlcPhase.Release => "🚀",
        SdlcPhase.Maintenance => "🔧",
        SdlcPhase.Deprecated => "⚠️",
        _ => "❓"
    };

    public string LastCommitDisplay => LastCommitDays switch
    {
        < 0 => "Never",
        0 => "Today",
        1 => "Yesterday",
        < 7 => $"{LastCommitDays} days ago",
        < 30 => $"{LastCommitDays / 7} weeks ago",
        < 365 => $"{LastCommitDays / 30} months ago",
        _ => $"{LastCommitDays / 365} years ago"
    };
}
