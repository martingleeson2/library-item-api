using Example.LibraryItem.Application;
using Example.LibraryItem.Application.Handlers;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Api
{
    public static class Endpoints
    {
        public static IEndpointRouteBuilder MapItemEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/v1/items")
                .WithTags("Items")
                .RequireAuthorization();

            group.MapGet("", async (
                HttpContext http,
                [AsParameters] ListItemsQuery query,
                IListItemsQueryHandler handler,
                IValidator<ListItemsQuery> validator) =>
            {
                var validation = await validator.ValidateAsync(query);
                if (!validation.IsValid)
                {
                    return Results.BadRequest(new ErrorResponseDto
                    {
                        error = "BAD_REQUEST",
                        message = "Invalid query parameters",
                        details = string.Join("; ", validation.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")),
                        timestamp = DateTime.UtcNow,
                        request_id = Guid.NewGuid(),
                        path = http.Request.Path
                    });
                }
                var result = await handler.HandleAsync(query, http.RequestAborted);
                var basePath = $"{http.Request.Scheme}://{http.Request.Host}";
                var newLinks = new Links
                {
                    self = new Link { href = $"/v1/items?page={query.page}&limit={query.limit}" },
                    next = result.pagination.has_next ? new Link { href = $"/v1/items?page={query.page + 1}&limit={query.limit}" } : null,
                    previous = result.pagination.has_previous ? new Link { href = $"/v1/items?page={query.page - 1}&limit={query.limit}" } : null,
                    first = new Link { href = $"/v1/items?page=1&limit={query.limit}" },
                    last = new Link { href = $"/v1/items?page={result.pagination.total_pages}&limit={query.limit}" }
                };
                var newData = result.data.Select(d => d with { _links = new Links { self = new Link { href = $"/v1/items/{d.id}" } } }).ToList();
                var shaped = new ItemListResponseDto
                {
                    data = newData,
                    pagination = result.pagination,
                    _links = newLinks
                };
                return Results.Ok(shaped);
            })
            .Produces<ItemListResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponseDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponseDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponseDto>(StatusCodes.Status500InternalServerError);

            group.MapGet("/{itemId:guid}", async (
                HttpContext http,
                Guid itemId,
                IGetItemQueryHandler handler) =>
            {
                var basePath = $"{http.Request.Scheme}://{http.Request.Host}";
                var item = await handler.HandleAsync(itemId, basePath, http.RequestAborted);
                return item is null
                    ? Results.NotFound(new ErrorResponseDto
                    {
                        error = "ITEM_NOT_FOUND",
                        message = "The requested library item could not be found",
                        timestamp = DateTime.UtcNow,
                        request_id = Guid.NewGuid(),
                        path = http.Request.Path
                    })
                    : Results.Ok(item);
            })
            .Produces<ItemDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponseDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponseDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponseDto>(StatusCodes.Status500InternalServerError);

            group.MapPost("/", async (
                HttpContext http,
                ItemCreateRequestDto dto,
                IValidator<ItemCreateRequestDto> validator,
                ICreateItemCommandHandler handler) =>
            {
                await validator.ValidateAndThrowAsync(dto);
                var basePath = $"{http.Request.Scheme}://{http.Request.Host}";
                var user = http.User.Identity?.Name;
                var created = await handler.HandleAsync(dto, basePath, user, http.RequestAborted);
                return Results.Created($"/v1/items/{created.id}", created);
            })
            .Produces<ItemDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ValidationErrorResponseDto>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ErrorResponseDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponseDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponseDto>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponseDto>(StatusCodes.Status500InternalServerError);

            group.MapPut("/{itemId:guid}", async (
                HttpContext http,
                Guid itemId,
                ItemUpdateRequestDto dto,
                IValidator<ItemUpdateRequestDto> validator,
                IUpdateItemCommandHandler handler) =>
            {
                await validator.ValidateAndThrowAsync(dto);
                var basePath = $"{http.Request.Scheme}://{http.Request.Host}";
                var user = http.User.Identity?.Name;
                var updated = await handler.HandleAsync(itemId, dto, basePath, user, http.RequestAborted);
                return updated is null
                    ? Results.NotFound(new ErrorResponseDto
                    {
                        error = "ITEM_NOT_FOUND",
                        message = "The requested library item could not be found",
                        timestamp = DateTime.UtcNow,
                        request_id = Guid.NewGuid(),
                        path = http.Request.Path
                    })
                    : Results.Ok(updated);
            })
            .Produces<ItemDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ValidationErrorResponseDto>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ErrorResponseDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponseDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponseDto>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponseDto>(StatusCodes.Status500InternalServerError);

            group.MapPatch("/{itemId:guid}", async (
                HttpContext http,
                Guid itemId,
                ItemPatchRequestDto dto,
                IValidator<ItemPatchRequestDto> validator,
                IPatchItemCommandHandler handler) =>
            {
                await validator.ValidateAndThrowAsync(dto);
                var basePath = $"{http.Request.Scheme}://{http.Request.Host}";
                var user = http.User.Identity?.Name;
                var updated = await handler.HandleAsync(itemId, dto, basePath, user, http.RequestAborted);
                return updated is null
                    ? Results.NotFound(new ErrorResponseDto
                    {
                        error = "ITEM_NOT_FOUND",
                        message = "The requested library item could not be found",
                        timestamp = DateTime.UtcNow,
                        request_id = Guid.NewGuid(),
                        path = http.Request.Path
                    })
                    : Results.Ok(updated);
            })
            .Produces<ItemDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ValidationErrorResponseDto>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ErrorResponseDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponseDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponseDto>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponseDto>(StatusCodes.Status500InternalServerError);

            group.MapDelete("/{itemId:guid}", async (
                Guid itemId,
                IDeleteItemCommandHandler handler) =>
            {
                var deleted = await handler.HandleAsync(itemId);
                return deleted ? Results.NoContent() : Results.NotFound(new ErrorResponseDto
                {
                    error = "ITEM_NOT_FOUND",
                    message = "The requested library item could not be found",
                    timestamp = DateTime.UtcNow,
                    request_id = Guid.NewGuid()
                });
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponseDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponseDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponseDto>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponseDto>(StatusCodes.Status500InternalServerError);

            return endpoints;
        }
    }
}
