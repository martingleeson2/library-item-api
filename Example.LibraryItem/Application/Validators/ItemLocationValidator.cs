using FluentValidation;
using Example.LibraryItem.Domain;

namespace Example.LibraryItem.Application
{
    /// <summary>
    /// Validator for ItemLocationDto following standardized DTO validation patterns
    /// </summary>
    public class ItemLocationValidator : AbstractValidator<ItemLocationDto>
    {
        public ItemLocationValidator()
        {
            // Required fields with validation
            RuleFor(x => x.Floor)
                .InclusiveBetween(-2, 20)
                .WithMessage("Floor must be between -2 and 20");

            RuleFor(x => x.Section)
                .NotEmpty()
                .WithMessage("Section is required")
                .MaximumLength(10)
                .WithMessage("Section cannot exceed 10 characters");

            RuleFor(x => x.ShelfCode)
                .NotEmpty()
                .WithMessage("Shelf code is required")
                .MaximumLength(20)
                .WithMessage("Shelf code cannot exceed 20 characters");

            // Optional fields with validation when provided
            RuleFor(x => x.Wing)
                .MaximumLength(20)
                .WithMessage("Wing cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.Wing));

            RuleFor(x => x.Position)
                .MaximumLength(10)
                .WithMessage("Position cannot exceed 10 characters")
                .When(x => !string.IsNullOrEmpty(x.Position));

            RuleFor(x => x.Notes)
                .MaximumLength(255)
                .WithMessage("Location notes cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}
