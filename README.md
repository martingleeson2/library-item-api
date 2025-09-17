# Example.LibraryItem — .NET 8 Minimal API for Library Items

One-stop CRUD and query API for library items with API-key auth, rigorous validation, predictable error handling, and clean CQRS.

## Badges

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![Build](https://img.shields.io/badge/build-GitHub_Actions-blue)
![Coverage](https://img.shields.io/badge/coverage-Coverlet_opencover-brightgreen)
![License](https://img.shields.io/badge/license-MIT-lightgrey)

## Features

- CRUD for library items with DTO-first contracts
- Rich search, filters, sorting, and pagination
- API key authentication (scheme: `ApiKey`, header: `X-API-Key`)
- FluentValidation with 400/422 responses
- Centralized errors with correlation (`X-Request-ID`), 404/409/499 supported
- EF Core with Sqlite or InMemory providers, value converters for lists, `Uri`, `DateOnly`
- Clean layers with CQRS handlers and mapping helpers
- OpenAPI via Swashbuckle

## Architecture Overview

- API: endpoints, middleware, auth → `Example.LibraryItem/Api/*`
- Application: CQRS handlers, DTOs, validators, mappings → `Example.LibraryItem/Application/*`
- Domain: entities, enums, value objects → `Example.LibraryItem/Domain/*`
- Infrastructure: EF Core `LibraryDbContext` & persistence → `Example.LibraryItem/Infrastructure/*`
- Patterns: CQRS handlers (`IListItemsHandler`, `IGetItemHandler`, `ICreateItemHandler`, `IUpdateItemHandler`, `IPatchItemHandler`, `IDeleteItemHandler`), DTO-first, validators, centralized mappings, global middleware

## Project Structure

```
Example.sln
Example.LibraryItem/
  Program.cs
  Api/
    Auth.cs
    Endpoints.cs
    ErrorHandling.cs
  Application/
    Dtos.cs
    Mappings.cs
    Queries.cs
    CreateItemHandler.cs
    Handlers/
      Interfaces.cs
      ListItemsHandler.cs
      GetItemHandler.cs
      UpdateItemHandler.cs
      PatchItemHandler.cs
      DeleteItemHandler.cs
    Validators/
      ItemCreateValidator.cs
      ItemUpdateValidator.cs
      ItemPatchValidator.cs
      ItemLocationValidator.cs
  Domain/
    Entities.cs
    Enums.cs
  Infrastructure/
    LibraryDbContext.cs
  appsettings.json
  appsettings.Development.json
Example.LibraryItem.Tests/
  Handlers/*, Validators/*, Dtos/*
Directory.Build.props
.github/copilot-instructions.md
.vscode/
```

## Getting Started

### Prerequisites

- .NET 8 SDK

Pinned NuGet dependencies:

- Microsoft.EntityFrameworkCore: 9.0.0
- Microsoft.EntityFrameworkCore.Design: 9.0.0
- Microsoft.EntityFrameworkCore.Sqlite: 9.0.0
- Microsoft.EntityFrameworkCore.InMemory: 9.0.0 (tests/dev only)
- Swashbuckle.AspNetCore: 6.6.2
- FluentValidation: 11.9.0
- FluentValidation.DependencyInjectionExtensions: 11.9.0
- FluentValidation.AspNetCore: 11.3.0

### Restore, build, run

```bash
dotnet restore
dotnet build
dotnet run --project Example.LibraryItem/Example.LibraryItem.csproj
```

PowerShell:
```powershell
dotnet restore; dotnet build; dotnet run --project Example.LibraryItem/Example.LibraryItem.csproj
```

### Run with watch

```bash
dotnet watch --project Example.LibraryItem/Example.LibraryItem.csproj run
```

## Configuration

- Keys in `appsettings*.json`:
  - `Database:Provider`: `"sqlite"` (default) or `"inmemory"`
  - `ConnectionStrings:Default`: Sqlite connection (e.g., `"Data Source=library.db"`)
- Environments: set `ASPNETCORE_ENVIRONMENT` to `Development` for Swagger and HTTP logging.
- API key auth: scheme `ApiKey`; header name `X-API-Key` (see [`Api/Auth.cs`](Example.LibraryItem/Api/Auth.cs) and [`Program.cs`](Example.LibraryItem/Program.cs)).

## Database Providers

- Sqlite (default): configured from `ConnectionStrings:Default`.
- InMemory (dev/tests): set `Database:Provider=inmemory` or run in Development without a configured provider (see provider selection in [`Program.cs`](Example.LibraryItem/Program.cs)).

## API Documentation

- Swagger UI (Development): `/swagger`
- OpenAPI JSON: `/swagger/v1/swagger.json`
- Enabled in Development via [`Program.cs`](Example.LibraryItem/Program.cs).

## Endpoints

All routes are nested under `/v1/items` and require authorization (`ApiKey`). See [`Api/Endpoints.cs`](Example.LibraryItem/Api/Endpoints.cs) for `Produces` annotations.

- `GET /v1/items`
  - Filters: `page (>=1)`, `limit (1..100)`, `title`, `author`, `isbn`, `item_type`, `status`, `collection`, `location_floor`, `location_section`, `call_number`, `publication_year_from`, `publication_year_to`, `sort_by`, `sort_order` (see [`Application/Queries.cs`](Example.LibraryItem/Application/Queries.cs)).
  - Produces: `200`, `400`, `401`, `403`, `500`.
- `GET /v1/items/{itemId}`
  - Produces: `200`, `400`, `401`, `403`, `404`, `500`.
- `POST /v1/items/`
  - Produces: `201`, `400`, `422`, `401`, `403`, `409`, `500`.
- `PUT /v1/items/{itemId}`
  - Produces: `200`, `400`, `422`, `401`, `403`, `404`, `409`, `500`.
- `PATCH /v1/items/{itemId}`
  - Produces: `200`, `400`, `422`, `401`, `403`, `404`, `409`, `500`.
- `DELETE /v1/items/{itemId}`
  - Produces: `204`, `400`, `401`, `403`, `404`, `409`, `500`.

Pagination: responses include `pagination` with `page`, `limit`, `total_items`, `total_pages`, `has_next`, `has_previous`. Collection responses also include `_links` with `self`, `next`, `previous`, `first`, `last`.

## Request/Response Contracts

- DTOs in [`Application/Dtos.cs`](Example.LibraryItem/Application/Dtos.cs):
  - `ItemDto` (response): identifiers, bibliographic fields, `ItemLocationDto`, status, audit fields, `_links.self`.
  - `ItemCreateRequestDto`, `ItemUpdateRequestDto`, `ItemPatchRequestDto` (requests).
- Validation (highlights) in [`Application/Validators/*`](Example.LibraryItem/Application/Validators):
  - `title` required (<=500); `call_number` required (<=50); enums required/valid; `isbn` and `issn` format checks; length caps on `publisher`, `edition`, `language`, `collection`, `barcode`, `condition_notes`, `description`.
  - `ItemLocationDto`: `floor` -2..20, `section` required (<=10), `shelf_code` required (<=20).
- 422 payload: `ValidationErrorResponseDto` with `validation_errors: [{ field, message }]`.

## Error Handling

Centralized in [`Api/ErrorHandling.cs`](Example.LibraryItem/Api/ErrorHandling.cs):

- 422: `VALIDATION_ERROR` with `ValidationErrorResponseDto`.
- 404: `{ "error": "ITEM_NOT_FOUND" }`.
- 409: domain-specific codes like `ITEM_ALREADY_EXISTS`, `ISBN_ALREADY_EXISTS`, `CANNOT_DELETE_CHECKED_OUT`.
- 499: set when client cancels the request.
- 500: `INTERNAL_SERVER_ERROR` for unhandled errors.
- Correlation: `X-Request-ID` header mirrored to response; generated if absent.

## Authentication

- Scheme: `ApiKey` (see [`Api/Auth.cs`](Example.LibraryItem/Api/Auth.cs)).
- Header: `X-API-Key`.
- Add to every request; empty/missing keys fail. Authentication wires in [`Program.cs`](Example.LibraryItem/Program.cs) and all item routes use `.RequireAuthorization()`.
- Failure responses: `401 Unauthorized` or `403 Forbidden` depending on the pipeline and middleware.

## Logging

- Structured logging from handlers with IDs and counts.
- HTTP logging enabled in Development; sensitive headers excluded (`Authorization`, `X-API-Key`, `Cookie`).
- No sensitive data logged.

## Running Tests and Coverage

Project uses Coverlet (opencover). Coverage thresholds: `100` for total `line,branch,method`, with excludes defined in [`Directory.Build.props`](Directory.Build.props).

Commands:

```bash
dotnet test Example.LibraryItem.Tests/Example.LibraryItem.Tests.csproj
```

PowerShell:
```powershell
dotnet test Example.LibraryItem.Tests/Example.LibraryItem.Tests.csproj
```

Coverage output: `Example.LibraryItem.Tests/coverage.opencover.xml` (and `coverage/` per test project). Use your preferred reporter to visualize.

## Development Notes

- CQRS handlers under `Application/Handlers/*` keep logic focused; endpoints delegate.
- Reads use `AsNoTracking()`; always paginate.
- Mapping centralized in [`Application/Mappings.cs`](Example.LibraryItem/Application/Mappings.cs); DTOs are immutable records.
- Adding a new filter (e.g., `publisher`):
  - Update `ListItemsQuery` (+ validator) → [`Application/Queries.cs`](Example.LibraryItem/Application/Queries.cs).
  - Extend `ListItemsHandler` composition → [`Application/Handlers/ListItemsHandler.cs`](Example.LibraryItem/Application/Handlers/ListItemsHandler.cs).
  - Endpoints already bind `[AsParameters]`; ensure pagination `_links` remain consistent.

## VS Code Tips

- Tasks are available under `.vscode/` (if present). Typical tasks include running tests. From the command palette, run “Tasks: Run Task” and choose one like "test: all".

## Contributing

- Conventions: C# 12 / .NET 8; records for DTOs; use `required` where appropriate; async EF Core; `AsNoTracking` for reads; structured logging.
- Definition of Done:
  - Contracts added/updated in DTOs and validators
  - Handler logic added/updated with tests
  - Endpoints validate and return correct status codes/payloads
  - Mappings in sync both directions
  - Swagger annotations updated
  - Builds cleanly (treat warnings as errors if configured)
  - Configuration respected (`Database:Provider`, `ConnectionStrings:Default`)

## License

This template uses MIT for the badge. If your repository includes a different license, update this section to link the correct file (e.g., `LICENSE`).

## Appendix

### Example requests

Replace `<API_KEY>` with your key.

List with pagination and filters:
```bash
curl -H "X-API-Key: <API_KEY>" "https://localhost:5001/v1/items?page=1&limit=10&title=Gatsby&sort_by=title&sort_order=asc"
```

Get by id:
```bash
curl -H "X-API-Key: <API_KEY>" "https://localhost:5001/v1/items/<GUID>"
```

Create:
```bash
curl -X POST -H "Content-Type: application/json" -H "X-API-Key: <API_KEY>" \
  -d '{
    "title": "The Great Gatsby",
    "item_type": "book",
    "call_number": "001.42",
    "classification_system": "dewey_decimal",
    "location": { "floor": 1, "section": "A", "shelf_code": "B" }
  }' \
  https://localhost:5001/v1/items/
```

Update (PUT):
```bash
curl -X PUT -H "Content-Type: application/json" -H "X-API-Key: <API_KEY>" \
  -d '{
    "title": "The Great Gatsby (Updated)",
    "item_type": "book",
    "call_number": "001.42",
    "classification_system": "dewey_decimal",
    "location": { "floor": 1, "section": "A", "shelf_code": "B" },
    "status": "available"
  }' \
  https://localhost:5001/v1/items/<GUID>
```

Patch (PATCH):
```bash
curl -X PATCH -H "Content-Type: application/json" -H "X-API-Key: <API_KEY>" \
  -d '{ "status": "checked_out" }' \
  https://localhost:5001/v1/items/<GUID>
```

Delete:
```bash
curl -X DELETE -H "X-API-Key: <API_KEY>" https://localhost:5001/v1/items/<GUID>
```

### Sample error payloads

422 validation error:
```json
{
  "error": "VALIDATION_ERROR",
  "message": "The request contains validation errors",
  "validation_errors": [
    { "field": "title", "message": "'title' must not be empty." }
  ],
  "timestamp": "2024-01-01T00:00:00Z",
  "request_id": "00000000-0000-0000-0000-000000000000"
}
```

404 not found:
```json
{ "error": "ITEM_NOT_FOUND", "message": "The requested library item could not be found" }
```

### Troubleshooting

- Missing API key → ensure `X-API-Key` header is present and non-empty.
- Invalid provider → set `Database:Provider` to `sqlite` or `inmemory` in `appsettings*.json`.
- Sqlite connection missing → add `ConnectionStrings:Default` (e.g., `Data Source=library.db`).
- Swagger not visible → ensure `ASPNETCORE_ENVIRONMENT=Development`.
