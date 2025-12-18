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
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogException(e.Exception, "UI Thread");
        // Prevent default crash dialog? Maybe. 
        // For a dev tool, showing the error is good, but we want to log it first.
        // e.Handled = true; // Uncomment to suppress crash
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogException(ex, "Background Thread");
        }
    }

    private void LogException(Exception ex, string source)
    {
        try
        {
            string message = $"[{DateTime.Now}] CRASH ({source}): {ex.Message}\n{ex.StackTrace}\n--------------------------------------------------\n";
            File.AppendAllText(CrashLogFile, message);
            
            // Optional: Show message box so user knows log exists
            // MessageBox.Show($"A critical error occurred. Logged to {CrashLogFile}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch 
        {
            // If logging fails, we are in trouble.
        }
    }
}
