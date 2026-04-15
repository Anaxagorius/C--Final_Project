using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StarPublications.Data
{
    /// <summary>
    /// Provides a simple CSV export helper that opens a SaveFileDialog and writes data.
    /// </summary>
    public static class CsvExporter
    {
        /// <summary>
        /// Prompts the user to choose a file and exports <paramref name="items"/> as CSV.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="items">Collection of items to export.</param>
        /// <param name="headers">Column header strings.</param>
        /// <param name="rowSelector">Function that converts an item to a string array (one value per column).</param>
        /// <param name="defaultFileName">Suggested file name (without extension).</param>
        /// <returns>True if the file was saved; false if the user cancelled.</returns>
        public static bool Export<T>(
            IEnumerable<T> items,
            string[] headers,
            Func<T, string[]> rowSelector,
            string defaultFileName = "export")
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = defaultFileName,
                DefaultExt = "csv",
                Title = "Export to CSV"
            };

            if (dialog.ShowDialog() != true)
                return false;

            using var writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            writer.WriteLine(string.Join(",", headers.Select(EscapeField)));

            foreach (var item in items)
            {
                var row = rowSelector(item);
                writer.WriteLine(string.Join(",", row.Select(v => EscapeField(v ?? string.Empty))));
            }

            return true;
        }

        private static string EscapeField(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
