using System.Diagnostics;
using System.Globalization;
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
        SourceInitialized += (_, _) => KeepWindowInsideWorkingArea();
        sections = new Dictionary<string, FrameworkElement>(StringComparer.OrdinalIgnoreCase)
        {
            ["General"] = GeneralSection,
            ["Appearance"] = AppearanceSection,
            ["Corners"] = CornersSection,
            ["Displays"] = DisplaysSection,
            ["Gaming"] = GamingSection,
            ["Presets"] = PresetsSection,
            ["About"] = AboutSection
        };

        RadiusSlider.ValueChanged += (_, _) => RadiusValue.Text = $"{(int)RadiusSlider.Value} px";
        SpeedSlider.ValueChanged += (_, _) => UpdateGamingValueLabels();
        GlowSlider.ValueChanged += (_, _) => UpdateGamingValueLabels();
        BloomSlider.ValueChanged += (_, _) => UpdateGamingValueLabels();

        VersionText.Text = $"Version {typeof(WpfSettingsWindow).Assembly.GetName().Version?.ToString(3) ?? "Unknown"}";
        LoadSettings();
        RefreshPresetList();
        SidebarList.SelectedIndex = 0;
        currentSectionKey = "General";
    }

    public event EventHandler<AppSettings>? SettingsApplied;
    public event EventHandler? PresetsChanged;

    public void ReloadFrom(AppSettings updated)
    {
        settings.HasLaunchedBefore = updated.HasLaunchedBefore;
        settings.IsEnabled = updated.IsEnabled;
        settings.CornerRadius = updated.CornerRadius;
        settings.CornerColorArgb = updated.CornerColorArgb;
        settings.TopLeftEnabled = updated.TopLeftEnabled;
        settings.TopRightEnabled = updated.TopRightEnabled;
        settings.BottomLeftEnabled = updated.BottomLeftEnabled;
        settings.BottomRightEnabled = updated.BottomRightEnabled;
        settings.SuperGamingMode = updated.SuperGamingMode;
        settings.GamingSpeed = updated.GamingSpeed;
        settings.GlowIntensity = updated.GlowIntensity;
        settings.BloomWidth = updated.BloomWidth;
        settings.CornerCutoutStyle = updated.CornerCutoutStyle;
        settings.LaunchAtLogin = updated.LaunchAtLogin;
        settings.ShowTrayIcon = updated.ShowTrayIcon;
        settings.DisplaySelectionInitialized = updated.DisplaySelectionInitialized;
        settings.KnownDisplays = [.. updated.KnownDisplays];
        settings.SelectedDisplays = [.. updated.SelectedDisplays];
        LoadSettings();
    }

    private void LoadSettings()
    {
        EnabledBox.IsChecked = settings.IsEnabled;
        LaunchAtLoginBox.IsChecked = settings.LaunchAtLogin;
        ShowTrayIconBox.IsChecked = settings.ShowTrayIcon;
        RadiusSlider.Value = settings.CornerRadius;
        RadiusValue.Text = $"{settings.CornerRadius} px";
        selectedColor = ToMediaColor(settings.CornerColor);
        ColorPreview.Background = new SolidColorBrush(selectedColor);
        UpdateColorSwatches();
        RoundedStyleButton.IsChecked = settings.CornerCutoutStyle == CornerCutoutStyle.Rounded;
        SquircleStyleButton.IsChecked = settings.CornerCutoutStyle == CornerCutoutStyle.Squircle;
        PolygonStyleButton.IsChecked = settings.CornerCutoutStyle == CornerCutoutStyle.Polygon;
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

    private static decimal SliderDecimal(Slider slider) => Math.Round((decimal)slider.Value, 1);

    private void LoadDisplays()
    {
        DisplayList.Items.Clear();
        var monitors = DisplayMonitor.GetAll();
        var selected = settings.DisplaySelectionInitialized
            ? settings.SelectedDisplays.ToHashSet(StringComparer.OrdinalIgnoreCase)
            : monitors.Select(screen => screen.DeviceName).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var screen in monitors)
        {
            var label = $"{screen.DeviceName}  {screen.Bounds.Width}×{screen.Bounds.Height}" + (screen.IsPrimary ? "  Primary" : "");
            DisplayList.Items.Add(new WpfCheckBox
            {
                Content = label,
                Tag = screen.DeviceName,
                IsChecked = selected.Contains(screen.DeviceName),
                Margin = new Thickness(4, 5, 4, 5)
            });
        }
    }

    private void Apply()
    {
        settings.IsEnabled = EnabledBox.IsChecked == true;
        settings.LaunchAtLogin = LaunchAtLoginBox.IsChecked == true;
        settings.ShowTrayIcon = ShowTrayIconBox.IsChecked == true;
        settings.CornerRadius = (int)Math.Round(RadiusSlider.Value);
        settings.CornerColor = ToDrawingColor(selectedColor);
        settings.CornerCutoutStyle = SelectedCornerStyle();
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
        settings.DisplaySelectionInitialized = true;
        settings.KnownDisplays = DisplayMonitor.GetAll().Select(screen => screen.DeviceName).ToList();
        SettingsApplied?.Invoke(this, settings.Clone());
    }

    private void RefreshPresetList()
    {
        PresetList.ItemsSource = null;
        PresetList.ItemsSource = presets;
    }

    private void SidebarList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (selectingFromScroll) return;
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
        if (selectingFromSidebar) return;
        var current = FindCurrentVisibleSection();
        if (current is null || string.Equals(currentSectionKey, current, StringComparison.OrdinalIgnoreCase)) return;
        var item = FindSidebarItem(current);
        if (item is null || ReferenceEquals(SidebarList.SelectedItem, item)) return;
        selectingFromScroll = true;
        currentSectionKey = current;
        SidebarList.SelectedItem = item;
        item.BringIntoView();
        selectingFromScroll = false;
    }

    private void ScrollSectionIntoView(FrameworkElement section)
    {
        if (!section.IsVisible) return;
        var position = section.TransformToAncestor(SectionsPanel).Transform(new System.Windows.Point(0, 0));
        DetailScroll.ScrollToVerticalOffset(Math.Clamp(position.Y, 0, DetailScroll.ScrollableHeight));
    }

    private string? FindCurrentVisibleSection()
    {
        var activationLine = DetailScroll.VerticalOffset + 72;
        return sections.Select(pair => new
            {
                pair.Key,
                Top = pair.Value.TransformToAncestor(SectionsPanel).Transform(new System.Windows.Point(0, 0)).Y
            })
            .Where(item => item.Top <= activationLine)
            .OrderByDescending(item => item.Top)
            .Select(item => item.Key)
            .FirstOrDefault() ?? sections.Keys.FirstOrDefault();
    }

    private ListBoxItem? FindSidebarItem(string tag) => SidebarList.Items
        .OfType<ListBoxItem>()
        .FirstOrDefault(candidate => string.Equals(candidate.Tag as string, tag, StringComparison.OrdinalIgnoreCase));

    private void RestoreCurrentSidebarSelection()
    {
        if (currentSectionKey is null) return;
        var item = FindSidebarItem(currentSectionKey);
        if (item is null || ReferenceEquals(SidebarList.SelectedItem, item)) return;
        selectingFromScroll = true;
        SidebarList.SelectedItem = item;
        selectingFromScroll = false;
    }

    private void RefreshMonitors_Click(object sender, RoutedEventArgs e)
    {
        var knownInView = DisplayList.Items.OfType<WpfCheckBox>()
            .Select(item => item.Tag?.ToString()).Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var selectedInView = DisplayList.Items.OfType<WpfCheckBox>()
            .Where(item => item.IsChecked == true)
            .Select(item => item.Tag?.ToString()).Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var current = DisplayMonitor.GetAll();
        foreach (var monitor in current.Where(monitor => !knownInView.Contains(monitor.DeviceName)))
        {
            selectedInView.Add(monitor.DeviceName);
        }
        settings.DisplaySelectionInitialized = true;
        settings.SelectedDisplays = [.. selectedInView];
        settings.KnownDisplays = current.Select(monitor => monitor.DeviceName).ToList();
        LoadDisplays();
    }

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
        if (sender is not System.Windows.Controls.Button { DataContext: CornerPreset preset }) return;
        preset.ApplyTo(settings);
        LoadSettings();
        Apply();
    }

    private void SaveCurrent_Click(object sender, RoutedEventArgs e)
    {
        ApplyControlsToSettingsOnly();
        var name = PromptDialogWindow.Show(this, "New Preset", "Preset name:");
        if (string.IsNullOrWhiteSpace(name)) return;
        presets.Add(CornerPreset.FromSettings(name, settings));
        RefreshPresetList();
        PresetsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EditPreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button { DataContext: CornerPreset preset }) return;
        var editor = new PresetEditorWindow(preset) { Owner = this };
        if (editor.ShowDialog() == true)
        {
            RefreshPresetList();
            PresetsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void DeletePreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button { DataContext: CornerPreset preset }) return;
        if (System.Windows.MessageBox.Show(this, $"Delete '{preset.Name}'?", "Rounder", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            presets.Remove(preset);
            RefreshPresetList();
            PresetsChanged?.Invoke(this, EventArgs.Empty);
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

    private void OpenGithub_Click(object sender, RoutedEventArgs e) =>
        Process.Start(new ProcessStartInfo("https://github.com/nisesimadao/rounder_windows") { UseShellExecute = true });

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
        settings.ShowTrayIcon = ShowTrayIconBox.IsChecked == true;
        settings.CornerRadius = (int)Math.Round(RadiusSlider.Value);
        settings.CornerColor = ToDrawingColor(selectedColor);
        settings.CornerCutoutStyle = SelectedCornerStyle();
        settings.TopLeftEnabled = TopLeftBox.IsChecked == true;
        settings.TopRightEnabled = TopRightBox.IsChecked == true;
        settings.BottomLeftEnabled = BottomLeftBox.IsChecked == true;
        settings.BottomRightEnabled = BottomRightBox.IsChecked == true;
        settings.SuperGamingMode = GamingBox.IsChecked == true;
        settings.GamingSpeed = SliderDecimal(SpeedSlider);
        settings.GlowIntensity = SliderDecimal(GlowSlider);
        settings.BloomWidth = SliderDecimal(BloomSlider);
        settings.DisplaySelectionInitialized = true;
        settings.KnownDisplays = DisplayMonitor.GetAll().Select(screen => screen.DeviceName).ToList();
    }

    private CornerCutoutStyle SelectedCornerStyle()
    {
        if (SquircleStyleButton.IsChecked == true) return CornerCutoutStyle.Squircle;
        if (PolygonStyleButton.IsChecked == true) return CornerCutoutStyle.Polygon;
        return CornerCutoutStyle.Rounded;
    }

    private void SetColor(MediaColor color)
    {
        selectedColor = color;
        ColorPreview.Background = new SolidColorBrush(color);
        UpdateColorSwatches();
    }

    private void UpdateColorSwatches()
    {
        BlackSwatch.IsChecked = selectedColor == Colors.Black;
        WhiteSwatch.IsChecked = selectedColor == Colors.White;
        GraySwatch.IsChecked = selectedColor == Colors.Gray;
    }

    private static MediaColor ToMediaColor(System.Drawing.Color color) => MediaColor.FromArgb(color.A, color.R, color.G, color.B);
    private static System.Drawing.Color ToDrawingColor(MediaColor color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
}
