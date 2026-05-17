using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;
using WpfApplication = System.Windows.Application;

namespace Rounder.Windows;

public static class WpfThemeBootstrap
{
    private static readonly MediaColor RounderAccentColor = MediaColor.FromRgb(96, 96, 96);
    private static bool initialized;
    private static ThemeResources? themeResources;

    public static void EnsureApplication()
    {
        if (WpfApplication.Current is null)
        {
            _ = new WpfApplication
            {
                ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown
            };
        }

        if (initialized)
        {
            return;
        }

        themeResources = new ThemeResources
        {
            AccentColor = RounderAccentColor
        };
        var app = WpfApplication.Current ?? throw new InvalidOperationException("WPF application could not be initialized.");
        app.Resources.MergedDictionaries.Add(themeResources);
        app.Resources.MergedDictionaries.Add(new XamlControlsResources());
        initialized = true;
    }

    public static void ApplySystemTheme()
    {
        EnsureApplication();

        var isDark = AppTheme.Current().IsDark;
        var theme = isDark ? ApplicationTheme.Dark : ApplicationTheme.Light;

        ThemeManager.Current.ApplicationTheme = theme;
        ThemeManager.Current.AccentColor = RounderAccentColor;
        
        if (themeResources is not null)
        {
            themeResources.RequestedTheme = theme;
            themeResources.AccentColor = RounderAccentColor;
        }
    }
}
