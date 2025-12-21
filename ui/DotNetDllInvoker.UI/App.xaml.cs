using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace DotNetDllInvoker.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const string CrashLogFile = "crash_log.txt";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // UI Thread Exceptions
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        
        // Background Thread Exceptions
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        
        // Task Exceptions
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnUnobservedTaskException(object? sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
    {
        LogException(e.Exception, "Background Task");
        e.SetObserved(); // Prevent process termination if possible
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogException(e.Exception, "UI Thread");
        // Try to keep alive?
        e.Handled = true; 
        MessageBox.Show($"CRASH CAUGHT (UI Thread):\n{e.Exception.Message}\n\nApp will attempt to continue.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogException(ex, "Background Thread (Fatal)");
            MessageBox.Show($"FATAL CRASH (Background):\n{ex.Message}\n\nApp will terminate.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LogException(Exception ex, string source)
    {
        try
        {
            string message = $"[{DateTime.Now}] CRASH ({source}): {ex.Message}\n{ex.StackTrace}\n--------------------------------------------------\n";
            File.AppendAllText(CrashLogFile, message);
        }
        catch (Exception logEx)
        {
            System.Diagnostics.Debug.WriteLine($"Log Failed: {logEx.Message}");
        }
    }
}
