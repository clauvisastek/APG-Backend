@echo off
REM Remove Last Migration Script for Windows
REM Usage: remove-migration.bat

setlocal

set SCRIPT_DIR=%~dp0
set ROOT_DIR=%SCRIPT_DIR%..
set PERSISTENCE_PROJECT=%ROOT_DIR%\src\APG.Persistence\APG.Persistence.csproj
set STARTUP_PROJECT=%ROOT_DIR%\src\APG.API\APG.API.csproj

echo Removing last migration...
echo Persistence project: %PERSISTENCE_PROJECT%
echo Startup project: %STARTUP_PROJECT%
echo.

dotnet ef migrations remove --project "%PERSISTENCE_PROJECT%" --startup-project "%STARTUP_PROJECT%" --force --verbose

echo.
echo Last migration removed successfully!

endlocal
