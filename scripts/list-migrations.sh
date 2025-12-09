#!/bin/bash

# List Migrations Script
# Usage: ./list-migrations.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
PERSISTENCE_PROJECT="$ROOT_DIR/src/APG.Persistence/APG.Persistence.csproj"
STARTUP_PROJECT="$ROOT_DIR/src/APG.API/APG.API.csproj"

echo "Listing all migrations..."
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

# List migrations
dotnet ef migrations list \
    --project "$PERSISTENCE_PROJECT" \
    --startup-project "$STARTUP_PROJECT"

echo ""
