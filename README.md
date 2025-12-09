# APG Backend - ASP.NET Core 8 Web API

A clean architecture .NET 8 Web API backend with SQL Server database, containerized with Docker.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with the following layers:

```
┌─────────────────────────────────────────┐
│           APG.API (Presentation)        │
│   Controllers, Middleware, Filters      │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      APG.Application (Business Logic)   │
│   Services, DTOs, Interfaces            │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│     APG.Persistence (Data Access)       │
│   DbContext, Repositories, Configs      │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│        APG.Domain (Core)                │
│   Entities, Value Objects, Interfaces   │
└─────────────────────────────────────────┘
```

### Layer Responsibilities

- **APG.API**: HTTP endpoints, request/response handling, authentication
- **APG.Application**: Business logic, validation, orchestration
- **APG.Persistence**: Database access, EF Core configurations, migrations
- **APG.Domain**: Core business entities and rules (no dependencies)

## 🚀 Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for local development)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) (optional)

### Quick Start with Docker

1. **Clone the repository** (if not already done)

2. **Start the containers** from the `APG_Backend` directory:
   ```bash
   docker compose up -d
   ```

3. **Verify services are running**:
   ```bash
   docker compose ps
   ```
   
   You should see:
   - `apg-sqlserver` running on port `1433`
   - `apg-api` running on port `5000`

4. **Check API health**:
   ```bash
   curl http://localhost:5000/health
   ```

5. **Access Swagger UI**:
   Open [http://localhost:5000/swagger](http://localhost:5000/swagger)

### Local Development (Without Docker)

1. **Start SQL Server container**:
   ```bash
   docker compose up sqlserver -d
   ```

2. **Restore NuGet packages**:
   ```bash
   cd APG_Backend
   dotnet restore
   ```

3. **Apply migrations**:
   ```bash
   # macOS/Linux
   ./scripts/update-database.sh
   
   # Windows
   scripts\update-database.bat
   ```

4. **Run the API**:
   ```bash
   cd src/APG.API
   dotnet run
   ```

5. **Access Swagger UI**:
   Open [http://localhost:5000/swagger](http://localhost:5000/swagger)

## 🗄️ Database Setup

For comprehensive database setup instructions, including:
- Connection string configuration
- EF Core migrations
- Connecting from Visual Studio
- Troubleshooting

**See: [docs/README_DB.md](./docs/README_DB.md)**

### Quick Migration Commands

```bash
# Create a new migration
./scripts/create-migration.sh AddNewTable

# Apply migrations to database
./scripts/update-database.sh

# List all migrations
./scripts/list-migrations.sh

# Remove last migration
./scripts/remove-migration.sh

# Generate SQL script for production
./scripts/generate-migration-sql.sh
```

Windows users: Replace `.sh` with `.bat`

## 📁 Project Structure

```
APG_Backend/
├── src/
│   ├── APG.API/                    # Web API project
│   │   ├── Controllers/            # API endpoints
│   │   ├── Properties/
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Program.cs              # Application entry point
│   │
│   ├── APG.Application/            # Business logic layer
│   │   └── APG.Application.csproj
│   │
│   ├── APG.Persistence/            # Data access layer
│   │   ├── Data/
│   │   │   └── AppDbContext.cs     # EF Core DbContext
│   │   ├── Migrations/             # EF Core migrations
│   │   └── DependencyInjection.cs  # Service registration
│   │
│   └── APG.Domain/                 # Core domain layer
│       └── Entities/
│           ├── BaseEntity.cs       # Base entity class
│           └── TestEntity.cs       # Sample entity
│
├── scripts/                        # Helper scripts
│   ├── create-migration.sh/.bat
│   ├── update-database.sh/.bat
│   ├── remove-migration.sh/.bat
│   ├── generate-migration-sql.sh/.bat
│   └── list-migrations.sh/.bat
│
├── APG_Backend.sln                 # Visual Studio solution
├── Dockerfile                      # Multi-stage Docker build
├── .dockerignore
├── .gitignore
├── README.md                       # This file
└── README_DB.md                    # Database documentation
```

## 🔧 Configuration

### Connection Strings

Connection strings are configured differently based on the environment:

#### Local Development (API outside Docker)
**File**: `src/APG.API/appsettings.Development.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=APGDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
  }
}
```

#### Docker Environment
**File**: `docker-compose.yml`
```yaml
environment:
  ConnectionStrings__DefaultConnection: "Server=sqlserver,1433;Database=APGDb;..."
```

**Note**: In Docker, use `Server=sqlserver` (service name) instead of `localhost`.

### Security Best Practices

⚠️ **Never commit passwords to source control!**

For local development, use .NET User Secrets:

```bash
cd src/APG.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=APGDb;User Id=sa;Password=<YOUR_PASSWORD>;TrustServerCertificate=True"
```

For production, use:
- Azure Key Vault
- Docker Secrets
- Environment variables (injected at runtime)

## 🧪 Testing the API

### Using Swagger

1. Open [http://localhost:5000/swagger](http://localhost:5000/swagger)
2. Try the `/api/Test` endpoints:
   - `GET /api/Test` - Get all test entities
   - `POST /api/Test` - Create a new test entity
   - `GET /api/Test/{id}` - Get entity by ID
   - `PUT /api/Test/{id}` - Update entity
   - `DELETE /api/Test/{id}` - Delete entity

### Using curl

```bash
# Create a test entity
curl -X POST http://localhost:5000/api/Test \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Item","description":"Sample description","isActive":true}'

# Get all test entities
curl http://localhost:5000/api/Test

# Get entity by ID
curl http://localhost:5000/api/Test/1

# Update entity
curl -X PUT http://localhost:5000/api/Test/1 \
  -H "Content-Type: application/json" \
  -d '{"name":"Updated Item","description":"Updated description","isActive":true}'

# Delete entity
curl -X DELETE http://localhost:5000/api/Test/1
```

## 🐳 Docker Commands

```bash
# Start all services
docker compose up -d

# Start only SQL Server
docker compose up sqlserver -d

# View logs
docker compose logs -f

# View API logs only
docker compose logs -f api

# Stop all services
docker compose down

# Stop and remove volumes (⚠️ deletes database data)
docker compose down -v

# Rebuild containers
docker compose build
docker compose up -d

# Restart a specific service
docker compose restart api

# Execute command in container
docker compose exec api bash
docker compose exec sqlserver bash
```

## 📊 Database Management

### Using SQL Server Management Studio (SSMS)

Connect using:
- **Server**: `localhost,1433`
- **Authentication**: SQL Server Authentication
- **Login**: `sa`
- **Password**: `YourStrong@Passw0rd`

### Using Visual Studio SQL Server Object Explorer

1. View → SQL Server Object Explorer
2. Right-click SQL Server → Add SQL Server
3. Enter connection details (see above)
4. Browse to `APGDb` database

### Using sqlcmd (command line)

```bash
# Connect to SQL Server
docker compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd"

# Sample queries
SELECT name FROM sys.databases;
GO

USE APGDb;
GO

SELECT * FROM TestEntities;
GO
```

## 🔍 Troubleshooting

### Port Already in Use

If port 1433 or 5000 is already in use:

1. **Check what's using the port**:
   ```bash
   # macOS/Linux
   lsof -i :1433
   lsof -i :5000
   
   # Windows
   netstat -ano | findstr :1433
   netstat -ano | findstr :5000
   ```

2. **Change ports in `docker-compose.yml`**:
   ```yaml
   services:
     sqlserver:
       ports:
         - "14330:1433"  # Use different host port
     api:
       ports:
         - "5001:80"     # Use different host port
   ```

### Cannot Connect to Database

1. **Wait for SQL Server to initialize** (takes ~30 seconds on first start):
   ```bash
   docker compose logs -f sqlserver
   ```
   Look for: "SQL Server is now ready for client connections"

2. **Verify SQL Server is running**:
   ```bash
   docker compose ps
   docker compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
     -S localhost -U sa -P "YourStrong@Passw0rd" \
     -Q "SELECT @@VERSION"
   ```

3. **Check connection string** in `appsettings.json` or environment variables

### Migrations Not Applying

1. **Ensure .NET SDK is installed**:
   ```bash
   dotnet --version
   ```

2. **Check if DbContext can be created**:
   ```bash
   dotnet ef dbcontext info \
     --project src/APG.Persistence/APG.Persistence.csproj \
     --startup-project src/APG.API/APG.API.csproj
   ```

3. **Apply migrations manually**:
   ```bash
   ./scripts/update-database.sh
   ```

## 📝 Development Workflow

### Adding a New Entity

1. **Create entity in Domain layer**:
   ```csharp
   // src/APG.Domain/Entities/MyEntity.cs
   namespace APG.Domain.Entities;
   
   public class MyEntity : BaseEntity
   {
       public string Name { get; set; } = string.Empty;
       // ... other properties
   }
   ```

2. **Add DbSet to AppDbContext**:
   ```csharp
   // src/APG.Persistence/Data/AppDbContext.cs
   public DbSet<MyEntity> MyEntities { get; set; }
   ```

3. **Configure entity in OnModelCreating**:
   ```csharp
   modelBuilder.Entity<MyEntity>(entity =>
   {
       entity.ToTable("MyEntities");
       entity.HasKey(e => e.Id);
       entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
   });
   ```

4. **Create migration**:
   ```bash
   ./scripts/create-migration.sh AddMyEntity
   ```

5. **Apply migration**:
   ```bash
   ./scripts/update-database.sh
   ```

6. **Create controller**:
   ```csharp
   // src/APG.API/Controllers/MyEntityController.cs
   [ApiController]
   [Route("api/[controller]")]
   public class MyEntityController : ControllerBase
   {
       private readonly AppDbContext _context;
       
       public MyEntityController(AppDbContext context)
       {
           _context = context;
       }
       
       // ... implement endpoints
   }
   ```

## 🔐 Security Checklist

- [ ] Change default SQL Server password
- [ ] Use User Secrets for local development
- [ ] Use Azure Key Vault or similar for production
- [ ] Enable HTTPS in production
- [ ] Implement authentication (JWT, OAuth, etc.)
- [ ] Add authorization policies
- [ ] Enable CORS only for trusted origins
- [ ] Implement rate limiting
- [ ] Add input validation
- [ ] Enable logging and monitoring

## 📚 Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Docker Documentation](https://docs.docker.com/)
- [SQL Server on Docker](https://hub.docker.com/_/microsoft-mssql-server)

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Test thoroughly
4. Create a pull request

## 📝 License

[Your License Here]

---

## 📚 Additional Documentation

- **[docs/README_DB.md](./docs/README_DB.md)** - Complete database setup guide
- **[docs/QUICKSTART.md](./docs/QUICKSTART.md)** - Quick start guide  
- **[docs/SETUP_GUIDE.md](./docs/SETUP_GUIDE.md)** - Environment setup
- **[docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md)** - Architecture diagrams
- **[docs/IMPLEMENTATION_CHECKLIST.md](./docs/IMPLEMENTATION_CHECKLIST.md)** - Progress tracking
- **[docs/SETUP_COMPLETE.md](./docs/SETUP_COMPLETE.md)** - Setup summary

**Need help with database setup?** See [docs/README_DB.md](./docs/README_DB.md)

**Last Updated**: December 4, 2025
