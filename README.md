# Example.LibraryItem — .NET 8 Minimal API for Library Items

One-stop CRUD and query API for library items with API-key auth, rigorous validation, predictable error handling, and clean CQRS.

## Table of Contents

- [Features](#features)
- [Architecture Overview](#architecture-overview)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Restore, Build, Run](#restore-build-run)
  - [Run with Watch](#run-with-watch)
- [Configuration](#configuration)
  - [Development Configuration](#development-configuration)
  - [Production Configuration](#production-configuration)
  - [Development Seed Data](#development-seed-data)
  - [Database Providers](#database-providers)
- [Authentication](#authentication)
  - [Configuration](#authentication-configuration)
  - [Usage](#authentication-usage)
  - [Implementation Details](#authentication-implementation-details)
- [API Documentation](#api-documentation)
- [Endpoints](#endpoints)
- [Request/Response Contracts](#requestresponse-contracts)
- [Error Handling](#error-handling)
- [Logging](#logging)
- [Running Tests and Coverage](#running-tests-and-coverage)
- [Development Notes](#development-notes)
- [VS Code Tips](#vs-code-tips)
- [Contributing](#contributing)
- [License](#license)
- [Appendix](#appendix)
  - [Example Requests](#example-requests)
  - [Sample Error Payloads](#sample-error-payloads)
  - [Troubleshooting](#troubleshooting)

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

### Restore, Build, Run

```bash
dotnet restore
dotnet build
dotnet run --project Example.LibraryItem/Example.LibraryItem.csproj
```

PowerShell:
```powershell
dotnet restore; dotnet build; dotnet run --project Example.LibraryItem/Example.LibraryItem.csproj
```

### Run with Watch

```bash
dotnet watch --project Example.LibraryItem/Example.LibraryItem.csproj run
```

## Configuration

### Development Configuration

For local development, use the default configuration in `appsettings.Development.json`:

```json
{
  "Database": {
    "Provider": "inmemory"
  },
  "ApiKeys": [
    "dev-key",
    "test-key", 
    "local-development-key"
  ]
}
```

- **Database**: Uses in-memory database (no setup required)
- **API Keys**: Development keys for easy testing
- **Environment**: Set via `Properties/launchSettings.json` or `ASPNETCORE_ENVIRONMENT=Development`
- **Seed Data**: Automatically populated with 20 diverse library items on startup

### Production Configuration

Create `appsettings.Production.json` or use environment variables:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Default": "Data Source=/app/data/library.db"
  },
  "Database": {
    "Provider": "sqlite"
  },
  "ApiKeys": [
    "your-secure-production-key-here",
    "backup-production-key"
  ]
}
```

**Key Production Settings:**

- **`AllowedHosts`**: Controls which hostnames are allowed for requests (used by ASP.NET Core Host Filtering)
- **`ConnectionStrings:Default`**: SQLite database file path - used by `Program.cs:91` for database connection
- **`Database:Provider`**: Set to `"sqlite"` for production (default behavior)
- **`ApiKeys`**: **Must** replace placeholder keys with secure values

**Environment Variables (Alternative):**
```bash
# Database
ConnectionStrings__Default="/app/data/library.db"
Database__Provider="sqlite"

# API Keys
ApiKeys__0="secure-production-key-1"
ApiKeys__1="secure-production-key-2"

# Environment
ASPNETCORE_ENVIRONMENT="Production"
```

**Production Deployment Commands:**
```bash
# Method 1: Using appsettings.Production.json
ASPNETCORE_ENVIRONMENT=Production dotnet run --project Example.LibraryItem --no-launch-profile

# Method 2: Using environment variables
ApiKeys__0="your-secure-key" ASPNETCORE_ENVIRONMENT=Production dotnet run --project Example.LibraryItem --no-launch-profile

# Method 3: Published application
dotnet publish Example.LibraryItem -c Release -o ./publish
cd publish
ApiKeys__0="your-secure-key" ASPNETCORE_ENVIRONMENT=Production dotnet Example.LibraryItem.dll
```

⚠️ **Security**: The application will crash on startup if placeholder keys like `"CHANGE-ME-OR-SERVER-WILL-CRASH"` are detected in non-Development environments.

### Development Seed Data

In Development environment, the application automatically seeds the database with 20 diverse library items across multiple categories:

**Sample Items Include:**
- **Classic Literature**: The Great Gatsby, To Kill a Mockingbird, 1984, Pride and Prejudice
- **Science & Technology**: Computer Programming texts, Introduction to Algorithms, Clean Code
- **Academic Journals**: Nature, Science magazines
- **History**: A People's History of the United States, Sapiens
- **Philosophy**: Meditations by Marcus Aurelius
- **Science Fiction**: Dune, Foundation, Lord of the Rings
- **And more**: Economics, Poetry, Children's Literature, Sociology, Religion, Drama, Medicine, Environmental Science, Music

**Seed Data Features:**
- Only applied in Development environment (`ASPNETCORE_ENVIRONMENT=Development`)
- Includes realistic ISBNs, call numbers, and location data
- Demonstrates different item types and statuses
- Automatically populated on application startup
- **Production Safe**: No seed data applied in Production/Staging environments

### Database Providers

The application supports two database providers configured via `Database:Provider`:

- **SQLite** (default): Uses `ConnectionStrings:Default` for file path
  - Development: `"Data Source=library.db"` (current directory)
  - Production: `"Data Source=/app/data/library.db"` (recommended for containers)
- **InMemory**: Set `Database:Provider=inmemory` for testing/development
  - Automatically used in Development environment if no provider specified
  - Data is lost when application stops
  - **Development only**: Automatically seeded with 20 diverse library items

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

API key authentication is required for all endpoints under `/v1/items`.

### Authentication Configuration

API keys are configured in the `ApiKeys` array in `appsettings*.json`:

```json
{
  "ApiKeys": [
    "your-secure-api-key-here",
    "another-valid-key"
  ]
}
```

**⚠️ Security Notice:** The application will crash on startup if placeholder keys like `"CHANGE-ME-OR-SERVER-WILL-CRASH"` are detected in non-Development environments.

### Authentication Usage

- **Scheme**: `ApiKey` (see [`Api/Authentication/ApiKeyDefaults.cs`](Example.LibraryItem/Api/Authentication/ApiKeyDefaults.cs))
- **Header**: `X-API-Key`
- **Required**: All `/v1/items` endpoints require a valid API key
- **Validation**: Empty/missing keys return `401 Unauthorized`

### Authentication Implementation Details

- Authentication handler: [`Api/Authentication/ApiKeyAuthenticationHandler.cs`](Example.LibraryItem/Api/Authentication/ApiKeyAuthenticationHandler.cs)
- Startup validation: [`Api/ApiKeyValidator.cs`](Example.LibraryItem/Api/ApiKeyValidator.cs) prevents deployment with insecure keys
- Logging: Failed attempts are logged with IP address and key prefix for security monitoring
- Claims: Authenticated requests receive `apikey-user` identity with API key prefix claim

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

### Example Requests

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

### Sample Error Payloads

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
