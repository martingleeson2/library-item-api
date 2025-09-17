using Example.LibraryItem.Application;

namespace Example.LibraryItem.Application.Handlers
{
    /// <summary>
    /// Handles listing library items with filtering, sorting, and pagination.
    /// This query handler supports various filtering options such as title, author, ISBN,
    /// item type, status, collection, location details, call number, and publication year ranges.
    /// Results are returned with pagination metadata and HATEOAS links.
    /// </summary>
    public interface IListItemsQueryHandler : IQueryHandler<ListItemsQuery, ItemListResponseDto>
    {
        // Inherits HandleAsync from IQueryHandler<ListItemsQuery, ItemListResponseDto>
    }
}