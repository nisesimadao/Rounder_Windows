using Rounder.Windows;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        ApplicationConfiguration.Initialize();
        EnsureWpfApplication();
        using var context = new RounderApplicationContext();
        System.Windows.Forms.Application.Run(context);
    }

    private static void EnsureWpfApplication()
    {
        if (System.Windows.Application.Current is not null)
        {
            return;
        }

        _ = new System.Windows.Application
        {
            ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown,
            ThemeMode = System.Windows.ThemeMode.System
        };
    }
}
