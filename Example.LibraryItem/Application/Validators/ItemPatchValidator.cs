using FluentValidation;
using Example.LibraryItem.Domain;

namespace Example.LibraryItem.Application
{
    /// <summary>
    /// Validator for ItemPatchRequestDto following standardized DTO validation patterns
    /// Note: All fields are optional for PATCH operations, but must be valid when provided
    /// </summary>
    public class ItemPatchValidator : AbstractValidator<ItemPatchRequestDto>
    {
        public ItemPatchValidator()
        {
            // Optional fields with validation when provided
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title cannot be empty when provided")
                .MaximumLength(500)
                .WithMessage("Title cannot exceed 500 characters")
                .When(x => x.Title != null);

            RuleFor(x => x.Subtitle)
                .MaximumLength(500)
                .WithMessage("Subtitle cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Subtitle));

            RuleFor(x => x.Author)
                .MaximumLength(255)
                .WithMessage("Author name cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.Author));

            RuleFor(x => x.Isbn)
                .Matches(@"^(?:978|979)?[0-9]{9}[0-9X]$")
                .WithMessage("ISBN format is invalid. Expected format: 9780743273565 or 978074327356X")
                .When(x => !string.IsNullOrEmpty(x.Isbn));

            RuleFor(x => x.Issn)
                .Matches(@"^[0-9]{4}-[0-9]{3}[0-9X]$")
                .WithMessage("ISSN format is invalid. Expected format: 1234-567X")
                .When(x => !string.IsNullOrEmpty(x.Issn));

            RuleFor(x => x.Publisher)
                .MaximumLength(255)
                .WithMessage("Publisher name cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.Publisher));

            RuleFor(x => x.Edition)
                .MaximumLength(50)
                .WithMessage("Edition cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.Edition));

            RuleFor(x => x.Pages)
                .GreaterThan(0)
                .WithMessage("Pages must be greater than 0")
                .When(x => x.Pages.HasValue);

            RuleFor(x => x.Language)
                .MaximumLength(10)
                .WithMessage("Language code cannot exceed 10 characters")
                .When(x => !string.IsNullOrEmpty(x.Language));

            RuleFor(x => x.Collection)
                .MaximumLength(100)
                .WithMessage("Collection name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Collection));

            RuleFor(x => x.CallNumber)
                .NotEmpty()
                .WithMessage("Call number cannot be empty when provided")
                .MaximumLength(50)
                .WithMessage("Call number cannot exceed 50 characters")
                .When(x => x.CallNumber != null);

            RuleFor(x => x.Barcode)
                .MaximumLength(50)
                .WithMessage("Barcode cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.Barcode));

            RuleFor(x => x.Cost)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Cost must be greater than or equal to 0")
                .When(x => x.Cost.HasValue);

            RuleFor(x => x.ConditionNotes)
                .MaximumLength(1000)
                .WithMessage("Condition notes cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.ConditionNotes));

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Description cannot exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.PublicationDate)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddYears(10)))
                .WithMessage("Publication date cannot be more than 10 years in the future")
                .When(x => x.PublicationDate.HasValue);

            RuleFor(x => x.AcquisitionDate)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Acquisition date cannot be in the future")
                .When(x => x.AcquisitionDate.HasValue);

            // Enum validations
            RuleFor(x => x.ItemType)
                .IsInEnum()
                .WithMessage("Invalid item type. Valid values are: book, periodical, dvd, cd, manuscript, digital_resource")
                .When(x => x.ItemType.HasValue);

            RuleFor(x => x.ClassificationSystem)
                .IsInEnum()
                .WithMessage("Invalid classification system. Valid values are: dewey_decimal, library_of_congress, other")
                .When(x => x.ClassificationSystem.HasValue);

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid status. Valid values are: available, checked_out, reserved, maintenance, lost, damaged")
                .When(x => x.Status.HasValue);

            // Nested object validation
            When(x => x.Location != null, () =>
            {
                RuleFor(x => x.Location!).SetValidator(new ItemLocationValidator());
            });

            // Collection validation
            RuleFor(x => x.Contributors)
                .Must(list => list == null || list.Count <= 20)
                .WithMessage("Cannot have more than 20 contributors")
                .When(x => x.Contributors != null);

            RuleFor(x => x.Subjects)
                .Must(list => list == null || list.Count <= 50)
                .WithMessage("Cannot have more than 50 subject tags")
                .When(x => x.Subjects != null);

            // Digital URL validation
            RuleFor(x => x.DigitalUrl)
                .Must(uri => uri == null || (uri.IsAbsoluteUri && (uri.Scheme == "http" || uri.Scheme == "https")))
                .WithMessage("Digital URL must be a valid HTTP or HTTPS URL")
                .When(x => x.DigitalUrl != null);
        }
    }
}
