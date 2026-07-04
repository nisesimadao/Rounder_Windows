using System.Diagnostics;

namespace Rounder.Windows;

/// <summary>
/// Shared animation clock and rainbow palette for Super Duper Gaming Mode.
/// Mirrors the macOS GamingGlowClock: every overlay window derives its color
/// from the same anchor, so corner cutouts and edge bloom stay in phase.
/// </summary>
internal static class GamingVisuals
{
    /// <summary>Seconds per full rainbow lap at speed 1.0 (macOS baseColorAnimationDuration).</summary>
    public const double LapSeconds = 3.0;

    private static readonly Stopwatch Clock = Stopwatch.StartNew();

    public static double Phase(decimal speed)
    {
        var effectiveSpeed = Math.Max(0.1, (double)speed);
        return Clock.Elapsed.TotalSeconds * effectiveSpeed / LapSeconds % 1.0;
    }

    /// <summary>Fully saturated, fully bright hue as 0x00RRGGBB. Hue wraps at 1.0.</summary>
    public static uint RgbFromHue(double hue)
    {
        var h = (hue % 1.0 + 1.0) % 1.0 * 6.0;
        var x = (byte)Math.Round(255.0 * (1.0 - Math.Abs(h % 2.0 - 1.0)));
        return (int)h switch
        {
            0 => Pack(255, x, 0),
            1 => Pack(x, 255, 0),
            2 => Pack(0, 255, x),
            3 => Pack(0, x, 255),
            4 => Pack(x, 0, 255),
            _ => Pack(255, 0, x)
        };
    }

    public static Color ColorFromHue(double hue)
    {
        return Color.FromArgb(unchecked((int)(0xFF000000u | RgbFromHue(hue))));
    }

    private static uint Pack(byte r, byte g, byte b)
    {
        return (uint)(r << 16 | g << 8 | b);
    }
}
