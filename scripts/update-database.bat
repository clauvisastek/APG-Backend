@echo off
REM Update Database Script for Windows
REM Usage: update-database.bat

setlocal

set SCRIPT_DIR=%~dp0
set ROOT_DIR=%SCRIPT_DIR%..
set PERSISTENCE_PROJECT=%ROOT_DIR%\src\APG.Persistence\APG.Persistence.csproj
set STARTUP_PROJECT=%ROOT_DIR%\src\APG.API\APG.API.csproj

echo Applying migrations to database...
echo Persistence project: %PERSISTENCE_PROJECT%
echo Startup project: %STARTUP_PROJECT%
echo.

dotnet ef database update --project "%PERSISTENCE_PROJECT%" --startup-project "%STARTUP_PROJECT%" --verbose

echo.
echo Database updated successfully!

endlocal
