using System.ComponentModel.DataAnnotations;

namespace Example.LibraryItem.Domain
{
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
        public static ItemLocation Create(int floor, string section, string shelfCode, string? wing = null, string? position = null, string? notes = null)
            => new(floor, section, shelfCode, wing, position, notes);
    }

    public class Item
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(500)]
        public required string Title { get; set; }
        [MaxLength(500)]
        public string? Subtitle { get; set; }

        [MaxLength(255)]
        public string? Author { get; set; }

        public List<string> Contributors { get; set; } = new();

        [RegularExpression("^(?:978|979)?[0-9]{9}[0-9X]$")]
        public string? Isbn { get; set; }

        [RegularExpression("^[0-9]{4}-[0-9]{3}[0-9X]$")]
        public string? Issn { get; set; }

        [MaxLength(255)]
        public string? Publisher { get; set; }

        public DateOnly? PublicationDate { get; set; }

        [MaxLength(50)]
        public string? Edition { get; set; }

        public int? Pages { get; set; }

        [MaxLength(10)]
        public string? Language { get; set; }

        public ItemType ItemType { get; set; }

        [MaxLength(50)]
        public required string CallNumber { get; set; }

        public ClassificationSystem ClassificationSystem { get; set; }

        [MaxLength(100)]
        public string? Collection { get; set; }

        public required ItemLocation Location { get; set; }

        public ItemStatus Status { get; set; } = ItemStatus.available;

        [MaxLength(50)]
        public string? Barcode { get; set; }

        public DateOnly? AcquisitionDate { get; set; }

        public decimal? Cost { get; set; }

        [MaxLength(1000)]
        public string? ConditionNotes { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public List<string> Subjects { get; set; } = new();

        public Uri? DigitalUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }
}
