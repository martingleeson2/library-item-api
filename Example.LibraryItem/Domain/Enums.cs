namespace Example.LibraryItem.Domain
{
    public enum ItemType
    {
        book,
        periodical,
        journal,
        magazine,
        newspaper,
        dvd,
        cd,
        audiobook,
        ebook,
        map,
        manuscript,
        thesis,
        government_document,
        reference,
        microfilm,
        microform,
        digital_resource
    }

    public enum ItemStatus
    {
        available,
        checked_out,
        reserved,
        in_processing,
        damaged,
        missing,
        withdrawn,
        on_hold,
        in_transit,
        reference_only
    }

    public enum ClassificationSystem
    {
        dewey_decimal,
        library_of_congress,
        sudoc,
        local
    }
}
