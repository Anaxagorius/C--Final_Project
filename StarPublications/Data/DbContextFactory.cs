using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace StarPublications.Data
{
    /// <summary>
    /// Factory class for creating instances of <see cref="PubsDbContext"/>.
    /// Reads the connection string from the application configuration file.
    /// </summary>
    public static class DbContextFactory
    {
        private const string SettingsFile = "appsettings.json";

        /// <summary>
        /// Resolves the directory that contains <c>appsettings.json</c>.
        /// Checks the application base directory first, then falls back to the
        /// current working directory so the file is located correctly regardless
        /// of how the application is launched.
        /// </summary>
        private static string FindBasePath()
        {
            if (File.Exists(Path.Combine(AppContext.BaseDirectory, SettingsFile)))
                return AppContext.BaseDirectory;

            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), SettingsFile)))
                return Directory.GetCurrentDirectory();

            // Default — let the configuration system report the missing file.
            return AppContext.BaseDirectory;
        }

        /// <summary>
        /// Creates a new instance of <see cref="PubsDbContext"/> using the configured connection string.
        /// </summary>
        public static PubsDbContext Create()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(FindBasePath())
                .AddJsonFile(SettingsFile, optional: false, reloadOnChange: false)
                .Build();

            var connectionString = configuration.GetConnectionString("PubsDatabase")
                ?? throw new InvalidOperationException("Connection string 'PubsDatabase' not found in appsettings.json.");

            var optionsBuilder = new DbContextOptionsBuilder<PubsDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new PubsDbContext(optionsBuilder.Options);
        }
    }
}
