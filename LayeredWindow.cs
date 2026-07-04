using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Rounder.Windows;

public abstract class LayeredWindow : Form
{
    private const int WsExLayered = 0x00080000;
    private const int WsExTransparent = 0x00000020;
    private const int WsExToolWindow = 0x00000080;
    private const int WsExNoActivate = 0x08000000;
    private const int HwndTopmost = -1;
    private const byte AcSrcOver = 0x00;
    private const byte AcSrcAlpha = 0x01;
    private const int UlwAlpha = 0x00000002;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpShowWindow = 0x0040;
    private const uint SwpNoOwnerZOrder = 0x0200;
    private const uint SwpNoSendChanging = 0x0400;
    private Rectangle layerBounds;

    protected LayeredWindow()
    {
        AutoScaleMode = AutoScaleMode.None;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
    }

    protected override bool ShowWithoutActivation => true;

    protected int LayerWidth => layerBounds.Width;

    protected int LayerHeight => layerBounds.Height;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= WsExLayered | WsExTransparent | WsExToolWindow | WsExNoActivate;
            return cp;
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyNativeBounds();
    }

    protected void SetLayerBounds(Rectangle bounds)
    {
        layerBounds = bounds;
        Bounds = bounds;
        ApplyNativeBounds();
    }

    protected void RenderLayer()
    {
        if (!IsHandleCreated || IsDisposed || layerBounds.Width <= 0 || layerBounds.Height <= 0)
        {
            return;
        }

        using var bitmap = new Bitmap(layerBounds.Width, layerBounds.Height, PixelFormat.Format32bppPArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.Transparent);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            DrawLayer(graphics, new Rectangle(0, 0, layerBounds.Width, layerBounds.Height));
        }

        var screenDc = GetDC(IntPtr.Zero);
        var memoryDc = CreateCompatibleDC(screenDc);
        var hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
        var oldBitmap = SelectObject(memoryDc, hBitmap);

        try
        {
            var size = new SizeRef(layerBounds.Width, layerBounds.Height);
            var source = new PointRef(0, 0);
            var destination = new PointRef(layerBounds.Left, layerBounds.Top);
            var blend = new BlendFunction
            {
                BlendOp = AcSrcOver,
                BlendFlags = 0,
                SourceConstantAlpha = 255,
                AlphaFormat = AcSrcAlpha
            };

            UpdateLayeredWindow(Handle, screenDc, ref destination, ref size, memoryDc, ref source, 0, ref blend, UlwAlpha);
        }
        finally
        {
            SelectObject(memoryDc, oldBitmap);
            DeleteObject(hBitmap);
            DeleteDC(memoryDc);
            ReleaseDC(IntPtr.Zero, screenDc);
        }
    }

    protected abstract void DrawLayer(Graphics graphics, Rectangle bounds);

    protected void KeepAboveTaskbar()
    {
        ApplyNativeBounds();
    }

    private void ApplyNativeBounds()
    {
        if (!IsHandleCreated || IsDisposed || layerBounds.Width <= 0 || layerBounds.Height <= 0)
        {
            return;
        }

        SetWindowPos(Handle, HwndTopmost, layerBounds.Left, layerBounds.Top, layerBounds.Width, layerBounds.Height, SwpNoActivate | SwpShowWindow | SwpNoOwnerZOrder | SwpNoSendChanging);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PointRef
    {
        public int X;
        public int Y;

        public PointRef(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SizeRef
    {
        public int Cx;
        public int Cy;

        public SizeRef(int cx, int cy)
        {
            Cx = cx;
            Cy = cy;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BlendFunction
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref PointRef pptDst, ref SizeRef psize, IntPtr hdcSrc, ref PointRef pptSrc, int crKey, ref BlendFunction pblend, int dwFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hDc);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hDc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hDc, IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
}
