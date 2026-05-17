using System.Drawing;

namespace Rounder.Windows;

public sealed class CornerPreset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Preset";
    public bool TopLeftEnabled { get; set; } = true;
    public bool TopRightEnabled { get; set; } = true;
    public bool BottomLeftEnabled { get; set; } = true;
    public bool BottomRightEnabled { get; set; } = true;
    public int CornerRadius { get; set; } = 20;
    public int CornerColorArgb { get; set; } = Color.Black.ToArgb();
    public bool SuperGamingMode { get; set; }
    public decimal GamingSpeed { get; set; } = 1.0m;
    public decimal GlowIntensity { get; set; } = 1.0m;

    public Color CornerColor
    {
        get => Color.FromArgb(CornerColorArgb);
        set => CornerColorArgb = value.ToArgb();
    }

    public static CornerPreset FromSettings(string name, AppSettings settings)
    {
        return new CornerPreset
        {
            Name = name,
            TopLeftEnabled = settings.TopLeftEnabled,
            TopRightEnabled = settings.TopRightEnabled,
            BottomLeftEnabled = settings.BottomLeftEnabled,
            BottomRightEnabled = settings.BottomRightEnabled,
            CornerRadius = settings.CornerRadius,
            CornerColorArgb = settings.CornerColorArgb,
            SuperGamingMode = settings.SuperGamingMode,
            GamingSpeed = settings.GamingSpeed,
            GlowIntensity = settings.GlowIntensity
        };
    }

    public void ApplyTo(AppSettings settings)
    {
        settings.TopLeftEnabled = TopLeftEnabled;
        settings.TopRightEnabled = TopRightEnabled;
        settings.BottomLeftEnabled = BottomLeftEnabled;
        settings.BottomRightEnabled = BottomRightEnabled;
        settings.CornerRadius = CornerRadius;
        settings.CornerColorArgb = CornerColorArgb;
        settings.SuperGamingMode = SuperGamingMode;
        settings.GamingSpeed = GamingSpeed;
        settings.GlowIntensity = GlowIntensity;
    }

    public override string ToString()
    {
        var mode = SuperGamingMode ? ", gaming" : "";
        return $"{Name} ({CornerRadius}px{mode})";
    }
}
