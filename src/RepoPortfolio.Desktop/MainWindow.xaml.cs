using System.Windows;
using RepoPortfolio.Desktop.ViewModels;

namespace RepoPortfolio.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            await vm.LoadRepositoriesCommand.ExecuteAsync(null);
        }
    }
}