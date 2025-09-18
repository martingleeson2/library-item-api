using Example.LibraryItem.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Example.LibraryItem.Infrastructure
{
    /// <summary>
    /// Entity Framework DbContext for the library management system.
    /// Configures entity mappings, value conversions, and database schema
    /// for optimal storage and retrieval of library items and related data.
    /// </summary>
    /// <param name="options">Database context configuration options</param>
    public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
    {
        /// <summary>
        /// Separator character used for storing list properties as delimited strings in the database.
        /// Pipe character chosen to avoid conflicts with common text content.
        /// </summary>
        private const char ListSeparator = '|';
        
        /// <summary>
        /// Database set for library items. Provides access to all CRUD operations.
        /// </summary>
        public DbSet<Item> Items => Set<Item>();

        /// <summary>
        /// Configures the entity framework model using Fluent API.
        /// Sets up complex type mappings, value conversions, and database constraints
        /// that cannot be expressed through data annotations alone.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance for configuration</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var item = modelBuilder.Entity<Item>();
            
            // Configure ItemLocation as owned entity - stores location data as flattened columns
            // This approach provides better query performance than separate location table
            item.OwnsOne(i => i.Location, nb =>
            {
                nb.Property(p => p.Floor).HasColumnName("Location_Floor").IsRequired();
                nb.Property(p => p.Section).HasColumnName("Location_Section").HasMaxLength(10).IsRequired();
                nb.Property(p => p.ShelfCode).HasColumnName("Location_ShelfCode").HasMaxLength(20).IsRequired();
                nb.Property(p => p.Wing).HasColumnName("Location_Wing").HasMaxLength(20);
                nb.Property(p => p.Position).HasColumnName("Location_Position").HasMaxLength(10);
                nb.Property(p => p.Notes).HasColumnName("Location_Notes").HasMaxLength(255);
            });

            // Convert Contributors list to pipe-separated string for database storage
            // This approach balances simplicity with query capabilities for most use cases
            item.Property(i => i.Contributors)
                .HasConversion(new ValueConverter<List<string>, string>(
                    v => string.Join(ListSeparator, v),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries).ToList()))
                .HasColumnName("Contributors");

            // Convert Subjects list to pipe-separated string for database storage
            // Enables basic searching while maintaining simple schema structure
            item.Property(i => i.Subjects)
                .HasConversion(new ValueConverter<List<string>, string>(
                    v => string.Join(ListSeparator, v),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries).ToList()))
                .HasColumnName("Subjects");

            // Convert Uri to string for database storage with proper null handling
            item.Property(i => i.DigitalUrl)
                .HasConversion(
                    v => v != null ? v.ToString() : null,
                    v => v != null ? new Uri(v) : null);

            // Convert DateOnly to DateTime for database storage (SQLite compatibility)
            item.Property(i => i.PublicationDate)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    v => v.HasValue ? DateOnly.FromDateTime(v.Value) : (DateOnly?)null);

            // Convert DateOnly to DateTime for database storage (SQLite compatibility)
            item.Property(i => i.AcquisitionDate)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    v => v.HasValue ? DateOnly.FromDateTime(v.Value) : (DateOnly?)null);

            // Configure decimal precision for cost field - 18 total digits, 2 decimal places
            // Supports currency values up to 9,999,999,999,999,999.99
            item.Property(i => i.Cost).HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
