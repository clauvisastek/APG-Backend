# Development Environment Setup

This guide will help you set up your development environment for the APG Backend project.

## Prerequisites Installation

### 1. Docker Desktop

**Why needed**: To run SQL Server and the API in containers.

**Installation**:
- **macOS**: [Download Docker Desktop for Mac](https://docs.docker.com/desktop/install/mac-install/)
- **Windows**: [Download Docker Desktop for Windows](https://docs.docker.com/desktop/install/windows-install/)
- **Linux**: [Install Docker Engine](https://docs.docker.com/engine/install/)

**Verify installation**:
```bash
docker --version
docker compose version
```

### 2. .NET 8 SDK

**Why needed**: To build, run, and debug the API locally, and to run EF Core migrations.

**Installation**:
- Visit: https://dotnet.microsoft.com/download/dotnet/8.0
- Download and install the SDK (not just the runtime)

**Verify installation**:
```bash
dotnet --version
# Should show 8.0.x
```

### 3. IDE / Code Editor (Choose one)

#### Option A: Visual Studio 2022 (Recommended for Windows)

**Features**:
- Full-featured IDE
- Built-in SQL Server Object Explorer
- Excellent debugging
- Entity Framework tools
- Docker integration

**Installation**:
- Download: https://visualstudio.microsoft.com/downloads/
- Install **ASP.NET and web development** workload
- Install **Data storage and processing** workload (for SQL Server tools)

#### Option B: Visual Studio Code (Cross-platform)

**Features**:
- Lightweight
- Cross-platform
- Great extensions ecosystem

**Installation**:
1. Download: https://code.visualstudio.com/
2. Install these extensions:
   - **C# Dev Kit** by Microsoft
   - **C#** by Microsoft
   - **Docker** by Microsoft
   - **SQL Server (mssql)** by Microsoft
   - **REST Client** by Huachao Mao (optional)

#### Option C: JetBrains Rider (Cross-platform, Commercial)

**Features**:
- Powerful IDE
- Excellent refactoring tools
- Built-in database tools

**Installation**:
- Download: https://www.jetbrains.com/rider/

### 4. Database Management Tools (Optional but Recommended)

#### Option A: SQL Server Management Studio (SSMS) - Windows only

**Installation**:
- Download: https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms

#### Option B: Azure Data Studio - Cross-platform

**Installation**:
- Download: https://docs.microsoft.com/sql/azure-data-studio/download-azure-data-studio

#### Option C: DBeaver - Cross-platform

**Installation**:
- Download: https://dbeaver.io/download/

### 5. Git

**Why needed**: Version control.

**Installation**:
- **macOS**: Included with Xcode Command Line Tools, or use Homebrew: `brew install git`
- **Windows**: Download from https://git-scm.com/download/win
- **Linux**: `sudo apt install git` (Ubuntu/Debian) or `sudo yum install git` (Red Hat/Fedora)

**Verify installation**:
```bash
git --version
```

---

## Setting Up the Project

### 1. Clone the Repository (if applicable)

```bash
cd ~/Documents/Projets/DPO
git clone <your-repo-url> Apps
cd Apps
```

### 2. Install .NET Tools

Install the Entity Framework Core CLI tools globally:

```bash
dotnet tool install --global dotnet-ef
# Or update if already installed
dotnet tool update --global dotnet-ef

# Verify
dotnet ef --version
```

### 3. Configure User Secrets (Recommended)

Instead of storing passwords in `appsettings.json`, use User Secrets for local development:

```bash
cd APG_Backend/src/APG.API

# Initialize user secrets
dotnet user-secrets init

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=APGDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"

# List secrets
dotnet user-secrets list
```

### 4. Start SQL Server

```bash
cd /path/to/Apps
docker compose up sqlserver -d

# Wait for SQL Server to be ready (30 seconds)
docker compose logs -f sqlserver
# Look for: "SQL Server is now ready for client connections"
```

### 5. Restore NuGet Packages

```bash
cd APG_Backend
dotnet restore
```

### 6. Create and Apply Initial Migration

```bash
# Create migration
./scripts/create-migration.sh InitialCreate

# Apply migration
./scripts/update-database.sh

# Or on Windows:
# scripts\create-migration.bat InitialCreate
# scripts\update-database.bat
```

### 7. Run the API

```bash
cd src/APG.API
dotnet run
```

Or press F5 in Visual Studio/Rider, or use the Run/Debug configuration in VS Code.

### 8. Verify Everything Works

Open your browser and navigate to:
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

Try creating a test entity using the Swagger UI or curl:

```bash
curl -X POST http://localhost:5000/api/Test \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Item","description":"My first test","isActive":true}'
```

---

## IDE-Specific Setup

### Visual Studio 2022

1. **Open Solution**:
   - File → Open → Project/Solution
   - Navigate to `APG_Backend/APG_Backend.sln`

2. **Set Startup Project**:
   - Right-click `APG.API` project → Set as Startup Project

3. **Configure Launch Settings**:
   - Already configured in `Properties/launchSettings.json`
   - Profile should be set to "http"

4. **Connect to Database**:
   - View → SQL Server Object Explorer
   - Add SQL Server: `localhost,1433`
   - Use SQL Server Authentication: `sa` / `YourStrong@Passw0rd`

5. **Run**:
   - Press F5 to start with debugging
   - Or Ctrl+F5 to start without debugging

### Visual Studio Code

1. **Open Workspace**:
   ```bash
   cd /path/to/Apps
   code .
   ```

2. **Install Recommended Extensions** (if prompted)

3. **Configure Launch Settings**:
   
   Create `.vscode/launch.json`:
   ```json
   {
     "version": "0.2.0",
     "configurations": [
       {
         "name": ".NET Core Launch (web)",
         "type": "coreclr",
         "request": "launch",
         "preLaunchTask": "build",
         "program": "${workspaceFolder}/APG_Backend/src/APG.API/bin/Debug/net8.0/APG.API.dll",
         "args": [],
         "cwd": "${workspaceFolder}/APG_Backend/src/APG.API",
         "stopAtEntry": false,
         "serverReadyAction": {
           "action": "openExternally",
           "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
         },
         "env": {
           "ASPNETCORE_ENVIRONMENT": "Development"
         }
       }
     ]
   }
   ```

   Create `.vscode/tasks.json`:
   ```json
   {
     "version": "2.0.0",
     "tasks": [
       {
         "label": "build",
         "command": "dotnet",
         "type": "process",
         "args": [
           "build",
           "${workspaceFolder}/APG_Backend/APG_Backend.sln",
           "/property:GenerateFullPaths=true",
           "/consoleloggerparameters:NoSummary"
         ],
         "problemMatcher": "$msCompile"
       }
     ]
   }
   ```

4. **Connect to Database**:
   - Install "SQL Server (mssql)" extension
   - Click SQL Server icon in sidebar
   - Add Connection: `localhost,1433`, sa, password

5. **Run**:
   - Press F5 or click Run → Start Debugging

### JetBrains Rider

1. **Open Solution**:
   - File → Open
   - Navigate to `APG_Backend/APG_Backend.sln`

2. **Configure Run Configuration**:
   - Should be automatically detected
   - Select `APG.API: http` profile

3. **Connect to Database**:
   - Database tool window (right sidebar)
   - + → Data Source → SQL Server
   - Host: `localhost`, Port: `1433`
   - User: `sa`, Password: `YourStrong@Passw0rd`
   - Database: `APGDb`

4. **Run**:
   - Press Shift+F10 to run
   - Or Shift+F9 to debug

---

## Useful Development Commands

### Project Management

```bash
# Add a new project to the solution
dotnet new classlib -n APG.NewProject -o src/APG.NewProject
dotnet sln add src/APG.NewProject/APG.NewProject.csproj

# Add project reference
cd src/APG.API
dotnet add reference ../APG.NewProject/APG.NewProject.csproj

# Install a NuGet package
dotnet add package PackageName

# Update all packages
dotnet restore
```

### Build and Run

```bash
# Build
dotnet build

# Run
dotnet run --project src/APG.API/APG.API.csproj

# Run with specific environment
dotnet run --project src/APG.API/APG.API.csproj --environment Production

# Watch mode (auto-restart on file changes)
dotnet watch --project src/APG.API/APG.API.csproj
```

### Testing

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Run specific test
dotnet test --filter FullyQualifiedName~NameOfTest
```

---

## Environment Variables

You can override settings using environment variables:

### macOS/Linux

```bash
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=APGDb;..."
dotnet run --project src/APG.API/APG.API.csproj
```

### Windows (PowerShell)

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=APGDb;..."
dotnet run --project src/APG.API/APG.API.csproj
```

### Windows (CMD)

```cmd
set ASPNETCORE_ENVIRONMENT=Development
set ConnectionStrings__DefaultConnection=Server=localhost,1433;Database=APGDb;...
dotnet run --project src/APG.API/APG.API.csproj
```

---

## Troubleshooting Development Environment

### "dotnet: command not found"

**Solution**: Install .NET SDK and ensure it's in your PATH.

```bash
# macOS/Linux: Add to ~/.bashrc or ~/.zshrc
export PATH="$PATH:/usr/local/share/dotnet"

# Windows: Usually added automatically, but verify:
# Control Panel → System → Advanced → Environment Variables
# Check that C:\Program Files\dotnet is in PATH
```

### "Cannot connect to Docker daemon"

**Solution**:
1. Make sure Docker Desktop is running
2. On macOS/Linux, your user must be in the `docker` group:
   ```bash
   sudo usermod -aG docker $USER
   # Log out and log back in
   ```

### "Port 1433 already in use"

**Solution**:
1. Check what's using the port:
   ```bash
   # macOS/Linux
   lsof -i :1433
   
   # Windows
   netstat -ano | findstr :1433
   ```

2. Stop the conflicting service or change the port in `docker-compose.yml`

### "Unable to load DLL 'sni.dll'"

**Solution**: This usually happens on macOS. Make sure you're using the correct .NET SDK for your architecture (ARM64 vs x64).

### Database connection fails with SSL error

**Solution**: Add `TrustServerCertificate=True` to your connection string.

---

## Next Steps

Now that your environment is set up:

1. ✅ Read [QUICKSTART.md](./QUICKSTART.md) for quick commands
2. ✅ Read [APG_Backend/README.md](./APG_Backend/README.md) for project overview
3. ✅ Read [APG_Backend/README_DB.md](./APG_Backend/README_DB.md) for database details
4. 🔨 Start developing!

---

## Additional Resources

- [.NET CLI Reference](https://docs.microsoft.com/dotnet/core/tools/)
- [Entity Framework Core Tools](https://docs.microsoft.com/ef/core/cli/dotnet)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [ASP.NET Core Configuration](https://docs.microsoft.com/aspnet/core/fundamentals/configuration/)

---

**Last Updated**: December 4, 2025
