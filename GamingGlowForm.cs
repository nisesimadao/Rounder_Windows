using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Rounder.Windows;

public enum ScreenEdge
{
    Top,
    Bottom,
    Left,
    Right
}

/// <summary>
/// One rainbow bloom band per screen edge, matching the macOS GamingGlowWindow:
/// a hue gradient running along the edge (quarter of the hue wheel per edge,
/// traveling clockwise), masked by a sharp inward falloff whose outermost sliver
/// is fully opaque. Colors are derived from the shared GamingVisuals clock so the
/// bands stay in phase with the corner cutouts.
/// </summary>
public sealed class GamingGlowForm : LayeredWindow
{
    // Inward alpha falloff from the macOS mask gradient: (px, alpha) stops over a
    // 24 px reference band; stops at <= 1 px stay fully opaque regardless of intensity.
    private static readonly (double Px, double Alpha)[] FalloffProfile =
    [
        (0.0, 1.0),
        (1.0, 1.0),
        (2.0, 0.5),
        (3.0, 0.3),
        (4.0, 0.25),
        (6.0, 0.16),
        (9.0, 0.09),
        (14.0, 0.04),
        (24.0, 0.0)
    ];

    private const double ProfileReferencePx = 24.0;

    private readonly ScreenEdge edge;
    private readonly AppSettings settings;
    private readonly double baseHue;
    private readonly System.Windows.Forms.Timer animationTimer;
    private readonly System.Windows.Forms.Timer zOrderTimer;
    private int[] alphaLut = [];
    private uint[] colorLut = [];
    private int[] rowBuffer = [];

    public GamingGlowForm(Rectangle screenBounds, ScreenEdge edge, double scale, AppSettings settings)
    {
        this.edge = edge;
        this.settings = settings;
        baseHue = edge switch
        {
            ScreenEdge.Top => 0.0,
            ScreenEdge.Right => 0.25,
            ScreenEdge.Bottom => 0.5,
            _ => 0.75
        };

        SetLayerBounds(CalculateBounds(screenBounds, scale));

        animationTimer = new System.Windows.Forms.Timer { Interval = 16 };
        animationTimer.Tick += (_, _) => RenderLayer();
        zOrderTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        zOrderTimer.Tick += (_, _) => KeepAboveTaskbar();

        animationTimer.Start();
        Show();
        RenderLayer();
        KeepAboveTaskbar();
        zOrderTimer.Start();
    }

    protected override void DrawLayer(Bitmap surface)
    {
        var width = surface.Width;
        var height = surface.Height;
        var horizontal = edge is ScreenEdge.Top or ScreenEdge.Bottom;
        var length = horizontal ? width : height;
        var reach = horizontal ? height : width;

        if (alphaLut.Length != reach)
        {
            alphaLut = BuildAlphaLut(reach, (double)settings.GlowIntensity);
        }

        if (colorLut.Length != length)
        {
            colorLut = new uint[length];
        }

        if (rowBuffer.Length != width)
        {
            rowBuffer = new int[width];
        }

        // Hue travels clockwise around the screen perimeter: each edge spans a
        // quarter of the hue wheel, phase-shifted by the shared clock.
        var phase = GamingVisuals.Phase(settings.GamingSpeed);
        for (var i = 0; i < length; i++)
        {
            var position = length <= 1 ? 0.0 : i / (double)(length - 1);
            var along = edge is ScreenEdge.Bottom or ScreenEdge.Left ? 1.0 - position : position;
            colorLut[i] = GamingVisuals.RgbFromHue(baseHue + phase + along * 0.25);
        }

        var data = surface.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
        try
        {
            for (var y = 0; y < height; y++)
            {
                if (horizontal)
                {
                    var alpha = alphaLut[edge == ScreenEdge.Top ? y : height - 1 - y];
                    for (var x = 0; x < width; x++)
                    {
                        rowBuffer[x] = Premultiply(colorLut[x], alpha);
                    }
                }
                else
                {
                    var color = colorLut[y];
                    for (var x = 0; x < width; x++)
                    {
                        rowBuffer[x] = Premultiply(color, alphaLut[edge == ScreenEdge.Left ? x : width - 1 - x]);
                    }
                }

                Marshal.Copy(rowBuffer, 0, data.Scan0 + y * data.Stride, width);
            }
        }
        finally
        {
            surface.UnlockBits(data);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            animationTimer.Stop();
            animationTimer.Dispose();
            zOrderTimer.Stop();
            zOrderTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private Rectangle CalculateBounds(Rectangle screen, double scale)
    {
        // macOS: band thickness = max(5, 24 * bloomWidth) points -> physical px via scale.
        var points = Math.Max(5.0, ProfileReferencePx * Math.Clamp((double)settings.BloomWidth, 0.1, 3.0));
        var reach = Math.Max(1, (int)Math.Round(points * Math.Max(0.5, scale)));
        return edge switch
        {
            ScreenEdge.Top => new Rectangle(screen.Left, screen.Top, screen.Width, reach),
            ScreenEdge.Bottom => new Rectangle(screen.Left, screen.Bottom - reach, screen.Width, reach),
            ScreenEdge.Left => new Rectangle(screen.Left, screen.Top, reach, screen.Height),
            _ => new Rectangle(screen.Right - reach, screen.Top, reach, screen.Height)
        };
    }

    private static int[] BuildAlphaLut(int reach, double intensity)
    {
        // Intensity scales the inner bloom only; the outermost <= 1 px stays opaque,
        // and the profile stretches with the band (locations are px/24 of the band).
        var opacity = Math.Min(1.0, Math.Clamp(intensity, 0.1, 3.0));
        var stops = new (double Location, double Alpha)[FalloffProfile.Length];
        for (var i = 0; i < FalloffProfile.Length; i++)
        {
            var (px, alpha) = FalloffProfile[i];
            stops[i] = (px / ProfileReferencePx, px <= 1.0 ? 1.0 : alpha * opacity);
        }

        var lut = new int[reach];
        for (var depth = 0; depth < reach; depth++)
        {
            var u = (depth + 0.5) / reach;
            var value = stops[^1].Alpha;
            if (u <= stops[0].Location)
            {
                value = stops[0].Alpha;
            }
            else
            {
                for (var i = 1; i < stops.Length; i++)
                {
                    if (u <= stops[i].Location)
                    {
                        var t = (u - stops[i - 1].Location) / (stops[i].Location - stops[i - 1].Location);
                        value = stops[i - 1].Alpha + (stops[i].Alpha - stops[i - 1].Alpha) * t;
                        break;
                    }
                }
            }

            lut[depth] = Math.Clamp((int)Math.Round(value * 255.0), 0, 255);
        }

        return lut;
    }

    private static int Premultiply(uint rgb, int alpha)
    {
        if (alpha <= 0)
        {
            return 0;
        }

        if (alpha >= 255)
        {
            return unchecked((int)(0xFF000000u | rgb));
        }

        var r = (int)((rgb >> 16) & 0xFF) * alpha / 255;
        var g = (int)((rgb >> 8) & 0xFF) * alpha / 255;
        var b = (int)(rgb & 0xFF) * alpha / 255;
        return alpha << 24 | r << 16 | g << 8 | b;
    }
}
