using System.Windows;
using RepoPortfolio.Desktop.Services;
using RepoPortfolio.Desktop.ViewModels;
using WpfApplication = System.Windows.Application;

namespace RepoPortfolio.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Apply default theme
        ThemeService.Instance.ApplyTheme(ThemeType.Dark);
        
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            await vm.LoadRepositoriesCommand.ExecuteAsync(null);
        }
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        WpfApplication.Current.Shutdown();
    }

    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Repo Portfolio Dashboard\n\n" +
            "Version 1.0.0\n\n" +
            "A tool for monitoring and scoring GitHub repositories.\n\n" +
            "© 2026 JuanfeA",
            "About",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}