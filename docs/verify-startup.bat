@echo off
REM Startup Verification Script for Windows
REM This script verifies that all services are running correctly

echo.
echo APG Backend - Startup Verification
echo ======================================
echo.

REM Check if Docker is running
echo 1. Checking Docker...
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Docker is not running
    echo    Please start Docker Desktop and try again
    exit /b 1
) else (
    echo [OK] Docker is running
)
echo.

REM Check if docker-compose.yml exists
echo 2. Checking docker-compose.yml...
if not exist "..\docker-compose.yml" (
    echo [ERROR] docker-compose.yml not found
    echo    Please run this script from the APG_Backend\docs directory
    exit /b 1
) else (
    echo [OK] docker-compose.yml found
)
echo.

REM Check if containers are running
echo 3. Checking containers...
docker compose ps sqlserver | find "Up" >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] SQL Server container is not running
    echo    Run: docker compose up sqlserver -d
) else (
    echo [OK] SQL Server container is running
)

docker compose ps api | find "Up" >nul 2>&1
if %errorlevel% neq 0 (
    echo [WARNING] API container is not running
    echo    This is OK if you're running the API locally
) else (
    echo [OK] API container is running
)
echo.

REM Check SQL Server connectivity
echo 4. Checking SQL Server connectivity...
timeout /t 2 /nobreak >nul
docker compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1" >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Cannot connect to SQL Server
    echo    SQL Server might still be initializing. Wait 30 seconds and try again.
) else (
    echo [OK] SQL Server is accepting connections
)
echo.

REM Check if database exists
echo 5. Checking if APGDb database exists...
docker compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT name FROM sys.databases WHERE name = 'APGDb'" -h -1 2>nul | find "APGDb" >nul
if %errorlevel% neq 0 (
    echo [WARNING] APGDb database does not exist yet
    echo    It will be created when you first run the API
) else (
    echo [OK] APGDb database exists
)
echo.

REM Check API health endpoint
echo 6. Checking API health endpoint...
curl -s -o nul -w "%%{http_code}" http://localhost:5000/health > temp_http_code.txt 2>nul
set /p HTTP_CODE=<temp_http_code.txt
del temp_http_code.txt

if "%HTTP_CODE%"=="200" (
    echo [OK] API health check passed (HTTP %HTTP_CODE%^)
    echo    API is running at: http://localhost:5000
) else if "%HTTP_CODE%"=="000" (
    echo [WARNING] API is not responding
    echo    Run: docker compose up api -d
    echo    Or run locally: cd APG_Backend\src\APG.API ^&^& dotnet run
) else (
    echo [ERROR] API health check failed (HTTP %HTTP_CODE%^)
)
echo.

REM Summary
echo ======================================
echo Summary
echo ======================================
echo.
echo Quick Links:
echo    - Swagger UI: http://localhost:5000/swagger
echo    - Health Check: http://localhost:5000/health
echo    - SQL Server: localhost,1433 (sa / YourStrong@Passw0rd^)
echo.
echo Next Steps:
echo    1. Open Swagger UI to test the API
echo    2. Connect Visual Studio to the database
echo    3. Start developing your features!
echo.
echo For more information, see:
echo   - QUICKSTART.md
echo   - APG_Backend\README_DB.md
echo.

endlocal
