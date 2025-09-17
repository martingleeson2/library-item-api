using Example.LibraryItem.Domain;

namespace Example.LibraryItem.Application
{
    public static class Mappings
    {
        public static ItemDto ToDto(this Item i, string basePath)
            => new()
            {
                Id = i.Id,
                Title = i.Title,
                Subtitle = i.Subtitle,
                Author = i.Author,
                Contributors = i.Contributors,
                Isbn = i.Isbn,
                Issn = i.Issn,
                Publisher = i.Publisher,
                PublicationDate = i.PublicationDate,
                Edition = i.Edition,
                Pages = i.Pages,
                Language = i.Language,
                ItemType = i.ItemType,
                CallNumber = i.CallNumber,
                ClassificationSystem = i.ClassificationSystem,
                Collection = i.Collection,
                Location = i.Location.ToDto(),
                Status = i.Status,
                Barcode = i.Barcode,
                AcquisitionDate = i.AcquisitionDate,
                Cost = i.Cost,
                ConditionNotes = i.ConditionNotes,
                Description = i.Description,
                Subjects = i.Subjects,
                DigitalUrl = i.DigitalUrl,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                CreatedBy = i.CreatedBy,
                UpdatedBy = i.UpdatedBy
            };

        public static ItemLocationDto ToDto(this ItemLocation location)
            => new()
            {
                Floor = location.Floor,
                Section = location.Section,
                ShelfCode = location.ShelfCode,
                Wing = location.Wing,
                Position = location.Position,
                Notes = location.Notes
            };

        public static ItemLocation ToEntity(this ItemLocationDto dto)
            => ItemLocation.Create(dto.Floor, dto.Section, dto.ShelfCode, dto.Wing, dto.Position, dto.Notes);

        public static Item ToEntity(this ItemCreateRequestDto d, DateTime utcNow, string? user)
            => new()
            {
                Id = Guid.NewGuid(),
                Title = d.Title,
                Subtitle = d.Subtitle,
                Author = d.Author,
                Contributors = d.Contributors ?? [],
                Isbn = d.Isbn,
                Issn = d.Issn,
                Publisher = d.Publisher,
                PublicationDate = d.PublicationDate,
                Edition = d.Edition,
                Pages = d.Pages,
                Language = d.Language,
                ItemType = d.ItemType,
                CallNumber = d.CallNumber,
                ClassificationSystem = d.ClassificationSystem,
                Collection = d.Collection,
                Location = d.Location.ToEntity(),
                Status = d.Status ?? ItemStatus.available,
                Barcode = d.Barcode,
                AcquisitionDate = d.AcquisitionDate,
                Cost = d.Cost,
                ConditionNotes = d.ConditionNotes,
                Description = d.Description,
                Subjects = d.Subjects ?? [],
                DigitalUrl = d.DigitalUrl,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
                CreatedBy = user,
                UpdatedBy = user
            };

        public static void ApplyUpdate(this Item entity, ItemUpdateRequestDto d, DateTime utcNow, string? user)
        {
            var data = new ItemUpdateData(
                d.Title, d.Subtitle, d.Author, d.Contributors, d.Isbn, d.Issn,
                d.Publisher, d.PublicationDate, d.Edition, d.Pages, d.Language,
                d.ItemType, d.CallNumber, d.ClassificationSystem, d.Collection,
                d.Location?.ToEntity(), d.Status, d.Barcode, d.AcquisitionDate,
                d.Cost, d.ConditionNotes, d.Description, d.Subjects, d.DigitalUrl
            );
            ApplyFullUpdate(entity, data, utcNow, user);
        }

        public static void ApplyPatch(this Item entity, ItemPatchRequestDto d, DateTime utcNow, string? user)
        {
            var data = new ItemPartialUpdateData(
                d.Title, d.Subtitle, d.Author, d.Contributors, d.Isbn, d.Issn,
                d.Publisher, d.PublicationDate, d.Edition, d.Pages, d.Language,
                d.ItemType, d.CallNumber, d.ClassificationSystem, d.Collection,
                d.Location?.ToEntity(), d.Status, d.Barcode, d.AcquisitionDate,
                d.Cost, d.ConditionNotes, d.Description, d.Subjects, d.DigitalUrl
            );
            ApplyPartialUpdate(entity, data, utcNow, user);
        }
        private static void ApplyFullUpdate(Item entity, ItemUpdateData d, DateTime utcNow, string? user)
        {
            entity.Title = d.Title;
            entity.Subtitle = d.Subtitle;
            entity.Author = d.Author;
            entity.Contributors = d.Contributors ?? [];
            entity.Isbn = d.Isbn;
            entity.Issn = d.Issn;
            entity.Publisher = d.Publisher;
            entity.PublicationDate = d.PublicationDate;
            entity.Edition = d.Edition;
            entity.Pages = d.Pages;
            entity.Language = d.Language;
            entity.ItemType = d.ItemType;
            entity.CallNumber = d.CallNumber;
            entity.ClassificationSystem = d.ClassificationSystem;
            entity.Collection = d.Collection;
            if (d.Location != null) entity.Location = d.Location;
            entity.Status = d.Status;
            entity.Barcode = d.Barcode;
            entity.AcquisitionDate = d.AcquisitionDate;
            entity.Cost = d.Cost;
            entity.ConditionNotes = d.ConditionNotes;
            entity.Description = d.Description;
            entity.Subjects = d.Subjects ?? [];
            entity.DigitalUrl = d.DigitalUrl;
            entity.UpdatedAt = utcNow;
            entity.UpdatedBy = user;
        }

        private static void ApplyPartialUpdate(Item entity, ItemPartialUpdateData d, DateTime utcNow, string? user)
        {
            if (d.Title != null) entity.Title = d.Title;
            if (d.Subtitle != null) entity.Subtitle = d.Subtitle;
            if (d.Author != null) entity.Author = d.Author;
            if (d.Contributors != null) entity.Contributors = d.Contributors;
            if (d.Isbn != null) entity.Isbn = d.Isbn;
            if (d.Issn != null) entity.Issn = d.Issn;
            if (d.Publisher != null) entity.Publisher = d.Publisher;
            if (d.PublicationDate != null) entity.PublicationDate = d.PublicationDate;
            if (d.Edition != null) entity.Edition = d.Edition;
            if (d.Pages != null) entity.Pages = d.Pages;
            if (d.Language != null) entity.Language = d.Language;
            if (d.ItemType != null) entity.ItemType = d.ItemType.Value;
            if (d.CallNumber != null) entity.CallNumber = d.CallNumber;
            if (d.ClassificationSystem != null) entity.ClassificationSystem = d.ClassificationSystem.Value;
            if (d.Collection != null) entity.Collection = d.Collection;
            if (d.Location != null) entity.Location = d.Location;
            if (d.Status != null) entity.Status = d.Status.Value;
            if (d.Barcode != null) entity.Barcode = d.Barcode;
            if (d.AcquisitionDate != null) entity.AcquisitionDate = d.AcquisitionDate;
            if (d.Cost != null) entity.Cost = d.Cost;
            if (d.ConditionNotes != null) entity.ConditionNotes = d.ConditionNotes;
            if (d.Description != null) entity.Description = d.Description;
            if (d.Subjects != null) entity.Subjects = d.Subjects;
            if (d.DigitalUrl != null) entity.DigitalUrl = d.DigitalUrl;
            entity.UpdatedAt = utcNow;
            entity.UpdatedBy = user;
        }

        private readonly record struct ItemUpdateData(
            string Title,
            string? Subtitle,
            string? Author,
            List<string>? Contributors,
            string? Isbn,
            string? Issn,
            string? Publisher,
            DateOnly? PublicationDate,
            string? Edition,
            int? Pages,
            string? Language,
            ItemType ItemType,
            string CallNumber,
            ClassificationSystem ClassificationSystem,
            string? Collection,
            ItemLocation? Location,
            ItemStatus Status,
            string? Barcode,
            DateOnly? AcquisitionDate,
            decimal? Cost,
            string? ConditionNotes,
            string? Description,
            List<string>? Subjects,
            Uri? DigitalUrl
        );

        private readonly record struct ItemPartialUpdateData(
            string? Title,
            string? Subtitle,
            string? Author,
            List<string>? Contributors,
            string? Isbn,
            string? Issn,
            string? Publisher,
            DateOnly? PublicationDate,
            string? Edition,
            int? Pages,
            string? Language,
            ItemType? ItemType,
            string? CallNumber,
            ClassificationSystem? ClassificationSystem,
            string? Collection,
            ItemLocation? Location,
            ItemStatus? Status,
            string? Barcode,
            DateOnly? AcquisitionDate,
            decimal? Cost,
            string? ConditionNotes,
            string? Description,
            List<string>? Subjects,
            Uri? DigitalUrl
        );
    }
}
