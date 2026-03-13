using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RepoPortfolio.Desktop.ViewModels;
using RepoPortfolio.Infrastructure;

namespace RepoPortfolio.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure services
        var services = new ServiceCollection();
        
        // Add RepoPortfolio services (Core, Application, Infrastructure)
        // Pass GitHub token from environment variable or leave null for public repos only
        var gitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        services.AddRepoPortfolio(gitHubToken: gitHubToken);
        
        // Add ViewModels
        services.AddTransient<MainViewModel>();
        
        Services = services.BuildServiceProvider();
        
        // Initialize database
        await Services.InitializeDatabaseAsync();
        
        // Show main window
        var mainWindow = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }
}

