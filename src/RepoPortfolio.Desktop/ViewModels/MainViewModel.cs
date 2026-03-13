using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RepoPortfolio.Application.Services;
using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly PortfolioService _portfolio;

    public MainViewModel(PortfolioService portfolio)
    {
        _portfolio = portfolio;
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
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private double _averageScore;

    [ObservableProperty]
    private int _totalRepositories;

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
        
        TotalScore = score?.TotalScore ?? 0;
        Health = score?.Health ?? HealthStatus.Critical;
        ActivityScore = score?.Categories.Activity ?? 0;
        QualityScore = score?.Categories.Quality ?? 0;
        MaturityScore = score?.Categories.Maturity ?? 0;
        RiskScore = score?.Categories.Risk ?? 0;
        
        LastCommit = repo.LastCommitDate?.ToString("yyyy-MM-dd") ?? "Never";
        Stars = repo.StarCount;
        Issues = repo.OpenIssueCount;
        HasCiCd = repo.HasCiCd;
        HasTests = repo.HasTests;
    }

    public Guid Id { get; }
    public string Name { get; }
    public string FullName { get; }
    public string Description { get; }
    public string Language { get; }
    
    public double TotalScore { get; }
    public HealthStatus Health { get; }
    public double ActivityScore { get; }
    public double QualityScore { get; }
    public double MaturityScore { get; }
    public double RiskScore { get; }
    
    public string LastCommit { get; }
    public int Stars { get; }
    public int Issues { get; }
    public bool HasCiCd { get; }
    public bool HasTests { get; }

    public string HealthColor => Health switch
    {
        HealthStatus.Excellent => "#22C55E",
        HealthStatus.Good => "#84CC16",
        HealthStatus.NeedsAttention => "#EAB308",
        HealthStatus.AtRisk => "#F97316",
        HealthStatus.Critical => "#EF4444",
        _ => "#6B7280"
    };
}
