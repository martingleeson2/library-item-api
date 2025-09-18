using Example.LibraryItem.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Example.LibraryItem.Infrastructure
{
    /// <summary>
    /// Entity Framework DbContext for the library management system.
    /// Uses a hybrid approach: leverages EF conventions where possible, 
    /// adds manual configuration only where needed for performance and business requirements.
    /// </summary>
    /// <param name="options">Database context configuration options</param>
    public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
    {
        #region Configuration Constants
        
        /// <summary>
        /// Separator character used for storing list properties as delimited strings in the database.
        /// Pipe character chosen to avoid conflicts with common text content.
        /// </summary>
        private const char ListSeparator = '|';
        
        /// <summary>
        /// Precision for financial decimal fields (total digits).
        /// Supports values up to 9,999,999,999,999,999.99 for high-value rare items.
        /// </summary>
        private const int CurrencyPrecision = 18;
        
        /// <summary>
        /// Scale for financial decimal fields (digits after decimal point).
        /// Standard currency scale supporting cents/pence precision.
        /// </summary>
        private const int CurrencyScale = 2;
        
        #endregion
        
        /// <summary>
        /// Database set for library items. 
        /// EF automatically recognizes this as the primary entity set.
        /// </summary>
        public DbSet<Item> Items => Set<Item>();

        /// <summary>
        /// Configures the entity framework model using a hybrid approach:
        /// 1. Let EF handle basic entity configuration through conventions and data annotations
        /// 2. Add manual configuration only for performance optimization and complex business rules
        /// 3. Explicitly document why each manual configuration is necessary
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Let EF apply its default conventions first
            // This handles: primary keys, required fields, max lengths, enum conversions
            base.OnModelCreating(modelBuilder);
            
            // Apply manual configurations in logical groups
            ConfigureOwnedEntities(modelBuilder);
            ConfigureValueConversions(modelBuilder);
            ConfigurePerformanceOptimizations(modelBuilder);
        }

        /// <summary>
        /// Configures owned entities that can't be handled by EF conventions.
        /// Manual configuration needed because:
        /// - Custom column naming for flattened storage
        /// - Optimized storage strategy (flattened vs separate table)
        /// </summary>
        private static void ConfigureOwnedEntities(ModelBuilder modelBuilder)
        {
            // Configure ItemLocation as owned entity - stores location data as flattened columns
            // WHY MANUAL: EF would create a separate LocationValue table by default, but flattening
            // provides better query performance for location-based searches and simpler schema
            modelBuilder.Entity<Item>().OwnsOne(i => i.Location, locationBuilder =>
            {
                // Use descriptive column names that include the property context
                locationBuilder.Property(p => p.Floor).HasColumnName("Location_Floor").IsRequired();
                locationBuilder.Property(p => p.Section).HasColumnName("Location_Section").HasMaxLength(10).IsRequired();
                locationBuilder.Property(p => p.ShelfCode).HasColumnName("Location_ShelfCode").HasMaxLength(20).IsRequired();
                locationBuilder.Property(p => p.Wing).HasColumnName("Location_Wing").HasMaxLength(20);
                locationBuilder.Property(p => p.Position).HasColumnName("Location_Position").HasMaxLength(10);
                locationBuilder.Property(p => p.Notes).HasColumnName("Location_Notes").HasMaxLength(255);
            });
        }

        /// <summary>
        /// Configures custom value conversions that EF cannot handle automatically.
        /// Manual configuration needed for:
        /// - Complex type conversions (List&lt;string&gt; to string, DateOnly to DateTime)
        /// - Database provider compatibility (DateOnly support)
        /// - Custom serialization strategies
        /// </summary>
        private static void ConfigureValueConversions(ModelBuilder modelBuilder)
        {
            var itemEntity = modelBuilder.Entity<Item>();

            // Configure Contributors list storage as pipe-separated string
            // WHY MANUAL: EF doesn't know how to convert List<string> to database-storable format
            // TRADE-OFF: Simple schema and fast reads vs. limited querying capability
            ConfigureStringListProperty(itemEntity.Property(i => i.Contributors), "Contributors");

            // Configure Subjects list storage as pipe-separated string  
            // WHY MANUAL: Same reasoning as Contributors - enables simple storage with basic search
            ConfigureStringListProperty(itemEntity.Property(i => i.Subjects), "Subjects");

            // Configure Uri to string conversion with proper null handling
            // WHY MANUAL: EF doesn't have built-in Uri conversion, and we need explicit null handling
            itemEntity.Property(i => i.DigitalUrl)
                .HasConversion(
                    v => v != null ? v.ToString() : null,
                    v => v != null ? new Uri(v) : null);

            // Configure DateOnly to DateTime conversion for database compatibility
            // WHY MANUAL: DateOnly is newer than most database providers - needs explicit conversion
            // TRADE-OFF: Lose time component but gain broad database compatibility
            itemEntity.Property(i => i.PublicationDate)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    v => v.HasValue ? DateOnly.FromDateTime(v.Value) : null);

            itemEntity.Property(i => i.AcquisitionDate)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    v => v.HasValue ? DateOnly.FromDateTime(v.Value) : null);
        }

        /// <summary>
        /// Configures performance optimizations that override EF defaults.
        /// Manual configuration needed for:
        /// - Custom precision for financial data
        /// - Essential indexes for common query patterns
        /// </summary>
        private static void ConfigurePerformanceOptimizations(ModelBuilder modelBuilder)
        {
            var itemEntity = modelBuilder.Entity<Item>();

            // Configure decimal precision for cost field - financial data requires explicit precision
            // WHY MANUAL: EF default decimal precision may not be suitable for currency values
            // BUSINESS REQUIREMENT: Supports values up to 9,999,999,999,999,999.99 for rare/valuable items
            itemEntity.Property(i => i.Cost).HasPrecision(CurrencyPrecision, CurrencyScale);

            // Configure essential indexes for performance-critical operations
            ConfigureEssentialIndexes(itemEntity);
        }

        /// <summary>
        /// Configures essential indexes for performance-critical operations.
        /// Focuses on the most important indexes to avoid configuration complexity.
        /// Additional indexes can be added incrementally as performance needs are identified.
        /// </summary>
        private static void ConfigureEssentialIndexes(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Item> itemEntity)
        {
            // Primary search and lookup indexes
            // WHY NEEDED: Core functionality requires fast lookups on these fields
            
            // Title search index - most common search operation
            itemEntity.HasIndex(i => i.Title)
                .HasDatabaseName("IX_Items_Title_Search");

            // ISBN unique constraint and lookup optimization
            itemEntity.HasIndex(i => i.Isbn)
                .IsUnique()
                .HasDatabaseName("IX_Items_Isbn_Unique")
                .HasFilter("[Isbn] IS NOT NULL");

            // Call number unique constraint - required for library organization
            itemEntity.HasIndex(i => i.CallNumber)
                .IsUnique()
                .HasDatabaseName("IX_Items_CallNumber_Unique");

            // Status and type filtering - most common filter combination
            itemEntity.HasIndex(i => new { i.Status, i.ItemType })
                .HasDatabaseName("IX_Items_Status_ItemType_Filter");

            // Creation timestamp for chronological operations
            itemEntity.HasIndex(i => i.CreatedAt)
                .HasDatabaseName("IX_Items_CreatedAt_Chronological");

            // FUTURE INDEXES: Additional performance indexes can be added based on:
            // 1. Query performance monitoring in production environments
            // 2. Specific use case requirements as they emerge
            // 3. Database size and growth patterns analysis
            // 4. User behavior analytics showing search patterns
            //
            // Candidates for future implementation:
            // - Author search index for contributor-based queries
            // - Barcode index for checkout system integration  
            // - Location-based indexes for physical item management
            // - Audit indexes for administrative reporting
            // - Publication date index for temporal filtering
        }

        /// <summary>
        /// Helper method to configure string list properties with consistent conversion logic.
        /// Reduces code duplication and ensures consistent behavior across all list properties.
        /// </summary>
        /// <param name="propertyBuilder">The property builder for the list property</param>
        /// <param name="columnName">The database column name</param>
        private static void ConfigureStringListProperty(
            Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<List<string>> propertyBuilder, 
            string columnName)
        {
            // Set up the value converter for List<string> <-> string conversion
            var converter = new ValueConverter<List<string>, string>(
                // To database: join list items with separator
                listValue => string.Join(ListSeparator, listValue),
                // From database: split string and filter empty entries
                stringValue => string.IsNullOrEmpty(stringValue) 
                    ? new List<string>() 
                    : stringValue.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries).ToList()
            );

            var property = propertyBuilder
                .HasConversion(converter)
                .HasColumnName(columnName);

            // Configure value comparer for proper change tracking
            // WHY NEEDED: EF needs to know how to compare List<string> instances to detect changes
            property.Metadata.SetValueComparer(new ValueComparer<List<string>>(
                // Equality comparison: compare lists element by element
                (left, right) => left!.SequenceEqual(right!),
                // Hash code generation: combine hash codes of all elements
                listValue => listValue.Aggregate(0, (accumulator, item) => HashCode.Combine(accumulator, item.GetHashCode())),
                // Snapshot: create a deep copy for change tracking
                listValue => listValue.ToList()
            ));
        }

    }
}
