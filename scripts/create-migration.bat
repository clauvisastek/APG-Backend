@echo off
REM Create Migration Script for Windows
REM Usage: create-migration.bat <MigrationName>

setlocal

if "%~1"=="" (
    echo Error: Migration name is required
    echo Usage: create-migration.bat ^<MigrationName^>
    echo Example: create-migration.bat InitialCreate
    exit /b 1
)

set MIGRATION_NAME=%~1
set SCRIPT_DIR=%~dp0
set ROOT_DIR=%SCRIPT_DIR%..
set PERSISTENCE_PROJECT=%ROOT_DIR%\src\APG.Persistence\APG.Persistence.csproj
set STARTUP_PROJECT=%ROOT_DIR%\src\APG.API\APG.API.csproj

echo Creating migration: %MIGRATION_NAME%
echo Persistence project: %PERSISTENCE_PROJECT%
echo Startup project: %STARTUP_PROJECT%
echo.

dotnet ef migrations add "%MIGRATION_NAME%" --project "%PERSISTENCE_PROJECT%" --startup-project "%STARTUP_PROJECT%" --output-dir Migrations --verbose

echo.
echo Migration '%MIGRATION_NAME%' created successfully!
echo.
echo Next steps:
echo   1. Review the generated migration in src\APG.Persistence\Migrations\
echo   2. Run 'scripts\update-database.bat' to apply the migration

endlocal
