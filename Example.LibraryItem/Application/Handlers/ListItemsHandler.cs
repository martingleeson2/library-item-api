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
            query = ApplySort(query, q.sort_by, q.sort_order);

            var totalItems = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalItems / (double)q.limit);
            var items = await query.Skip((q.page - 1) * q.limit).Take(q.limit).ToListAsync(ct);
            
            var basePath = string.Empty; // Actual base path will be added in endpoint composition
            var data = items.Select(i => i.ToDto(basePath)).ToList();
            
            var response = new ItemListResponseDto
            {
                Data = data,
                Pagination = CreatePaginationDto(q, totalItems, totalPages)
            };
            
            logger.LogInformation("Listed {Count} of {Total} items on page {Page}", data.Count, totalItems, q.page);
            return response;
        }

        private static IQueryable<Item> ApplyFilters(IQueryable<Item> query, ListItemsQuery q)
        {
            if (!string.IsNullOrWhiteSpace(q.title)) 
                query = query.Where(i => i.Title.Contains(q.title));
                
            if (!string.IsNullOrWhiteSpace(q.author)) 
                query = query.Where(i => i.Author != null && i.Author.Contains(q.author));
                
            if (!string.IsNullOrWhiteSpace(q.isbn)) 
                query = query.Where(i => i.Isbn == q.isbn);
                
            if (q.item_type.HasValue) 
                query = query.Where(i => i.ItemType == q.item_type);
                
            if (q.status.HasValue) 
                query = query.Where(i => i.Status == q.status);
                
            if (!string.IsNullOrWhiteSpace(q.collection)) 
                query = query.Where(i => i.Collection == q.collection);
                
            if (q.location_floor.HasValue) 
                query = query.Where(i => i.Location.Floor == q.location_floor);
                
            if (!string.IsNullOrWhiteSpace(q.location_section)) 
                query = query.Where(i => i.Location.Section == q.location_section);
                
            if (!string.IsNullOrWhiteSpace(q.call_number)) 
                query = query.Where(i => i.CallNumber.Contains(q.call_number));
                
            if (q.publication_year_from.HasValue) 
                query = query.Where(i => i.PublicationDate != null && i.PublicationDate.Value.Year >= q.publication_year_from);
                
            if (q.publication_year_to.HasValue) 
                query = query.Where(i => i.PublicationDate != null && i.PublicationDate.Value.Year <= q.publication_year_to);

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
                Page = q.page,
                Limit = q.limit,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasNext = q.page < totalPages,
                HasPrevious = q.page > 1
            };
        }
    }
}
