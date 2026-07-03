using System.Drawing.Drawing2D;

namespace Rounder.Windows;

public enum ScreenEdge
{
    Top,
    Bottom,
    Left,
    Right
}

public sealed class GamingGlowForm : LayeredWindow
{
    private readonly ScreenEdge edge;
    private readonly AppSettings settings;
    private readonly System.Windows.Forms.Timer animationTimer;
    private readonly System.Windows.Forms.Timer zOrderTimer;
    private double hue;

    public GamingGlowForm(Rectangle screenBounds, ScreenEdge edge, AppSettings settings)
    {
        this.edge = edge;
        this.settings = settings;

        Bounds = CalculateBounds(screenBounds);

        animationTimer = new System.Windows.Forms.Timer { Interval = 16 };
        animationTimer.Tick += (_, _) =>
        {
            hue = (hue + 0.004 * (double)Math.Max(0.1m, settings.GamingSpeed)) % 1.0;
            RenderLayer();
        };
        zOrderTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        zOrderTimer.Tick += (_, _) => KeepAboveTaskbar();

        animationTimer.Start();
        Show();
        RenderLayer();
        KeepAboveTaskbar();
        zOrderTimer.Start();
    }

    protected override void DrawLayer(Graphics graphics, Rectangle bounds)
    {
        var intensity = Math.Clamp((double)settings.GlowIntensity, 0.1, 3.0);
        const int colorCount = 12;
        var reach = edge is ScreenEdge.Top or ScreenEdge.Bottom ? bounds.Height : bounds.Width;
        for (var depth = 0; depth < reach; depth++)
        {
            var alpha = EdgeAlpha(depth, reach, intensity);
            if (alpha <= 0)
            {
                continue;
            }

            for (var i = 0; i < colorCount; i++)
            {
                var t0 = i / (double)colorCount;
                var t1 = (i + 1) / (double)colorCount;
                using var brush = new LinearGradientBrush(Segment(t0, t1, depth), WithAlpha(ColorAt(t0), alpha), WithAlpha(ColorAt(t1), alpha), GradientMode());
                graphics.FillRectangle(brush, Segment(t0, t1, depth));
            }
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

    private Rectangle CalculateBounds(Rectangle screen)
    {
        var reach = Math.Max(5, (int)Math.Round(24 * Math.Clamp((double)settings.BloomWidth, 0.1, 3.0)));
        return edge switch
        {
            ScreenEdge.Top => new Rectangle(screen.Left, screen.Top, screen.Width, reach),
            ScreenEdge.Bottom => new Rectangle(screen.Left, screen.Bottom - reach, screen.Width, reach),
            ScreenEdge.Left => new Rectangle(screen.Left, screen.Top, reach, screen.Height),
            _ => new Rectangle(screen.Right - reach, screen.Top, reach, screen.Height)
        };
    }

    private Rectangle Segment(double t0, double t1, int depth)
    {
        if (edge is ScreenEdge.Top or ScreenEdge.Bottom)
        {
            var y = edge == ScreenEdge.Top ? depth : Height - depth - 1;
            return new Rectangle((int)Math.Round(Width * t0), y, Math.Max(1, (int)Math.Round(Width * (t1 - t0)) + 1), 1);
        }

        var x = edge == ScreenEdge.Left ? depth : Width - depth - 1;
        return new Rectangle(x, (int)Math.Round(Height * t0), 1, Math.Max(1, (int)Math.Round(Height * (t1 - t0)) + 1));
    }

    private LinearGradientMode GradientMode()
    {
        return edge is ScreenEdge.Top or ScreenEdge.Bottom ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical;
    }

    private Color ColorAt(double segment)
    {
        var baseHue = edge switch
        {
            ScreenEdge.Top => 0.0,
            ScreenEdge.Right => 0.25,
            ScreenEdge.Bottom => 0.5,
            _ => 0.75
        };
        var local = edge is ScreenEdge.Bottom or ScreenEdge.Left ? 1.0 - segment : segment;
        return ColorFromHsv(((baseHue + hue + local * 0.25) % 1.0) * 360.0, 1.0, 1.0);
    }

    private static int EdgeAlpha(int depth, int reach, double intensity)
    {
        if (depth <= 1)
        {
            return 255;
        }

        var t = depth / Math.Max(1.0, reach - 1.0);
        var profile = Math.Pow(1.0 - t, 3.2);
        return Math.Clamp((int)Math.Round(210 * Math.Min(1.0, intensity) * profile), 0, 220);
    }

    private static Color WithAlpha(Color color, int alpha)
    {
        return Color.FromArgb(alpha, color.R, color.G, color.B);
    }

    private static Color ColorFromHsv(double hue, double saturation, double value)
    {
        var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        var f = hue / 60 - Math.Floor(hue / 60);
        value *= 255;
        var v = Convert.ToInt32(value);
        var p = Convert.ToInt32(value * (1 - saturation));
        var q = Convert.ToInt32(value * (1 - f * saturation));
        var t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        return hi switch
        {
            0 => Color.FromArgb(255, v, t, p),
            1 => Color.FromArgb(255, q, v, p),
            2 => Color.FromArgb(255, p, v, t),
            3 => Color.FromArgb(255, p, q, v),
            4 => Color.FromArgb(255, t, p, q),
            _ => Color.FromArgb(255, v, p, q)
        };
    }
}
