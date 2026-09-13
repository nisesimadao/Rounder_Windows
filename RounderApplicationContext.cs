using System.Diagnostics;

namespace Rounder.Windows;

public sealed class RounderApplicationContext : ApplicationContext
{
    private readonly NotifyIcon notifyIcon;
    private readonly OverlayManager overlayManager;
    private readonly List<CornerPreset> presets;
    private readonly System.Windows.Forms.Timer restartTimer;
    private readonly Control activationInvoker = new();
    private AppSettings settings;
    private WpfSettingsWindow? settingsWindow;
    private TrayPanelWindow? trayPanelWindow;
    private bool isRestarting;

    public RounderApplicationContext(bool launchedInBackground = false)
    {
        _ = activationInvoker.Handle;
        settings = JsonStore.LoadSettings();
        settings.LaunchAtLogin = StartupManager.IsEnabled();
        if (settings.LaunchAtLogin)
        {
            settings.LaunchAtLogin = StartupManager.SetEnabled(true);
        }

        EnsureDisplayDefaults();
        presets = JsonStore.LoadPresets();
        restartTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        restartTimer.Tick += (_, _) => RestartApplication();
        overlayManager = new OverlayManager(settings);
        overlayManager.DisplaySettingsChanged += (_, _) => ScheduleRestartAfterDisplayChange();
        overlayManager.Recreate();

        notifyIcon = new NotifyIcon
        {
            Icon = AppAssets.AppIcon(),
            Text = "Rounder - screen corner rounding utility",
            Visible = settings.ShowTrayIcon
        };
        notifyIcon.DoubleClick += (_, _) => ShowSettings();
        notifyIcon.MouseUp += (_, args) =>
        {
            if (args.Button is MouseButtons.Left or MouseButtons.Right)
            {
                ShowTrayPanel();
            }
        };

        if (!settings.HasLaunchedBefore)
        {
            settings.HasLaunchedBefore = true;
            JsonStore.SaveSettings(settings);
        }

        if (!launchedInBackground)
        {
            ShowSettings();
        }
    }

    public void ActivateFromSecondaryInstance()
    {
        if (activationInvoker.IsDisposed)
        {
            return;
        }

        if (activationInvoker.InvokeRequired)
        {
            activationInvoker.BeginInvoke((MethodInvoker)ShowSettings);
            return;
        }

        ShowSettings();
    }

    private void ShowTrayPanel()
    {
        if (!notifyIcon.Visible)
        {
            return;
        }

        if (trayPanelWindow is { IsVisible: true })
        {
            trayPanelWindow.ReloadFrom(settings);
            trayPanelWindow.Activate();
            return;
        }

        trayPanelWindow = new TrayPanelWindow(
            settings,
            ApplyQuickSetting,
            ShowSettings,
            ExitThread);
        trayPanelWindow.Closed += (_, _) => trayPanelWindow = null;
        trayPanelWindow.Show();
    }

    private void ApplyQuickSetting(Action<AppSettings> update)
    {
        update(settings);
        JsonStore.SaveSettings(settings);
        overlayManager.Apply(settings);
        settingsWindow?.ReloadFrom(settings);
    }

    private void ScheduleRestartAfterDisplayChange()
    {
        EnsureDisplayDefaults();
        restartTimer.Stop();
        restartTimer.Start();
    }

    private void RestartApplication()
    {
        if (isRestarting)
        {
            return;
        }

        isRestarting = true;
        restartTimer.Stop();
        JsonStore.SaveSettings(settings);
        JsonStore.SavePresets(presets);

        var executablePath = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(executablePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = "--background",
                WorkingDirectory = AppContext.BaseDirectory,
                UseShellExecute = true
            });
        }

        ExitThread();
    }

    private void ShowSettings()
    {
        trayPanelWindow?.Close();

        if (settingsWindow is { IsVisible: true })
        {
            settingsWindow.Activate();
            return;
        }

        settingsWindow = new WpfSettingsWindow(settings.Clone(), presets);
        settingsWindow.SettingsApplied += (_, updatedSettings) =>
        {
            settings = updatedSettings.Clone();
            settings.LaunchAtLogin = StartupManager.SetEnabled(settings.LaunchAtLogin);
            EnsureDisplayDefaults();
            JsonStore.SaveSettings(settings);
            JsonStore.SavePresets(presets);
            overlayManager.Apply(settings);
            notifyIcon.Visible = settings.ShowTrayIcon;
            if (!notifyIcon.Visible)
            {
                trayPanelWindow?.Close();
            }
            else
            {
                trayPanelWindow?.ReloadFrom(settings);
            }
        };
        settingsWindow.PresetsChanged += (_, _) => JsonStore.SavePresets(presets);
        settingsWindow.Closed += (_, _) => settingsWindow = null;
        settingsWindow.Show();
        settingsWindow.Activate();
    }

    private void EnsureDisplayDefaults()
    {
        var currentDisplays = DisplayMonitor.GetAll()
            .Select(screen => screen.DeviceName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var currentSet = currentDisplays.ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!settings.DisplaySelectionInitialized)
        {
            if (settings.SelectedDisplays.Count == 0)
            {
                settings.SelectedDisplays = [.. currentDisplays];
            }
            else
            {
                settings.SelectedDisplays = settings.SelectedDisplays
                    .Where(currentSet.Contains)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            settings.DisplaySelectionInitialized = true;
            settings.KnownDisplays = [.. currentDisplays];
            JsonStore.SaveSettings(settings);
            return;
        }

        var knownSet = settings.KnownDisplays.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var newlyConnected = currentDisplays.Where(display => !knownSet.Contains(display));
        settings.SelectedDisplays = settings.SelectedDisplays
            .Where(currentSet.Contains)
            .Concat(newlyConnected)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        settings.KnownDisplays = [.. currentDisplays];
        JsonStore.SaveSettings(settings);
    }

    protected override void ExitThreadCore()
    {
        trayPanelWindow?.Close();
        settingsWindow?.Close();
        restartTimer.Stop();
        restartTimer.Dispose();
        overlayManager.Dispose();
        notifyIcon.Visible = false;
        notifyIcon.Dispose();
        activationInvoker.Dispose();
        System.Windows.Application.Current?.Shutdown();
        base.ExitThreadCore();
    }
}
