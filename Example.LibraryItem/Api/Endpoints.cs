using Example.LibraryItem.Application;
using Example.LibraryItem.Application.Handlers;
using Example.LibraryItem.Api.Interfaces;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Api
{
    /// <summary>
    /// Defines minimal API endpoints for library item management operations.
    /// Provides RESTful HTTP endpoints for CRUD operations with proper authentication,
    /// validation, error handling, and OpenAPI documentation.
    /// </summary>
    public static class Endpoints
    {
        /// <summary>
        /// Maps all library item endpoints to the provided route builder.
        /// Creates a versioned API group (/v1/items) with authentication requirements
        /// and comprehensive OpenAPI documentation for each endpoint.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder to configure</param>
        /// <returns>The configured endpoint route builder for method chaining</returns>
        public static IEndpointRouteBuilder MapItemEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/v1/items")
                .WithTags("Items")
                .RequireAuthorization();

            // GET /v1/items - Retrieve paginated list of library items
            // Supports filtering, sorting, and pagination through query parameters
            group.MapGet("", async (
                HttpContext http,
                [AsParameters] ListItemsQuery query,
                IListItemsQueryHandler handler,
                IValidator<ListItemsQuery> validator,
                IEndpointHelpers helpers) =>
            {
                var validation = await validator.ValidateAsync(query);
                if (!validation.IsValid)
                {
                    var details = string.Join("; ", validation.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                    return Results.BadRequest(helpers.CreateBadRequestResponse(http, "Invalid query parameters", details));
                }
                var result = await handler.HandleAsync(query, http.RequestAborted);
                return Results.Ok(result);
            })
            .Produces<ItemListResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponseDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponseDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponseDto>(StatusCodes.Status500InternalServerError);

            // GET /v1/items/{itemId} - Retrieve a specific library item by ID
            // Returns 404 if item doesn't exist, 200 with item data if found
            group.MapGet("/{itemId:guid}", async (
                HttpContext http,
                Guid itemId,
                IGetItemQueryHandler handler,
                IEndpointHelpers helpers) =>
            {
                var basePath = helpers.GetBasePath(http);
                var item = await handler.HandleAsync(itemId, basePath, http.RequestAborted);
                return item is null
                    ? Results.NotFound(helpers.CreateNotFoundResponse(http))
                    : Results.Ok(item);
            })
            .Produces<ItemDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponseDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponseDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponseDto>(StatusCodes.Status500InternalServerError);

            // POST /v1/items - Create a new library item
            // Validates request data, ensures ISBN uniqueness, and returns created item with 201 status
            group.MapPost("/", async (
                HttpContext http,
                ItemCreateRequestDto dto,
                IValidator<ItemCreateRequestDto> validator,
                ICreateItemCommandHandler handler,
                IEndpointHelpers helpers) =>
            {
                await validator.ValidateAndThrowAsync(dto);
                var basePath = helpers.GetBasePath(http);
                var user = helpers.GetCurrentUser(http);
                var created = await handler.HandleAsync(dto, basePath, user, http.RequestAborted);
                return Results.Created($"/v1/items/{created.Id}", created);
            })
            .Produces<ItemDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ValidationErrorResponseDto>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ErrorResponseDto>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponseDto>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponseDto>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponseDto>(StatusCodes.Status500InternalServerError);

            // PUT /v1/items/{itemId} - Replace an existing library item completely
            // Validates request data, ensures ISBN uniqueness, returns 404 if item doesn't exist
            group.MapPut("/{itemId:guid}", async (
                HttpContext http,
                Guid itemId,
                ItemUpdateRequestDto dto,
                IValidator<ItemUpdateRequestDto> validator,
                IUpdateItemCommandHandler handler,
                IEndpointHelpers helpers) =>
            {
                await validator.ValidateAndThrowAsync(dto);
                var basePath = helpers.GetBasePath(http);
                var user = helpers.GetCurrentUser(http);
                var updated = await handler.HandleAsync(itemId, dto, basePath, user, http.RequestAborted);
                return updated is null
                    ? Results.NotFound(helpers.CreateNotFoundResponse(http))
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

            // PATCH /v1/items/{itemId} - Partially update an existing library item
            // Only updates provided fields, validates changes, returns 404 if item doesn't exist
            group.MapPatch("/{itemId:guid}", async (
                HttpContext http,
                Guid itemId,
                ItemPatchRequestDto dto,
                IValidator<ItemPatchRequestDto> validator,
                IPatchItemCommandHandler handler,
                IEndpointHelpers helpers) =>
            {
                await validator.ValidateAndThrowAsync(dto);
                var basePath = helpers.GetBasePath(http);
                var user = helpers.GetCurrentUser(http);
                var updated = await handler.HandleAsync(itemId, dto, basePath, user, http.RequestAborted);
                return updated is null
                    ? Results.NotFound(helpers.CreateNotFoundResponse(http))
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

            // DELETE /v1/items/{itemId} - Remove a library item
            // Returns 204 No Content on success, 404 if item doesn't exist
            group.MapDelete("/{itemId:guid}", async (
                HttpContext http,
                Guid itemId,
                IDeleteItemCommandHandler handler,
                IEndpointHelpers helpers) =>
            {
                var deleted = await handler.HandleAsync(itemId);
                return deleted ? Results.NoContent() : Results.NotFound(helpers.CreateNotFoundResponse(http));
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
