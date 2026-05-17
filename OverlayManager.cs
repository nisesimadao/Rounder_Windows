using Microsoft.Win32;

namespace Rounder.Windows;

public sealed class OverlayManager : IDisposable
{
    private readonly List<CornerOverlayForm> overlays = [];
    private AppSettings settings;

    public OverlayManager(AppSettings settings)
    {
        this.settings = settings;
        SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;
    }

    public event EventHandler? DisplaySettingsChanged;

    public void Apply(AppSettings newSettings)
    {
        settings = newSettings;
        Recreate();
    }

    public void Recreate()
    {
        Clear();
        if (!settings.IsEnabled || settings.CornerRadius <= 0)
        {
            return;
        }

        var selected = settings.SelectedDisplays.Count == 0
            ? Screen.AllScreens.Select(screen => screen.DeviceName).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : settings.SelectedDisplays.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var screen in Screen.AllScreens.Where(screen => selected.Contains(screen.DeviceName)))
        {
            AddScreenOverlays(screen.Bounds);
        }
    }

    private void AddScreenOverlays(Rectangle bounds)
    {
        var radius = Math.Clamp(settings.CornerRadius, 0, 200);
        if (settings.TopLeftEnabled)
        {
            overlays.Add(new CornerOverlayForm(CornerKind.TopLeft, bounds, radius, settings));
        }

        if (settings.TopRightEnabled)
        {
            overlays.Add(new CornerOverlayForm(CornerKind.TopRight, bounds, radius, settings));
        }

        if (settings.BottomLeftEnabled)
        {
            overlays.Add(new CornerOverlayForm(CornerKind.BottomLeft, bounds, radius, settings));
        }

        if (settings.BottomRightEnabled)
        {
            overlays.Add(new CornerOverlayForm(CornerKind.BottomRight, bounds, radius, settings));
        }
    }

    private void HandleDisplaySettingsChanged(object? sender, EventArgs e)
    {
        DisplaySettingsChanged?.Invoke(this, EventArgs.Empty);
        Recreate();
    }

    private void Clear()
    {
        foreach (var overlay in overlays)
        {
            overlay.Close();
            overlay.Dispose();
        }

        overlays.Clear();
    }

    public void Dispose()
    {
        SystemEvents.DisplaySettingsChanged -= HandleDisplaySettingsChanged;
        Clear();
    }
}
