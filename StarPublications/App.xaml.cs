using System.Windows;
using System.Windows.Threading;

namespace StarPublications;

/// <summary>
/// Application startup and global exception handling.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Handles any unhandled dispatcher exceptions and shows a user-friendly error dialog.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Catch unhandled exceptions on the UI thread
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe application will continue.",
            "Star Publications — Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        // Mark as handled so the app does not crash
        e.Handled = true;
    }
}

