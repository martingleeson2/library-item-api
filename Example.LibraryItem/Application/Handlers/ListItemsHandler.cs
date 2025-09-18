using Example.LibraryItem.Application;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Application.Handlers
{
    public class ListItemsHandler(LibraryDbContext db, ILogger<ListItemsHandler> logger) : IListItemsQueryHandler
    {
        public async Task<ItemListResponseDto> HandleAsync(ListItemsQuery q, CancellationToken ct = default)
        {
            logger.LogInformation("Listing items with filters: {@Filters}", q);
            
            var query = db.Items.AsNoTracking().AsQueryable();
            query = ApplyFilters(query, q);
            query = ApplySort(query, q.SortBy, q.SortOrder);

            var totalItems = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalItems / (double)q.Limit);
            var items = await query.Skip((q.Page - 1) * q.Limit).Take(q.Limit).ToListAsync(ct);
            
            var basePath = string.Empty; // Actual base path will be added in endpoint composition
            var data = items.Select(i => i.ToDto(basePath)).ToList();
            
            var response = new ItemListResponseDto
            {
                Data = data,
                Pagination = CreatePaginationDto(q, totalItems, totalPages)
            };
            
            logger.LogInformation("Listed {Count} of {Total} items on page {Page}", data.Count, totalItems, q.Page);
            return response;
        }

        private static IQueryable<Item> ApplyFilters(IQueryable<Item> query, ListItemsQuery q)
        {
            if (!string.IsNullOrWhiteSpace(q.Title)) 
                query = query.Where(i => i.Title.Contains(q.Title));
                
            if (!string.IsNullOrWhiteSpace(q.Author)) 
                query = query.Where(i => i.Author != null && i.Author.Contains(q.Author));
                
            if (!string.IsNullOrWhiteSpace(q.Isbn)) 
                query = query.Where(i => i.Isbn == q.Isbn);
                
            if (q.ItemType.HasValue) 
                query = query.Where(i => i.ItemType == q.ItemType);
                
            if (q.Status.HasValue) 
                query = query.Where(i => i.Status == q.Status);
                
            if (!string.IsNullOrWhiteSpace(q.Collection)) 
                query = query.Where(i => i.Collection == q.Collection);
                
            if (q.LocationFloor.HasValue) 
                query = query.Where(i => i.Location.Floor == q.LocationFloor);
                
            if (!string.IsNullOrWhiteSpace(q.LocationSection)) 
                query = query.Where(i => i.Location.Section == q.LocationSection);
                
            if (!string.IsNullOrWhiteSpace(q.CallNumber)) 
                query = query.Where(i => i.CallNumber.Contains(q.CallNumber));
                
            if (q.PublicationYearFrom.HasValue) 
                query = query.Where(i => i.PublicationDate != null && i.PublicationDate.Value.Year >= q.PublicationYearFrom);
                
            if (q.PublicationYearTo.HasValue) 
                query = query.Where(i => i.PublicationDate != null && i.PublicationDate.Value.Year <= q.PublicationYearTo);

            return query;
        }

        private static IQueryable<Item> ApplySort(IQueryable<Item> query, string? sortBy, string? sortOrder)
        {
            return (sortBy, sortOrder) switch
            {
                ("title", "desc") => query.OrderByDescending(i => i.Title),
                ("title", _) => query.OrderBy(i => i.Title),
                ("author", "desc") => query.OrderByDescending(i => i.Author),
                ("author", _) => query.OrderBy(i => i.Author),
                ("publication_date", "desc") => query.OrderByDescending(i => i.PublicationDate),
                ("publication_date", _) => query.OrderBy(i => i.PublicationDate),
                ("call_number", "desc") => query.OrderByDescending(i => i.CallNumber),
                ("call_number", _) => query.OrderBy(i => i.CallNumber),
                ("created_at", "desc") => query.OrderByDescending(i => i.CreatedAt),
                ("created_at", _) => query.OrderBy(i => i.CreatedAt),
                ("updated_at", "desc") => query.OrderByDescending(i => i.UpdatedAt),
                ("updated_at", _) => query.OrderBy(i => i.UpdatedAt),
                _ => query.OrderBy(i => i.Title)
            };
        }

        private static PaginationDto CreatePaginationDto(ListItemsQuery q, int totalItems, int totalPages)
        {
            return new PaginationDto
            {
                Page = q.Page,
                Limit = q.Limit,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasNext = q.Page < totalPages,
                HasPrevious = q.Page > 1
            };
        }
    }
}
