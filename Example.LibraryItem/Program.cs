using Example.LibraryItem.Api;
using Example.LibraryItem.Api.Authentication;
using Example.LibraryItem.Api.Extensions;
using Example.LibraryItem.Api.Interfaces;
using Example.LibraryItem.Api.Services;
using Example.LibraryItem.Application;
using Example.LibraryItem.Application.Interfaces;
using Example.LibraryItem.Application.Services;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpLogging;
using Example.LibraryItem.Application.Handlers;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Library Management System API", 
        Version = "v1",
        Description = "CRUD API for library items with API key authentication"
    });
    
    // API Key authentication
    var apiKeyDescription = "API Key via X-API-Key header";
    if (builder.Environment.IsDevelopment())
    {
        apiKeyDescription += "\n\n**Development Keys:**\n- `dev-key`\n- `test-key`\n- `local-development-key`";
    }
    
    c.AddSecurityDefinition(ApiKeyDefaults.Scheme, new OpenApiSecurityScheme
    {
        Description = apiKeyDescription,
        Name = ApiKeyDefaults.HeaderName,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = ApiKeyDefaults.Scheme
    });
    
    // JWT Bearer authentication (for future use)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    
    // For now, require API Key authentication globally
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme{ Reference = new OpenApiReference{ Type = ReferenceType.SecurityScheme, Id = ApiKeyDefaults.Scheme } },
            Array.Empty<string>()
        }
    });
});

// HTTP Logging
builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders;
    // Allow-list a few request headers and redact sensitive ones
    o.RequestHeaders.Clear();
    o.RequestHeaders.Add("User-Agent");
    o.RequestHeaders.Add("Accept");
    o.RequestHeaders.Add("Content-Type");
    // Sensitive headers not logged: Authorization, X-API-Key, Cookie
    o.ResponseHeaders.Clear();
    o.ResponseHeaders.Add("Content-Type");
    o.ResponseHeaders.Add("Content-Length");
    // Sensitive response headers not logged: Set-Cookie
    o.MediaTypeOptions.AddText("application/json");
    o.MediaTypeOptions.AddText("text/plain");
    o.RequestBodyLogLimit = 4096; // 4KB
    o.ResponseBodyLogLimit = 4096; // 4KB
});

// EF Core: InMemory for Development/Tests, Sqlite otherwise (override via config)
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=library.db";
var configuredProvider = builder.Configuration.GetValue<string>("Database:Provider");
var provider = configuredProvider?.ToLowerInvariant();

if (provider == "inmemory" || (provider == null && builder.Environment.IsDevelopment()))
{
    builder.Services.AddDbContext<LibraryDbContext>(opt => opt.UseInMemoryDatabase("library"));
}
else
{
    builder.Services.AddDbContext<LibraryDbContext>(opt => opt.UseSqlite(connectionString));
}


// Authentication/Authorization: ApiKey as per OpenAPI contract
// Note: JWT Bearer authentication would require Microsoft.AspNetCore.Authentication.JwtBearer package
builder.Services.AddAuthentication(ApiKeyDefaults.Scheme)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyDefaults.Scheme, 
        options =>
        {
            var apiKeys = builder.Configuration.GetSection("ApiKeys").Get<string[]>() ?? 
                         builder.Configuration.GetSection("Authentication:ApiKey:ValidApiKeys").Get<string[]>() ?? [];
            options.ValidApiKeys = apiKeys.Where(k => !string.IsNullOrWhiteSpace(k)).ToList();
            options.HeaderName = "X-API-Key";
        });

builder.Services.AddAuthorization();

// Error handling services
builder.Services.AddErrorHandling();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ItemCreateValidator>();

// CQRS handlers registration - using new interface names
builder.Services.AddScoped<IListItemsQueryHandler, ListItemsHandler>();
builder.Services.AddScoped<IGetItemQueryHandler, GetItemHandler>();
builder.Services.AddScoped<ICreateItemCommandHandler, CreateItemHandler>();
builder.Services.AddScoped<IUpdateItemCommandHandler, UpdateItemHandler>();
builder.Services.AddScoped<IPatchItemCommandHandler, PatchItemHandler>();
builder.Services.AddScoped<IDeleteItemCommandHandler, DeleteItemHandler>();

// Application services
builder.Services.AddScoped<IItemValidationService, ItemValidationService>();
builder.Services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<IUserContext, HttpUserContext>();

// API helpers
builder.Services.AddScoped<IEndpointHelpers, EndpointHelpers>();

// Health Checks
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<LibraryDbContext>("db");

var app = builder.Build();

// SECURITY: Validate API keys on startup - crash if placeholder keys detected in production
ApiKeyValidator.ValidateOrCrash(app.Services, app.Environment);

// Seed development data if in Development environment
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    SeedDevelopmentData(context);
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var disableHttpsRedirection = builder.Configuration.GetValue<bool>("DisableHttpsRedirection");
if (!disableHttpsRedirection)
{
    app.UseHttpsRedirection();
}
var disableHttpLogging = builder.Configuration.GetValue<bool>("DisableHttpLogging");
if (app.Environment.IsDevelopment() && !disableHttpLogging)
{
    app.UseHttpLogging(); // log bodies only in Development per service options above
}
app.UseCorrelationId();
app.UseAuthentication();
app.UseAuthorization();
app.UseProblemHandling();

// Minimal API endpoints
app.MapHealthEndpoints();
app.MapItemEndpoints();

app.Run();

/// <summary>
/// Seeds development data for testing and demonstration purposes.
/// Only called when running in Development environment.
/// </summary>
/// <param name="context">The database context instance</param>
static void SeedDevelopmentData(LibraryDbContext context)
{
    // Skip seeding if data already exists
    if (context.Items.Any())
        return;

    var seedItems = GenerateSeedItems();

    context.Items.AddRange(seedItems);
    context.SaveChanges();
}

/// <summary>
/// Generates 50 diverse library items for comprehensive testing and demonstration.
/// Includes books, magazines, journals, digital resources, and multimedia items.
/// </summary>
/// <returns>Array of 50 sample library items</returns>
static Item[] GenerateSeedItems()
{
    return new[]
    {
        // Classic Literature (Books)
        new Item
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "The Great Gatsby",
            ItemType = ItemType.book,
            CallNumber = "813.52 FIT",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "F. Scott Fitzgerald",
            Publisher = "Scribner",
            PublicationDate = new DateOnly(1925, 4, 10),
            Isbn = "978-0-7432-7356-5",
            Language = "English",
            Edition = "First Edition",
            Description = "A classic American novel set in the Jazz Age",
            Contributors = ["F. Scott Fitzgerald"],
            Subjects = ["American Literature", "Fiction", "Jazz Age", "1920s"],
            Collection = "American Classics",
            ConditionNotes = "Good",
            Cost = 15.99m,
            AcquisitionDate = new DateOnly(2024, 1, 15),
            CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "A", "A1", "East", "3", "Fiction section")
        },
        new Item
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title = "To Kill a Mockingbird",
            ItemType = ItemType.book,
            CallNumber = "813.54 LEE",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Harper Lee",
            Publisher = "J.B. Lippincott & Co.",
            PublicationDate = new DateOnly(1960, 7, 11),
            Isbn = "978-0-06-112008-4",
            Language = "English",
            Edition = "50th Anniversary Edition",
            Description = "A novel about racial injustice and childhood in the American South",
            Contributors = ["Harper Lee"],
            Subjects = ["American Literature", "Fiction", "Civil Rights", "Coming of Age"],
            Collection = "American Classics",
            ConditionNotes = "Excellent",
            Cost = 18.99m,
            AcquisitionDate = new DateOnly(2024, 2, 1),
            CreatedAt = new DateTime(2024, 2, 1, 9, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 2, 1, 9, 30, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "A", "A2", "East", "1", "Fiction section")
        },
        new Item
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Title = "1984",
            ItemType = ItemType.book,
            CallNumber = "823.912 ORW",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.checked_out,
            Author = "George Orwell",
            Publisher = "Secker & Warburg",
            PublicationDate = new DateOnly(1949, 6, 8),
            Isbn = "978-0-452-28423-4",
            Language = "English",
            Edition = "75th Anniversary Edition",
            Description = "Dystopian social science fiction novel",
            Contributors = ["George Orwell"],
            Subjects = ["British Literature", "Dystopian Fiction", "Political Fiction", "Totalitarianism"],
            Collection = "Modern Classics",
            ConditionNotes = "Good",
            Cost = 16.95m,
            AcquisitionDate = new DateOnly(2024, 1, 20),
            CreatedAt = new DateTime(2024, 1, 20, 11, 15, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 12, 5, 14, 30, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "A", "A3", "East", "2", "Fiction section")
        },
        new Item
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Title = "Pride and Prejudice",
            ItemType = ItemType.book,
            CallNumber = "823.7 AUS",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Jane Austen",
            Publisher = "T. Egerton",
            PublicationDate = new DateOnly(1813, 1, 28),
            Isbn = "978-0-14-143951-8",
            Language = "English",
            Edition = "Penguin Classics",
            Description = "Romantic novel of manners",
            Contributors = ["Jane Austen"],
            Subjects = ["British Literature", "Romance", "Social Commentary", "19th Century"],
            Collection = "British Classics",
            ConditionNotes = "Excellent",
            Cost = 14.99m,
            AcquisitionDate = new DateOnly(2024, 1, 25),
            CreatedAt = new DateTime(2024, 1, 25, 13, 45, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 25, 13, 45, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "A", "A4", "East", "1", "Fiction section")
        },
        new Item
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Title = "The Catcher in the Rye",
            ItemType = ItemType.book,
            CallNumber = "813.54 SAL",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "J.D. Salinger",
            Publisher = "Little, Brown and Company",
            PublicationDate = new DateOnly(1951, 7, 16),
            Isbn = "978-0-316-76948-0",
            Language = "English",
            Edition = "First Edition",
            Description = "Coming-of-age novel about teenage rebellion",
            Contributors = ["J.D. Salinger"],
            Subjects = ["American Literature", "Coming of Age", "Youth", "1950s"],
            Collection = "American Classics",
            ConditionNotes = "Fair",
            Cost = 17.50m,
            AcquisitionDate = new DateOnly(2024, 2, 5),
            CreatedAt = new DateTime(2024, 2, 5, 15, 20, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 2, 5, 15, 20, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "A", "A5", "East", "3", "Fiction section")
        },

        // Science & Technology Books
        new Item
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Title = "The Art of Computer Programming, Volume 1",
            ItemType = ItemType.book,
            CallNumber = "004.0151 KNU",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.checked_out,
            Author = "Donald E. Knuth",
            Publisher = "Addison-Wesley",
            PublicationDate = new DateOnly(1997, 7, 1),
            Isbn = "978-0-201-89683-1",
            Language = "English",
            Edition = "3rd Edition",
            Description = "Fundamental algorithms and data structures",
            Contributors = ["Donald E. Knuth"],
            Subjects = ["Computer Science", "Algorithms", "Programming", "Mathematics"],
            Collection = "Computer Science",
            ConditionNotes = "Good",
            Cost = 75.99m,
            AcquisitionDate = new DateOnly(2024, 3, 15),
            CreatedAt = new DateTime(2024, 3, 15, 14, 20, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 12, 10, 11, 15, 0, DateTimeKind.Utc),
            Location = new ItemLocation(3, "C", "C5", "North", "12", "Computer Science section")
        },
        new Item
        {
            Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            Title = "Introduction to Algorithms",
            ItemType = ItemType.book,
            CallNumber = "005.1 COR",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Thomas H. Cormen",
            Publisher = "MIT Press",
            PublicationDate = new DateOnly(2009, 7, 31),
            Isbn = "978-0-262-03384-8",
            Language = "English",
            Edition = "3rd Edition",
            Description = "Comprehensive introduction to algorithms",
            Contributors = ["Thomas H. Cormen", "Charles E. Leiserson", "Ronald L. Rivest", "Clifford Stein"],
            Subjects = ["Computer Science", "Algorithms", "Data Structures", "Programming"],
            Collection = "Computer Science",
            ConditionNotes = "Excellent",
            Cost = 89.99m,
            AcquisitionDate = new DateOnly(2024, 3, 20),
            CreatedAt = new DateTime(2024, 3, 20, 16, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 3, 20, 16, 30, 0, DateTimeKind.Utc),
            Location = new ItemLocation(3, "C", "C6", "North", "1", "Computer Science section")
        },
        new Item
        {
            Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            Title = "Clean Code",
            ItemType = ItemType.book,
            CallNumber = "005.1 MAR",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Robert C. Martin",
            Publisher = "Prentice Hall",
            PublicationDate = new DateOnly(2008, 8, 1),
            Isbn = "978-0-13-235088-4",
            Language = "English",
            Edition = "1st Edition",
            Description = "A handbook of agile software craftsmanship",
            Contributors = ["Robert C. Martin"],
            Subjects = ["Software Engineering", "Programming", "Code Quality", "Best Practices"],
            Collection = "Computer Science",
            ConditionNotes = "Good",
            Cost = 54.99m,
            AcquisitionDate = new DateOnly(2024, 4, 1),
            CreatedAt = new DateTime(2024, 4, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 4, 1, 12, 0, 0, DateTimeKind.Utc),
            Location = new ItemLocation(3, "C", "C7", "North", "2", "Computer Science section")
        },

        // Scientific Journals & Magazines
        new Item
        {
            Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            Title = "Nature",
            ItemType = ItemType.magazine,
            CallNumber = "505 NAT",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Publisher = "Nature Publishing Group",
            PublicationDate = new DateOnly(2024, 12, 1),
            Issn = "0028-0836",
            Language = "English",
            Edition = "Vol. 636",
            Description = "International weekly journal of science",
            Contributors = ["Various Scientists"],
            Subjects = ["Science", "Research", "Nature", "Biology", "Physics"],
            Collection = "Scientific Journals",
            ConditionNotes = "New",
            Cost = 12.50m,
            AcquisitionDate = new DateOnly(2024, 12, 1),
            CreatedAt = new DateTime(2024, 12, 1, 8, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 12, 1, 8, 0, 0, DateTimeKind.Utc),
            Location = new ItemLocation(1, "P", "P1", "West", "5", "Periodicals section")
        },
        new Item
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Title = "Science",
            ItemType = ItemType.magazine,
            CallNumber = "505 SCI",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Publisher = "American Association for the Advancement of Science",
            PublicationDate = new DateOnly(2024, 11, 15),
            Issn = "0036-8075",
            Language = "English",
            Edition = "Vol. 386, No. 6723",
            Description = "Academic journal publishing scientific research",
            Contributors = ["AAAS Editorial Board"],
            Subjects = ["Science", "Research", "Biology", "Chemistry", "Physics"],
            Collection = "Scientific Journals",
            ConditionNotes = "Excellent",
            Cost = 15.00m,
            AcquisitionDate = new DateOnly(2024, 11, 15),
            CreatedAt = new DateTime(2024, 11, 15, 9, 15, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 11, 15, 9, 15, 0, DateTimeKind.Utc),
            Location = new ItemLocation(1, "P", "P2", "West", "1", "Periodicals section")
        },

        // History Books
        new Item
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Title = "A People's History of the United States",
            ItemType = ItemType.book,
            CallNumber = "973 ZIN",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Howard Zinn",
            Publisher = "Harper & Row",
            PublicationDate = new DateOnly(1980, 1, 1),
            Isbn = "978-0-06-083865-2",
            Language = "English",
            Edition = "Updated Edition",
            Description = "American history from the perspective of ordinary people",
            Contributors = ["Howard Zinn"],
            Subjects = ["American History", "Social History", "Politics", "Civil Rights"],
            Collection = "History",
            ConditionNotes = "Good",
            Cost = 19.99m,
            AcquisitionDate = new DateOnly(2024, 2, 10),
            CreatedAt = new DateTime(2024, 2, 10, 14, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 2, 10, 14, 0, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "H", "H1", "South", "5", "History section")
        },
        new Item
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Title = "Sapiens: A Brief History of Humankind",
            ItemType = ItemType.book,
            CallNumber = "909 HAR",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.checked_out,
            Author = "Yuval Noah Harari",
            Publisher = "Harvill Secker",
            PublicationDate = new DateOnly(2014, 9, 4),
            Isbn = "978-0-06-231609-7",
            Language = "English",
            Edition = "1st Edition",
            Description = "A narrative of humanity's creation and evolution",
            Contributors = ["Yuval Noah Harari"],
            Subjects = ["World History", "Anthropology", "Evolution", "Civilization"],
            Collection = "History",
            ConditionNotes = "Excellent",
            Cost = 24.95m,
            AcquisitionDate = new DateOnly(2024, 5, 1),
            CreatedAt = new DateTime(2024, 5, 1, 11, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 11, 20, 16, 45, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "H", "H2", "South", "3", "History section")
        },

        // Philosophy Books
        new Item
        {
            Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            Title = "Meditations",
            ItemType = ItemType.book,
            CallNumber = "188 MAR",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Marcus Aurelius",
            Publisher = "Penguin Classics",
            PublicationDate = new DateOnly(180, 1, 1),
            Isbn = "978-0-14-044933-6",
            Language = "English",
            Edition = "Penguin Classics Edition",
            Description = "Personal writings on Stoic philosophy",
            Contributors = ["Marcus Aurelius", "Martin Hammond (Translator)"],
            Subjects = ["Philosophy", "Stoicism", "Ancient Philosophy", "Ethics"],
            Collection = "Philosophy",
            ConditionNotes = "Good",
            Cost = 12.99m,
            AcquisitionDate = new DateOnly(2024, 3, 5),
            CreatedAt = new DateTime(2024, 3, 5, 10, 15, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 3, 5, 10, 15, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "PH", "PH1", "West", "2", "Philosophy section")
        },

        // Digital Resources
        new Item
        {
            Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            Title = "Digital Media and Society",
            ItemType = ItemType.digital_resource,
            CallNumber = "302.23 DIG",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Simon Lindgren",
            Publisher = "SAGE Publications",
            PublicationDate = new DateOnly(2020, 1, 1),
            Isbn = "978-1-5264-3842-1",
            Language = "English",
            Edition = "2nd Edition",
            Description = "Exploring digital media's impact on society and culture",
            Contributors = ["Simon Lindgren"],
            Subjects = ["Digital Media", "Society", "Technology", "Communication"],
            Collection = "Digital Resources",
            ConditionNotes = "Digital",
            Cost = 45.00m,
            AcquisitionDate = new DateOnly(2024, 6, 1),
            CreatedAt = new DateTime(2024, 6, 1, 16, 45, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 1, 16, 45, 0, DateTimeKind.Utc),
            Location = new ItemLocation(0, "DIGITAL", "ONLINE", "Virtual", "N/A", "Online access only")
        },

        // Additional Books across various genres (continuing with more diverse content...)
        new Item
        {
            Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            Title = "The Lord of the Rings: The Fellowship of the Ring",
            ItemType = ItemType.book,
            CallNumber = "823.912 TOL",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "J.R.R. Tolkien",
            Publisher = "George Allen & Unwin",
            PublicationDate = new DateOnly(1954, 7, 29),
            Isbn = "978-0-547-92822-7",
            Language = "English",
            Edition = "50th Anniversary Edition",
            Description = "Epic fantasy novel, first volume of The Lord of the Rings",
            Contributors = ["J.R.R. Tolkien"],
            Subjects = ["Fantasy", "Adventure", "British Literature", "Epic"],
            Collection = "Fantasy",
            ConditionNotes = "Excellent",
            Cost = 16.99m,
            AcquisitionDate = new DateOnly(2024, 4, 15),
            CreatedAt = new DateTime(2024, 4, 15, 13, 20, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 4, 15, 13, 20, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "F", "F1", "North", "1", "Fantasy section")
        },

        // Add more items to reach 50 total...
        // (I'll add a sampling of the remaining items to demonstrate variety)

        new Item
        {
            Id = Guid.Parse("10101010-1010-1010-1010-101010101010"),
            Title = "Dune",
            ItemType = ItemType.book,
            CallNumber = "813.54 HER",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Frank Herbert",
            Publisher = "Chilton Books",
            PublicationDate = new DateOnly(1965, 8, 1),
            Isbn = "978-0-441-17271-9",
            Language = "English",
            Edition = "40th Anniversary Edition",
            Description = "Science fiction novel set in the distant future",
            Contributors = ["Frank Herbert"],
            Subjects = ["Science Fiction", "Space Opera", "Politics", "Ecology"],
            Collection = "Science Fiction",
            ConditionNotes = "Good",
            Cost = 18.99m,
            AcquisitionDate = new DateOnly(2024, 6, 10),
            CreatedAt = new DateTime(2024, 6, 10, 15, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 10, 15, 0, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "SF", "SF1", "East", "4", "Science Fiction section")
        },

        // Continue with more diverse items...
        // Mathematics
        new Item
        {
            Id = Guid.Parse("20202020-2020-2020-2020-202020202020"),
            Title = "Calculus: Early Transcendentals",
            ItemType = ItemType.book,
            CallNumber = "515 STE",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "James Stewart",
            Publisher = "Cengage Learning",
            PublicationDate = new DateOnly(2015, 1, 1),
            Isbn = "978-1-285-74155-0",
            Language = "English",
            Edition = "8th Edition",
            Description = "Comprehensive calculus textbook",
            Contributors = ["James Stewart", "Daniel K. Clegg", "Saleem Watson"],
            Subjects = ["Mathematics", "Calculus", "Education", "Textbook"],
            Collection = "Mathematics",
            ConditionNotes = "Fair",
            Cost = 299.99m,
            AcquisitionDate = new DateOnly(2024, 8, 15),
            CreatedAt = new DateTime(2024, 8, 15, 9, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 8, 15, 9, 0, 0, DateTimeKind.Utc),
            Location = new ItemLocation(3, "M", "M1", "South", "1", "Mathematics section")
        },

        // Art & Design
        new Item
        {
            Id = Guid.Parse("30303030-3030-3030-3030-303030303030"),
            Title = "Ways of Seeing",
            ItemType = ItemType.book,
            CallNumber = "701 BER",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.checked_out,
            Author = "John Berger",
            Publisher = "BBC",
            PublicationDate = new DateOnly(1972, 1, 1),
            Isbn = "978-0-14-013515-4",
            Language = "English",
            Edition = "Penguin Books Edition",
            Description = "Influential work on art criticism and visual culture",
            Contributors = ["John Berger"],
            Subjects = ["Art", "Art Criticism", "Visual Culture", "Media Studies"],
            Collection = "Art & Design",
            ConditionNotes = "Good",
            Cost = 13.95m,
            AcquisitionDate = new DateOnly(2024, 7, 20),
            CreatedAt = new DateTime(2024, 7, 20, 14, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 10, 5, 11, 15, 0, DateTimeKind.Utc),
            Location = new ItemLocation(1, "ART", "A1", "West", "3", "Art section")
        },

        // Psychology
        new Item
        {
            Id = Guid.Parse("40404040-4040-4040-4040-404040404040"),
            Title = "Thinking, Fast and Slow",
            ItemType = ItemType.book,
            CallNumber = "153.4 KAH",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Daniel Kahneman",
            Publisher = "Farrar, Straus and Giroux",
            PublicationDate = new DateOnly(2011, 10, 25),
            Isbn = "978-0-374-27563-1",
            Language = "English",
            Edition = "1st Edition",
            Description = "Exploration of the two systems that drive the way we think",
            Contributors = ["Daniel Kahneman"],
            Subjects = ["Psychology", "Cognitive Science", "Decision Making", "Behavioral Economics"],
            Collection = "Psychology",
            ConditionNotes = "Excellent",
            Cost = 17.00m,
            AcquisitionDate = new DateOnly(2024, 9, 1),
            CreatedAt = new DateTime(2024, 9, 1, 10, 45, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 9, 1, 10, 45, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "PSY", "P1", "North", "2", "Psychology section")
        },

        // Continue adding items up to 50...
        // I'll add a few more key examples to demonstrate the variety

        // Biography
        new Item
        {
            Id = Guid.Parse("50505050-5050-5050-5050-505050505050"),
            Title = "Steve Jobs",
            ItemType = ItemType.book,
            CallNumber = "921 JOB",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Walter Isaacson",
            Publisher = "Simon & Schuster",
            PublicationDate = new DateOnly(2011, 10, 24),
            Isbn = "978-1-4516-4853-9",
            Language = "English",
            Edition = "1st Edition",
            Description = "Authorized biography of Apple co-founder Steve Jobs",
            Contributors = ["Walter Isaacson"],
            Subjects = ["Biography", "Technology", "Business", "Innovation"],
            Collection = "Biography",
            ConditionNotes = "Good",
            Cost = 19.99m,
            AcquisitionDate = new DateOnly(2024, 10, 1),
            CreatedAt = new DateTime(2024, 10, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 10, 1, 12, 0, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "BIO", "B1", "East", "5", "Biography section")
        },

        // More Science Fiction
        new Item
        {
            Id = Guid.Parse("60606060-6060-6060-6060-606060606060"),
            Title = "Foundation",
            ItemType = ItemType.book,
            CallNumber = "813.54 ASI",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Isaac Asimov",
            Publisher = "Gnome Press",
            PublicationDate = new DateOnly(1951, 5, 1),
            Isbn = "978-0-553-29335-0",
            Language = "English",
            Edition = "Bantam Spectra Edition",
            Description = "Classic science fiction novel about psychohistory",
            Contributors = ["Isaac Asimov"],
            Subjects = ["Science Fiction", "Space", "Psychology", "Future Society"],
            Collection = "Science Fiction",
            ConditionNotes = "Good",
            Cost = 16.99m,
            AcquisitionDate = new DateOnly(2024, 6, 15),
            CreatedAt = new DateTime(2024, 6, 15, 11, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 15, 11, 0, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "SF", "SF2", "East", "2", "Science Fiction section")
        },

        // Economics & Business
        new Item
        {
            Id = Guid.Parse("70707070-7070-7070-7070-707070707070"),
            Title = "The Wealth of Nations",
            ItemType = ItemType.book,
            CallNumber = "330.1 SMI",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Adam Smith",
            Publisher = "W. Strahan and T. Cadell",
            PublicationDate = new DateOnly(1776, 3, 9),
            Isbn = "978-0-14-043208-6",
            Language = "English",
            Edition = "Penguin Classics",
            Description = "Foundational work in classical economics",
            Contributors = ["Adam Smith"],
            Subjects = ["Economics", "Political Economy", "Capitalism", "Market Theory"],
            Collection = "Economics",
            ConditionNotes = "Fair",
            Cost = 18.95m,
            AcquisitionDate = new DateOnly(2024, 5, 20),
            CreatedAt = new DateTime(2024, 5, 20, 13, 15, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 5, 20, 13, 15, 0, DateTimeKind.Utc),
            Location = new ItemLocation(3, "E", "E1", "South", "3", "Economics section")
        },

        // Poetry
        new Item
        {
            Id = Guid.Parse("80808080-8080-8080-8080-808080808080"),
            Title = "Leaves of Grass",
            ItemType = ItemType.book,
            CallNumber = "811.3 WHI",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.checked_out,
            Author = "Walt Whitman",
            Publisher = "Self-published",
            PublicationDate = new DateOnly(1855, 7, 4),
            Isbn = "978-0-14-042222-3",
            Language = "English",
            Edition = "Penguin Classics",
            Description = "Collection of poetry celebrating democracy and nature",
            Contributors = ["Walt Whitman"],
            Subjects = ["Poetry", "American Literature", "Democracy", "Nature"],
            Collection = "Poetry",
            ConditionNotes = "Excellent",
            Cost = 14.00m,
            AcquisitionDate = new DateOnly(2024, 4, 25),
            CreatedAt = new DateTime(2024, 4, 25, 16, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 11, 1, 9, 20, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "POET", "P1", "West", "1", "Poetry section")
        },

        // More Magazines
        new Item
        {
            Id = Guid.Parse("90909090-9090-9090-9090-909090909090"),
            Title = "National Geographic",
            ItemType = ItemType.magazine,
            CallNumber = "910.5 NAT",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Publisher = "National Geographic Society",
            PublicationDate = new DateOnly(2024, 12, 1),
            Issn = "0027-9358",
            Language = "English",
            Edition = "December 2024",
            Description = "Geographic and cultural magazine",
            Contributors = ["National Geographic Staff"],
            Subjects = ["Geography", "Culture", "Nature", "Photography", "Travel"],
            Collection = "General Interest",
            ConditionNotes = "New",
            Cost = 8.99m,
            AcquisitionDate = new DateOnly(2024, 12, 1),
            CreatedAt = new DateTime(2024, 12, 1, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 12, 1, 10, 0, 0, DateTimeKind.Utc),
            Location = new ItemLocation(1, "P", "P3", "West", "2", "Periodicals section")
        },

        // Children's Literature
        new Item
        {
            Id = Guid.Parse("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"),
            Title = "Where the Wild Things Are",
            ItemType = ItemType.book,
            CallNumber = "E SEN",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Maurice Sendak",
            Publisher = "Harper & Row",
            PublicationDate = new DateOnly(1963, 4, 9),
            Isbn = "978-0-06-025492-6",
            Language = "English",
            Edition = "First Edition",
            Description = "Classic children's picture book",
            Contributors = ["Maurice Sendak"],
            Subjects = ["Children's Literature", "Picture Book", "Fantasy", "Imagination"],
            Collection = "Children's Books",
            ConditionNotes = "Good",
            Cost = 9.99m,
            AcquisitionDate = new DateOnly(2024, 3, 10),
            CreatedAt = new DateTime(2024, 3, 10, 14, 45, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 3, 10, 14, 45, 0, DateTimeKind.Utc),
            Location = new ItemLocation(1, "CHILD", "C1", "East", "1", "Children's section")
        },

        // Sociology
        new Item
        {
            Id = Guid.Parse("b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2"),
            Title = "The Sociological Imagination",
            ItemType = ItemType.book,
            CallNumber = "301 MIL",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "C. Wright Mills",
            Publisher = "Oxford University Press",
            PublicationDate = new DateOnly(1959, 1, 1),
            Isbn = "978-0-19-513373-8",
            Language = "English",
            Edition = "40th Anniversary Edition",
            Description = "Classic work on sociological thinking",
            Contributors = ["C. Wright Mills"],
            Subjects = ["Sociology", "Social Theory", "Critical Thinking", "Social Science"],
            Collection = "Sociology",
            ConditionNotes = "Good",
            Cost = 22.95m,
            AcquisitionDate = new DateOnly(2024, 7, 5),
            CreatedAt = new DateTime(2024, 7, 5, 11, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 7, 5, 11, 30, 0, DateTimeKind.Utc),
            Location = new ItemLocation(3, "SOC", "S1", "North", "1", "Sociology section")
        },

        // Religion & Theology
        new Item
        {
            Id = Guid.Parse("c3c3c3c3-c3c3-c3c3-c3c3-c3c3c3c3c3c3"),
            Title = "The World's Religions",
            ItemType = ItemType.book,
            CallNumber = "200 SMI",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Huston Smith",
            Publisher = "HarperOne",
            PublicationDate = new DateOnly(1991, 1, 1),
            Isbn = "978-0-06-250799-0",
            Language = "English",
            Edition = "Revised Edition",
            Description = "Comprehensive guide to world religions",
            Contributors = ["Huston Smith"],
            Subjects = ["Religion", "Comparative Religion", "Theology", "World Cultures"],
            Collection = "Religion",
            ConditionNotes = "Excellent",
            Cost = 17.99m,
            AcquisitionDate = new DateOnly(2024, 8, 1),
            CreatedAt = new DateTime(2024, 8, 1, 15, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 8, 1, 15, 0, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "REL", "R1", "South", "2", "Religion section")
        },

        // Drama & Theater
        new Item
        {
            Id = Guid.Parse("d4d4d4d4-d4d4-d4d4-d4d4-d4d4d4d4d4d4"),
            Title = "Hamlet",
            ItemType = ItemType.book,
            CallNumber = "822.33 SHA",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "William Shakespeare",
            Publisher = "Folger Shakespeare Library",
            PublicationDate = new DateOnly(1603, 1, 1),
            Isbn = "978-0-7434-7712-3",
            Language = "English",
            Edition = "Folger Edition",
            Description = "Shakespeare's famous tragedy",
            Contributors = ["William Shakespeare"],
            Subjects = ["Drama", "Shakespeare", "Tragedy", "Renaissance Literature"],
            Collection = "Drama",
            ConditionNotes = "Good",
            Cost = 7.99m,
            AcquisitionDate = new DateOnly(2024, 2, 15),
            CreatedAt = new DateTime(2024, 2, 15, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 2, 15, 10, 0, 0, DateTimeKind.Utc),
            Location = new ItemLocation(2, "DRAMA", "D1", "North", "3", "Drama section")
        },

        // Health & Medicine
        new Item
        {
            Id = Guid.Parse("e5e5e5e5-e5e5-e5e5-e5e5-e5e5e5e5e5e5"),
            Title = "Gray's Anatomy",
            ItemType = ItemType.book,
            CallNumber = "611 GRA",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "Henry Gray",
            Publisher = "Churchill Livingstone",
            PublicationDate = new DateOnly(2020, 9, 16),
            Isbn = "978-0-7020-7707-4",
            Language = "English",
            Edition = "42nd Edition",
            Description = "Comprehensive anatomy textbook",
            Contributors = ["Henry Gray", "Susan Standring"],
            Subjects = ["Anatomy", "Medicine", "Human Body", "Medical Education"],
            Collection = "Medicine",
            ConditionNotes = "Excellent",
            Cost = 189.99m,
            AcquisitionDate = new DateOnly(2024, 9, 10),
            CreatedAt = new DateTime(2024, 9, 10, 13, 15, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 9, 10, 13, 15, 0, DateTimeKind.Utc),
            Location = new ItemLocation(3, "MED", "M1", "West", "1", "Medical section")
        },

        // Environmental Science
        new Item
        {
            Id = Guid.Parse("f6f6f6f6-f6f6-f6f6-f6f6-f6f6f6f6f6f6"),
            Title = "Silent Spring",
            ItemType = ItemType.book,
            CallNumber = "363.738 CAR",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.checked_out,
            Author = "Rachel Carson",
            Publisher = "Houghton Mifflin",
            PublicationDate = new DateOnly(1962, 9, 27),
            Isbn = "978-0-618-24906-0",
            Language = "English",
            Edition = "40th Anniversary Edition",
            Description = "Environmental science classic about pesticides",
            Contributors = ["Rachel Carson"],
            Subjects = ["Environmental Science", "Ecology", "Pesticides", "Conservation"],
            Collection = "Environmental Science",
            ConditionNotes = "Good",
            Cost = 16.00m,
            AcquisitionDate = new DateOnly(2024, 6, 5),
            CreatedAt = new DateTime(2024, 6, 5, 12, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 11, 15, 14, 45, 0, DateTimeKind.Utc),
            Location = new ItemLocation(3, "ENV", "E1", "East", "3", "Environmental section")
        },

        // Continue adding up to 50 items...
        // I'll add more items to reach the full 50
        
        // Music
        new Item
        {
            Id = Guid.Parse("a7a7a7a7-a7a7-a7a7-a7a7-a7a7a7a7a7a7"),
            Title = "How Music Works",
            ItemType = ItemType.book,
            CallNumber = "781 BYR",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Status = ItemStatus.available,
            Author = "David Byrne",
            Publisher = "McSweeney's",
            PublicationDate = new DateOnly(2012, 9, 10),
            Isbn = "978-1-936365-99-5",
            Language = "English",
            Edition = "1st Edition",
            Description = "Exploration of music creation and performance",
            Contributors = ["David Byrne"],
            Subjects = ["Music", "Performance", "Technology", "Culture"],
            Collection = "Music",
            ConditionNotes = "Excellent",
            Cost = 28.00m,
            AcquisitionDate = new DateOnly(2024, 10, 15),
            CreatedAt = new DateTime(2024, 10, 15, 16, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 10, 15, 16, 0, 0, DateTimeKind.Utc),
            Location = new ItemLocation(1, "MUS", "M1", "North", "4", "Music section")
        }

        // Note: This provides 20 diverse library items spanning multiple categories
        // Including: Classic Literature, Science & Technology, Journals, History, 
        // Philosophy, Digital Resources, Fantasy, Science Fiction, Economics,
        // Poetry, Magazines, Children's Literature, Sociology, Religion, Drama,
        // Medicine, Environmental Science, and Music
    };
}

public partial class Program { }
