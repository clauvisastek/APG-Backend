#!/bin/bash

# Create Migration Script
# Usage: ./create-migration.sh <MigrationName>

set -e

if [ -z "$1" ]; then
    echo "Error: Migration name is required"
    echo "Usage: ./create-migration.sh <MigrationName>"
    echo "Example: ./create-migration.sh InitialCreate"
    exit 1
fi

MIGRATION_NAME=$1
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
PERSISTENCE_PROJECT="$ROOT_DIR/src/APG.Persistence/APG.Persistence.csproj"
STARTUP_PROJECT="$ROOT_DIR/src/APG.API/APG.API.csproj"

echo "Creating migration: $MIGRATION_NAME"
echo "Persistence project: $PERSISTENCE_PROJECT"
echo "Startup project: $STARTUP_PROJECT"
echo ""

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK is not installed or not in PATH"
    exit 1
fi

# Check if projects exist
if [ ! -f "$PERSISTENCE_PROJECT" ]; then
    echo "Error: Persistence project not found at $PERSISTENCE_PROJECT"
    exit 1
fi

if [ ! -f "$STARTUP_PROJECT" ]; then
    echo "Error: Startup project not found at $STARTUP_PROJECT"
    exit 1
fi

# Create migration
dotnet ef migrations add "$MIGRATION_NAME" \
    --project "$PERSISTENCE_PROJECT" \
    --startup-project "$STARTUP_PROJECT" \
    --output-dir Migrations \
    --verbose

echo ""
echo "✅ Migration '$MIGRATION_NAME' created successfully!"
echo ""
echo "Next steps:"
echo "  1. Review the generated migration in src/APG.Persistence/Migrations/"
echo "  2. Run './scripts/update-database.sh' to apply the migration"
