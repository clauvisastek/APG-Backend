# APG Backend Quick Start Guide

This is a quick reference for getting your .NET 8 Web API + SQL Server environment up and running.

## 🚀 Quick Start (5 minutes)

### Option 1: Docker Compose (Recommended)

```bash
# From the APG_Backend directory
docker compose up -d

# Wait for services to start (30 seconds)
docker compose logs -f

# Verify
curl http://localhost:5000/health
open http://localhost:5000/swagger
```

**That's it!** The API will automatically:
- Start SQL Server
- Create the database
- Apply migrations
- Be ready to use

### Option 2: Local Development

```bash
# 1. Start SQL Server only
docker compose up sqlserver -d

# 2. Navigate to API
cd src/APG.API

# 3. Apply migrations (macOS/Linux)
./scripts/update-database.sh

# Or on Windows
scripts\update-database.bat

# 4. Run API
cd src/APG.API
dotnet run

# 5. Access
open http://localhost:5000/swagger
```

## 🔗 Service URLs

| Service | URL | Description |
|---------|-----|-------------|
| API | http://localhost:5000 | REST API endpoints |
| Swagger UI | http://localhost:5000/swagger | API documentation |
| Health Check | http://localhost:5000/health | Health status |
| SQL Server | localhost,1433 | Database server |

## 🔑 Default Credentials

**SQL Server:**
- **Host**: `localhost,1433`
- **Username**: `sa`
- **Password**: `YourStrong@Passw0rd`
- **Database**: `APGDb`

⚠️ **Change these in production!**

## 📝 Common Commands

```bash
# Start/stop services
docker compose up -d
docker compose down

# View logs
docker compose logs -f

# Rebuild after code changes
docker compose build api
docker compose up -d

# Create migration (with .NET SDK installed)
cd APG_Backend
./scripts/create-migration.sh MyMigration

# Connect to SQL Server
docker compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd"
```

## 🔍 Connect from Visual Studio

1. Open **SQL Server Object Explorer**
2. Add SQL Server: `localhost,1433`
3. Authentication: **SQL Server Authentication**
4. Login: `sa` / Password: `YourStrong@Passw0rd`
5. Browse `APGDb` database

## 📖 Full Documentation

- **Database Setup**: [README_DB.md](./README_DB.md)
- **Backend README**: [../README.md](../README.md)
- **Frontend**: [../../APG_Front/README.md](../../APG_Front/README.md)

## ❓ Quick Troubleshooting

**API won't start?**
```bash
docker compose logs api
```

**Can't connect to database?**
```bash
docker compose logs sqlserver
# Wait for: "SQL Server is now ready for client connections"
```

**Port conflict?**
```bash
# macOS/Linux: Check what's using port
lsof -i :1433
lsof -i :5000

# Windows
netstat -ano | findstr :1433
netstat -ano | findstr :5000
```

**Reset everything:**
```bash
docker compose down -v  # ⚠️ Deletes all data!
docker compose up -d
```

## 🎯 Next Steps

1. ✅ Start services with `docker compose up -d`
2. ✅ Test API at http://localhost:5000/swagger
3. ✅ Connect Visual Studio to database
4. 📖 Read full documentation in README_DB.md
5. 🔨 Start building your features!

---

**Need help?** See [README_DB.md](./README_DB.md) for detailed instructions.
