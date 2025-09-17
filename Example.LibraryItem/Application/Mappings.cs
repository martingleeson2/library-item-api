using Example.LibraryItem.Domain;

namespace Example.LibraryItem.Application
{
    public static class Mappings
    {
        public static ItemDto ToDto(this Item i, string basePath)
            => new()
            {
                id = i.Id,
                title = i.Title,
                subtitle = i.Subtitle,
                author = i.Author,
                contributors = i.Contributors,
                isbn = i.Isbn,
                issn = i.Issn,
                publisher = i.Publisher,
                publication_date = i.PublicationDate,
                edition = i.Edition,
                pages = i.Pages,
                language = i.Language,
                item_type = i.ItemType,
                call_number = i.CallNumber,
                classification_system = i.ClassificationSystem,
                collection = i.Collection,
                location = new ItemLocationDto
                {
                    floor = i.Location.Floor,
                    section = i.Location.Section,
                    shelf_code = i.Location.ShelfCode,
                    wing = i.Location.Wing,
                    position = i.Location.Position,
                    notes = i.Location.Notes
                },
                status = i.Status,
                barcode = i.Barcode,
                acquisition_date = i.AcquisitionDate,
                cost = i.Cost,
                condition_notes = i.ConditionNotes,
                description = i.Description,
                subjects = i.Subjects,
                digital_url = i.DigitalUrl,
                created_at = i.CreatedAt,
                updated_at = i.UpdatedAt,
                created_by = i.CreatedBy,
                updated_by = i.UpdatedBy,
                _links = new Links
                {
                    self = new Link { href = $"{basePath}/v1/items/{i.Id}" }
                }
            };

        public static Item ToEntity(this ItemCreateRequestDto d, DateTime utcNow, string? user)
            => new()
            {
                Id = Guid.NewGuid(),
                Title = d.title,
                Subtitle = d.subtitle,
                Author = d.author,
                Contributors = d.contributors ?? new List<string>(),
                Isbn = d.isbn,
                Issn = d.issn,
                Publisher = d.publisher,
                PublicationDate = d.publication_date,
                Edition = d.edition,
                Pages = d.pages,
                Language = d.language,
                ItemType = d.item_type,
                CallNumber = d.call_number,
                ClassificationSystem = d.classification_system,
                Collection = d.collection,
                Location = ItemLocation.Create(d.location.floor, d.location.section, d.location.shelf_code, d.location.wing, d.location.position, d.location.notes),
                Status = d.status ?? ItemStatus.available,
                Barcode = d.barcode,
                AcquisitionDate = d.acquisition_date,
                Cost = d.cost,
                ConditionNotes = d.condition_notes,
                Description = d.description,
                Subjects = d.subjects ?? new List<string>(),
                DigitalUrl = d.digital_url,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
                CreatedBy = user,
                UpdatedBy = user
            };

        public static void ApplyUpdate(this Item entity, ItemUpdateRequestDto d, DateTime utcNow, string? user)
        {
            entity.Title = d.title;
            entity.Subtitle = d.subtitle;
            entity.Author = d.author;
            entity.Contributors = d.contributors ?? new List<string>();
            entity.Isbn = d.isbn;
            entity.Issn = d.issn;
            entity.Publisher = d.publisher;
            entity.PublicationDate = d.publication_date;
            entity.Edition = d.edition;
            entity.Pages = d.pages;
            entity.Language = d.language;
            entity.ItemType = d.item_type;
            entity.CallNumber = d.call_number;
            entity.ClassificationSystem = d.classification_system;
            entity.Collection = d.collection;
            entity.Location = ItemLocation.Create(d.location.floor, d.location.section, d.location.shelf_code, d.location.wing, d.location.position, d.location.notes);
            entity.Status = d.status;
            entity.Barcode = d.barcode;
            entity.AcquisitionDate = d.acquisition_date;
            entity.Cost = d.cost;
            entity.ConditionNotes = d.condition_notes;
            entity.Description = d.description;
            entity.Subjects = d.subjects ?? new List<string>();
            entity.DigitalUrl = d.digital_url;
            entity.UpdatedAt = utcNow;
            entity.UpdatedBy = user;
        }

        public static void ApplyPatch(this Item entity, ItemPatchRequestDto d, DateTime utcNow, string? user)
        {
            entity.Title = d.title ?? entity.Title;
            entity.Subtitle = d.subtitle ?? entity.Subtitle;
            entity.Author = d.author ?? entity.Author;
            entity.Contributors = d.contributors ?? entity.Contributors;
            entity.Isbn = d.isbn ?? entity.Isbn;
            entity.Issn = d.issn ?? entity.Issn;
            entity.Publisher = d.publisher ?? entity.Publisher;
            entity.PublicationDate = d.publication_date ?? entity.PublicationDate;
            entity.Edition = d.edition ?? entity.Edition;
            entity.Pages = d.pages ?? entity.Pages;
            entity.Language = d.language ?? entity.Language;
            entity.ItemType = d.item_type ?? entity.ItemType;
            entity.CallNumber = d.call_number ?? entity.CallNumber;
            entity.ClassificationSystem = d.classification_system ?? entity.ClassificationSystem;
            entity.Collection = d.collection ?? entity.Collection;
            if (d.location != null)
            {
                entity.Location = ItemLocation.Create(
                    d.location.floor,
                    d.location.section,
                    d.location.shelf_code,
                    d.location.wing,
                    d.location.position,
                    d.location.notes);
            }
            entity.Status = d.status ?? entity.Status;
            entity.Barcode = d.barcode ?? entity.Barcode;
            entity.AcquisitionDate = d.acquisition_date ?? entity.AcquisitionDate;
            entity.Cost = d.cost ?? entity.Cost;
            entity.ConditionNotes = d.condition_notes ?? entity.ConditionNotes;
            entity.Description = d.description ?? entity.Description;
            entity.Subjects = d.subjects ?? entity.Subjects;
            entity.DigitalUrl = d.digital_url ?? entity.DigitalUrl;
            entity.UpdatedAt = utcNow;
            entity.UpdatedBy = user;
        }
    }
}
