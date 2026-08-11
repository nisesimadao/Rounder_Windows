using System.Globalization;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;
using WpfCheckBox = System.Windows.Controls.CheckBox;

namespace Rounder.Windows;

public partial class WpfSettingsWindow : Window
{
    private readonly AppSettings settings;
    private readonly List<CornerPreset> presets;
    private readonly Dictionary<string, FrameworkElement> sections;
    private MediaColor selectedColor;
    private bool selectingFromSidebar;
    private bool selectingFromScroll;
    private string? currentSectionKey;

    public WpfSettingsWindow(AppSettings settings, List<CornerPreset> presets)
    {
        this.settings = settings;
        this.presets = presets;
        InitializeComponent();
        SourceInitialized += (_, _) =>
        {
            WpfWindowEffects.ApplyMica(this);
            KeepWindowInsideWorkingArea();
        };
        sections = new Dictionary<string, FrameworkElement>(StringComparer.OrdinalIgnoreCase)
        {
            ["General"] = GeneralSection,
            ["Appearance"] = AppearanceSection,
            ["Corners"] = CornersSection,
            ["Displays"] = DisplaysSection,
            ["Gaming"] = GamingSection,
            ["Presets"] = PresetsSection,
            ["Permissions"] = PermissionsSection,
            ["About"] = AboutSection
        };
        
        RadiusSlider.ValueChanged += (_, _) => RadiusText.Text = ((int)RadiusSlider.Value).ToString(CultureInfo.InvariantCulture);
        SpeedSlider.ValueChanged += (_, _) => UpdateGamingValueLabels();
        GlowSlider.ValueChanged += (_, _) => UpdateGamingValueLabels();
        BloomSlider.ValueChanged += (_, _) => UpdateGamingValueLabels();

        LoadSettings();
        RefreshPresetList();
        SidebarList.SelectedIndex = 0;
        currentSectionKey = "General";
    }

    public event EventHandler<AppSettings>? SettingsApplied;
    public event EventHandler? PresetsChanged;

    private void LoadSettings()
    {
        EnabledBox.IsChecked = settings.IsEnabled;
        LaunchAtLoginBox.IsChecked = settings.LaunchAtLogin;
        RadiusText.Text = settings.CornerRadius.ToString(CultureInfo.InvariantCulture);
        RadiusSlider.Value = settings.CornerRadius;
        selectedColor = ToMediaColor(settings.CornerColor);
        ColorPreview.Background = new SolidColorBrush(selectedColor);
        CutoutStyleBox.SelectedIndex = settings.CornerCutoutStyle switch
        {
            CornerCutoutStyle.Squircle => 1,
            CornerCutoutStyle.Polygon => 2,
            _ => 0
        };
        TopLeftBox.IsChecked = settings.TopLeftEnabled;
        TopRightBox.IsChecked = settings.TopRightEnabled;
        BottomLeftBox.IsChecked = settings.BottomLeftEnabled;
        BottomRightBox.IsChecked = settings.BottomRightEnabled;
        GamingBox.IsChecked = settings.SuperGamingMode;
        SpeedSlider.Value = (double)Math.Clamp(settings.GamingSpeed, 0.1m, 5.0m);
        GlowSlider.Value = (double)Math.Clamp(settings.GlowIntensity, 0.1m, 3.0m);
        BloomSlider.Value = (double)Math.Clamp(settings.BloomWidth, 0.1m, 3.0m);
        UpdateGamingValueLabels();
        LoadDisplays();
    }

    private void UpdateGamingValueLabels()
    {
        SpeedValue.Text = SliderDecimal(SpeedSlider).ToString("0.0", CultureInfo.InvariantCulture) + "x";
        GlowValue.Text = SliderDecimal(GlowSlider).ToString("0.0", CultureInfo.InvariantCulture);
        BloomValue.Text = SliderDecimal(BloomSlider).ToString("0.0", CultureInfo.InvariantCulture);
    }

    private static decimal SliderDecimal(Slider slider)
    {
        return Math.Round((decimal)slider.Value, 1);
    }

    private void LoadDisplays()
    {
        DisplayList.Items.Clear();
        var monitors = DisplayMonitor.GetAll();
        var selected = settings.SelectedDisplays.Count == 0
            ? monitors.Select(screen => screen.DeviceName).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : settings.SelectedDisplays.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var screen in monitors)
        {
            var label = $"{screen.DeviceName}  {screen.Bounds.Width}x{screen.Bounds.Height}" + (screen.IsPrimary ? "  Primary" : "");
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
        settings.LaunchAtLogin = LaunchAtLoginBox.IsChecked == true;
        settings.CornerRadius = ParseInt(RadiusText.Text, 20, 0, 40);
        settings.CornerColor = ToDrawingColor(selectedColor);
        settings.CornerCutoutStyle = CutoutStyleBox.SelectedIndex switch
        {
            1 => CornerCutoutStyle.Squircle,
            2 => CornerCutoutStyle.Polygon,
            _ => CornerCutoutStyle.Rounded
        };
        settings.TopLeftEnabled = TopLeftBox.IsChecked == true;
        settings.TopRightEnabled = TopRightBox.IsChecked == true;
        settings.BottomLeftEnabled = BottomLeftBox.IsChecked == true;
        settings.BottomRightEnabled = BottomRightBox.IsChecked == true;
        settings.SuperGamingMode = GamingBox.IsChecked == true;
        settings.GamingSpeed = SliderDecimal(SpeedSlider);
        settings.GlowIntensity = SliderDecimal(GlowSlider);
        settings.BloomWidth = SliderDecimal(BloomSlider);
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

    private void SidebarList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (selectingFromScroll)
        {
            return;
        }

        if (SidebarList.SelectedItem is not ListBoxItem { Tag: string tag } || !sections.TryGetValue(tag, out var section))
        {
            RestoreCurrentSidebarSelection();
            return;
        }

        selectingFromSidebar = true;
        currentSectionKey = tag;
        ScrollSectionIntoView(section);
        selectingFromSidebar = false;
    }

    private void DetailScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (selectingFromSidebar)
        {
            return;
        }

        var current = FindCurrentVisibleSection();

        if (current is null)
        {
            return;
        }

        if (string.Equals(currentSectionKey, current, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var item = FindSidebarItem(current);
        if (item is not null && !ReferenceEquals(SidebarList.SelectedItem, item))
        {
            selectingFromScroll = true;
            currentSectionKey = current;
            SidebarList.SelectedItem = item;
            item.BringIntoView();
            selectingFromScroll = false;
        }
    }

    private void ScrollSectionIntoView(FrameworkElement section)
    {
        if (!section.IsVisible)
        {
            return;
        }

        var position = section.TransformToAncestor(SectionsPanel).Transform(new System.Windows.Point(0, 0));
        var targetOffset = Math.Clamp(position.Y, 0, DetailScroll.ScrollableHeight);
        DetailScroll.ScrollToVerticalOffset(targetOffset);
    }

    private string? FindCurrentVisibleSection()
    {
        if (sections.Count == 0)
        {
            return null;
        }

        var activationLine = DetailScroll.VerticalOffset + 72;
        var visible = sections
            .Select(pair => new
            {
                pair.Key,
                Top = pair.Value.TransformToAncestor(SectionsPanel).Transform(new System.Windows.Point(0, 0)).Y
            })
            .Where(item => item.Top <= activationLine)
            .OrderByDescending(item => item.Top)
            .FirstOrDefault();

        return visible?.Key ?? sections.First().Key;
    }

    private ListBoxItem? FindSidebarItem(string tag)
    {
        return SidebarList.Items
            .OfType<ListBoxItem>()
            .FirstOrDefault(candidate => string.Equals(candidate.Tag as string, tag, StringComparison.OrdinalIgnoreCase));
    }

    private void RestoreCurrentSidebarSelection()
    {
        if (currentSectionKey is null)
        {
            return;
        }

        var item = FindSidebarItem(currentSectionKey);
        if (item is null || ReferenceEquals(SidebarList.SelectedItem, item))
        {
            return;
        }

        selectingFromScroll = true;
        SidebarList.SelectedItem = item;
        selectingFromScroll = false;
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

        PresetDetails.Text = $"{preset.Name}: {preset.CornerRadius}px, {preset.CornerCutoutStyle}, {System.Drawing.ColorTranslator.ToHtml(preset.CornerColor)}";
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
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
        System.Windows.Forms.Application.Exit();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        Apply();
        Close();
    }

    private void OpenGithub_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/nisesimadao/rounder_windows") { UseShellExecute = true });
    }

    private void KeepWindowInsideWorkingArea()
    {
        var handle = new WindowInteropHelper(this).Handle;
        var screen = Screen.PrimaryScreen ?? Screen.FromHandle(handle);
        var source = PresentationSource.FromVisual(this);
        var transform = source?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
        var topLeft = transform.Transform(new System.Windows.Point(screen.WorkingArea.Left, screen.WorkingArea.Top));
        var bottomRight = transform.Transform(new System.Windows.Point(screen.WorkingArea.Right, screen.WorkingArea.Bottom));
        var workingWidth = bottomRight.X - topLeft.X;
        var workingHeight = bottomRight.Y - topLeft.Y;
        var width = Math.Min(Width, Math.Max(MinWidth, workingWidth - 32));
        var height = Math.Min(Height, Math.Max(MinHeight, workingHeight - 32));

        Width = width;
        Height = height;
        Left = topLeft.X + Math.Max(16, (workingWidth - width) / 2);
        Top = topLeft.Y + Math.Max(16, (workingHeight - height) / 2);
    }

    private void ApplyControlsToSettingsOnly()
    {
        settings.IsEnabled = EnabledBox.IsChecked == true;
        settings.LaunchAtLogin = LaunchAtLoginBox.IsChecked == true;
        settings.CornerRadius = ParseInt(RadiusText.Text, 20, 0, 40);
        settings.CornerColor = ToDrawingColor(selectedColor);
        settings.CornerCutoutStyle = CutoutStyleBox.SelectedIndex switch
        {
            1 => CornerCutoutStyle.Squircle,
            2 => CornerCutoutStyle.Polygon,
            _ => CornerCutoutStyle.Rounded
        };
        settings.TopLeftEnabled = TopLeftBox.IsChecked == true;
        settings.TopRightEnabled = TopRightBox.IsChecked == true;
        settings.BottomLeftEnabled = BottomLeftBox.IsChecked == true;
        settings.BottomRightEnabled = BottomRightBox.IsChecked == true;
        settings.SuperGamingMode = GamingBox.IsChecked == true;
        settings.GamingSpeed = SliderDecimal(SpeedSlider);
        settings.GlowIntensity = SliderDecimal(GlowSlider);
        settings.BloomWidth = SliderDecimal(BloomSlider);
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

    private static MediaColor ToMediaColor(System.Drawing.Color color)
    {
        return MediaColor.FromArgb(color.A, color.R, color.G, color.B);
    }

    private static System.Drawing.Color ToDrawingColor(MediaColor color)
    {
        return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

}

internal static class WpfWindowEffects
{
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwaSystemBackdropType = 38;
    private const int DwmwcpRound = 2;
    private const int DwmSystemBackdropMica = 2;

    public static void ApplyMica(Window window)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            return;
        }

        if (PresentationSource.FromVisual(window) is HwndSource source)
        {
            source.CompositionTarget.BackgroundColor = Colors.Transparent;
        }

        var handle = new WindowInteropHelper(window).Handle;
        var dark = 1;
        _ = DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkMode, ref dark, sizeof(int));

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            var rounded = DwmwcpRound;
            _ = DwmSetWindowAttribute(handle, DwmwaWindowCornerPreference, ref rounded, sizeof(int));
        }

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621))
        {
            var backdrop = DwmSystemBackdropMica;
            _ = DwmSetWindowAttribute(handle, DwmwaSystemBackdropType, ref backdrop, sizeof(int));
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);
}
