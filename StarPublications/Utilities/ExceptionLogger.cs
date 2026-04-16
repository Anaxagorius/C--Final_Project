using System;
using System.IO;

namespace StarPublications.Utilities
{
    /// <summary>
    /// Writes exception details to a log file next to the application executable.
    /// The log file is named <c>StarPublications_errors.log</c>.
    /// </summary>
    public static class ExceptionLogger
    {
        private static readonly string LogPath =
            Path.Combine(AppContext.BaseDirectory, "StarPublications_errors.log");

        private static readonly object _lock = new();

        /// <summary>
        /// Appends a timestamped entry for <paramref name="ex"/> to the log file.
        /// </summary>
        /// <param name="context">Short description of where the error originated.</param>
        /// <param name="ex">The exception to log.</param>
        public static void Log(string context, Exception ex)
        {
            try
            {
                var entry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] [{context}] {ex.GetType().Name}: {ex.Message}";

                if (ex.InnerException != null)
                    entry += $"{Environment.NewLine}  Inner: {ex.InnerException.Message}";

                entry += $"{Environment.NewLine}  StackTrace: {ex.StackTrace}";
                entry += Environment.NewLine + new string('-', 80) + Environment.NewLine;

                lock (_lock)
                {
                    File.AppendAllText(LogPath, entry);
                }
            }
            catch
            {
                // Logging must never throw — swallow any I/O failures silently.
            }
        }
    }
}
