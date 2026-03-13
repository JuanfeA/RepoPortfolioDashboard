using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using RepoPortfolio.Core.Models;

namespace RepoPortfolio.Desktop;

/// <summary>
/// Converts bool to inverted bool.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public static InverseBoolConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;
}

/// <summary>
/// Converts bool to Visibility.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public static BoolToVisibilityConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Visible;
}

/// <summary>
/// Converts SdlcPhase to display string.
/// </summary>
public class SdlcPhaseConverter : IValueConverter
{
    public static SdlcPhaseConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SdlcPhase phase)
        {
            return phase switch
            {
                SdlcPhase.Ideation => "💡 Ideation",
                SdlcPhase.Planning => "📋 Planning",
                SdlcPhase.Development => "🔨 Development",
                SdlcPhase.Testing => "🧪 Testing",
                SdlcPhase.Release => "🚀 Release",
                SdlcPhase.Maintenance => "🔧 Maintenance",
                SdlcPhase.Deprecated => "⚠️ Deprecated",
                _ => "❓ Unknown"
            };
        }
        return "❓ Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts SdlcPhase to color.
/// </summary>
public class SdlcPhaseColorConverter : IValueConverter
{
    public static SdlcPhaseColorConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var colorHex = value is SdlcPhase phase ? phase switch
        {
            SdlcPhase.Ideation => "#9333EA",      // Purple
            SdlcPhase.Planning => "#3B82F6",      // Blue
            SdlcPhase.Development => "#22C55E",   // Green
            SdlcPhase.Testing => "#F59E0B",       // Amber
            SdlcPhase.Release => "#10B981",       // Emerald
            SdlcPhase.Maintenance => "#6366F1",   // Indigo
            SdlcPhase.Deprecated => "#6B7280",    // Gray
            _ => "#6B7280"                         // Gray
        } : "#6B7280";

        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts HealthStatus to display string with emoji.
/// </summary>
public class HealthStatusConverter : IValueConverter
{
    public static HealthStatusConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is HealthStatus status)
        {
            return status switch
            {
                HealthStatus.Excellent => "🌟 Excellent",
                HealthStatus.Good => "✅ Good",
                HealthStatus.NeedsAttention => "⚠️ Needs Attention",
                HealthStatus.AtRisk => "🔶 At Risk",
                HealthStatus.Critical => "🔴 Critical",
                _ => "❓ Unknown"
            };
        }
        return "❓ Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts null or empty to visibility (collapsed if null/empty).
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public static NullToVisibilityConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return Visibility.Collapsed;
        if (value is string s && string.IsNullOrWhiteSpace(s)) return Visibility.Collapsed;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts a score to a color gradient (red-yellow-green).
/// </summary>
public class ScoreToColorConverter : IValueConverter
{
    public static ScoreToColorConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var score = value switch
        {
            double d => d,
            int i => i,
            _ => 0.0
        };

        var colorHex = score switch
        {
            >= 80 => "#22C55E",  // Green
            >= 60 => "#84CC16",  // Lime
            >= 40 => "#EAB308",  // Yellow
            >= 20 => "#F97316",  // Orange
            _ => "#EF4444"       // Red
        };

        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
