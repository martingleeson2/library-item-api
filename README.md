# Library Item API

A modern REST API for managing library items built with .NET 8 minimal APIs. This project provides comprehensive CRUD operations for library collections with advanced filtering, pagination, authentication, and validation.

## 🚀 Features

- **Full CRUD Operations**: Create, read, update, patch, and delete library items
- **Advanced Querying**: Rich filtering by title, author, ISBN, location, status, and more
- **Pagination & Sorting**: Efficient pagination with customizable sorting options
- **API Key Authentication**: Secure authentication with configurable API keys
- **Comprehensive Validation**: FluentValidation with detailed error responses
- **Multiple Database Support**: SQLite for production, InMemory for development/testing
- **Clean Architecture**: Layered design with CQRS pattern implementation
- **OpenAPI Documentation**: Interactive Swagger UI for API exploration
- **Health Checks**: Built-in health monitoring endpoints
- **Structured Logging**: Comprehensive logging with correlation IDs
- **Development Seed Data**: Pre-populated sample data for testing
- **100% Test Coverage**: Complete unit and integration test coverage

## 🏗️ Architecture

This project follows Clean Architecture principles with clear separation of concerns:

```
API Layer (Endpoints, Middleware, Auth)
    ↓
Application Layer (CQRS Handlers, DTOs, Validators, Mappings)
    ↓
Domain Layer (Entities, Enums, Value Objects)
    ↓
Infrastructure Layer (EF Core DbContext, Persistence)
```

### Key Components

- **API Layer**: Minimal API endpoints, authentication middleware, error handling
- **Application Layer**: CQRS command/query handlers, data transfer objects, validation rules
- **Domain Layer**: Core business entities and domain logic
- **Infrastructure Layer**: Database context, migrations, and data access

## 📋 Prerequisites

- .NET 8 SDK (8.0.100 or later)
- SQLite (for production database, optional for development)

## 🛠️ Quick Start

### 1. Clone and Restore

```bash
git clone <repository-url>
cd library-item-api
dotnet restore
```

### 2. Run in Development Mode

```bash
dotnet run --project Example.LibraryItem
```

The API will start on `https://localhost:7278` and `http://localhost:5288` with Swagger UI available at `/swagger`.

### 3. Test the API

Use the Swagger UI or curl commands to test endpoints. Default development API key: `dev-key`

```bash
# List items
curl -H "X-API-Key: dev-key" https://localhost:7278/v1/items

# Create an item
curl -X POST -H "Content-Type: application/json" -H "X-API-Key: dev-key" \
  -d '{"title":"Sample Book","item_type":"book","call_number":"001.42","location":{"floor":1,"section":"A","shelf_code":"B"}}' \
  https://localhost:7278/v1/items
```

## ⚙️ Configuration

### Development Configuration

For local development, the app uses in-memory database with pre-seeded data:

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

### Production Configuration

For production deployment, configure SQLite database and secure API keys:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=library.db"
  },
  "Database": {
    "Provider": "sqlite"
  },
  "ApiKeys": [
    "production-secure-key-12345",
    "backup-production-key-67890"
  ]
}
```

### Environment Variables

```bash
# Database
ConnectionStrings__Default="Data Source=library.db"
Database__Provider="sqlite"

# API Keys
ApiKeys__0="secure-key-1"
ApiKeys__1="secure-key-2"

# Environment
ASPNETCORE_ENVIRONMENT="Production"
```


## 🔐 Authentication

All API endpoints require API key authentication.

### API Key Authentication

Include the `X-API-Key` header with a valid API key:

```bash
curl -H "X-API-Key: dev-key" https://localhost:7278/v1/items
```


### Development Keys

- `dev-key` (default)
- `test-key`
- `local-development-key`

### Production Security

- Use strong, unique API keys (avoid placeholder keys like "dev-key")
- Rotate keys regularly
- Store keys securely (environment variables, Key Vault)
- The application will refuse to start with placeholder keys in non-Development environments

## 📚 API Endpoints

### Base URL

```
https://localhost:7278/v1/items
```

### Core Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/v1/items` | List items with filtering/pagination |
| GET | `/v1/items/{id}` | Get single item by ID |
| POST | `/v1/items` | Create new item |
| PUT | `/v1/items/{id}` | Update entire item |
| PATCH | `/v1/items/{id}` | Partially update item |
| DELETE | `/v1/items/{id}` | Delete item |

### Query Parameters (GET /v1/items)

- **Pagination**: `page`, `limit` (1-100)
- **Filters**: `title`, `author`, `isbn`, `item_type`, `status`, `collection`
- **Location**: `location_floor`, `location_section`
- **Sorting**: `sort_by`, `sort_order` (asc/desc)
- **Date Range**: `publication_year_from`, `publication_year_to`

### Response Format

All responses use snake_case JSON naming:

```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "limit": 10,
    "total_items": 150,
    "total_pages": 15,
    "has_next": true,
    "has_previous": false
  },
  "_links": {
    "self": "/v1/items?page=1&limit=10",
    "next": "/v1/items?page=2&limit=10",
    "first": "/v1/items?page=1&limit=10",
    "last": "/v1/items?page=15&limit=10"
  }
}
```

## 🧪 Testing

### Run All Tests

```bash
dotnet test
```

### Run with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Categories

- **Unit Tests**: Handler logic, validation rules, mapping functions
- **Integration Tests**: Full API endpoint testing with test database
- **Coverage**: 100% line/branch/method coverage requirement

## 📖 API Documentation

### Swagger UI

- **Development**: `https://localhost:7278/swagger`
- **Interactive**: Try endpoints directly from the browser
- **Authentication**: Enter API key in the "Authorize" button

### OpenAPI Spec

- **JSON**: `/swagger/v1/swagger.json`
- **YAML**: Available via Swagger UI export

## 🏥 Health Checks

### Endpoints

- `GET /health` - Overall health status
- `GET /health/db` - Database connectivity check

### Response

```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "db",
      "status": "Healthy",
      "description": "Database connection is healthy"
    }
  ]
}
```

## 🔍 Development

### Project Structure

```
Example.LibraryItem/
├── Api/
│   ├── Endpoints.cs          # Minimal API endpoint definitions
│   ├── Authentication/       # API Key and JWT authentication
│   ├── Middleware/           # Request correlation, exception handling
│   └── Services/             # Helper services and utilities
├── Application/
│   ├── Dtos/                 # Data transfer objects
│   ├── Handlers/             # CQRS command/query handlers
│   ├── Validators/           # FluentValidation rules
│   └── Services/             # Application services
├── Domain/
│   ├── Entities.cs           # Domain entities
│   └── Enums.cs              # Domain enumerations
├── Infrastructure/
│   └── LibraryDbContext.cs   # EF Core database context
└── Program.cs                # Application startup

Example.LibraryItem.Tests/
├── Handlers/                 # Handler unit tests
├── Validators/               # Validation rule tests
└── Integration/              # Full API integration tests
```

### Adding New Features

1. **Define DTOs** in `Application/Dtos/`
2. **Create Validators** in `Application/Validators/`
3. **Implement Handler** in `Application/Handlers/`
4. **Add Endpoint** in `Api/Endpoints.cs`
5. **Update Mappings** in `Application/Mappings.cs`
6. **Add Tests** for all new components

### Code Standards

- **C# 12** features and patterns
- **Async/await** for all I/O operations
- **Records** for immutable DTOs
- **PascalCase** for all property names
- **Structured logging** with semantic values
- **XML documentation** for all public APIs

## 📊 Monitoring & Observability

### Logging

- Structured JSON logging with correlation IDs
- Request correlation IDs (`X-Request-ID`)
- Sensitive data automatically redacted
- Configurable log levels per environment

### Health Monitoring

- Database connectivity checks
- Application health status
- Custom health check endpoints

## 🚀 Deployment

### Production Checklist

- [ ] Configure secure API keys
- [ ] Set up SQLite database with proper permissions
- [ ] Configure HTTPS certificates
- [ ] Set environment variables
- [ ] Configure logging destination
- [ ] Set up health check monitoring
- [ ] Configure backup strategy
- [ ] Test all endpoints thoroughly

### Environment Variables

```bash
# Required
ASPNETCORE_ENVIRONMENT=Production
ApiKeys__0=your-secure-api-key

# Database
ConnectionStrings__Default=Data Source=library.db
Database__Provider=sqlite

# Optional
ASPNETCORE_URLS=https://0.0.0.0:8080
Logging__LogLevel__Default=Information
```

### Running in Production

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

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Setup

```bash
# Install dependencies
dotnet restore

# Run tests
dotnet test

# Run with hot reload
dotnet watch --project Example.LibraryItem run

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with .NET 8 minimal APIs
- Uses Entity Framework Core for data access
- FluentValidation for input validation
- Swashbuckle for OpenAPI documentation
- Clean Architecture principles
- CQRS pattern implementation

---

**Last Updated**: September 2025