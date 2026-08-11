using System.Runtime.InteropServices;

namespace Rounder.Windows;

public sealed record DisplayMonitor(string DeviceName, Rectangle Bounds, bool IsPrimary, double Scale)
{
    public static IReadOnlyList<DisplayMonitor> GetAll()
    {
        var monitors = new List<DisplayMonitor>();
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (monitor, _, _, _) =>
        {
            var info = new MonitorInfoEx();
            info.Size = Marshal.SizeOf<MonitorInfoEx>();
            if (GetMonitorInfo(monitor, ref info))
            {
                monitors.Add(new DisplayMonitor(
                    info.DeviceName,
                    Rectangle.FromLTRB(info.Monitor.Left, info.Monitor.Top, info.Monitor.Right, info.Monitor.Bottom),
                    (info.Flags & MonitorInfoPrimary) != 0,
                    GetScaleFactor(monitor)));
            }

            return true;
        }, IntPtr.Zero);

        if (monitors.Count > 0)
        {
            return monitors;
        }

        return Screen.AllScreens
            .Select(screen => new DisplayMonitor(screen.DeviceName, screen.Bounds, screen.Primary, 1.0))
            .ToList();
    }

    private static double GetScaleFactor(IntPtr monitor)
    {
        try
        {
            if (GetDpiForMonitor(monitor, MdtEffectiveDpi, out var dpiX, out _) == 0 && dpiX > 0)
            {
                return dpiX / 96.0;
            }
        }
        catch (DllNotFoundException)
        {
        }
        catch (EntryPointNotFoundException)
        {
        }

        return 1.0;
    }

    private const int MonitorInfoPrimary = 1;
    private const int DeviceNameLength = 32;
    private const int MdtEffectiveDpi = 0;

    private delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr clipRect, IntPtr data);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MonitorInfoEx
    {
        public int Size;
        public Rect Monitor;
        public Rect WorkArea;
        public int Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DeviceNameLength)]
        public string DeviceName;
    }

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr clip, MonitorEnumProc callback, IntPtr data);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfoEx info);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr monitor, int dpiType, out uint dpiX, out uint dpiY);
}
