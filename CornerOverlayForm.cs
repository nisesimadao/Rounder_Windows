using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Rounder.Windows;

public enum CornerKind
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public sealed class CornerOverlayForm : Form
{
    private const int WsExTransparent = 0x00000020;
    private const int WsExToolWindow = 0x00000080;
    private const int WsExNoActivate = 0x08000000;
    private const int HwndTopmost = -1;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpShowWindow = 0x0040;
    private const uint SwpNoOwnerZOrder = 0x0200;
    private const uint SwpNoSendChanging = 0x0400;

    private static readonly Color TransparentKeyColor = Color.FromArgb(255, 1, 2, 3);
    private readonly CornerKind corner;
    private readonly int radius;
    private readonly int padding;
    private readonly AppSettings settings;
    private readonly System.Windows.Forms.Timer animationTimer;
    private readonly System.Windows.Forms.Timer zOrderTimer;
    private double hue;

    public CornerOverlayForm(CornerKind corner, Rectangle screenBounds, int radius, AppSettings settings)
    {
        this.corner = corner;
        this.radius = radius;
        this.settings = settings;
        padding = settings.SuperGamingMode ? 80 : 1;

        AutoScaleMode = AutoScaleMode.None;
        BackColor = TransparentKeyColor;
        TransparencyKey = TransparentKeyColor;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        Bounds = CalculateBounds(screenBounds);
        DoubleBuffered = true;

        animationTimer = new System.Windows.Forms.Timer { Interval = 16 };
        animationTimer.Tick += (_, _) =>
        {
            hue = (hue + 0.004 * (double)Math.Max(0.1m, settings.GamingSpeed)) % 1.0;
            Invalidate();
        };
        zOrderTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        zOrderTimer.Tick += (_, _) => KeepAboveTaskbar();

        if (settings.SuperGamingMode)
        {
            animationTimer.Start();
        }

        Show();
        KeepAboveTaskbar();
        zOrderTimer.Start();
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= WsExTransparent | WsExToolWindow | WsExNoActivate;
            return cp;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.Clear(TransparentKeyColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var content = new Rectangle(padding, padding, radius, radius);
        var overlayColor = settings.SuperGamingMode ? ColorFromHsv(hue * 360.0, 0.9, 0.95) : settings.CornerColor;

        if (settings.SuperGamingMode)
        {
            DrawGlow(e.Graphics, content, overlayColor);
        }

        using var fill = new SolidBrush(overlayColor);
        e.Graphics.FillRectangle(fill, content);

        using var transparentBrush = new SolidBrush(TransparentKeyColor);
        e.Graphics.FillEllipse(transparentBrush, CircleBounds(content));
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

    private void KeepAboveTaskbar()
    {
        if (!IsHandleCreated || IsDisposed)
        {
            return;
        }

        SetWindowPos(
            Handle,
            HwndTopmost,
            0,
            0,
            0,
            0,
            SwpNoMove | SwpNoSize | SwpNoActivate | SwpShowWindow | SwpNoOwnerZOrder | SwpNoSendChanging);
    }

    private Rectangle CalculateBounds(Rectangle screen)
    {
        var size = radius + padding * 2;
        var x = corner switch
        {
            CornerKind.TopLeft or CornerKind.BottomLeft => screen.Left - padding,
            _ => screen.Right - radius - padding
        };
        var y = corner switch
        {
            CornerKind.TopLeft or CornerKind.TopRight => screen.Top - padding,
            _ => screen.Bottom - radius - padding
        };

        return new Rectangle(x, y, size, size);
    }

    private Rectangle CircleBounds(Rectangle content)
    {
        var center = corner switch
        {
            CornerKind.TopLeft => new Point(content.Right, content.Bottom),
            CornerKind.TopRight => new Point(content.Left, content.Bottom),
            CornerKind.BottomLeft => new Point(content.Right, content.Top),
            _ => new Point(content.Left, content.Top)
        };

        return new Rectangle(center.X - radius, center.Y - radius, radius * 2, radius * 2);
    }

    private void DrawGlow(Graphics graphics, Rectangle content, Color color)
    {
        var glow = (float)Math.Clamp(settings.GlowIntensity, 0.1m, 3.0m);
        var center = corner switch
        {
            CornerKind.TopLeft => new Point(content.Left, content.Top),
            CornerKind.TopRight => new Point(content.Right, content.Top),
            CornerKind.BottomLeft => new Point(content.Left, content.Bottom),
            _ => new Point(content.Right, content.Bottom)
        };

        for (var i = 5; i >= 1; i--)
        {
            var alpha = Math.Clamp((int)(20 * glow / i), 5, 90);
            var diameter = (int)(radius * (1.3 + i * 0.55));
            using var brush = new SolidBrush(Color.FromArgb(alpha, color));
            graphics.FillEllipse(brush, center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);
        }
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
            4 => Color.FromArgb(255, t, p, v),
            _ => Color.FromArgb(255, v, p, q)
        };
    }

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        int hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);
}
