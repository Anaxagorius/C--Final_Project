using Microsoft.EntityFrameworkCore;
using StarPublications.Models;

namespace StarPublications.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for the PUBs database.
    /// Provides access to all tables in the centralized PUBs database.
    /// </summary>
    public class PubsDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PubsDbContext"/> with the given options.
        /// </summary>
        public PubsDbContext(DbContextOptions<PubsDbContext> options) : base(options)
        {
        }

        // DbSets for all relevant tables
        public DbSet<Publisher> Publishers { get; set; } = null!;
        public DbSet<Title> Titles { get; set; } = null!;
        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<TitleAuthor> TitleAuthors { get; set; } = null!;
        public DbSet<Store> Stores { get; set; } = null!;
        public DbSet<Sale> Sales { get; set; } = null!;
        public DbSet<PubInfo> PubInfos { get; set; } = null!;

        /// <summary>
        /// Configures the entity model using Fluent API.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite primary keys
            modelBuilder.Entity<TitleAuthor>()
                .HasKey(ta => new { ta.AuId, ta.TitleId });

            modelBuilder.Entity<Sale>()
                .HasKey(s => new { s.StorId, s.OrdNum, s.TitleId });

            // Configure relationships
            modelBuilder.Entity<Title>()
                .HasOne(t => t.Publisher)
                .WithMany(p => p.Titles)
                .HasForeignKey(t => t.PubId)
                .IsRequired(false);

            modelBuilder.Entity<TitleAuthor>()
                .HasOne(ta => ta.Author)
                .WithMany(a => a.TitleAuthors)
                .HasForeignKey(ta => ta.AuId);

            modelBuilder.Entity<TitleAuthor>()
                .HasOne(ta => ta.Title)
                .WithMany(t => t.TitleAuthors)
                .HasForeignKey(ta => ta.TitleId);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Store)
                .WithMany(st => st.Sales)
                .HasForeignKey(s => s.StorId);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Title)
                .WithMany(t => t.Sales)
                .HasForeignKey(s => s.TitleId);

            modelBuilder.Entity<PubInfo>()
                .HasOne(pi => pi.Publisher)
                .WithMany(p => p.PubInfos)
                .HasForeignKey(pi => pi.PubId);

            // Column configurations
            modelBuilder.Entity<Title>()
                .Property(t => t.Price)
                .HasColumnType("money");

            modelBuilder.Entity<Title>()
                .Property(t => t.Advance)
                .HasColumnType("money");
        }
    }
}
