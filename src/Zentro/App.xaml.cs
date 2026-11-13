using System.Threading;
using System.Windows;
using Zentro.Views;

namespace Zentro;

public partial class App : Application
{
    private Mutex? instanceLock;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        instanceLock = new Mutex(true, @"Local\Zentro.Desktop", out bool firstInstance);
        if (!firstInstance)
        {
            MessageBox.Show("Zentro is already open. Switch to its window to continue.", "Zentro");
            Shutdown();
            return;
        }
        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        instanceLock?.Dispose();
        base.OnExit(e);
    }
}
