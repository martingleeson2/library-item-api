using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Example.LibraryItem.Domain;

namespace Example.LibraryItem.Application.Dtos
{
    /// <summary>
    /// Data required to create a new library item
    /// </summary>
    public record ItemCreateRequestDto
    {
        /// <summary>
        /// Primary title of the library item (required)
        /// </summary>
        /// <example>The Great Gatsby</example>
        [JsonPropertyName("title")]
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(500, ErrorMessage = "Title cannot exceed 500 characters")]
        public required string Title { get; init; }

        /// <summary>
        /// Optional subtitle providing additional context
        /// </summary>
        /// <example>A Novel</example>
        [JsonPropertyName("subtitle")]
        [MaxLength(500, ErrorMessage = "Subtitle cannot exceed 500 characters")]
        public string? Subtitle { get; init; }

        /// <summary>
        /// Primary author or creator of the item
        /// </summary>
        /// <example>F. Scott Fitzgerald</example>
        [JsonPropertyName("author")]
        [MaxLength(255, ErrorMessage = "Author cannot exceed 255 characters")]
        public string? Author { get; init; }

        /// <summary>
        /// List of additional contributors (maximum 20 contributors allowed)
        /// </summary>
        /// <example>["Jane Smith (Editor)", "John Doe (Translator)"]</example>
        [JsonPropertyName("contributors")]
        [MaxLength(20, ErrorMessage = "Cannot have more than 20 contributors")]
        public List<string>? Contributors { get; init; }

        /// <summary>
        /// International Standard Book Number (ISBN-10 or ISBN-13 format)
        /// </summary>
        /// <example>9780743273565</example>
        [JsonPropertyName("isbn")]
        [RegularExpression(@"^(?:978|979)?[0-9]{9}[0-9X]$", ErrorMessage = "ISBN format is invalid")]
        public string? Isbn { get; init; }

        /// <summary>
        /// International Standard Serial Number for periodicals (format: XXXX-XXXC)
        /// </summary>
        /// <example>1234-567X</example>
        [JsonPropertyName("issn")]
        [RegularExpression(@"^[0-9]{4}-[0-9]{3}[0-9X]$", ErrorMessage = "ISSN format is invalid")]
        public string? Issn { get; init; }

        /// <summary>
        /// Publisher or publishing organization
        /// </summary>
        /// <example>Scribner</example>
        [JsonPropertyName("publisher")]
        [MaxLength(255, ErrorMessage = "Publisher cannot exceed 255 characters")]
        public string? Publisher { get; init; }

        /// <summary>
        /// Date when the item was published (ISO 8601 date format)
        /// </summary>
        /// <example>1925-04-10</example>
        [JsonPropertyName("publication_date")]
        public DateOnly? PublicationDate { get; init; }

        /// <summary>
        /// Edition information (e.g., '1st', '2nd', 'Revised')
        /// </summary>
        /// <example>1st Edition</example>
        [JsonPropertyName("edition")]
        [MaxLength(50, ErrorMessage = "Edition cannot exceed 50 characters")]
        public string? Edition { get; init; }

        /// <summary>
        /// Total number of pages (must be positive for physical items)
        /// </summary>
        /// <example>180</example>
        [JsonPropertyName("pages")]
        [Range(1, int.MaxValue, ErrorMessage = "Pages must be greater than 0")]
        public int? Pages { get; init; }

        /// <summary>
        /// Primary language using standard language codes
        /// </summary>
        /// <example>en</example>
        [JsonPropertyName("language")]
        [MaxLength(10, ErrorMessage = "Language code cannot exceed 10 characters")]
        public string? Language { get; init; }

        /// <summary>
        /// Type of library item (required)
        /// </summary>
        [JsonPropertyName("item_type")]
        [Required(ErrorMessage = "Item type is required")]
        public required ItemType ItemType { get; init; }

        /// <summary>
        /// Library-specific call number for organization (required)
        /// </summary>
        /// <example>813.52 F55g</example>
        [JsonPropertyName("call_number")]
        [Required(ErrorMessage = "Call number is required")]
        [MaxLength(50, ErrorMessage = "Call number cannot exceed 50 characters")]
        public required string CallNumber { get; init; }

        /// <summary>
        /// Classification system used for organizing items (required)
        /// </summary>
        [JsonPropertyName("classification_system")]
        [Required(ErrorMessage = "Classification system is required")]
        public required ClassificationSystem ClassificationSystem { get; init; }

        /// <summary>
        /// Named collection this item belongs to
        /// </summary>
        /// <example>General Collection</example>
        [JsonPropertyName("collection")]
        [MaxLength(100, ErrorMessage = "Collection cannot exceed 100 characters")]
        public string? Collection { get; init; }

        /// <summary>
        /// Physical location of the item within the library building (required)
        /// </summary>
        [JsonPropertyName("location")]
        [Required(ErrorMessage = "Location is required")]
        public required ItemLocationDto Location { get; init; }

        /// <summary>
        /// Initial availability status (defaults to 'available' if not specified)
        /// </summary>
        [JsonPropertyName("status")]
        public ItemStatus? Status { get; init; }

        /// <summary>
        /// Barcode identifier for scanning and checkout systems
        /// </summary>
        /// <example>123456789012</example>
        [JsonPropertyName("barcode")]
        [MaxLength(50, ErrorMessage = "Barcode cannot exceed 50 characters")]
        public string? Barcode { get; init; }

        /// <summary>
        /// Date when the library acquired this item
        /// </summary>
        /// <example>2023-03-15</example>
        [JsonPropertyName("acquisition_date")]
        public DateOnly? AcquisitionDate { get; init; }

        /// <summary>
        /// Acquisition cost for inventory valuation (must be non-negative)
        /// </summary>
        /// <example>29.95</example>
        [JsonPropertyName("cost")]
        [Range(0, double.MaxValue, ErrorMessage = "Cost must be non-negative")]
        public decimal? Cost { get; init; }

        /// <summary>
        /// Notes about the physical condition of the item
        /// </summary>
        /// <example>Excellent condition, minor wear on cover</example>
        [JsonPropertyName("condition_notes")]
        [MaxLength(1000, ErrorMessage = "Condition notes cannot exceed 1000 characters")]
        public string? ConditionNotes { get; init; }

        /// <summary>
        /// Detailed description or abstract of the item content
        /// </summary>
        /// <example>A classic American novel set in the Jazz Age...</example>
        [JsonPropertyName("description")]
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; init; }

        /// <summary>
        /// Subject categories and keywords for classification (maximum 50 subjects allowed)
        /// </summary>
        /// <example>["American Literature", "Fiction", "Jazz Age"]</example>
        [JsonPropertyName("subjects")]
        [MaxLength(50, ErrorMessage = "Cannot have more than 50 subjects")]
        public List<string>? Subjects { get; init; }

        /// <summary>
        /// URL for digital access to the item (must be valid HTTP/HTTPS URL, or null if not available digitally)
        /// </summary>
        /// <example>https://digital.library.edu/books/great-gatsby</example>
        [JsonPropertyName("digital_url")]
        public Uri? DigitalUrl { get; init; }
    }

    public record ItemUpdateRequestDto
    {
        [JsonPropertyName("title")] public required string Title { get; init; }
        [JsonPropertyName("subtitle")] public string? Subtitle { get; init; }
        [JsonPropertyName("author")] public string? Author { get; init; }
        [JsonPropertyName("contributors")] public List<string>? Contributors { get; init; }
        [JsonPropertyName("isbn")] public string? Isbn { get; init; }
        [JsonPropertyName("issn")] public string? Issn { get; init; }
        [JsonPropertyName("publisher")] public string? Publisher { get; init; }
        [JsonPropertyName("publication_date")] public DateOnly? PublicationDate { get; init; }
        [JsonPropertyName("edition")] public string? Edition { get; init; }
        [JsonPropertyName("pages")] public int? Pages { get; init; }
        [JsonPropertyName("language")] public string? Language { get; init; }
        [JsonPropertyName("item_type")] public required ItemType ItemType { get; init; }
        [JsonPropertyName("call_number")] public required string CallNumber { get; init; }
        [JsonPropertyName("classification_system")] public required ClassificationSystem ClassificationSystem { get; init; }
        [JsonPropertyName("collection")] public string? Collection { get; init; }
        [JsonPropertyName("location")] public required ItemLocationDto Location { get; init; }
        [JsonPropertyName("status")] public required ItemStatus Status { get; init; }
        [JsonPropertyName("barcode")] public string? Barcode { get; init; }
        [JsonPropertyName("acquisition_date")] public DateOnly? AcquisitionDate { get; init; }
        [JsonPropertyName("cost")] public decimal? Cost { get; init; }
        [JsonPropertyName("condition_notes")] public string? ConditionNotes { get; init; }
        [JsonPropertyName("description")] public string? Description { get; init; }
        [JsonPropertyName("subjects")] public List<string>? Subjects { get; init; }
        [JsonPropertyName("digital_url")] public Uri? DigitalUrl { get; init; }
    }

    public record ItemPatchRequestDto
    {
        [JsonPropertyName("title")] public string? Title { get; init; }
        [JsonPropertyName("subtitle")] public string? Subtitle { get; init; }
        [JsonPropertyName("author")] public string? Author { get; init; }
        [JsonPropertyName("contributors")] public List<string>? Contributors { get; init; }
        [JsonPropertyName("isbn")] public string? Isbn { get; init; }
        [JsonPropertyName("issn")] public string? Issn { get; init; }
        [JsonPropertyName("publisher")] public string? Publisher { get; init; }
        [JsonPropertyName("publication_date")] public DateOnly? PublicationDate { get; init; }
        [JsonPropertyName("edition")] public string? Edition { get; init; }
        [JsonPropertyName("pages")] public int? Pages { get; init; }
        [JsonPropertyName("language")] public string? Language { get; init; }
        [JsonPropertyName("item_type")] public ItemType? ItemType { get; init; }
        [JsonPropertyName("call_number")] public string? CallNumber { get; init; }
        [JsonPropertyName("classification_system")] public ClassificationSystem? ClassificationSystem { get; init; }
        [JsonPropertyName("collection")] public string? Collection { get; init; }
        [JsonPropertyName("location")] public ItemLocationDto? Location { get; init; }
        [JsonPropertyName("status")] public ItemStatus? Status { get; init; }
        [JsonPropertyName("barcode")] public string? Barcode { get; init; }
        [JsonPropertyName("acquisition_date")] public DateOnly? AcquisitionDate { get; init; }
        [JsonPropertyName("cost")] public decimal? Cost { get; init; }
        [JsonPropertyName("condition_notes")] public string? ConditionNotes { get; init; }
        [JsonPropertyName("description")] public string? Description { get; init; }
        [JsonPropertyName("subjects")] public List<string>? Subjects { get; init; }
        [JsonPropertyName("digital_url")] public Uri? DigitalUrl { get; init; }
    }
}