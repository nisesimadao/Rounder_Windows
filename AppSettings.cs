using System.Drawing;

namespace Rounder.Windows;

public sealed class AppSettings
{
    public bool HasLaunchedBefore { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int CornerRadius { get; set; } = 20;
    public int CornerColorArgb { get; set; } = Color.Black.ToArgb();
    public bool TopLeftEnabled { get; set; } = true;
    public bool TopRightEnabled { get; set; } = true;
    public bool BottomLeftEnabled { get; set; } = true;
    public bool BottomRightEnabled { get; set; } = true;
    public bool SuperGamingMode { get; set; }
    public decimal GamingSpeed { get; set; } = 1.0m;
    public decimal GlowIntensity { get; set; } = 1.0m;
    public List<string> SelectedDisplays { get; set; } = [];

    public Color CornerColor
    {
        get => Color.FromArgb(CornerColorArgb);
        set => CornerColorArgb = value.ToArgb();
    }

    public AppSettings Clone()
    {
        return new AppSettings
        {
            HasLaunchedBefore = HasLaunchedBefore,
            IsEnabled = IsEnabled,
            CornerRadius = CornerRadius,
            CornerColorArgb = CornerColorArgb,
            TopLeftEnabled = TopLeftEnabled,
            TopRightEnabled = TopRightEnabled,
            BottomLeftEnabled = BottomLeftEnabled,
            BottomRightEnabled = BottomRightEnabled,
            SuperGamingMode = SuperGamingMode,
            GamingSpeed = GamingSpeed,
            GlowIntensity = GlowIntensity,
            SelectedDisplays = [.. SelectedDisplays]
        };
    }
}
