using System.Drawing.Drawing2D;

namespace Rounder.Windows;

public enum CornerKind
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public sealed class CornerOverlayForm : LayeredWindow
{
    private readonly CornerKind corner;
    private readonly int radius;
    private readonly int visualSize;
    private readonly double baseHue;
    private readonly AppSettings settings;
    private readonly System.Windows.Forms.Timer animationTimer;
    private readonly System.Windows.Forms.Timer zOrderTimer;

    public CornerOverlayForm(CornerKind corner, Rectangle screenBounds, int radius, double baseHue, AppSettings settings)
    {
        this.corner = corner;
        this.radius = radius;
        this.baseHue = baseHue;
        this.settings = settings;
        visualSize = CalculateVisualSize(radius, settings.CornerCutoutStyle);

        SetLayerBounds(CalculateBounds(screenBounds));

        animationTimer = new System.Windows.Forms.Timer { Interval = 16 };
        animationTimer.Tick += (_, _) => RenderLayer();
        zOrderTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        zOrderTimer.Tick += (_, _) => KeepAboveTaskbar();

        if (settings.SuperGamingMode)
        {
            animationTimer.Start();
        }

        Show();
        RenderLayer();
        KeepAboveTaskbar();
        zOrderTimer.Start();
    }

    protected override void DrawLayer(Bitmap surface)
    {
        using var graphics = Graphics.FromImage(surface);
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.Half;

        var content = new Rectangle(0, 0, surface.Width, surface.Height);
        var overlayColor = settings.SuperGamingMode
            ? GamingVisuals.ColorFromHue(baseHue + GamingVisuals.Phase(settings.GamingSpeed))
            : settings.CornerColor;

        using var fill = new SolidBrush(overlayColor);
        graphics.CompositingMode = CompositingMode.SourceOver;
        switch (settings.CornerCutoutStyle)
        {
            case CornerCutoutStyle.Polygon:
                using (var polygon = PolygonMaskPath(content))
                {
                    graphics.FillPath(fill, polygon);
                }
                break;
            case CornerCutoutStyle.Squircle:
                graphics.FillRectangle(fill, content);
                using (var squircle = SquircleCutoutPath(content))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    using var clear = new SolidBrush(Color.Transparent);
                    graphics.FillPath(clear, squircle);
                }
                break;
            default:
                graphics.FillRectangle(fill, content);
                using (var rounded = RoundedCutoutPath(content))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    using var clear = new SolidBrush(Color.Transparent);
                    graphics.FillPath(clear, rounded);
                }
                break;
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
        var size = Math.Max(1, visualSize);
        var x = corner switch
        {
            CornerKind.TopLeft or CornerKind.BottomLeft => screen.Left,
            _ => screen.Right - size
        };
        var y = corner switch
        {
            CornerKind.TopLeft or CornerKind.TopRight => screen.Top,
            _ => screen.Bottom - size
        };

        return new Rectangle(x, y, size, size);
    }

    private static int CalculateVisualSize(int radius, CornerCutoutStyle style)
    {
        var factor = style == CornerCutoutStyle.Squircle ? 1.8 : 1.0;
        return Math.Max(1, (int)Math.Round(radius * factor));
    }

    private GraphicsPath RoundedCutoutPath(Rectangle content)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;
        var bounds = corner switch
        {
            CornerKind.TopLeft => new Rectangle(content.Right - radius, content.Bottom - radius, diameter, diameter),
            CornerKind.TopRight => new Rectangle(content.Left - radius, content.Bottom - radius, diameter, diameter),
            CornerKind.BottomLeft => new Rectangle(content.Right - radius, content.Top - radius, diameter, diameter),
            _ => new Rectangle(content.Left - radius, content.Top - radius, diameter, diameter)
        };

        path.AddEllipse(bounds);
        return path;
    }

    private GraphicsPath SquircleCutoutPath(Rectangle content)
    {
        var path = new GraphicsPath();
        var points = new List<PointF>();
        const double n = 4.0;
        const int steps = 72;

        var center = corner switch
        {
            CornerKind.TopLeft => new PointF(content.Right, content.Bottom),
            CornerKind.TopRight => new PointF(content.Left, content.Bottom),
            CornerKind.BottomLeft => new PointF(content.Right, content.Top),
            _ => new PointF(content.Left, content.Top)
        };
        var sx = corner is CornerKind.TopLeft or CornerKind.BottomLeft ? -1f : 1f;
        var sy = corner is CornerKind.TopLeft or CornerKind.TopRight ? -1f : 1f;
        points.Add(center);

        for (var i = 0; i <= steps; i++)
        {
            var theta = i / (double)steps * Math.PI / 2.0;
            var x = content.Width * Math.Pow(Math.Cos(theta), 2.0 / n);
            var y = content.Height * Math.Pow(Math.Sin(theta), 2.0 / n);
            points.Add(new PointF(center.X + sx * (float)x, center.Y + sy * (float)y));
        }

        path.AddPolygon(points.ToArray());
        return path;
    }

    private GraphicsPath PolygonMaskPath(Rectangle content)
    {
        var path = new GraphicsPath();
        var inset = Math.Min(radius, Math.Min(content.Width, content.Height));
        if (inset <= 0)
        {
            return path;
        }

        var points = corner switch
        {
            CornerKind.TopLeft => new[]
            {
                new Point(content.Left, content.Top),
                new Point(content.Left + inset, content.Top),
                new Point(content.Left, content.Top + inset)
            },
            CornerKind.TopRight => new[]
            {
                new Point(content.Right, content.Top),
                new Point(content.Right - inset, content.Top),
                new Point(content.Right, content.Top + inset)
            },
            CornerKind.BottomLeft => new[]
            {
                new Point(content.Left, content.Bottom),
                new Point(content.Left + inset, content.Bottom),
                new Point(content.Left, content.Bottom - inset)
            },
            _ => new[]
            {
                new Point(content.Right, content.Bottom),
                new Point(content.Right - inset, content.Bottom),
                new Point(content.Right, content.Bottom - inset)
            }
        };
        path.AddPolygon(points);
        return path;
    }
}
