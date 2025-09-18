using Example.LibraryItem.Api;
using Example.LibraryItem.Api.Authentication;
using Example.LibraryItem.Api.Extensions;
using Example.LibraryItem.Api.Interfaces;
using Example.LibraryItem.Api.Services;
using Example.LibraryItem.Application;
using Example.LibraryItem.Application.Interfaces;
using Example.LibraryItem.Application.Services;
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Library Management System API", Version = "v1" });
    
    // API Key authentication
    c.AddSecurityDefinition(ApiKeyDefaults.Scheme, new OpenApiSecurityScheme
    {
        Description = "API Key via X-API-Key header",
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

public partial class Program { }
