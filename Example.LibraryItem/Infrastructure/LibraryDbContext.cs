using Example.LibraryItem.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Example.LibraryItem.Infrastructure
{
    public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
    {
        private const char ListSeparator = '|';
        public DbSet<Item> Items => Set<Item>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var item = modelBuilder.Entity<Item>();
            item.OwnsOne(i => i.Location, nb =>
            {
                nb.Property(p => p.Floor).HasColumnName("Location_Floor").IsRequired();
                nb.Property(p => p.Section).HasColumnName("Location_Section").HasMaxLength(10).IsRequired();
                nb.Property(p => p.ShelfCode).HasColumnName("Location_ShelfCode").HasMaxLength(20).IsRequired();
                nb.Property(p => p.Wing).HasColumnName("Location_Wing").HasMaxLength(20);
                nb.Property(p => p.Position).HasColumnName("Location_Position").HasMaxLength(10);
                nb.Property(p => p.Notes).HasColumnName("Location_Notes").HasMaxLength(255);
            });

            item.Property(i => i.Contributors)
                .HasConversion(new ValueConverter<List<string>, string>(
                    v => string.Join(ListSeparator, v),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries).ToList()))
                .HasColumnName("Contributors");

            item.Property(i => i.Subjects)
                .HasConversion(new ValueConverter<List<string>, string>(
                    v => string.Join(ListSeparator, v),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries).ToList()))
                .HasColumnName("Subjects");

            item.Property(i => i.DigitalUrl)
                .HasConversion(
                    v => v != null ? v.ToString() : null,
                    v => v != null ? new Uri(v) : null);

            item.Property(i => i.PublicationDate)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    v => v.HasValue ? DateOnly.FromDateTime(v.Value) : (DateOnly?)null);

            item.Property(i => i.AcquisitionDate)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    v => v.HasValue ? DateOnly.FromDateTime(v.Value) : (DateOnly?)null);

            item.Property(i => i.Cost).HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
