using System.Globalization;
using System.Diagnostics;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using MediaColor = System.Windows.Media.Color;
using WpfCheckBox = System.Windows.Controls.CheckBox;

namespace Rounder.Windows;

public partial class WpfSettingsWindow : Window
{
    private readonly AppSettings settings;
    private readonly List<CornerPreset> presets;
    private MediaColor selectedColor;

    public WpfSettingsWindow(AppSettings settings, List<CornerPreset> presets)
    {
        this.settings = settings;
        this.presets = presets;
        WpfThemeBootstrap.EnsureApplication();
        WpfThemeBootstrap.ApplySystemTheme();
        InitializeComponent();
        
        // Ensure the window specifically requests the correct theme
        var isDark = AppTheme.Current().IsDark;
        ThemeManager.SetRequestedTheme(this, isDark ? ElementTheme.Dark : ElementTheme.Light);
        
        LoadSettings();
        RefreshPresetList();
        Navigation.SelectedItem = Navigation.MenuItems.OfType<NavigationViewItem>().FirstOrDefault();
        ShowPage("Settings");
        SystemEvents.UserPreferenceChanged += HandleUserPreferenceChanged;
    }

    public event EventHandler<AppSettings>? SettingsApplied;
    public event EventHandler? PresetsChanged;

    private void LoadSettings()
    {
        EnabledBox.IsChecked = settings.IsEnabled;
        RadiusText.Text = settings.CornerRadius.ToString(CultureInfo.InvariantCulture);
        RadiusSlider.Value = settings.CornerRadius;
        RadiusSlider.ValueChanged += (_, _) => RadiusText.Text = ((int)RadiusSlider.Value).ToString(CultureInfo.InvariantCulture);
        selectedColor = ToMediaColor(settings.CornerColor);
        ColorPreview.Background = new SolidColorBrush(selectedColor);
        TopLeftBox.IsChecked = settings.TopLeftEnabled;
        TopRightBox.IsChecked = settings.TopRightEnabled;
        BottomLeftBox.IsChecked = settings.BottomLeftEnabled;
        BottomRightBox.IsChecked = settings.BottomRightEnabled;
        GamingBox.IsChecked = settings.SuperGamingMode;
        SpeedText.Text = settings.GamingSpeed.ToString(CultureInfo.InvariantCulture);
        GlowText.Text = settings.GlowIntensity.ToString(CultureInfo.InvariantCulture);
        LoadDisplays();
    }

    private void LoadDisplays()
    {
        DisplayList.Items.Clear();
        var selected = settings.SelectedDisplays.Count == 0
            ? Screen.AllScreens.Select(screen => screen.DeviceName).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : settings.SelectedDisplays.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var screen in Screen.AllScreens)
        {
            var label = $"{screen.DeviceName}  {screen.Bounds.Width}x{screen.Bounds.Height}" + (screen.Primary ? "  Primary" : "");
            DisplayList.Items.Add(new WpfCheckBox
            {
                Content = label,
                Tag = screen.DeviceName,
                IsChecked = selected.Contains(screen.DeviceName),
                Margin = new Thickness(0, 4, 0, 4)
            });
        }
    }

    private void Apply()
    {
        settings.IsEnabled = EnabledBox.IsChecked == true;
        settings.CornerRadius = ParseInt(RadiusText.Text, 20, 0, 40);
        settings.CornerColor = ToDrawingColor(selectedColor);
        settings.TopLeftEnabled = TopLeftBox.IsChecked == true;
        settings.TopRightEnabled = TopRightBox.IsChecked == true;
        settings.BottomLeftEnabled = BottomLeftBox.IsChecked == true;
        settings.BottomRightEnabled = BottomRightBox.IsChecked == true;
        settings.SuperGamingMode = GamingBox.IsChecked == true;
        settings.GamingSpeed = ParseDecimal(SpeedText.Text, 1.0m, 0.1m, 5.0m);
        settings.GlowIntensity = ParseDecimal(GlowText.Text, 1.0m, 0.1m, 3.0m);
        settings.SelectedDisplays = DisplayList.Items.OfType<WpfCheckBox>()
            .Where(item => item.IsChecked == true)
            .Select(item => item.Tag?.ToString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToList();

        if (settings.SelectedDisplays.Count == 0)
        {
            System.Windows.MessageBox.Show(this, "Select at least one monitor.", "Rounder", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SettingsApplied?.Invoke(this, settings.Clone());
    }

    private void RefreshPresetList()
    {
        PresetList.ItemsSource = null;
        PresetList.ItemsSource = presets;
        if (PresetList.SelectedIndex < 0 && presets.Count > 0)
        {
            PresetList.SelectedIndex = 0;
        }

        UpdatePresetDetails();
    }

    private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string page)
        {
            ShowPage(page);
        }
    }

    private void ShowPage(string page)
    {
        SettingsPage.Visibility = page == "Settings" ? Visibility.Visible : Visibility.Collapsed;
        PresetsPage.Visibility = page == "Presets" ? Visibility.Visible : Visibility.Collapsed;
        PermissionsPage.Visibility = page == "Permissions" ? Visibility.Visible : Visibility.Collapsed;
        CreditsPage.Visibility = page == "Credits" ? Visibility.Visible : Visibility.Collapsed;
    }

    private CornerPreset? SelectedPreset()
    {
        return PresetList.SelectedItem as CornerPreset;
    }

    private void UpdatePresetDetails()
    {
        if (SelectedPreset() is not { } preset)
        {
            PresetDetails.Text = "No preset selected.";
            return;
        }

        PresetDetails.Text = $"{preset.Name}: {preset.CornerRadius}px, {System.Drawing.ColorTranslator.ToHtml(preset.CornerColor)}";
    }

    private void RefreshMonitors_Click(object sender, RoutedEventArgs e) => LoadDisplays();
    private void Black_Click(object sender, RoutedEventArgs e) => SetColor(Colors.Black);
    private void White_Click(object sender, RoutedEventArgs e) => SetColor(Colors.White);
    private void Gray_Click(object sender, RoutedEventArgs e) => SetColor(Colors.Gray);

    private void CustomColor_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new ColorDialog { Color = ToDrawingColor(selectedColor), FullOpen = true };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SetColor(ToMediaColor(dialog.Color));
        }
    }

    private void ApplyPreset_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedPreset() is not { } preset)
        {
            return;
        }

        preset.ApplyTo(settings);
        LoadSettings();
        Apply();
    }

    private void SaveCurrent_Click(object sender, RoutedEventArgs e)
    {
        ApplyControlsToSettingsOnly();
        var name = PromptDialog.Show("New Preset", "Preset name:");
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        presets.Add(CornerPreset.FromSettings(name, settings));
        RefreshPresetList();
        PresetsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EditPreset_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedPreset() is not { } preset)
        {
            return;
        }

        using var editor = new PresetEditorForm(preset);
        if (editor.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            RefreshPresetList();
            PresetsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void DeletePreset_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedPreset() is not { } preset)
        {
            return;
        }

        if (System.Windows.MessageBox.Show(this, $"Delete '{preset.Name}'?", "Rounder", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            presets.Remove(preset);
            RefreshPresetList();
            PresetsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ImportPresets_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.OpenFileDialog { Filter = "Rounder presets (*.json)|*.json|JSON files (*.json)|*.json|All files (*.*)|*.*" };
        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return;
        }

        var imported = JsonStore.ReadPresetFile(dialog.FileName);
        foreach (var preset in imported.Where(preset => presets.All(existing => !string.Equals(existing.Name, preset.Name, StringComparison.OrdinalIgnoreCase))))
        {
            presets.Add(preset);
        }

        RefreshPresetList();
        PresetsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ExportPresets_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.SaveFileDialog
        {
            Filter = "Rounder presets (*.json)|*.json|JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = "rounder_presets.json"
        };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            JsonStore.WritePresetFile(dialog.FileName, presets);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    private void Apply_Click(object sender, RoutedEventArgs e) => Apply();
    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        Apply();
        Close();
    }

    private void OpenGithub_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/nisesimadao/rounder") { UseShellExecute = true });
    }

    protected override void OnClosed(EventArgs e)
    {
        SystemEvents.UserPreferenceChanged -= HandleUserPreferenceChanged;
        base.OnClosed(e);
    }

    private void HandleUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is UserPreferenceCategory.General or UserPreferenceCategory.Color)
        {
            Dispatcher.Invoke(() => {
                WpfThemeBootstrap.ApplySystemTheme();
                var isDark = AppTheme.Current().IsDark;
                ThemeManager.SetRequestedTheme(this, isDark ? ElementTheme.Dark : ElementTheme.Light);
            });
        }
    }

    private void ApplyControlsToSettingsOnly()
    {
        settings.IsEnabled = EnabledBox.IsChecked == true;
        settings.CornerRadius = ParseInt(RadiusText.Text, 20, 0, 40);
        settings.CornerColor = ToDrawingColor(selectedColor);
        settings.TopLeftEnabled = TopLeftBox.IsChecked == true;
        settings.TopRightEnabled = TopRightBox.IsChecked == true;
        settings.BottomLeftEnabled = BottomLeftBox.IsChecked == true;
        settings.BottomRightEnabled = BottomRightBox.IsChecked == true;
        settings.SuperGamingMode = GamingBox.IsChecked == true;
        settings.GamingSpeed = ParseDecimal(SpeedText.Text, 1.0m, 0.1m, 5.0m);
        settings.GlowIntensity = ParseDecimal(GlowText.Text, 1.0m, 0.1m, 3.0m);
    }

    private void SetColor(MediaColor color)
    {
        selectedColor = color;
        ColorPreview.Background = new SolidColorBrush(color);
    }

    private static int ParseInt(string text, int fallback, int min, int max)
    {
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? Math.Clamp(value, min, max)
            : fallback;
    }

    private static decimal ParseDecimal(string text, decimal fallback, decimal min, decimal max)
    {
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? Math.Clamp(value, min, max)
            : fallback;
    }

    private static MediaColor ToMediaColor(System.Drawing.Color color)
    {
        return MediaColor.FromArgb(color.A, color.R, color.G, color.B);
    }

    private static System.Drawing.Color ToDrawingColor(MediaColor color)
    {
        return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

}
