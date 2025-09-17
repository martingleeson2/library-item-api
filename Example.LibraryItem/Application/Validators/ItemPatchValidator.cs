using FluentValidation;
using Example.LibraryItem.Domain;

namespace Example.LibraryItem.Application
{
    public class ItemPatchValidator : AbstractValidator<ItemPatchRequestDto>
    {
        public ItemPatchValidator()
        {
            RuleFor(x => x.title).MaximumLength(500).When(x => x.title != null);
            RuleFor(x => x.item_type).IsInEnum().When(x => x.item_type.HasValue);
            RuleFor(x => x.call_number).MaximumLength(50).When(x => x.call_number != null);
            RuleFor(x => x.classification_system).IsInEnum().When(x => x.classification_system.HasValue);
            When(x => x.location != null, () =>
            {
                RuleFor(x => x.location!).SetValidator(new ItemLocationValidator());
            });
            RuleFor(x => x.isbn).Matches("^(?:978|979)?[0-9]{9}[0-9X]$").When(x => !string.IsNullOrEmpty(x.isbn));
            RuleFor(x => x.issn).Matches("^[0-9]{4}-[0-9]{3}[0-9X]$").When(x => !string.IsNullOrEmpty(x.issn));
            RuleFor(x => x.publisher).MaximumLength(255).When(x => x.publisher != null);
            RuleFor(x => x.edition).MaximumLength(50).When(x => x.edition != null);
            RuleFor(x => x.language).MaximumLength(10).When(x => x.language != null);
            RuleFor(x => x.collection).MaximumLength(100).When(x => x.collection != null);
            RuleFor(x => x.barcode).MaximumLength(50).When(x => x.barcode != null);
            RuleFor(x => x.condition_notes).MaximumLength(1000).When(x => x.condition_notes != null);
            RuleFor(x => x.description).MaximumLength(2000).When(x => x.description != null);
        }
    }
}
