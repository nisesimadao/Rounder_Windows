using System.Windows;
using System.Windows.Media;

namespace Rounder.Windows;

public partial class PresetEditorWindow : Window
{
    private readonly CornerPreset preset;
    private System.Drawing.Color selectedColor;

    public PresetEditorWindow(CornerPreset preset)
    {
        this.preset = preset;
        selectedColor = preset.CornerColor;
        InitializeComponent();
        LoadPreset();
        RadiusSlider.ValueChanged += (_, _) => RadiusValue.Text = $"{(int)Math.Round(RadiusSlider.Value)} px";
        SpeedSlider.ValueChanged += (_, _) => UpdateGamingLabels();
        GlowSlider.ValueChanged += (_, _) => UpdateGamingLabels();
        BloomSlider.ValueChanged += (_, _) => UpdateGamingLabels();
        UpdateGamingLabels();
    }

    private void LoadPreset()
    {
        NameBox.Text = preset.Name;
        RadiusSlider.Value = Math.Clamp(preset.CornerRadius, 0, 40);
        RadiusValue.Text = $"{preset.CornerRadius} px";
        ShapeBox.SelectedIndex = preset.CornerCutoutStyle switch
        {
            CornerCutoutStyle.Squircle => 1,
            CornerCutoutStyle.Polygon => 2,
            _ => 0
        };
        UpdateColorPreview();
        TopLeftBox.IsChecked = preset.TopLeftEnabled;
        TopRightBox.IsChecked = preset.TopRightEnabled;
        BottomLeftBox.IsChecked = preset.BottomLeftEnabled;
        BottomRightBox.IsChecked = preset.BottomRightEnabled;
        GamingBox.IsChecked = preset.SuperGamingMode;
        SpeedSlider.Value = (double)Math.Clamp(preset.GamingSpeed, 0.1m, 5.0m);
        GlowSlider.Value = (double)Math.Clamp(preset.GlowIntensity, 0.1m, 3.0m);
        BloomSlider.Value = (double)Math.Clamp(preset.BloomWidth, 0.1m, 3.0m);
    }

    private void UpdateGamingLabels()
    {
        if (SpeedValue is null) return;
        SpeedValue.Text = $"{SpeedSlider.Value:0.0}x";
        GlowValue.Text = $"{GlowSlider.Value:0.0}";
        BloomValue.Text = $"{BloomSlider.Value:0.0}";
    }

    private void ChooseColor_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new ColorDialog { Color = selectedColor, FullOpen = true };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            selectedColor = dialog.Color;
            UpdateColorPreview();
        }
    }

    private void UpdateColorPreview()
    {
        ColorPreview.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B));
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            System.Windows.MessageBox.Show(this, "Preset name is required.", "Rounder", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        preset.Name = name;
        preset.CornerRadius = (int)Math.Round(RadiusSlider.Value);
        preset.CornerCutoutStyle = ShapeBox.SelectedIndex switch
        {
            1 => CornerCutoutStyle.Squircle,
            2 => CornerCutoutStyle.Polygon,
            _ => CornerCutoutStyle.Rounded
        };
        preset.CornerColor = selectedColor;
        preset.TopLeftEnabled = TopLeftBox.IsChecked == true;
        preset.TopRightEnabled = TopRightBox.IsChecked == true;
        preset.BottomLeftEnabled = BottomLeftBox.IsChecked == true;
        preset.BottomRightEnabled = BottomRightBox.IsChecked == true;
        preset.SuperGamingMode = GamingBox.IsChecked == true;
        preset.GamingSpeed = Math.Round((decimal)SpeedSlider.Value, 1);
        preset.GlowIntensity = Math.Round((decimal)GlowSlider.Value, 1);
        preset.BloomWidth = Math.Round((decimal)BloomSlider.Value, 1);
        DialogResult = true;
    }
}
