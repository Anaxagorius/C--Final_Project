using StarPublications.Utilities;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StarPublications;

/// <summary>
/// Application startup and global exception handling.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Registers all global exception handlers so no unhandled exception can silently crash the app.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Catch unhandled exceptions on the UI thread
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        // Catch unhandled exceptions thrown on non-UI (background) threads
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // Catch unobserved Task exceptions (fire-and-forget tasks that faulted)
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    /// <summary>UI-thread unhandled exception — log, inform the user, and keep the app alive.</summary>
    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ExceptionLogger.Log("DispatcherUnhandledException", e.Exception);

        MessageBox.Show(
            $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe application will continue.",
            "Star Publications — Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        // Mark as handled so the app does not crash
        e.Handled = true;
    }

    /// <summary>Background-thread unhandled exception — log and inform the user before the CLR terminates the process.</summary>
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        var message = ex?.Message ?? e.ExceptionObject?.ToString() ?? "Unknown error";

        if (ex != null)
            ExceptionLogger.Log("UnhandledException", ex);

        MessageBox.Show(
            $"A fatal error occurred on a background thread:\n\n{message}\n\nThe application will now close.",
            "Star Publications — Fatal Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    /// <summary>Unobserved Task exception — log and mark as observed to prevent process termination.</summary>
    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        ExceptionLogger.Log("UnobservedTaskException", e.Exception);

        // Prevent the default behavior of re-throwing the exception and terminating the process
        e.SetObserved();
    }
}

