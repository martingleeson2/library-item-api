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
