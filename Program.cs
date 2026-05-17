using Rounder.Windows;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        ApplicationConfiguration.Initialize();
        using var context = new RounderApplicationContext();
        System.Windows.Forms.Application.Run(context);
    }
}
