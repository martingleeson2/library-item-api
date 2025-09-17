# Copilot Custom Instructions for GPT-5 — Example.LibraryItem (v2)

These instructions customize GitHub Copilot (GPT-5) for this repository to ensure agentic consistency, correct use of dependencies, alignment with the project’s architecture/conventions, and high code quality.

## Project Overview
- Runtime: .NET 8 minimal API
- Layers: API (endpoints, middleware, auth) → Application (CQRS handlers, DTOs, validators, mappings) → Domain (entities, enums, value objects) → Infrastructure (EF Core DbContext & persistence concerns)
- Primary goal: CRUD + query for Library Items with strong validation, predictable error handling, and simple API-key authentication.

## Dependencies (pin these versions unless asked to upgrade)
- Microsoft.EntityFrameworkCore: 9.0.0
- Microsoft.EntityFrameworkCore.Design: 9.0.0
- Microsoft.EntityFrameworkCore.Sqlite: 9.0.0
- Microsoft.EntityFrameworkCore.InMemory: 9.0.0 (tests/dev only)
- Swashbuckle.AspNetCore: 6.6.2 (OpenAPI/Swagger)
- FluentValidation: 11.9.0
- FluentValidation.DependencyInjectionExtensions: 11.9.0
- FluentValidation.AspNetCore: 11.3.0

## Coding Standards & Style (C# / .NET 8)
- Naming:
  - PascalCase for classes, methods, properties, public fields; camelCase for locals/parameters; private fields camelCase prefixed with _.
  - ALL_CAPS for constants.
- Methods: keep small, single-responsibility; prefer expression-bodied members when it helps clarity.
- Use var when type is obvious; otherwise use explicit types for clarity.
- Avoid magic numbers/strings; use constants or enums.
- Prefer string interpolation over string.Format/concatenation.
- Nullable reference types must be enabled; do not suppress warnings without justification.
- Async:
  - Prefer async/await; avoid blocking (.Result, .Wait()).
  - Propagate CancellationToken parameters across public APIs/handlers and EF Core calls.
- Immutability: prefer records for DTOs; use required properties where appropriate.
- Prefer switch expressions over long if/else chains.
- LINQ: be mindful of query translation; push filtering/pagination to the database (IQueryable) when using EF Core.
- Time: use TimeProvider (injectable) rather than DateTime.Now; use Utc for timestamps.
- Serialization: use System.Text.Json; avoid Newtonsoft unless required.

## Architecture & Methodologies
- Clean layering with clear boundaries: API ↔ Application ↔ Domain ↔ Infrastructure.
- CQRS in Application with focused handler interfaces/classes:
  - Queries: IListItemsHandler, IGetItemHandler
  - Commands: ICreateItemHandler, IUpdateItemHandler, IPatchItemHandler, IDeleteItemHandler
- DTO-first API contracts live in Application (Dtos.cs). Mapping is centralized in Mappings.cs.
- Validation with FluentValidation in Application (Validators.cs). Endpoints call ValidateAndThrowAsync or return 400 for invalid query params.
- EF Core in Infrastructure using LibraryDbContext with value converters for lists, Uri, and DateOnly.
  - Register both AddDbContext and AddDbContextFactory; prefer factory in background tasks or long-lived services.
- Minimal API endpoints in API/Endpoints with RequireAuthorization() using ApiKey scheme (ApiKeyAuthenticationHandler).
- Prefer vertical slice additions (feature-focused changes across layers) while retaining layer boundaries.

## Source of Truth
- Domain is canonical for business concepts: Item, ItemLocation, ItemType, ItemStatus, ClassificationSystem.
- Application DTOs define external API shapes; do not leak EF or domain internals.
- All public HTTP contracts must be reflected in DTOs and FluentValidators.

## Error Handling Contract
- Validation errors: 422 with ValidationErrorResponseDto.
- Not found: 404 with ErrorResponseDto { error = "ITEM_NOT_FOUND" }.
- Conflicts: 409 with domain-specific codes (e.g., ITEM_ALREADY_EXISTS, CANNOT_DELETE_CHECKED_OUT).
- AuthZ/AuthN: 401/403 as appropriate (API uses RequireAuthorization() and ApiKey scheme name ApiKey).
- Cancellation: return 499 when client cancels.
- Correlation: include X-Request-ID; middleware adds it if missing and mirrors to response.
- Do not return raw exceptions from endpoints. Throw/let exceptions bubble in handlers; GlobalExceptionMiddleware shapes responses.

## Swagger / OpenAPI
- Endpoints should have .Produces<...>(StatusCodes.XXX) annotations to describe responses.
- Keep request/response types aligned with DTOs and validators.
- Document auth requirements; group endpoints logically.

## Data & Query Guidelines
- Filtering and sorting follow ListItemsQuery. If adding new filters, update:
  - ListItemsQuery + ListItemsQueryValidator
  - Handler composition in ListItemsHandler
  - _links shaping for pagination
- Default sort to a stable, user-friendly column (Title).
- EF Core:
  - Use async APIs and AsNoTracking() for read queries; use projections to DTOs when possible.
  - Avoid N+1; use explicit includes only when necessary; prefer select projections.
  - Batch save changes where possible; minimize SaveChanges calls.
  - Use value converters for lists/Uri/DateOnly; prefer owned types for value objects.
- Access DbContext via scoped injection in request lifetimes.
  - For factories, inject IDbContextFactory<LibraryDbContext> and dispose promptly: await using var db = await factory.CreateDbContextAsync(token);

## Performance & Safety
- Favor allocation-free patterns in hot paths; avoid unnecessary boxing.
- Consider ValueTask for frequently-called async methods with sync completion.
- Consider IAsyncEnumerable<T> for streaming scenarios.
- Use caching (IMemoryCache/Redis) for expensive or popular reads when appropriate.
- Prefer FrozenDictionary and Random.Shared where relevant.
- Use parallelism carefully (Parallel.ForEachAsync, Channels, Dataflow) and respect cancellation.
- Optimize JSON serialization settings via System.Text.Json options when needed.

## Security Best Practices
- Validate all inputs with FluentValidation; return detailed 422 payloads.
- Use parameterized queries (EF Core does this by default).
- Store secrets outside code (user-secrets, env vars, Key Vault); never log secrets.
- Enforce HTTPS; configure HSTS in production.
- Use simple API key auth scheme (ApiKey) for this service; extend with policies if needed.
- Sanitize outputs and content-types. For forms (if added), use anti-forgery tokens.
- Keep dependencies patched; review NuGet advisories.

## Testing & Quality
- Unit tests for business logic (xUnit or NUnit).
- Handler tests under Tests/Handlers using EF Core InMemory provider.
- Validator tests under Tests/Validators for rules and messages.
- Integration tests with WebApplicationFactory for endpoints and middleware.
- Mock external dependencies with Moq (preferred). Use clear Arrange/Act/Assert and leverage `It.Is<>()`, `Verify()` with explicit times (e.g., `Times.Once`).
- Use Shouldly for assertions (preferred) for readable, intention-revealing tests: `result.ShouldNotBeNull(); items.Count.ShouldBe(3); response.StatusCode.ShouldBe(StatusCodes.Status200OK);`
- Test happy paths and edge cases; aim for meaningful coverage.
- Automate tests in CI; ensure consistent seeds/fixtures.

### Testing Conventions (Moq + Shouldly)
- Prefer strict, explicit verifications: `mock.Verify(s => s.Do(It.Is<Arg>(a => a.Id == id)), Times.Once);`
- Avoid overspecifying setups; only mock what you assert. Favor behavior verification over internal state checks.
- Shouldly assertion patterns to favor:
  - Equality and nullability: `value.ShouldBe(expected); obj.ShouldNotBeNull();`
  - Collections: `list.ShouldBeEmpty(); list.ShouldContain(x => x.Id == id);`
  - Exceptions: `await Should.ThrowAsync<ValidationException>(() => handler.Handle(cmd, ct));`
  - Ranges: `duration.ShouldBeLessThan(100.Milliseconds());`

## Logging & Observability
- Use structured logging (ILogger<T>) with message templates; include IDs and counts.
- Include correlation IDs in scopes and responses (X-Request-ID).
- Centralize logs where possible (Seq/ELK/Azure Monitor).
- Add health checks (AddHealthChecks) endpoints.
- Expose metrics (OpenTelemetry/Prometheus) if required.
- Don’t swallow exceptions; log with appropriate levels.

## Deployment & Maintenance
- Use per-environment configuration (appsettings.*.json).
- Migrations applied via CI/CD or at startup as appropriate.
- Containerize for portability; use Kestrel behind a reverse proxy/CDN.
- Apply rate limiting and transient-fault handling (Polly).
- Handle graceful shutdown (IHostApplicationLifetime); prefer IAsyncDisposable for async cleanup.
- Respect configuration:
  - Database:Provider in appsettings (sqlite or inmemory).
  - ConnectionStrings:Default for Sqlite (e.g., Data Source=library.db).

## Modern .NET 8 Guidance
- Use primary constructors for simple services.
- Use collection expressions ([1, 2, 3]).
- Use required on DTOs/entities when needed.
- Consider EF Core 8/9 interceptors for auditing/logging.
- Prefer TimeProvider for time; avoid DateTime.Now/DateTimeOffset.Now.
- Use minimal API endpoint filters/groups for cross-cutting concerns.

## When Implementing New Features
1) Model changes (Domain)
   - Only add/modify entities when truly required.
   - Update LibraryDbContext mappings/value converters; prefer owned types for value objects.

2) API contract (Application)
   - Add/adjust DTOs in Dtos.cs (records, immutable; use required).
   - Add validators in Validators.cs with strict rules and helpful messages.
   - Extend mappings in Mappings.cs both directions.

3) Business flow (Application)
   - Add a new handler interface + implementation in CQRS.cs.
   - Keep handlers focused; reuse query composition patterns from ListItemsHandler.
   - Log start/end with identifiers; accept CancellationToken.

4) HTTP endpoints (API)
   - Add routes in Api/Endpoints.cs.
   - Require authorization (ApiKey).
   - Validate inputs early; call ValidateAndThrowAsync where applicable.
   - Return shaped error payloads per contract (422/404/409, etc.).
   - Maintain pagination and _links consistency.

5) Tests
   - Add unit tests alongside Tests/Handlers and Tests/Validators patterns.
   - Use EF Core InMemory for handler tests; integration tests for real API behavior.
   - Follow Arrange → Act → Assert.

## Definition of Done (Contributor Checklist)
- Public contracts added/changed in DTOs and validators.
- Handler logic added/updated with tests (including edge cases).
- Endpoints validate and return correct status codes and error payloads.
- Mappings kept in sync both directions.
- Swagger annotations updated.
- Builds cleanly with TreatWarningsAsErrors=true; no analyzers suppressed without justification.
- Nullable enabled and warnings addressed.
- Configuration respected (Database:Provider, ConnectionStrings:Default).

## Examples
- Add a new filter (e.g., publisher):
  - Domain: usually none.
  - Application: extend ListItemsQuery (+ validator), add .Where(i => i.Publisher.Contains(q.publisher)) in ListItemsHandler.
  - API: support query param via [AsParameters] model binder (already handled).
  - Tests: add validator and handler coverage.

- Add soft-delete:
  - Domain: add IsDeleted flag.
  - Infrastructure: global query filter modelBuilder.Entity<Item>().HasQueryFilter(i => !i.IsDeleted).
  - Application: DeleteItemHandler sets flag; adjust queries to respect filter.

- Swap database provider via configuration:
  - Set Database:Provider to sqlite (default) or inmemory in appsettings.*.json.
  - Provide ConnectionStrings:Default for Sqlite (e.g., Data Source=library.db).

## Things to Avoid
- Bypassing validators or returning raw exceptions from endpoints.
- Mixing Infrastructure concerns into Application or Domain.
- Returning EF entities directly to HTTP clients.
- Creating handlers that perform both query and mutation in one method.
- Blocking on async code (.Result/.Wait()).
- Logging sensitive data (API keys, PII).

## Repository Expectations (for Copilot)
- Prefer adding/changing code in the correct layer/file:
  - API: Api/Endpoints.cs, ErrorHandling.cs, auth/middleware
  - Application: Dtos.cs, Validators.cs, Mappings.cs, CQRS.cs (handlers/interfaces)
  - Domain: entities/enums/value objects only
  - Infrastructure: LibraryDbContext, configurations, migrations
- Always include CancellationToken in handlers and EF calls.
- Use AsNoTracking for read queries, projections for DTOs, and structured logging.
- Keep DTOs immutable; use records and required.
- Mirror HTTP response contracts in DTOs and validators.
 - In tests, default to Moq for mocks and Shouldly for assertions. Only use alternative frameworks when a scenario cannot be expressed clearly with these.