using Microsoft.Win32;

namespace Rounder.Windows;

public sealed class OverlayManager : IDisposable
{
    private readonly List<CornerOverlayForm> overlays = [];
    private readonly List<GamingGlowForm> glowOverlays = [];
    private readonly Control invoker = new();
    private AppSettings settings;

    public OverlayManager(AppSettings settings)
    {
        this.settings = settings;
        _ = invoker.Handle;
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
        if (invoker.IsHandleCreated && invoker.InvokeRequired)
        {
            invoker.BeginInvoke((MethodInvoker)Recreate);
            return;
        }

        Clear();
        if (!settings.IsEnabled || settings.CornerRadius <= 0)
        {
            return;
        }

        var monitors = DisplayMonitor.GetAll();
        var selected = settings.SelectedDisplays.Count == 0
            ? monitors.Select(screen => screen.DeviceName).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : settings.SelectedDisplays.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var screen in monitors.Where(screen => selected.Contains(screen.DeviceName)))
        {
            AddScreenOverlays(screen.Bounds);
            if (settings.SuperGamingMode)
            {
                AddGlowOverlays(screen.Bounds);
            }
        }
    }

    private void AddScreenOverlays(Rectangle bounds)
    {
        var radius = Math.Clamp(settings.CornerRadius, 0, 200);
        if (settings.TopLeftEnabled)
        {
            overlays.Add(new CornerOverlayForm(CornerKind.TopLeft, bounds, radius, 0.0, settings));
        }

        if (settings.TopRightEnabled)
        {
            overlays.Add(new CornerOverlayForm(CornerKind.TopRight, bounds, radius, 0.25, settings));
        }

        if (settings.BottomLeftEnabled)
        {
            overlays.Add(new CornerOverlayForm(CornerKind.BottomLeft, bounds, radius, 0.75, settings));
        }

        if (settings.BottomRightEnabled)
        {
            overlays.Add(new CornerOverlayForm(CornerKind.BottomRight, bounds, radius, 0.5, settings));
        }
    }

    private void AddGlowOverlays(Rectangle bounds)
    {
        glowOverlays.Add(new GamingGlowForm(bounds, ScreenEdge.Top, settings));
        glowOverlays.Add(new GamingGlowForm(bounds, ScreenEdge.Right, settings));
        glowOverlays.Add(new GamingGlowForm(bounds, ScreenEdge.Bottom, settings));
        glowOverlays.Add(new GamingGlowForm(bounds, ScreenEdge.Left, settings));
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

        foreach (var overlay in glowOverlays)
        {
            overlay.Close();
            overlay.Dispose();
        }

        glowOverlays.Clear();
    }

    public void Dispose()
    {
        SystemEvents.DisplaySettingsChanged -= HandleDisplaySettingsChanged;
        Clear();
        invoker.Dispose();
    }
}
