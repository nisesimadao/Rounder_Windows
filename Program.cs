using Rounder.Windows;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var launchedInBackground = args.Any(arg => string.Equals(arg, "--background", StringComparison.OrdinalIgnoreCase));
        using var singleInstance = new SingleInstanceManager();
        if (!singleInstance.IsFirstInstance)
        {
            if (!launchedInBackground)
            {
                singleInstance.SignalActivate();
            }

            return;
        }

        System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        ApplicationConfiguration.Initialize();
        EnsureWpfApplication();
        using var context = new RounderApplicationContext(launchedInBackground);
        singleInstance.StartListening(context.ActivateFromSecondaryInstance);
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
