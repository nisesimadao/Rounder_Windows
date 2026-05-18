using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Resources;

namespace Rounder.Windows;

public static class AppAssets
{
    public static string AssetDirectory { get; } = Path.Combine(AppContext.BaseDirectory, "Assets");

    public static Icon AppIcon()
    {
        var path = Path.Combine(AssetDirectory, "rounder.ico");
        if (File.Exists(path))
        {
            return new Icon(path);
        }

        using var stream = OpenResource("rounder.ico");
        return stream is not null ? new Icon(stream) : SystemIcons.Application;
    }

    public static Image? HeaderImage()
    {
        var path = Path.Combine(AssetDirectory, "rounder.png");
        if (File.Exists(path))
        {
            return Image.FromFile(path);
        }

        using var stream = OpenResource("rounder.png");
        return stream is not null ? new Bitmap(stream) : null;
    }

    public static ImageList TabImages()
    {
        var images = new ImageList
        {
            ColorDepth = ColorDepth.Depth32Bit,
            ImageSize = new Size(18, 18)
        };
        images.Images.Add("settings", DrawIconGlyph(DrawSliders));
        images.Images.Add("presets", DrawIconGlyph(DrawBookmark));
        images.Images.Add("permissions", DrawIconGlyph(DrawShield));
        images.Images.Add("credits", DrawIconGlyph(DrawInfo));
        return images;
    }

    private static Bitmap DrawIconGlyph(Action<Graphics, Pen, Brush> draw)
    {
        var bitmap = new Bitmap(18, 18);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(Color.FromArgb(50, 58, 69), 1.8F) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        using var brush = new SolidBrush(Color.FromArgb(50, 58, 69));
        draw(graphics, pen, brush);
        return bitmap;
    }

    private static Stream? OpenResource(string fileName)
    {
        var resource = System.Windows.Application.GetResourceStream(new Uri($"/Assets/{fileName}", UriKind.Relative));
        return resource?.Stream;
    }

    private static void DrawSliders(Graphics graphics, Pen pen, Brush brush)
    {
        graphics.DrawLine(pen, 3, 5, 15, 5);
        graphics.FillEllipse(brush, 6, 2, 5, 5);
        graphics.DrawLine(pen, 3, 10, 15, 10);
        graphics.FillEllipse(brush, 11, 7, 5, 5);
        graphics.DrawLine(pen, 3, 15, 15, 15);
        graphics.FillEllipse(brush, 4, 12, 5, 5);
    }

    private static void DrawBookmark(Graphics graphics, Pen pen, Brush brush)
    {
        using var path = new GraphicsPath();
        path.AddLines([
            new PointF(5, 3),
            new PointF(13, 3),
            new PointF(13, 15),
            new PointF(9, 12),
            new PointF(5, 15),
            new PointF(5, 3)
        ]);
        graphics.DrawPath(pen, path);
    }

    private static void DrawShield(Graphics graphics, Pen pen, Brush brush)
    {
        using var path = new GraphicsPath();
        path.AddLines([
            new PointF(9, 2.5F),
            new PointF(14, 4.5F),
            new PointF(13, 11),
            new PointF(9, 15.5F),
            new PointF(5, 11),
            new PointF(4, 4.5F),
            new PointF(9, 2.5F)
        ]);
        graphics.DrawPath(pen, path);
    }

    private static void DrawInfo(Graphics graphics, Pen pen, Brush brush)
    {
        graphics.DrawEllipse(pen, 3, 3, 12, 12);
        graphics.FillEllipse(brush, 8, 5, 2, 2);
        graphics.FillRectangle(brush, 8, 8, 2, 6);
    }
}
