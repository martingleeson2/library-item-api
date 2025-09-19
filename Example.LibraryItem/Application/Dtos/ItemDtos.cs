using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Example.LibraryItem.Domain;

namespace Example.LibraryItem.Application.Dtos
{
    /// <summary>
    /// Physical location information for library items within the building
    /// </summary>
    public record ItemLocationDto
    {
        /// <summary>
        /// Floor number where the item is located (e.g., 1, 2, 3). Supports basement levels with negative numbers.
        /// </summary>
        /// <example>2</example>
        [JsonPropertyName("floor")]
        [Range(-2, 20, ErrorMessage = "Floor must be between -2 (basement levels) and 20")]
        public required int Floor { get; init; }

        /// <summary>
        /// Section identifier within the floor (e.g., 'A', 'B', 'REF' for reference)
        /// </summary>
        /// <example>A</example>
        [JsonPropertyName("section")]
        [Required(ErrorMessage = "Section is required")]
        [MaxLength(10, ErrorMessage = "Section cannot exceed 10 characters")]
        public required string Section { get; init; }

        /// <summary>
        /// Specific shelf identifier within the section (e.g., 'A01', 'REF-001')
        /// </summary>
        /// <example>A01</example>
        [JsonPropertyName("shelf_code")]
        [Required(ErrorMessage = "Shelf code is required")]
        [MaxLength(20, ErrorMessage = "Shelf code cannot exceed 20 characters")]
        public required string ShelfCode { get; init; }

        /// <summary>
        /// Optional wing designation for large libraries (e.g., 'North', 'South', 'East Wing')
        /// </summary>
        /// <example>North Wing</example>
        [JsonPropertyName("wing")]
        [MaxLength(20, ErrorMessage = "Wing cannot exceed 20 characters")]
        public string? Wing { get; init; }

        /// <summary>
        /// Optional position on shelf (e.g., 'Top', 'Middle', 'Bottom', 'Left Side')
        /// </summary>
        /// <example>Top Shelf</example>
        [JsonPropertyName("position")]
        [MaxLength(10, ErrorMessage = "Position cannot exceed 10 characters")]
        public string? Position { get; init; }

        /// <summary>
        /// Optional additional location notes or special instructions for finding the item
        /// </summary>
        /// <example>Behind the reference desk</example>
        [JsonPropertyName("notes")]
        [MaxLength(255, ErrorMessage = "Location notes cannot exceed 255 characters")]
        public string? Notes { get; init; }
    }

    /// <summary>
    /// Complete library item information including metadata, location, and audit fields
    /// </summary>
    public record ItemDto
    {
        /// <summary>
        /// Unique identifier for the library item
        /// </summary>
        /// <example>123e4567-e89b-12d3-a456-426614174000</example>
        [JsonPropertyName("id")] 
        public required Guid Id { get; init; }

        /// <summary>
        /// Primary title of the library item
        /// </summary>
        /// <example>The Great Gatsby</example>
        [JsonPropertyName("title")] 
        public required string Title { get; init; }

        /// <summary>
        /// Optional subtitle providing additional context
        /// </summary>
        /// <example>A Novel</example>
        [JsonPropertyName("subtitle")] 
        public string? Subtitle { get; init; }

        /// <summary>
        /// Primary author or creator of the item
        /// </summary>
        /// <example>F. Scott Fitzgerald</example>
        [JsonPropertyName("author")] 
        public string? Author { get; init; }

        /// <summary>
        /// List of additional contributors (co-authors, editors, translators, etc.)
        /// </summary>
        /// <example>["Jane Smith (Editor)", "John Doe (Translator)"]</example>
        [JsonPropertyName("contributors")] 
        public List<string>? Contributors { get; init; }

        /// <summary>
        /// International Standard Book Number (ISBN-10 or ISBN-13 format)
        /// </summary>
        /// <example>9780743273565</example>
        [JsonPropertyName("isbn")] 
        public string? Isbn { get; init; }

        /// <summary>
        /// International Standard Serial Number for periodicals (format: XXXX-XXXC)
        /// </summary>
        /// <example>1234-567X</example>
        [JsonPropertyName("issn")] 
        public string? Issn { get; init; }

        /// <summary>
        /// Publisher or publishing organization
        /// </summary>
        /// <example>Scribner</example>
        [JsonPropertyName("publisher")] 
        public string? Publisher { get; init; }

        /// <summary>
        /// Date when the item was published (ISO 8601 date format)
        /// </summary>
        /// <example>1925-04-10</example>
        [JsonPropertyName("publication_date")] 
        public DateOnly? PublicationDate { get; init; }

        /// <summary>
        /// Edition information (e.g., '1st', '2nd', 'Revised', 'Special')
        /// </summary>
        /// <example>1st Edition</example>
        [JsonPropertyName("edition")] 
        public string? Edition { get; init; }

        /// <summary>
        /// Total number of pages (for physical books and documents)
        /// </summary>
        /// <example>180</example>
        [JsonPropertyName("pages")] 
        public int? Pages { get; init; }

        /// <summary>
        /// Primary language of the content using standard language codes
        /// </summary>
        /// <example>en</example>
        [JsonPropertyName("language")] 
        public string? Language { get; init; }

        /// <summary>
        /// Type of library item (book, periodical, dvd, cd, manuscript, digital_resource)
        /// </summary>
        [JsonPropertyName("item_type")] 
        public required ItemType ItemType { get; init; }

        /// <summary>
        /// Library-specific call number for organization and retrieval
        /// </summary>
        /// <example>813.52 F55g</example>
        [JsonPropertyName("call_number")] 
        public required string CallNumber { get; init; }

        /// <summary>
        /// Classification system used for organizing items
        /// </summary>
        [JsonPropertyName("classification_system")] 
        public required ClassificationSystem ClassificationSystem { get; init; }

        /// <summary>
        /// Named collection this item belongs to (e.g., 'Reference', 'Special Collections')
        /// </summary>
        /// <example>General Collection</example>
        [JsonPropertyName("collection")] 
        public string? Collection { get; init; }

        /// <summary>
        /// Physical location of the item within the library building
        /// </summary>
        [JsonPropertyName("location")] 
        public required ItemLocationDto Location { get; init; }

        /// <summary>
        /// Current availability status of the item
        /// </summary>
        [JsonPropertyName("status")] 
        public required ItemStatus Status { get; init; }

        /// <summary>
        /// Barcode identifier for scanning and checkout systems
        /// </summary>
        /// <example>123456789012</example>
        [JsonPropertyName("barcode")] 
        public string? Barcode { get; init; }

        /// <summary>
        /// Date when the library acquired this item (ISO 8601 date format)
        /// </summary>
        /// <example>2023-03-15</example>
        [JsonPropertyName("acquisition_date")] 
        public DateOnly? AcquisitionDate { get; init; }

        /// <summary>
        /// Acquisition cost of the item for inventory valuation (in local currency)
        /// </summary>
        /// <example>29.95</example>
        [JsonPropertyName("cost")] 
        public decimal? Cost { get; init; }

        /// <summary>
        /// Notes about the physical condition of the item
        /// </summary>
        /// <example>Excellent condition, minor wear on cover</example>
        [JsonPropertyName("condition_notes")] 
        public string? ConditionNotes { get; init; }

        /// <summary>
        /// Detailed description or abstract of the item content
        /// </summary>
        /// <example>A classic American novel set in the Jazz Age...</example>
        [JsonPropertyName("description")] 
        public string? Description { get; init; }

        /// <summary>
        /// Subject categories and keywords for classification and search
        /// </summary>
        /// <example>["American Literature", "Fiction", "Jazz Age", "Classic Literature"]</example>
        [JsonPropertyName("subjects")] 
        public List<string>? Subjects { get; init; }

        /// <summary>
        /// URL for digital access to the item (e-books, online journals, etc.)
        /// </summary>
        /// <example>https://digital.library.edu/books/great-gatsby</example>
        [JsonPropertyName("digital_url")] 
        public Uri? DigitalUrl { get; init; }

        /// <summary>
        /// Timestamp when the item record was initially created (ISO 8601 format)
        /// </summary>
        /// <example>2023-03-15T10:30:00Z</example>
        [JsonPropertyName("created_at")] 
        public required DateTime CreatedAt { get; init; }

        /// <summary>
        /// Timestamp when the item record was last modified (ISO 8601 format)
        /// </summary>
        /// <example>2023-03-20T14:45:00Z</example>
        [JsonPropertyName("updated_at")] 
        public required DateTime UpdatedAt { get; init; }

        /// <summary>
        /// Identifier of the user who created this item record
        /// </summary>
        /// <example>librarian@library.edu</example>
        [JsonPropertyName("created_by")] 
        public string? CreatedBy { get; init; }

        /// <summary>
        /// Identifier of the user who last updated this item record
        /// </summary>
        /// <example>admin@library.edu</example>
        [JsonPropertyName("updated_by")] 
        public string? UpdatedBy { get; init; }
    }
}