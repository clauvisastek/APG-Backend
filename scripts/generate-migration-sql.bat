@echo off
REM Generate SQL Script for Migrations for Windows
REM Usage: generate-migration-sql.bat

setlocal

set SCRIPT_DIR=%~dp0
set ROOT_DIR=%SCRIPT_DIR%..
set PERSISTENCE_PROJECT=%ROOT_DIR%\src\APG.Persistence\APG.Persistence.csproj
set STARTUP_PROJECT=%ROOT_DIR%\src\APG.API\APG.API.csproj
set OUTPUT_FILE=%ROOT_DIR%\migrations.sql

echo Generating SQL script for migrations...
echo Persistence project: %PERSISTENCE_PROJECT%
echo Startup project: %STARTUP_PROJECT%
echo Output file: %OUTPUT_FILE%
echo.

dotnet ef migrations script --project "%PERSISTENCE_PROJECT%" --startup-project "%STARTUP_PROJECT%" --output "%OUTPUT_FILE%" --idempotent --verbose

echo.
echo SQL script generated successfully at: %OUTPUT_FILE%
echo.
echo This script can be reviewed and executed by a DBA in production.

endlocal
