#!/bin/bash

# Remove Last Migration Script
# Usage: ./remove-migration.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
PERSISTENCE_PROJECT="$ROOT_DIR/src/APG.Persistence/APG.Persistence.csproj"
STARTUP_PROJECT="$ROOT_DIR/src/APG.API/APG.API.csproj"

echo "Removing last migration..."
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

# Remove migration
dotnet ef migrations remove \
    --project "$PERSISTENCE_PROJECT" \
    --startup-project "$STARTUP_PROJECT" \
    --force \
    --verbose

echo ""
echo "✅ Last migration removed successfully!"
