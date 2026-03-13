using System.Windows;
using WpfApplication = System.Windows.Application;

namespace RepoPortfolio.Desktop.Services;

/// <summary>
/// Service for managing application themes.
/// Implements Observer pattern for theme changes.
/// </summary>
public class ThemeService
{
    private static ThemeService? _instance;
    public static ThemeService Instance => _instance ??= new ThemeService();

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public ThemeType CurrentTheme { get; private set; } = ThemeType.Dark;

    private ThemeService() { }

    /// <summary>
    /// Available themes.
    /// </summary>
    public static IReadOnlyList<ThemeType> AvailableThemes { get; } = 
        [ThemeType.Dark, ThemeType.Classic];

    /// <summary>
    /// Apply a theme to the application.
    /// </summary>
    public void ApplyTheme(ThemeType theme)
    {
        var resourcePath = theme switch
        {
            ThemeType.Classic => "Themes/ClassicTheme.xaml",
            _ => "Themes/DarkTheme.xaml"
        };

        var oldTheme = CurrentTheme;
        CurrentTheme = theme;

        // Clear existing theme resources and apply new ones
        var app = WpfApplication.Current;
        var existingTheme = app.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme") == true);
        
        if (existingTheme != null)
        {
            app.Resources.MergedDictionaries.Remove(existingTheme);
        }

        var newTheme = new ResourceDictionary
        {
            Source = new Uri(resourcePath, UriKind.Relative)
        };
        app.Resources.MergedDictionaries.Add(newTheme);

        // Notify listeners
        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(oldTheme, theme));
    }

    /// <summary>
    /// Toggle between available themes.
    /// </summary>
    public void ToggleTheme()
    {
        var next = CurrentTheme == ThemeType.Dark ? ThemeType.Classic : ThemeType.Dark;
        ApplyTheme(next);
    }
}

/// <summary>
/// Available theme types.
/// </summary>
public enum ThemeType
{
    Dark,
    Classic
}

/// <summary>
/// Event args for theme change notifications.
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
    public ThemeType OldTheme { get; }
    public ThemeType NewTheme { get; }

    public ThemeChangedEventArgs(ThemeType oldTheme, ThemeType newTheme)
    {
        OldTheme = oldTheme;
        NewTheme = newTheme;
    }
}
