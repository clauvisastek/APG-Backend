# Database Setup Guide - SQL Server with Docker

This guide explains how to set up, run, and connect to the SQL Server database for the APG Backend API.

## 📋 Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Database Configuration](#database-configuration)
- [Running Migrations](#running-migrations)
- [Connecting from Visual Studio](#connecting-from-visual-studio)
- [Troubleshooting](#troubleshooting)
- [Best Practices](#best-practices)

---

## 🎯 Overview

The APG Backend uses:
- **Database**: Microsoft SQL Server 2022 (Linux container)
- **ORM**: Entity Framework Core 8.0
- **Architecture**: Clean Architecture with separate layers
- **Containerization**: Docker Compose for both API and SQL Server

### Architecture Layers

```
APG.API          → Web API layer (Controllers, endpoints)
APG.Application  → Business logic layer
APG.Persistence  → Data access layer (DbContext, configurations)
APG.Domain       → Domain entities and interfaces
```

---

## 📦 Prerequisites

1. **Docker Desktop** - [Download here](https://www.docker.com/products/docker-desktop)
2. **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
3. **Visual Studio 2022** (optional) - For SQL Server Object Explorer
4. **SQL Server Management Studio** (optional) - For advanced database management

---

## 🚀 Quick Start

### 1. Start the Stack

From the root directory (where `docker-compose.yml` is located):

```bash
# Start SQL Server and API containers
docker compose up -d

# Check if containers are running
docker compose ps

# View logs
docker compose logs -f
```

The following services will start:
- **SQL Server**: `localhost:1433`
- **API**: `localhost:5000`

### 2. Verify the API is Running

Open your browser or use curl:

```bash
curl http://localhost:5000/health
```

You should see a healthy status response.

### 3. Access Swagger UI

Navigate to: `http://localhost:5000/swagger`

---

## ⚙️ Database Configuration

### Connection Strings

#### Local Development (API running outside Docker)
File: `src/APG.API/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=APGDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

#### Docker Environment (API running in container)
File: `docker-compose.yml`

```yaml
environment:
  ConnectionStrings__DefaultConnection: "Server=sqlserver,1433;Database=APGDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**Key Difference**: 
- Local: Uses `Server=localhost,1433`
- Docker: Uses `Server=sqlserver,1433` (service name)

### 🔐 Security Note

⚠️ **IMPORTANT**: The password `YourStrong@Passw0rd` is a placeholder for development only!

**For production:**
1. Use strong, unique passwords
2. Store secrets in:
   - Azure Key Vault
   - Docker Secrets
   - Environment variables (not in source control)
   - .NET User Secrets for local development

**Setting User Secrets (Recommended for Local Dev):**

```bash
cd src/APG.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=APGDb;User Id=sa;Password=<YOUR_SECURE_PASSWORD>;TrustServerCertificate=True"
```

---

## 🔄 Running Migrations

### Method 1: Automatic Migrations (Configured by Default)

The API is configured to automatically apply migrations on startup. This happens in `Program.cs`:

```csharp
context.Database.Migrate();
```

**When to use**: Development and testing environments.

**To disable**: Comment out the migration code in `Program.cs`.

### Method 2: Manual Migrations with .NET CLI

#### Create a New Migration

```bash
# From the solution root
dotnet ef migrations add InitialCreate \
  --project src/APG.Persistence/APG.Persistence.csproj \
  --startup-project src/APG.API/APG.API.csproj \
  --output-dir Migrations

# Or use the migration script
./scripts/create-migration.sh InitialCreate
```

#### Apply Migrations

```bash
# Apply to local database (SQL Server running in Docker)
dotnet ef database update \
  --project src/APG.Persistence/APG.Persistence.csproj \
  --startup-project src/APG.API/APG.API.csproj

# Or use the migration script
./scripts/update-database.sh
```

#### Remove Last Migration

```bash
dotnet ef migrations remove \
  --project src/APG.Persistence/APG.Persistence.csproj \
  --startup-project src/APG.API/APG.API.csproj

# Or use the script
./scripts/remove-migration.sh
```

### Method 3: Migrations Inside Docker Container

If you need to run migrations from within the Docker container:

```bash
# Execute migration command in the running API container
docker compose exec api dotnet ef database update \
  --project /src/src/APG.Persistence/APG.Persistence.csproj \
  --startup-project /src/src/APG.API/APG.API.csproj
```

### Method 4: Generate SQL Script for Migrations

Useful for production deployments:

```bash
# Generate SQL script for all migrations
dotnet ef migrations script \
  --project src/APG.Persistence/APG.Persistence.csproj \
  --startup-project src/APG.API/APG.API.csproj \
  --output migrations.sql \
  --idempotent

# Or use the script
./scripts/generate-migration-sql.sh
```

---

## 🔌 Connecting from Visual Studio

### SQL Server Object Explorer

1. **Open Visual Studio 2022**

2. **Open SQL Server Object Explorer**
   - Go to: `View` → `SQL Server Object Explorer`

3. **Add Connection**
   - Right-click on `SQL Server` node
   - Click `Add SQL Server...`

4. **Enter Connection Details**

   ```
   Server type:        Database Engine
   Server name:        localhost,1433
   Authentication:     SQL Server Authentication
   Login:              sa
   Password:           YourStrong@Passw0rd
   ```

5. **Connection Properties** (if needed)
   - Check `Trust Server Certificate` ✓
   - Or add `TrustServerCertificate=True` to connection string

6. **Connect**
   - Click `Connect`
   - Navigate to: `localhost,1433` → `Databases` → `APGDb`

### Visual Studio Server Explorer

1. **Open Server Explorer**
   - Go to: `View` → `Server Explorer`

2. **Add Connection**
   - Right-click `Data Connections`
   - Click `Add Connection...`

3. **Choose Data Source**
   - Data source: `Microsoft SQL Server`
   - Click `Continue`

4. **Add Connection Dialog**
   ```
   Server name:              localhost,1433
   Authentication:           SQL Server Authentication
   User name:                sa
   Password:                 YourStrong@Passw0rd
   Select or enter database: APGDb
   ```

5. **Advanced Properties** (if SSL error occurs)
   - Click `Advanced...`
   - Find `TrustServerCertificate`
   - Set to `True`

6. **Test Connection**
   - Click `Test Connection`
   - Should show "Test connection succeeded"

### SQL Server Management Studio (SSMS)

1. **Open SSMS**

2. **Connect to Server**
   ```
   Server type:        Database Engine
   Server name:        localhost,1433
   Authentication:     SQL Server Authentication
   Login:              sa
   Password:           YourStrong@Passw0rd
   Encryption:         Optional (or Mandatory with Trust certificate)
   ```

3. **Connect** and browse the `APGDb` database

### Azure Data Studio

1. **Open Azure Data Studio**

2. **New Connection**
   ```
   Connection type:    Microsoft SQL Server
   Server:             localhost,1433
   Authentication:     SQL Login
   User name:          sa
   Password:           YourStrong@Passw0rd
   Database:           APGDb
   Trust certificate:  True
   ```

---

## 🐛 Troubleshooting

### Issue: "Cannot connect to SQL Server"

**Solution:**

1. Check if SQL Server container is running:
   ```bash
   docker compose ps
   docker compose logs sqlserver
   ```

2. Wait for SQL Server to be ready (it takes ~30 seconds on first start):
   ```bash
   docker compose logs -f sqlserver
   ```
   Look for: "SQL Server is now ready for client connections"

3. Test connection with sqlcmd:
   ```bash
   docker compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
     -S localhost -U sa -P "YourStrong@Passw0rd" \
     -Q "SELECT @@VERSION"
   ```

### Issue: "Login failed for user 'sa'"

**Possible causes:**
1. Incorrect password
2. SQL Server not fully initialized
3. Container restarted and password changed

**Solution:**

```bash
# Recreate containers with fresh state
docker compose down -v
docker compose up -d

# Wait for SQL Server to be ready
docker compose logs -f sqlserver
```

### Issue: "A connection was successfully established, but then an error occurred during the login process"

**Solution:** Add `TrustServerCertificate=True` to your connection string.

### Issue: "Database does not exist"

**Solution:** 

The API will create the database automatically on first run. If not:

```bash
# Create database manually
docker compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" \
  -Q "CREATE DATABASE APGDb"

# Then run migrations
dotnet ef database update \
  --project src/APG.Persistence/APG.Persistence.csproj \
  --startup-project src/APG.API/APG.API.csproj
```

### Issue: "The wait operation timed out"

**Solution:**

1. Check if port 1433 is already in use:
   ```bash
   lsof -i :1433  # macOS/Linux
   netstat -ano | findstr :1433  # Windows
   ```

2. Use a different port in `docker-compose.yml`:
   ```yaml
   ports:
     - "14330:1433"
   ```

3. Update connection string to use new port:
   ```
   Server=localhost,14330;...
   ```

### Issue: Migrations not applying

**Solution:**

1. Check if DbContext can be instantiated:
   ```bash
   dotnet ef dbcontext info \
     --project src/APG.Persistence/APG.Persistence.csproj \
     --startup-project src/APG.API/APG.API.csproj
   ```

2. List existing migrations:
   ```bash
   dotnet ef migrations list \
     --project src/APG.Persistence/APG.Persistence.csproj \
     --startup-project src/APG.API/APG.API.csproj
   ```

3. View pending migrations:
   ```bash
   dotnet ef migrations has-pending-model-changes \
     --project src/APG.Persistence/APG.Persistence.csproj \
     --startup-project src/APG.API/APG.API.csproj
   ```

---

## 📚 Best Practices

### 1. Connection String Management

✅ **DO:**
- Use User Secrets for local development
- Use environment variables in Docker
- Use Azure Key Vault in production
- Keep production passwords out of source control

❌ **DON'T:**
- Commit passwords to Git
- Use weak passwords
- Reuse passwords across environments

### 2. Migrations

✅ **DO:**
- Create descriptive migration names
- Review generated migration code
- Test migrations on a copy of production data
- Use idempotent SQL scripts for production
- Keep migrations in source control

❌ **DON'T:**
- Edit generated migration files manually (unless necessary)
- Delete old migrations
- Apply migrations directly to production without testing

### 3. Docker Volumes

✅ **DO:**
- Use named volumes for data persistence
- Backup your database regularly
- Test volume mounting

❌ **DON'T:**
- Delete volumes without backup
- Store sensitive data in containers without volumes

### 4. Performance

✅ **DO:**
- Use async/await for database operations
- Implement connection resiliency (already configured)
- Use `IQueryable` and projection
- Index frequently queried columns
- Use `.AsNoTracking()` for read-only queries

❌ **DON'T:**
- Load entire tables into memory
- Use `SELECT *` when you only need specific columns
- Forget to dispose DbContext (handled by DI)

---

## 🔄 Common Commands Cheat Sheet

```bash
# Docker Commands
docker compose up -d              # Start all services
docker compose down               # Stop all services
docker compose down -v            # Stop and remove volumes
docker compose logs -f            # Follow logs
docker compose ps                 # List running containers
docker compose restart api        # Restart API container

# Database Commands (inside container)
docker compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" \
  -Q "SELECT name FROM sys.databases"

# EF Core Migrations
dotnet ef migrations add <MigrationName> --project ... --startup-project ...
dotnet ef database update --project ... --startup-project ...
dotnet ef migrations remove --project ... --startup-project ...
dotnet ef database drop --project ... --startup-project ...
dotnet ef dbcontext info --project ... --startup-project ...

# .NET CLI
dotnet build                     # Build solution
dotnet run --project src/APG.API # Run API locally
dotnet test                      # Run tests
dotnet restore                   # Restore NuGet packages
```

---

## 📞 Support

If you encounter issues not covered in this guide:

1. Check Docker logs: `docker compose logs -f`
2. Check API logs in the console
3. Verify network connectivity between containers
4. Ensure SQL Server is fully initialized before connecting

---

## 🎓 Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
- [SQL Server on Docker](https://hub.docker.com/_/microsoft-mssql-server)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**Last Updated**: December 4, 2025
