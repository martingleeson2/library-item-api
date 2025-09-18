using System.ComponentModel.DataAnnotations;

namespace Example.LibraryItem.Domain
{
    /// <summary>
    /// Represents the physical location of a library item within the library building.
    /// Uses a record type for immutability and value-based equality semantics.
    /// Supports hierarchical location tracking from floor level down to specific shelf positions.
    /// </summary>
    /// <param name="Floor">The floor number where the item is located (e.g., 1, 2, 3)</param>
    /// <param name="Section">The section identifier within the floor (e.g., 'A', 'B', 'REF')</param>
    /// <param name="ShelfCode">The specific shelf identifier (e.g., 'A01', 'REF-001')</param>
    /// <param name="Wing">Optional wing designation for large libraries (e.g., 'North', 'South')</param>
    /// <param name="Position">Optional position on shelf (e.g., 'Top', 'Middle', 'Bottom')</param>
    /// <param name="Notes">Optional additional location notes or special instructions</param>
    public record ItemLocation
    (
        int Floor,
        string Section,
        string ShelfCode,
        string? Wing = null,
        string? Position = null,
        string? Notes = null
    )
    {
        /// <summary>
        /// Factory method for creating ItemLocation instances with validation.
        /// Provides a fluent API alternative to the primary constructor.
        /// </summary>
        /// <param name="floor">The floor number where the item is located</param>
        /// <param name="section">The section identifier within the floor</param>
        /// <param name="shelfCode">The specific shelf identifier</param>
        /// <param name="wing">Optional wing designation</param>
        /// <param name="position">Optional position on shelf</param>
        /// <param name="notes">Optional additional location notes</param>
        /// <returns>A new ItemLocation instance</returns>
        public static ItemLocation Create(int floor, string section, string shelfCode, string? wing = null, string? position = null, string? notes = null)
            => new(floor, section, shelfCode, wing, position, notes);
    }

    /// <summary>
    /// Represents a library item entity with comprehensive metadata for cataloging and management.
    /// Supports various item types (books, journals, digital resources) with standardized identifiers,
    /// physical location tracking, and full audit trail capabilities.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Unique identifier for the library item. Auto-generated GUID for database independence.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Primary title of the library item. Required field with maximum 500 characters.
        /// </summary>
        [MaxLength(500)]
        public required string Title { get; set; }
        
        /// <summary>
        /// Optional subtitle providing additional context or description.
        /// </summary>
        [MaxLength(500)]
        public string? Subtitle { get; set; }

        /// <summary>
        /// Primary author of the library item. Typically the main creator or first-listed author.
        /// </summary>
        [MaxLength(255)]
        public string? Author { get; set; }

        /// <summary>
        /// List of additional contributors (co-authors, editors, translators, etc.).
        /// Stored as pipe-separated values in the database for simplicity.
        /// </summary>
        public List<string> Contributors { get; set; } = new();

        /// <summary>
        /// International Standard Book Number (ISBN) - unique identifier for books.
        /// Supports both ISBN-10 and ISBN-13 formats. Must be unique across the system.
        /// </summary>
        [RegularExpression("^(?:978|979)?[0-9]{9}[0-9X]$")]
        public string? Isbn { get; set; }

        /// <summary>
        /// International Standard Serial Number (ISSN) - unique identifier for periodicals.
        /// Format: XXXX-XXXC where C is a check digit that can be 0-9 or X.
        /// </summary>
        [RegularExpression("^[0-9]{4}-[0-9]{3}[0-9X]$")]
        public string? Issn { get; set; }

        /// <summary>
        /// Publisher or publishing organization responsible for the item.
        /// </summary>
        [MaxLength(255)]
        public string? Publisher { get; set; }

        /// <summary>
        /// Date when the item was published. Uses DateOnly for date-specific operations.
        /// </summary>
        public DateOnly? PublicationDate { get; set; }

        /// <summary>
        /// Edition information (e.g., '1st', '2nd', 'Revised', 'Special').
        /// </summary>
        [MaxLength(50)]
        public string? Edition { get; set; }

        /// <summary>
        /// Total number of pages in the item (applicable for physical books/documents).
        /// </summary>
        public int? Pages { get; set; }

        /// <summary>
        /// Primary language of the item content using standard language codes.
        /// </summary>
        [MaxLength(10)]
        public string? Language { get; set; }

        /// <summary>
        /// Type of library item (Book, Journal, DigitalResource, etc.) from the ItemType enum.
        /// </summary>
        public ItemType ItemType { get; set; }

        /// <summary>
        /// Library-specific call number for item organization and retrieval.
        /// Must be unique within the classification system being used.
        /// </summary>
        [MaxLength(50)]
        public required string CallNumber { get; set; }

        /// <summary>
        /// Classification system used for organizing items (Dewey, Library of Congress, etc.).
        /// </summary>
        public ClassificationSystem ClassificationSystem { get; set; }

        /// <summary>
        /// Named collection this item belongs to (e.g., 'Reference', 'Special Collections').
        /// </summary>
        [MaxLength(100)]
        public string? Collection { get; set; }

        /// <summary>
        /// Physical location of the item within the library building.
        /// Required for physical items to enable patron and staff retrieval.
        /// </summary>
        public required ItemLocation Location { get; set; }

        /// <summary>
        /// Current availability status of the item (Available, CheckedOut, Reserved, etc.).
        /// Defaults to Available for new items.
        /// </summary>
        public ItemStatus Status { get; set; } = ItemStatus.available;

        /// <summary>
        /// Barcode identifier for physical item scanning and checkout systems.
        /// </summary>
        [MaxLength(50)]
        public string? Barcode { get; set; }

        /// <summary>
        /// Date when the library acquired this item for inventory tracking.
        /// </summary>
        public DateOnly? AcquisitionDate { get; set; }

        /// <summary>
        /// Acquisition cost of the item for budget and inventory valuation.
        /// </summary>
        public decimal? Cost { get; set; }

        /// <summary>
        /// Notes about the physical condition of the item for maintenance tracking.
        /// </summary>
        [MaxLength(1000)]
        public string? ConditionNotes { get; set; }

        /// <summary>
        /// Detailed description or abstract of the item content.
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// Subject categories and keywords for search and classification.
        /// Stored as pipe-separated values in the database.
        /// </summary>
        public List<string> Subjects { get; set; } = new();

        /// <summary>
        /// URL for digital access to the item (e-books, online journals, etc.).
        /// </summary>
        public Uri? DigitalUrl { get; set; }

        /// <summary>
        /// Timestamp when the item record was initially created in the system.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Timestamp when the item record was last modified.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Identifier of the user who created this item record for audit purposes.
        /// </summary>
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Identifier of the user who last updated this item record for audit purposes.
        /// </summary>
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }
}
