using System.Globalization;
using System.Windows;
using System.Windows.Media;
using MediaBrush = System.Windows.Media.Brush;
using MediaFontFamily = System.Windows.Media.FontFamily;
using WpfPoint = System.Windows.Point;
using WpfSystemColors = System.Windows.SystemColors;

namespace Rounder.Windows;

/// <summary>
/// Draws a font glyph by centering its actual vector outline rather than its
/// text baseline/line box. Segoe MDL2 Assets contains glyphs with noticeably
/// different bearings, so a normal TextBlock makes sidebar icons look shifted.
/// </summary>
public sealed class CenteredGlyph : FrameworkElement
{
    public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
        nameof(Glyph), typeof(string), typeof(CenteredGlyph),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty GlyphSizeProperty = DependencyProperty.Register(
        nameof(GlyphSize), typeof(double), typeof(CenteredGlyph),
        new FrameworkPropertyMetadata(16d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
        nameof(Foreground), typeof(MediaBrush), typeof(CenteredGlyph),
        new FrameworkPropertyMetadata(WpfSystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.AffectsRender));

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public double GlyphSize
    {
        get => (double)GetValue(GlyphSizeProperty);
        set => SetValue(GlyphSizeProperty, value);
    }

    public MediaBrush Foreground
    {
        get => (MediaBrush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (string.IsNullOrEmpty(Glyph) || ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }

        var typeface = new Typeface(new MediaFontFamily("Segoe MDL2 Assets"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        var formatted = new FormattedText(
            Glyph,
            CultureInfo.CurrentUICulture,
            System.Windows.FlowDirection.LeftToRight,
            typeface,
            GlyphSize,
            Foreground,
            dpi);

        var geometry = formatted.BuildGeometry(new WpfPoint(0, 0));
        var bounds = geometry.Bounds;
        if (bounds.IsEmpty)
        {
            return;
        }

        var x = ((ActualWidth - bounds.Width) / 2d) - bounds.X;
        var y = ((ActualHeight - bounds.Height) / 2d) - bounds.Y;

        drawingContext.PushTransform(new TranslateTransform(x, y));
        drawingContext.DrawGeometry(Foreground, null, geometry);
        drawingContext.Pop();
    }
}
