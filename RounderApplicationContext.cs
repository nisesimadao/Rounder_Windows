namespace Rounder.Windows;

public sealed class RounderApplicationContext : ApplicationContext
{
    private readonly NotifyIcon notifyIcon;
    private readonly OverlayManager overlayManager;
    private readonly List<CornerPreset> presets;
    private AppSettings settings;
    private WpfSettingsWindow? settingsWindow;

    public RounderApplicationContext()
    {
        settings = JsonStore.LoadSettings();
        EnsureDisplayDefaults();
        presets = JsonStore.LoadPresets();
        overlayManager = new OverlayManager(settings);
        overlayManager.DisplaySettingsChanged += (_, _) => EnsureDisplayDefaults();
        overlayManager.Recreate();

        notifyIcon = new NotifyIcon
        {
            Icon = AppAssets.AppIcon(),
            Text = "Rounder - screen corner rounding utility",
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };
        notifyIcon.DoubleClick += (_, _) => ShowSettings();
        notifyIcon.MouseUp += (_, args) =>
        {
            if (args.Button == MouseButtons.Left)
            {
                ShowSettings();
            }
        };

        if (!settings.HasLaunchedBefore)
        {
            settings.HasLaunchedBefore = true;
            JsonStore.SaveSettings(settings);
            ShowSettings();
        }
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Settings...", null, (_, _) => ShowSettings());
        menu.Items.Add(settings.IsEnabled ? "Disable Rounded Corners" : "Enable Rounded Corners", null, (_, _) => ToggleEnabled());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Quit", null, (_, _) => ExitThread());
        return menu;
    }

    private void ShowSettings()
    {
        if (settingsWindow is { IsVisible: true })
        {
            settingsWindow.Activate();
            return;
        }

        settingsWindow = new WpfSettingsWindow(settings.Clone(), presets);
        settingsWindow.SettingsApplied += (_, updatedSettings) =>
        {
            settings = updatedSettings.Clone();
            EnsureDisplayDefaults();
            JsonStore.SaveSettings(settings);
            JsonStore.SavePresets(presets);
            overlayManager.Apply(settings);
            notifyIcon.ContextMenuStrip = BuildMenu();
        };
        settingsWindow.PresetsChanged += (_, _) => JsonStore.SavePresets(presets);
        settingsWindow.Closed += (_, _) => settingsWindow = null;
        settingsWindow.Show();
    }

    private void ToggleEnabled()
    {
        settings.IsEnabled = !settings.IsEnabled;
        JsonStore.SaveSettings(settings);
        overlayManager.Apply(settings);
        notifyIcon.ContextMenuStrip = BuildMenu();
    }

    private void EnsureDisplayDefaults()
    {
        var currentDisplays = Screen.AllScreens.Select(screen => screen.DeviceName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (settings.SelectedDisplays.Count == 0)
        {
            settings.SelectedDisplays = [.. currentDisplays];
            JsonStore.SaveSettings(settings);
            return;
        }

        settings.SelectedDisplays = settings.SelectedDisplays
            .Where(currentDisplays.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (settings.SelectedDisplays.Count == 0)
        {
            settings.SelectedDisplays = [.. currentDisplays];
        }

        JsonStore.SaveSettings(settings);
    }

    protected override void ExitThreadCore()
    {
        settingsWindow?.Close();
        overlayManager.Dispose();
        notifyIcon.Visible = false;
        notifyIcon.Dispose();
        base.ExitThreadCore();
    }
}
