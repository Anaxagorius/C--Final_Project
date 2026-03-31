using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace StarPublications.Data
{
    /// <summary>
    /// Factory class for creating instances of <see cref="PubsDbContext"/>.
    /// Reads the connection string from the application configuration file.
    /// </summary>
    public static class DbContextFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="PubsDbContext"/> using the configured connection string.
        /// </summary>
        public static PubsDbContext Create()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("PubsDatabase")
                ?? throw new InvalidOperationException("Connection string 'PubsDatabase' not found in appsettings.json.");

            var optionsBuilder = new DbContextOptionsBuilder<PubsDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new PubsDbContext(optionsBuilder.Options);
        }
    }
}
