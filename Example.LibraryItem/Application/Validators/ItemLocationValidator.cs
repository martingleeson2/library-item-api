using FluentValidation;
using Example.LibraryItem.Domain;

namespace Example.LibraryItem.Application
{
    public class ItemLocationValidator : AbstractValidator<ItemLocationDto>
    {
        public ItemLocationValidator()
        {
            RuleFor(x => x.floor).InclusiveBetween(-2, 20);
            RuleFor(x => x.section).NotEmpty().MaximumLength(10);
            RuleFor(x => x.shelf_code).NotEmpty().MaximumLength(20);
            RuleFor(x => x.wing).MaximumLength(20).When(x => x.wing != null);
            RuleFor(x => x.position).MaximumLength(10).When(x => x.position != null);
            RuleFor(x => x.notes).MaximumLength(255).When(x => x.notes != null);
        }
    }
}
