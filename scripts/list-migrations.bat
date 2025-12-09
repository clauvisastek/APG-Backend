@echo off
REM List Migrations Script for Windows
REM Usage: list-migrations.bat

setlocal

set SCRIPT_DIR=%~dp0
set ROOT_DIR=%SCRIPT_DIR%..
set PERSISTENCE_PROJECT=%ROOT_DIR%\src\APG.Persistence\APG.Persistence.csproj
set STARTUP_PROJECT=%ROOT_DIR%\src\APG.API\APG.API.csproj

echo Listing all migrations...
echo.

dotnet ef migrations list --project "%PERSISTENCE_PROJECT%" --startup-project "%STARTUP_PROJECT%"

echo.

endlocal
