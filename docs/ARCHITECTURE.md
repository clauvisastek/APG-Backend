# APG Architecture Diagram

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         CLIENT BROWSER                          │
│                     http://localhost:5173                       │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ HTTP/HTTPS
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      APG FRONTEND (React)                       │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Components: Layout, Forms, DataTables, Modals           │  │
│  │  Pages: Home, Projects, Resources, Calculette, Admin    │  │
│  │  Services: API Client, Auth0 Integration                │  │
│  │  Routing: React Router                                   │  │
│  │  State: React Hooks + Context                            │  │
│  └──────────────────────────────────────────────────────────┘  │
│                         Port: 5173                              │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ REST API (JSON)
                             │ CORS Enabled
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│               APG BACKEND (ASP.NET Core 8 Web API)              │
│                     http://localhost:5000                       │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    APG.API (Presentation)                 │  │
│  │  • Controllers (TestController, etc.)                    │  │
│  │  • Middleware (CORS, Auth, Error Handling)               │  │
│  │  • Swagger UI / OpenAPI                                  │  │
│  │  • Health Checks                                          │  │
│  └────────────────────────┬─────────────────────────────────┘  │
│                            │                                     │
│  ┌────────────────────────▼─────────────────────────────────┐  │
│  │              APG.Application (Business Logic)             │  │
│  │  • Services                                               │  │
│  │  • DTOs                                                   │  │
│  │  • Interfaces                                             │  │
│  │  • Validation                                             │  │
│  └────────────────────────┬─────────────────────────────────┘  │
│                            │                                     │
│  ┌────────────────────────▼─────────────────────────────────┐  │
│  │              APG.Persistence (Data Access)                │  │
│  │  • AppDbContext (EF Core)                                │  │
│  │  • Entity Configurations                                 │  │
│  │  • Migrations                                             │  │
│  │  • Repository Pattern (optional)                         │  │
│  └────────────────────────┬─────────────────────────────────┘  │
│                            │                                     │
│  ┌────────────────────────▼─────────────────────────────────┐  │
│  │                 APG.Domain (Core)                         │  │
│  │  • Entities (BaseEntity, TestEntity, ...)               │  │
│  │  • Value Objects                                          │  │
│  │  • Domain Logic                                           │  │
│  │  • Interfaces                                             │  │
│  └──────────────────────────────────────────────────────────┘  │
│                         Port: 5000                              │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ EF Core
                             │ SQL Connection
                             │ (TrustServerCertificate)
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│              SQL SERVER 2022 (Linux Container)                  │
│                     localhost:1433                              │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Database: APGDb                                         │  │
│  │  ┌────────────────────────────────────────────────────┐ │  │
│  │  │  Tables:                                           │ │  │
│  │  │  • TestEntities                                    │ │  │
│  │  │  • __EFMigrationsHistory                          │ │  │
│  │  │  • [Your future tables...]                        │ │  │
│  │  └────────────────────────────────────────────────────┘ │  │
│  │                                                          │  │
│  │  Authentication: SQL Server (sa)                        │  │
│  │  Persistence: Docker Volume (apg_sqlserver_data)        │  │
│  └──────────────────────────────────────────────────────────┘  │
│                         Port: 1433                              │
└─────────────────────────────────────────────────────────────────┘
```

## Docker Compose Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                        Docker Host                               │
│                                                                  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │              Docker Network: apg-network                   │  │
│  │                    (bridge driver)                         │  │
│  │                                                            │  │
│  │  ┌──────────────────────┐    ┌──────────────────────┐   │  │
│  │  │  Container:          │    │  Container:          │   │  │
│  │  │  apg-sqlserver       │    │  apg-api             │   │  │
│  │  │                      │    │                      │   │  │
│  │  │  Image:              │    │  Build:              │   │  │
│  │  │  mcr.microsoft.com/  │    │  ./APG_Backend/      │   │  │
│  │  │  mssql/server:2022   │    │  Dockerfile          │   │  │
│  │  │                      │    │                      │   │  │
│  │  │  Ports:              │    │  Ports:              │   │  │
│  │  │  1433 → 1433         │◄───┤  5000 → 80           │   │  │
│  │  │                      │    │                      │   │  │
│  │  │  Volume:             │    │  Depends On:         │   │  │
│  │  │  sqlserver_data      │    │  • sqlserver         │   │  │
│  │  │                      │    │    (health check)    │   │  │
│  │  │  Health Check:       │    │                      │   │  │
│  │  │  sqlcmd -Q "SELECT1" │    │  Environment:        │   │  │
│  │  │                      │    │  • ConnectionString  │   │  │
│  │  └──────────────────────┘    └──────────────────────┘   │  │
│  │                                                            │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │         Named Volume: apg_sqlserver_data                   │  │
│  │         Location: /var/lib/docker/volumes/...             │  │
│  │         Purpose: Persist database files                    │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
        │                            │
        │ Port 1433                  │ Port 5000
        ▼                            ▼
   ┌─────────┐                  ┌─────────┐
   │  SSMS   │                  │ Browser │
   │  VS     │                  │ Swagger │
   │  Azure  │                  │  curl   │
   │  Data   │                  │ Postman │
   │ Studio  │                  └─────────┘
   └─────────┘
```

## Clean Architecture Layers

```
┌────────────────────────────────────────────────────────────────┐
│  Direction of Dependencies: Inward (→)                         │
└────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                   PRESENTATION LAYER                          │
│                      (APG.API)                                │
│                                                               │
│  • HTTP Endpoints (Controllers)                              │
│  • Request/Response Models                                   │
│  • Middleware & Filters                                       │
│  • Dependency Injection Configuration                         │
│                                                               │
│  Dependencies: Application, Persistence                       │
└──────────────────────┬───────────────────────────────────────┘
                       │ depends on ↓
┌──────────────────────▼───────────────────────────────────────┐
│                  APPLICATION LAYER                            │
│                  (APG.Application)                            │
│                                                               │
│  • Business Logic                                             │
│  • Application Services                                       │
│  • DTOs (Data Transfer Objects)                              │
│  • Interfaces for Infrastructure                             │
│  • Validation Logic                                           │
│                                                               │
│  Dependencies: Domain only                                    │
└──────────────────────┬───────────────────────────────────────┘
                       │ depends on ↓
┌──────────────────────▼───────────────────────────────────────┐
│                    DOMAIN LAYER                               │
│                    (APG.Domain)                               │
│                                                               │
│  • Entities                                                   │
│  • Value Objects                                              │
│  • Domain Events                                              │
│  • Domain Services                                            │
│  • Business Rules                                             │
│                                                               │
│  Dependencies: None (Pure business logic)                     │
└───────────────────────────────────────────────────────────────┘
                       ▲
                       │ implements ↑
┌──────────────────────┴───────────────────────────────────────┐
│               INFRASTRUCTURE LAYER                            │
│                  (APG.Persistence)                            │
│                                                               │
│  • Database Context (EF Core)                                │
│  • Entity Configurations                                      │
│  • Migrations                                                 │
│  • Repository Implementations                                 │
│  • External Service Integrations                             │
│                                                               │
│  Dependencies: Application, Domain                            │
└───────────────────────────────────────────────────────────────┘
```

## Data Flow

### Creating a New Entity

```
1. Client (Browser)
   │
   │ HTTP POST /api/Test
   │ Body: { name: "Test", description: "..." }
   │
   ▼
2. TestController.Create()
   │
   │ Validate request
   │ Map DTO to Entity
   │
   ▼
3. AppDbContext
   │
   │ _context.TestEntities.Add(entity)
   │ await _context.SaveChangesAsync()
   │
   ▼
4. Entity Framework Core
   │
   │ Generate SQL: INSERT INTO TestEntities...
   │ Execute command
   │
   ▼
5. SQL Server
   │
   │ Insert data into table
   │ Return inserted ID
   │
   ▼
6. TestController.Create()
   │
   │ Return 201 Created
   │ Location header: /api/Test/{id}
   │ Body: Created entity
   │
   ▼
7. Client (Browser)
   │
   │ Receive response
   │ Update UI
```

### Reading Data

```
1. Client → GET /api/Test
   │
   ▼
2. TestController.GetAll()
   │
   ▼
3. AppDbContext.TestEntities.ToListAsync()
   │
   ▼
4. EF Core → SELECT * FROM TestEntities
   │
   ▼
5. SQL Server → Return rows
   │
   ▼
6. EF Core → Map to entities
   │
   ▼
7. TestController → Return 200 OK + JSON
   │
   ▼
8. Client → Display data
```

## Migration Flow

```
1. Developer creates/modifies entity
   │
   ▼
2. Run: ./scripts/create-migration.sh AddEntity
   │
   ▼
3. EF Core Tools
   │
   │ Compare current model with last snapshot
   │ Generate migration code (Up/Down methods)
   │ Create files in Persistence/Migrations/
   │
   ▼
4. Developer reviews migration
   │
   ▼
5. Run: ./scripts/update-database.sh
   │
   ▼
6. EF Core
   │
   │ Check __EFMigrationsHistory table
   │ Execute pending migrations
   │ Update __EFMigrationsHistory
   │
   ▼
7. SQL Server
   │
   │ Schema updated
   │ Tables/columns/indexes created
```

## Request/Response Flow with Authentication (Future)

```
┌──────────┐
│ Browser  │
└────┬─────┘
     │ 1. Request resource
     ▼
┌─────────────────┐
│   APG Frontend  │
└────┬────────────┘
     │ 2. Check auth
     │    No token? → Redirect to Auth0
     │    Has token? → Include in API request
     ▼
┌─────────────────┐
│     Auth0       │ (if needed)
└────┬────────────┘
     │ 3. Return JWT token
     ▼
┌─────────────────┐
│   APG Frontend  │
└────┬────────────┘
     │ 4. HTTP GET /api/resource
     │    Header: Authorization: Bearer <token>
     ▼
┌─────────────────┐
│   APG Backend   │
└────┬────────────┘
     │ 5. Validate JWT
     │    • Check signature
     │    • Check expiration
     │    • Extract claims
     ▼
┌─────────────────┐
│  Authorization  │
│   Middleware    │
└────┬────────────┘
     │ 6. Check permissions
     │    • User role
     │    • Resource access
     ▼
┌─────────────────┐
│   Controller    │
└────┬────────────┘
     │ 7. Execute business logic
     ▼
┌─────────────────┐
│   Database      │
└────┬────────────┘
     │ 8. Return data
     ▼
┌──────────┐
│ Browser  │ Display result
└──────────┘
```

## Development vs Production Architecture

### Development (Current)

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│   Frontend   │    │   Backend    │    │  SQL Server  │
│ (npm run dev)│───▶│ (dotnet run) │───▶│   (Docker)   │
│ localhost:   │    │ localhost:   │    │ localhost:   │
│    5173      │    │    5000      │    │    1433      │
└──────────────┘    └──────────────┘    └──────────────┘
```

### Production (Future)

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│   Frontend   │    │ Load Balancer│    │   Backend    │
│   (Static    │───▶│   (Nginx/    │───▶│   (Docker    │
│    Files)    │    │   Azure LB)  │    │   Swarm/K8s) │
└──────────────┘    └──────────────┘    └──────┬───────┘
                                                │
                                                ▼
                                        ┌──────────────┐
                                        │  SQL Server  │
                                        │   (Azure/    │
                                        │    AWS RDS)  │
                                        └──────────────┘
```

---

**Legend:**
- `│` / `─` : Flow direction
- `▼` : Dependency direction
- `◄──►` : Bidirectional communication
- `→` : Data flow

---

**Last Updated**: December 4, 2025
